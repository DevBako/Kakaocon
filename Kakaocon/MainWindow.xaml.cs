using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
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
		TabState windowState = TabState.Local;
		int requestId = 1;
		string ci_c;
		IconSet selectedSet;
		string selectedLocalSet;
		List<IconItem> selectedList;
		bool showing = false;
		bool closeFlag = false;
		IntPtr kakaoHandle;
		string searchText = "";
		int page = 1;

		enum TabState { Local, Search, Info };

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.DefaultConnectionLimit = 9999;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls 
				| SecurityProtocolType.Tls11
				| SecurityProtocolType.Tls12
				| SecurityProtocolType.Ssl3;

			Store.CleanUpTemp();
			Store.Load();

			InitTray();

			foreach (string id in Store.DataList) {
				addItem(id);
			}

			if (Store.DataList.Count > 0) {
				IconLocalSet_Clicked(Store.DataList[0]);
			}
			else {
				ImageButton_Response(null, new CustomButtonEventArgs("click", "online", ""));
			}

			launcher = new Launcher(this);
			launcher.Show();

			DispatcherTimer mainTimer = new DispatcherTimer();
			mainTimer.Interval = TimeSpan.FromMilliseconds(300);
			mainTimer.Tick += MainTimer_Tick;
			mainTimer.Start();

			DispatcherTimer updateTimer = new DispatcherTimer();
			updateTimer.Interval = TimeSpan.FromMinutes(30);
			updateTimer.Tick += UpdateTimer_Tick;
			updateTimer.Start();

			checkUpdate();
		}

		private void MainTimer_Tick(object sender, EventArgs e) {
			if (showing) {
				WindowInteropHelper helper = new WindowInteropHelper(this);
				IntPtr foreground = Utils.GetForegroundWindow();

				if (!Utils.IsWindowAvailable(kakaoHandle)) {
					hideWindow();
					return;
				}

				if (foreground != helper.Handle && foreground != kakaoHandle) {
					hideWindow();				
				}
			}
		}

		private void addItem(string id) {
			try {
				if (Store.GetTitle(id) != null) {
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
			Store.CleanUpTemp();
			this.Hide();
		}
		
		private void Window_Deactivated(object sender, EventArgs e) {
			//hideWindow();
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
			this.kakaoHandle = kakaoHandle;
			this.showing = true;
			if (launcher != null) {
				launcher.Hide();
			}

			if (Utils.isTowardsLeft(left, right)) {
				this.Left = right;
			}
			else {
				this.Left = left - this.Width;
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
					if (windowState != TabState.Local) {
						windowState = TabState.Local;
						buttonLocal.Selected = ImageButton.SelectedMode.True;
						buttonOnline.Selected = ImageButton.SelectedMode.False;
						buttonInfo.Selected = ImageButton.SelectedMode.False;
						gridOnline.Visibility = Visibility.Collapsed;
						gridLocal.Visibility = Visibility.Visible;
						gridInfo.Visibility = Visibility.Collapsed;

						if (selectedLocalSet != null) {
							buttonRemove.ViewMode = ImageButton.Mode.Visible;
						}
					}
					break;

				case "online":
					if (windowState != TabState.Search) {
						windowState = TabState.Search;
						buttonLocal.Selected = ImageButton.SelectedMode.False;
						buttonOnline.Selected = ImageButton.SelectedMode.True;
						buttonInfo.Selected = ImageButton.SelectedMode.False;
						gridOnline.Visibility = Visibility.Visible;
						gridLocal.Visibility = Visibility.Collapsed;
						gridInfo.Visibility = Visibility.Collapsed;

						textboxSearch.Focus();
						textboxSearch.SelectAll();
					}
					break;

				case "info":
					if (windowState != TabState.Info) {
						windowState = TabState.Info;
						buttonLocal.Selected = ImageButton.SelectedMode.False;
						buttonOnline.Selected = ImageButton.SelectedMode.False;
						buttonInfo.Selected = ImageButton.SelectedMode.True;
						gridOnline.Visibility = Visibility.Collapsed;
						gridLocal.Visibility = Visibility.Collapsed;
						gridInfo.Visibility = Visibility.Visible;

						checkUpdate();
					}
					break;

				case "update":
					Process.Start(Update.LastestUrl);
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
					gridModal.Visibility = Visibility.Collapsed;
					break;

				case "remove":
					if (selectedLocalSet != null) {
						int index = -1;
						for (int i = 0; i < stackLocalList.Children.Count; i++) {
							if (stackLocalList.Children[i] is IconItemView) {
								IconItemView view = (IconItemView)stackLocalList.Children[i];
								if (view.Id == selectedLocalSet) {
									stackLocalList.Children.RemoveAt(i);
									index = i;
									break;
								}

							}
						}
						Store.Remove(selectedLocalSet);

						if (Store.DataList.Count == 0) {
							IconLocalSet_Clicked(null);
						}
						else {
							IconLocalSet_Clicked(Store.DataList[Math.Min(Store.DataList.Count - 1, index)]);
						}
					}
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
			gridNoResult.Visibility = Visibility.Collapsed;
			buttonPrev.ViewMode = page == 1 ? ImageButton.Mode.Disable : ImageButton.Mode.Visible;
			buttonNext.ViewMode = ImageButton.Mode.Visible;
			textPage.Text = page.ToString();
			Network.getIconSet(searchText, page, ++requestId, (list, id, ci_c) => {
				if (requestId == id && list != null) {
					this.ci_c = ci_c;

					if (list.Count > 0) {
						for (int i = 0; i < list.Count; i++) {
							IconItemView view = new IconItemView();
							view.Margin = new Thickness((i % 5) * 90, (i / 5) * 120, 0, 0);
							view.setIconSetClickListener(this);
							view.setSelectable(true);
							gridResult.Children.Add(view);
							view.setIconSet(list[i]);
						}
					}
					else {
						gridNoResult.Visibility = Visibility.Visible;
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
			gridModal.Visibility = Visibility.Visible;
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
							view.Margin = new Thickness((i % 4) * 90, (i / 4) * 90, 0, 0);
							view.setIconSetClickListener(this);
							gridOnlineItemList.Children.Add(view);
							view.setUrl(list[i].path);
						}
					}
				});
			}
		}

		public void IconLocalSet_Clicked(string id) {
			scrollList.ScrollToTop();
			gridLocalItemList.Children.Clear();

			selectedLocalSet = id;
			buttonRemove.ViewMode = id == null ? ImageButton.Mode.Hidden : ImageButton.Mode.Visible;

			string title = Store.GetTitle(id);

			if (title != null) {
				textLocalTitle.Text = title;

				List<string> list = Parser.parseLocaItemSet(id);
				if (list != null) {
					for (int i = 0; i < list.Count; i++) {
						IconItemView view = new IconItemView();
						view.Margin = new Thickness((i % 4) * 90, (i / 4) * 90 + 40, 0, 0);
						view.setLocalImageClickListener(this);
						view.setSelectable(true);
						gridLocalItemList.Children.Add(view);
						view.setPath(Path.Combine(Store.OnlinePath, id, list[i]));
					}
				}
				else {
					textLocalTitle.Text = "";
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

		private void UpdateTimer_Tick(object sender, EventArgs e) {
			checkUpdate();
		}

		private void checkUpdate() {
			textNowVersion.Text = string.Format("v{0}", Version.version);

			Network.getLastest((s) => {
				string v = Parser.GetLastestVersion(s);
				if (v != null && Version.version != v) {
					buttonUpdate.ViewMode = ImageButton.Mode.Visible;
					textLastestVersion.Text = string.Format("{0} available!", v);
				}
			});
		}

		private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape) {
				this.Close();
			}
		}
	}
}
