using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace Kakaocon {
	/// <summary>
	/// Launcher.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class Launcher : Window {
		public Launcher(MainWindow mainWindow) {
			InitializeComponent();
			this.mainWindow = mainWindow;
		}
		const int GWL_EXSTYLE = -20;
		const int WS_EX_NOACTIVATE = 0x08000000;

		[DllImport("user32.dll")]
		public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
		
		[DllImport("user32.dll")]
		public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
	

		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
		[StructLayout(LayoutKind.Sequential)]
		private struct RECT {
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		struct DropFiles {
			public uint pFiles;
			public int x;
			public int y;
			[MarshalAs(UnmanagedType.Bool)]
			public bool fNC;
			[MarshalAs(UnmanagedType.Bool)]
			public bool fWide;
		}

		delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

		[DllImport("user32.dll")]
		static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);


		MainWindow mainWindow = null;
		int savedLeft = 0, savedBottom = 0, savedRight = 0;

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
			source.AddHook(new HwndSourceHook(WndProc));

			DispatcherTimer mainTimer = new DispatcherTimer();
			mainTimer.Interval = TimeSpan.FromMilliseconds(300);
			mainTimer.Tick += MainTimer_Tick;
			mainTimer.Start();

			DispatcherTimer positionTimer = new DispatcherTimer();
			positionTimer.Interval = TimeSpan.FromMilliseconds(20);
			positionTimer.Tick += PositionTimer_Tick;
			positionTimer.Start();
		}

		private IntPtr getFocusedWindow(int processId) {
			List<IntPtr> handles = new List<IntPtr>();

			foreach (ProcessThread thread in Process.GetProcessById(processId).Threads) {
				EnumThreadWindows(thread.Id, (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);
			}

			IntPtr foreground = Utils.GetForegroundWindow();
			foreach (IntPtr intPtr in handles) {
				if (intPtr == foreground) {
					return intPtr;
				}
			}
			return IntPtr.Zero;
		}

		private IntPtr kakaoHandle;

		private void MainTimer_Tick(object sender, EventArgs e) {
			Process[] processes = Process.GetProcessesByName("kakaotalk");

			IntPtr newHandle = IntPtr.Zero;

			foreach (Process p in processes) {
				newHandle = p.MainWindowHandle;

				IntPtr intPtr = getFocusedWindow(p.Id);
				if (Utils.IsWindowAvailable(intPtr) && mainWindow != null && !mainWindow.isShowing()) {
					kakaoHandle = intPtr;
					PositionTimer_Tick(null, null);
					this.Opacity = 1;
					this.Show();
				}
				else {
					kakaoHandle = IntPtr.Zero;
					this.Hide();
				}

				break;
			}
		}

		private void PositionTimer_Tick(object sender, EventArgs e) {
			if (kakaoHandle != IntPtr.Zero) {
				RECT rect = new RECT();
				GetWindowRect(kakaoHandle, ref rect);

				if (Utils.isTowardsLeft(rect.Left, rect.Right)) {
					this.Left = rect.Right;
				}
				else {
					this.Left = rect.Left - this.Width;
				}

				this.Top = rect.Bottom - this.Height;

				this.savedLeft = rect.Left;
				this.savedRight = rect.Right;
				this.savedBottom = rect.Bottom;
			}
		}

		protected override void OnActivated(EventArgs e) {
			base.OnActivated(e);

			//Set the window style to noactivate.
			WindowInteropHelper helper = new WindowInteropHelper(this);
			SetWindowLong(helper.Handle, GWL_EXSTYLE, GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
			const int WM_MOUSEACTIVATE = 0x0021;
			const int MA_NOACTIVATE = 3;

			if (msg == WM_MOUSEACTIVATE) {
				handled = true;
				return new IntPtr(MA_NOACTIVATE);
			}

			return IntPtr.Zero;
		}

		private bool closeFlag = false;
		public void CloseForce() {
			closeFlag = true;
			this.Close();
		}

		private void Window_Closing(object sender, CancelEventArgs e) {
			if (!closeFlag) {
				e.Cancel = true;
			}
		}

		private void buttonShow_Response(object sender, CustomButtonEventArgs e) {
			if (mainWindow != null) {
				mainWindow.Launcher_Clicked(kakaoHandle, savedLeft, savedRight, savedBottom);
			}
		}
	}
}
