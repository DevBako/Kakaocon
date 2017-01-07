using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kakaocon.Model;

namespace Kakaocon.Handler {
	public interface IconSetListener {
		void IconSet_Clicked(IconSet iconSet);
	}
}
