using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kakaocon.Model {
	public class IconSet {
		public string Id { get; }
		public string Url { get; }
		public string Title { get; }
		public string Seller { get; }

		public IconSet(String id, String url, String title, String seller) {
			this.Id = id;
			this.Url = url;
			this.Title = title;
			this.Seller = seller;
		}
	}
}
