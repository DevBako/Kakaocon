using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kakaocon.Model {
	public class IconItem {
		public string idx { get; set; }
		public string package_idx { get; set; }
		public string title { get; set; }
		public string sort { get; set; }
		public string ext { get; set; }

		private string _path;
		public string path {
			get { return _path; }
			set {
				_path = string.Format("http://dcimg1.dcinside.com/dccon.php?no={0}", value);
			}
		}
	}
}
