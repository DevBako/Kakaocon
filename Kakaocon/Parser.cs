using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HtmlAgilityPack;
using Kakaocon.Model;

namespace Kakaocon {
	class Parser {
		public static List<IconSet> parseSearchResult(string html) {
			List<IconSet> list = new List<IconSet>();

			HtmlDocument doc = new HtmlDocument();
			doc.LoadHtml(html);

			HtmlNodeCollection nodeList = doc.DocumentNode.SelectNodes("//div[@class='sticker_list_box']//ul//li");
			if (nodeList != null) {
				foreach (HtmlNode node in nodeList) {
					try {
						string id = node.GetAttributeValue("package_idx", "");
						string url = node.SelectSingleNode(".//img").GetAttributeValue("src", "");
						string name = node.SelectSingleNode(".//*[@class='sticker1_name']").InnerText;
						string seller = node.SelectSingleNode(".//*[@class='seller']").InnerText;

						list.Add(new IconSet(id, url, name, seller));
					}
					catch {
						return new List<IconSet>();
					}
				}
			}
			return list;
		}

		public static List<IconItem> parseIconList(string html) {
			List<IconItem> list = new List<IconItem>();
			try {

				JsonTextParser parser = new JsonTextParser();
				JsonObjectCollection root = (JsonObjectCollection)parser.Parse(html);
				JsonArrayCollection detail = (JsonArrayCollection)root["detail"];

				foreach (JsonObjectCollection obj in detail) {
					IconItem iconItem = new IconItem() {
						idx = getString(obj["idx"]),
						package_idx = getString(obj["package_idx"]),
						title = getString(obj["title"]),
						sort = getString(obj["sort"]),
						ext = getString(obj["ext"]),
						path = getString(obj["path"]),
					};
					list.Add(iconItem);
				}
			}
			catch (Exception ex) {
				return new List<IconItem>();
			}
			return list;
		}

		public static string getString(JsonObject obj) {
			try {
				return obj.GetValue().ToString();
			}
			catch {
				return null;
			}
		}

		internal static List<string> parseLocaItemSet(string id) {
			JsonTextParser parser = new JsonTextParser();
			JsonObjectCollection root = null;

			List<string> list = new List<string>();

			try {
				using (StreamReader sr = new StreamReader(Path.Combine(Store.OnlinePath, id, Store.DataFileName))) {
					root = (JsonObjectCollection)parser.Parse(sr.ReadToEnd());
				}

				foreach (JsonObjectCollection obj in (JsonArrayCollection) root["list"]) {
					list.Add(getString(obj["name"]));
				}
			}
			catch (Exception ex) {
				return null;
			}
			return list;
		}

		public static String GetLastestVersion(string html) {
			if (html == null) { return null; }

			try {
				HtmlDocument doc = new HtmlDocument();
				doc.LoadHtml(html);

				HtmlNodeCollection nodeList = doc.DocumentNode.SelectNodes("//h1[@class='release-title']");

				for (int i = 0; i < nodeList.Count; i++) {
					HtmlNode node = nodeList[i];
					return node.InnerText.Trim();
				}
			}
			catch { }

			return null;
		}
	}
}
