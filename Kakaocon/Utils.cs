using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Json;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Kakaocon.Model;

namespace Kakaocon {
	class Utils {
		const int GCL_HMODULE = -16;
		const int GWL_EXSTYLE = -20;
		const long WS_VISIBLE = 0x10000000L;
		const long WS_MINIMIZE = 0x20000000L;
		const long WS_EX_ACCEPTFILES = 0x00000010L; // accept drag drop
		const long WS_EX_TOPMOST = 0x00000008L;
		const int WS_EX_NOACTIVATE = 0x08000000;
		const int WS_EX_TOOLWINDOW = 0x00000080;
		const uint WM_DROPFILES = 0x233;

		struct DropFiles {
			public uint pFiles;
			public int x;
			public int y;
			[MarshalAs(UnmanagedType.Bool)]
			public bool fNC;
			[MarshalAs(UnmanagedType.Bool)]
			public bool fWide;
		}

		[DllImport("user32.dll")]
		public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern IntPtr GetForegroundWindow();

		[SuppressUnmanagedCodeSecurity, DllImport("user32")]
		static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		public static bool IsWindowAvailable(IntPtr handle) {
			if (handle == IntPtr.Zero) {
				return false;
			}

			UInt32 style = (UInt32)GetWindowLong(handle, GCL_HMODULE);
			UInt32 exStyle = (UInt32)GetWindowLong(handle, GWL_EXSTYLE);

			return (style & WS_VISIBLE) == WS_VISIBLE && (style & WS_MINIMIZE) == 0 && (exStyle & WS_EX_ACCEPTFILES) == WS_EX_ACCEPTFILES;
		}

		public static void SendFile(IntPtr handle, String path) {
			if (IsWindowAvailable(handle)) {
				byte[] name = Encoding.Unicode.GetBytes((path + "\0\0").ToCharArray());

				var dropFileSize = Marshal.SizeOf(typeof(DropFiles));
				IntPtr alloc = Marshal.AllocHGlobal(dropFileSize + name.Length);

				DropFiles dropFiles = new DropFiles {
					pFiles = (uint)dropFileSize,
					x = 0,
					y = 0,
					fNC = false,
					fWide = true,
				};

				Marshal.StructureToPtr(dropFiles, alloc, true);
				Marshal.Copy(name, 0, new IntPtr(alloc.ToInt32() + dropFiles.pFiles), name.Length);

				PostMessage(handle, WM_DROPFILES, alloc, IntPtr.Zero);
			}
		}

		public static WebClient CreateImageWebClient() {
			WebClient webClient = new WebClient();
			webClient.Headers.Add("Accept", "image/webp,image/*,*/*;q=0.8");
			//webClient.Headers.Add("Accept-Encoding", "gzip, deflate, sdch");
			//webClient.Headers.Add("Accept-Language", "ko,en-US;q=0.8,en;q=0.6,ja;q=0.4");
			webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");
			webClient.Headers.Add("Referer", "http://dccon.dcinside.com/");
			//webClient.Headers.Add("Host", "dcimg1.dcinside.com");

			return webClient;
		}

		public static CookieAwareWebClient CreateHtmlWebClient() {
			CookieAwareWebClient webClient = new CookieAwareWebClient();
			webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");
			webClient.Headers.Add("Referer", "http://dccon.dcinside.com/");
			webClient.Headers.Add("Host", "dccon.dcinside.com");
			webClient.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
			webClient.Headers.Add("Accept-Language", "ko,en-US;q=0.8,en;q=0.6,ja;q=0.4");
			webClient.Encoding = Encoding.UTF8;

			return webClient;
		}

		public static WebClient CreateListWebClient() {
			WebClient webClient = new WebClient();
			webClient.Headers.Add("X-Requested-With", "XMLHttpRequest");
			webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36");
			webClient.Headers.Add("Host", "dccon.dcinside.com");
			webClient.Headers.Add("Accept", "Accept:*/*");
			webClient.Headers.Add("Accept-Language", "ko,en-US;q=0.8,en;q=0.6,ja;q=0.4");
			webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
			webClient.Encoding = Encoding.UTF8;

			return webClient;
		}

		public static string GetCiC(string cookie) {
			string[] cookies = cookie.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
			foreach (string cookieSet in cookies) {
				if (cookieSet.Trim().StartsWith("domain")) {
					string[] kvps = cookieSet.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
					foreach (string kvp in kvps) {
						if (kvp.Trim().StartsWith("ci_c")) {
							string[] data = kvp.Trim().Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
							if (data.Length >= 2) {
								return data[1];
							}
						}
					}
				}
			}
			return null;
		}

		private static Random random = new Random();
		private static string randomString(int length) {
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
		}

		private static bool downloadFile(string url, string path) {
			WebClient webClient = Utils.CreateImageWebClient();
			try {
				webClient.DownloadFile(url, path);
				if (File.Exists(path)) {
					return true;
				}
			}
			catch (Exception ex) {
			}
			return false;
		}

		public static void DownloadFiles(IconSet iconSet, List<IconItem> list, Action<int> progress, Action<bool, string> result) {
			if (iconSet == null || list == null) {
				if (result != null) {
					result(false, null);
				}
				return;
			}

			string id = iconSet.Id;

			if (!Directory.Exists(Store.TempPath)) { Directory.CreateDirectory(Store.TempPath); }
			if (!Directory.Exists(Store.OnlinePath)) { Directory.CreateDirectory(Store.OnlinePath); }
			if (!Directory.Exists(Store.OnlinePath + id)) { Directory.CreateDirectory(Store.OnlinePath + id); }

			BackgroundWorker worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.DoWork += (s, e) => {
				string outputPath = Store.OnlinePath + id + "\\" + Store.TitleImageFileName;
				if (!File.Exists(outputPath)) {
					string downloadPath = Store.TempPath + randomString(30);
					if (downloadFile(iconSet.Url, downloadPath)) {
						File.Move(downloadPath, outputPath);
					}
					else {
						e.Result = false;
						return;
					}
				}

				for (int i = 0; i < list.Count; i++) {
					IconItem item = list[i];
					outputPath = Store.OnlinePath + id + "\\" + item.idx + "." + item.ext;
					if (!File.Exists(outputPath)) {
						string downloadPath = Store.TempPath + randomString(30);
						if (downloadFile(item.path, downloadPath)) {
							File.Move(downloadPath, outputPath);
						}
						else {
							e.Result = false;
							return;
						}
					}
					(s as BackgroundWorker).ReportProgress(Math.Min(100, i + 1));
				}

				JsonObjectCollection root = new JsonObjectCollection();
				root.Add(new JsonStringValue("title", iconSet.Title));
				JsonArrayCollection detail = new JsonArrayCollection("list");

				foreach (IconItem item in list) {
					JsonObjectCollection collect = new JsonObjectCollection();
					collect.Add(new JsonStringValue("name", item.idx + "." + item.ext));
					collect.Add(new JsonStringValue("sort", item.sort));
					collect.Add(new JsonStringValue("ext", item.ext));
					detail.Add(collect);
				}
				root.Add(detail);
				using (StreamWriter sw = new StreamWriter(Store.OnlinePath + id + "\\" + Store.DataFileName, false)) {
					sw.Write(root);
				}
				e.Result = true;
			};
			worker.ProgressChanged += (s, e) => {
				if (progress != null) {
					progress(e.ProgressPercentage);
				}
			};
			worker.RunWorkerCompleted += (s, e) => {
				try {
					bool flag = (bool)e.Result;
					if (flag) {
						if (result != null) {
							result(true, id);
							return;
						}
					}
				}
				catch (Exception ex) {
					/*
					while (ex != null) {
						MessageBox.Show(ex.Message);
						ex = ex.InnerException;
					}
					*/
				}
				if (result != null) {
					result(false, null);
				}
			};
			worker.RunWorkerAsync();
		}

		private static BitmapSource ReplaceTransparency(BitmapSource bitmap, System.Windows.Media.Color color) {
			var rect = new Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);
			var visual = new DrawingVisual();
			var context = visual.RenderOpen();
			context.DrawRectangle(new SolidColorBrush(color), null, rect);
			context.DrawImage(bitmap, rect);
			context.Close();

			var render = new RenderTargetBitmap(bitmap.PixelWidth, bitmap.PixelHeight,
				96, 96, PixelFormats.Pbgra32);
			render.Render(visual);
			return render;
		}

		public static string ResizeImageTemporary(string inputPath, int width, int height) {
			if (!File.Exists(inputPath)) {
				return null;
			}
			string ext = Path.GetExtension(inputPath);

			if (ext == ".gif") {
				string outputPath = Store.TempPath + randomString(30) + ".gif";
				File.Copy(inputPath, outputPath);
				return outputPath;
			}
			else {
				var bitmap = new BitmapImage();

				using (var stream = new FileStream(inputPath, FileMode.Open)) {
					bitmap.BeginInit();
					bitmap.DecodePixelWidth = width;
					bitmap.DecodePixelHeight = height;
					bitmap.CacheOption = BitmapCacheOption.OnLoad;
					bitmap.StreamSource = stream;
					bitmap.EndInit();
				}

				BitmapSource bitmapSource = ReplaceTransparency(bitmap, Colors.White);
				BitmapEncoder encoder = new JpegBitmapEncoder() { QualityLevel = 100 };
				encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

				string outputPath = Store.TempPath + randomString(30) + ".jpg";
				using (var stream = new FileStream(outputPath, FileMode.Create)) {
					encoder.Save(stream);
				}
				return outputPath;
			}
		}
	}
}
