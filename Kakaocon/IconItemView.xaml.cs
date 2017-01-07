using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using System.Windows.Shapes;
using Kakaocon.Handler;
using Kakaocon.Model;
using WpfAnimatedGif;

namespace Kakaocon {
	/// <summary>
	/// IconItemView.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class IconItemView : UserControl {
		public IconItemView() {
			InitializeComponent();
		}

		IconSetListener iconSetListener;
		LocalImageListener localImageListener;

		IconSet iconSet;
		string id;
		string path;

		public void setUrl(string url) {
			WebClient webClient = Utils.CreateImageWebClient();
			webClient.DownloadDataCompleted += (s, e) => {
				if (e.Cancelled || e.Error != null) {
					loading.Visibility = Visibility.Collapsed;
					image.Visibility = Visibility.Collapsed;
					failed.Visibility = Visibility.Visible;
					return;
				}
				Byte[] MyData = e.Result;

				BitmapImage bitmap = new BitmapImage();
				bitmap.BeginInit();
				bitmap.StreamSource = new MemoryStream(MyData);
				bitmap.EndInit();

				ImageBehavior.SetAnimatedSource(image, bitmap);
			};
			webClient.DownloadDataAsync(new Uri(url));
		}

		public void setPath(string path) {
			this.path = path;
			loading.Visibility = Visibility.Collapsed;

			try {
				BitmapImage bitmap = new BitmapImage();
				bitmap.BeginInit();
				bitmap.UriSource = new Uri(path);
				bitmap.CacheOption = BitmapCacheOption.OnLoad;
				bitmap.EndInit();
				ImageBehavior.SetAnimatedSource(image, bitmap);
			}
			catch {
				image.Visibility = Visibility.Collapsed;
				failed.Visibility = Visibility.Visible;
			}
		}

		public void setIconSet(string id, string path) {
			this.id = id;
			setPath(path);
		}

		public void setIconSet(IconSet iconSet) {
			this.iconSet = iconSet;

			grid.Width = grid.Height = 70;
			image.Margin = new Thickness(5);
			info.Visibility = Visibility.Visible;

			title.Text = iconSet.Title;
			seller.Text = iconSet.Seller;
			ToolTip = iconSet.Title;

			setUrl(iconSet.Url);
		}

		public void setIconSetClickListener(IconSetListener listener) {
			this.iconSetListener = listener;
		}

		public void setLocalImageClickListener(LocalImageListener listener) {
			this.localImageListener = listener;
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			if (iconSet != null && iconSetListener != null) {
				iconSetListener.IconSet_Clicked(iconSet);
			}
			if (id != null && localImageListener != null) {
				localImageListener.IconSet_Clicked(id);
			}
			else if(path != null && localImageListener != null) {
				localImageListener.LocalImage_Clicked(path);
			}
		}
	}
}
