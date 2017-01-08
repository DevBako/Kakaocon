using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using HtmlAgilityPack;
using Kakaocon.Handler;
using Kakaocon.Model;

namespace Kakaocon {
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window, IconSetListener, LocalImageListener {
		public MainWindow() {
			InitializeComponent();
		}

		Launcher launcher;
		bool isLocal = true;
		int requestId = 1;
		string ci_c;
		IconSet selectedSet;
		List<IconItem> selectedList;
		bool showing = false;
		bool closeFlag = false;
		IntPtr kakaoHandle;
		string searchText = "";
		int page = 1;

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			Store.CleanUpTemp();
			Store.Load();

			InitTray();

			foreach (string id in Store.DataList) {
				addItem(id);
			}

			launcher = new Launcher(this);
			launcher.Show();
		}

		private void addItem(string id) {
			try {
				if (Store.validation(id)) {
					IconItemView view = new IconItemView();
					view.setIconSet(id, Path.Combine(Store.OnlinePath, id, Store.TitleImageFileName));
					view.setLocalImageClickListener(this);
					view.setSelectable(true);

					stackLocalList.Children.Add(view);
				}
			}
			catch (Exception ex) {
			}
		}

		private void closeForce() {
			this.closeFlag = true;
			this.Close();
		}

		private void hideWindow() {
			showing = false;
			kakaoHandle = IntPtr.Zero;
			this.Hide();
		}
		
		private void Window_Deactivated(object sender, EventArgs e) {
			hideWindow();
		}

		private void Window_Closing(object sender, CancelEventArgs e) {
			if (closeFlag) {
				if (tray != null) {
					tray.Dispose();
				}
				if (launcher != null) {
					launcher.CloseForce();
				}
			}
			else {
				e.Cancel = true;
				hideWindow();
			}
		}

		public bool isShowing() {
			return showing;
		}

		public void Launcher_Clicked(IntPtr kakaoHandle, int left, int right, int bottom) {
			//Utils.SendFile(kakaoHandle, path);
			this.kakaoHandle = kakaoHandle;
			this.showing = true;
			if (launcher != null) {
				launcher.Hide();
			}
			if (left > 300) {
				this.Left = left - this.Width;
			}
			else {
				this.Left = right;
			}
			this.Top = bottom - this.Height;
			this.Opacity = 1;
			this.Show();
			this.Activate();
		}

		private void buttonClose_Response(object sender, CustomButtonEventArgs e) {
			this.Close();
		}

		private void Grid_MouseDown(object sender, MouseButtonEventArgs e) {
			DragMove();
		}

		private void ImageButton_Response(object sender, CustomButtonEventArgs e) {
			switch (e.Main) {
				case "local":
					if (!isLocal) {
						isLocal = true;
						buttonLocal.Selected = ImageButton.SelectedMode.True;
						buttonOnline.Selected = ImageButton.SelectedMode.False;
						gridOnline.Visibility = Visibility.Collapsed;
						gridLocal.Visibility = Visibility.Visible;
					}
					break;

				case "online":
					if (isLocal) {
						isLocal = false;
						buttonLocal.Selected = ImageButton.SelectedMode.False;
						buttonOnline.Selected = ImageButton.SelectedMode.True;
						gridOnline.Visibility = Visibility.Visible;
						gridLocal.Visibility = Visibility.Collapsed;

						textboxSearch.Focus();
						textboxSearch.SelectAll();
					}
					break;

				case "search":
					searchText = textboxSearch.Text;
					search(1);
					break;

				case "prev":
					search(page - 1);
					break;

				case "next":
					search(page + 1);
					break;

				case "close_modal":
					selectedSet = null;
					selectedList = null;
					gridInfo.Visibility = Visibility.Collapsed;
					break;

				case "download":
					textProgress.Text = "";
					gridProgress.Visibility = Visibility.Visible;

					if (selectedList != null) {
						int count = selectedList.Count;
						Utils.DownloadFiles(
							selectedSet,
							selectedList,
							(p) => { textProgress.Text = string.Format("{0} / {1}", p, count); },
							(f, id) => {
								gridProgress.Visibility = Visibility.Collapsed;
								if (f) {
									Store.add(id);
									addItem(id);
									buttonDownload.ViewMode = ImageButton.Mode.Disable;
									ImageButton_Response(null, new CustomButtonEventArgs("click", "close_modal", ""));
								}
								else {
									MessageBox.Show("잠시 후 다시 시도해주세요.");
								}
							});
					}
					break;
			}
		}

		private void search(int newPage) {
			page = Math.Max(1, newPage);
			gridResult.Children.Clear();
			buttonPrev.ViewMode = page == 1 ? ImageButton.Mode.Disable : ImageButton.Mode.Visible;
			buttonNext.ViewMode = ImageButton.Mode.Visible;
			textPage.Text = page.ToString();
			Network.getIconSet(searchText, page, ++requestId, (list, id, ci_c) => {
				if (requestId == id && list != null) {
					this.ci_c = ci_c;
					for (int i = 0; i < list.Count; i++) {
						IconItemView view = new IconItemView();
						view.Margin = new Thickness((i % 5) * 100, (i / 5) * 120, 0, 0);
						view.setIconSetClickListener(this);
						view.setSelectable(true);
						gridResult.Children.Add(view);
						view.setIconSet(list[i]);
					}
				}
			});
		}

		private void textboxSearch_KeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Enter) {
				ImageButton_Response(null, new CustomButtonEventArgs("click", "search", ""));
			}
		}

		public void IconSet_Clicked(IconSet iconSet) {
			scrollPreview.ScrollToTop();
			buttonDownload.ViewMode = Store.dataExists(iconSet.Id) ? ImageButton.Mode.Disable : ImageButton.Mode.Visible;
			gridInfo.Visibility = Visibility.Visible;
			gridOnlineItemList.Children.Clear();

			if (ci_c == null) {
				// show error
			}
			else {
				textDetailTitle.Text = iconSet.Title;
				Network.getIconList(iconSet.Id, ci_c, ++requestId, (list, id, rid) => {
					if (requestId == rid && list != null) {
						this.selectedSet = iconSet;
						this.selectedList = list;
						for (int i = 0; i < list.Count; i++) {
							IconItemView view = new IconItemView();
							view.Margin = new Thickness((i % 4) * 100, (i / 4) * 100, 0, 0);
							view.setIconSetClickListener(this);
							gridOnlineItemList.Children.Add(view);
							view.setUrl(list[i].path);
						}
					}
				});
			}
		}

		public void IconSet_Clicked(string id) {
			scrollList.ScrollToTop();
			gridLocalItemList.Children.Clear();

			if (Store.validation(id)) {
				List<string> list = Parser.parseLocaItemSet(id);
				if (list != null) {
					for (int i = 0; i < list.Count; i++) {
						IconItemView view = new IconItemView();
						view.Margin = new Thickness((i % 4) * 100, (i / 4) * 100, 0, 0);
						view.setLocalImageClickListener(this);
						view.setSelectable(true);
						gridLocalItemList.Children.Add(view);
						view.setPath(Path.Combine(Store.OnlinePath, id, list[i]));
					}
				}
				else {
					// show error
				}
			}
		}

		public void LocalImage_Clicked(string path) {
			if (Utils.IsWindowAvailable(kakaoHandle)) {
				string outputPath = Utils.ResizeImageTemporary(path, 150, 150);
				try {
					if (outputPath != null) {
						Utils.SendFile(kakaoHandle, outputPath);
					}
				}
				catch { }
			}
		}
	}
}
