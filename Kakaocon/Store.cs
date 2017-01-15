using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Kakaocon {
	class Store {
		public static string RootPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Shimika\Kakaocon\";
		public static string TempPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Shimika\Kakaocon\temp\";
		public static string OnlinePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Shimika\Kakaocon\online\";
		public static string MasterFileName = "master.json";
		public static string TitleImageFileName = "title.png";
		public static string DataFileName = "data.json";
		public static string LogFileName = "error.log";

		public static List<string> DataList = new List<string>();

		public static void Load() {
			if (!Directory.Exists(RootPath)) { Directory.CreateDirectory(RootPath); }

			try {
				JsonTextParser parser = new JsonTextParser();
				JsonArrayCollection root = null;

				using (StreamReader sr = new StreamReader(RootPath + MasterFileName)) {
					root = (JsonArrayCollection)parser.Parse(sr.ReadToEnd());
				}

				foreach (JsonObjectCollection obj in root) {
					string id = Parser.getString(obj["id"]);
					if (GetTitle(id) != null) {
						DataList.Add(id);
					}
				}
			}
			catch {
				DataList.Clear();
				migrate();
			}
		}

		private static void migrate() {
			if (!Directory.Exists(RootPath)) { Directory.CreateDirectory(RootPath); }
			if (!Directory.Exists(OnlinePath)) { Directory.CreateDirectory(OnlinePath); }

			foreach (string path in Directory.GetDirectories(OnlinePath)) {
				string id = Path.GetFileNameWithoutExtension(path);
				if (GetTitle(id) != null) {
					DataList.Add(id);
				}
			}
			save();
		}

		public static string GetTitle(string id) {
			if (id != null) {
				string titleImagePath = Path.Combine(OnlinePath, id, TitleImageFileName);
				string dataFilePath = Path.Combine(OnlinePath, id, DataFileName);

				if (File.Exists(titleImagePath) && File.Exists(dataFilePath)) {
					JsonTextParser parser = new JsonTextParser();
					JsonObjectCollection root = null;

					using (StreamReader sr = new StreamReader(dataFilePath)) {

						try {
							root = (JsonObjectCollection)parser.Parse(sr.ReadToEnd());
							return Parser.getString(root["title"]);
						}
						catch (Exception ex) { }
					}
				}
			}
			return null;
		}

		public static bool add(string id) {
			if (GetTitle(id) != null) {
				DataList.Add(id);
				save();

				return true;
			}
			return false;
		}

		public static void save() {
			if (!Directory.Exists(RootPath)) { Directory.CreateDirectory(RootPath); }

			JsonArrayCollection list = new JsonArrayCollection();

			foreach (string id in DataList) {
				JsonObjectCollection collect = new JsonObjectCollection();
				collect.Add(new JsonStringValue("id", id));
				list.Add(collect);
			}
			using (StreamWriter sw = new StreamWriter(Path.Combine(RootPath, MasterFileName), false)) {
				sw.Write(list);
			}
		}

		public static bool dataExists(string id) {
			return DataList.Exists(x => x == id);
		}

		public static void Remove(string id) {
			DataList.Remove(id);
			save();

			if (Directory.Exists(Path.Combine(OnlinePath, id))) {
				foreach(string path in Directory.GetFiles(Path.Combine(OnlinePath, id))) {
					try {
						File.Delete(path);
					}
					catch { }
				}
				try {
					Directory.Delete(Path.Combine(OnlinePath, id));
				}
				catch { }
			}
		}

		public static void CleanUpTemp() {
			if (Directory.Exists(TempPath)) {
				foreach (string path in Directory.GetFiles(TempPath)) {
					if (File.Exists(path)) {
						try {
							File.Delete(path);
						}
						catch { }
					}
				}
			}
		}
	}
}
