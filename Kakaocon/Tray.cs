using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Kakaocon {
	public partial class MainWindow : Window {
		public NotifyIcon tray = new NotifyIcon();
		ContextMenuStrip menu = new ContextMenuStrip();

		private void InitTray() {
			tray.Visible = true;

			tray.Icon = System.Drawing.Icon.FromHandle(Kakaocon.Properties.Resources.update.Handle);
			tray.Text = "Kakaocon";
			
			ToolStripMenuItem exit = new ToolStripMenuItem("Exit");
			exit.Click += (s, e) => { closeForce(); };
		
			menu.Items.Add(exit);
			tray.ContextMenuStrip = menu;
		}
	}
}
