using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Shell;

namespace Kakaocon {
	/// <summary>
	/// App.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class App : Application, ISingleInstanceApp {
		public App() {
			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);
		}

		[STAThread]
		public static void Main() {
			if (SingleInstance<App>.InitializeAsFirstInstance("Kakaocon")) {
				App application = new App();

				application.Init();
				application.Run();

				// Allow single instance code to perform cleanup operations
				SingleInstance<App>.Cleanup();
			}
		}

		static void MyHandler(object sender, UnhandledExceptionEventArgs args) {
			if (!Directory.Exists(Store.RootPath)) { Directory.CreateDirectory(Store.RootPath); }
			using (StreamWriter sw = new StreamWriter(Path.Combine(Store.RootPath, Store.LogFileName), true)) {
				try {
					Exception ex = (Exception)args.ExceptionObject;
					sw.WriteLine(DateTime.Now);
					sw.WriteLine(ex.Message);
					sw.WriteLine(ex.StackTrace);
					sw.WriteLine();
				}
				finally {

				}
			}
		}

		public void Init() {
			this.InitializeComponent();
		}
		
		public bool SignalExternalCommandLineArgs(IList<string> args) {
			return false;
		}
	}
}
