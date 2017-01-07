using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Kakaocon.Model;

namespace Kakaocon {
	class Network {
		public static void getIconSet(string text, int page, int requestId, Action<List<IconSet>, int, string> handler) {
			CookieAwareWebClient webClient = Utils.CreateHtmlWebClient();
			webClient.DownloadStringCompleted += (sender, e) => {
				if (handler != null) {
					if (e.Cancelled || e.Error != null) {
						handler(new List<IconSet>(), requestId, null);
					}
					else {
						handler(Parser.parseSearchResult(e.Result), requestId, Utils.GetCiC((sender as CookieAwareWebClient).Cookies));
					}
				}
			};
			webClient.DownloadStringAsync(new Uri(String.Format("http://dccon.dcinside.com/hot/{0}/all/{1}", page, text)));
		}

		public static void getIconList(string id, string ci_c, int requestId, Action<List<IconItem>, string, int> handler) {
			WebClient webClient = Utils.CreateListWebClient();
			webClient.Headers.Add(HttpRequestHeader.Cookie, "ci_c=" + ci_c);
			string post = string.Format("ci_t={0}&package_idx={1}&code=", ci_c, id);
			webClient.UploadStringCompleted += (sender, e) => {
				if (handler != null) {
					if (e.Cancelled || e.Error != null) {
						handler(new List<IconItem>(), id, requestId);
					}
					else {
						handler(Parser.parseIconList(e.Result), id, requestId);
					}
				}
			};
			webClient.UploadStringAsync(new Uri("http://dccon.dcinside.com/index/package_detail"), post);
		}
	}
}
