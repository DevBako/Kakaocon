using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kakaocon.Handler {
	public interface LocalImageListener {
		void IconSet_Clicked(string id);
		void LocalImage_Clicked(string path);
	}
}
