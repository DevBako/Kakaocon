using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Kakaocon {
	public class CookieAwareWebClient : WebClient {
		private WebRequest _request = null;

		/// Initializes a new instance of the BetterWebClient class.  <pa...
		public CookieAwareWebClient(CookieContainer cookies = null, bool autoRedirect = true) {
			CookieContainer = cookies ?? new CookieContainer();
			AutoRedirect = autoRedirect;
		}

		/// Gets or sets a value indicating whether to automatically redi...
		public bool AutoRedirect { get; set; }

		/// Gets or sets the cookie container. This contains all the cook...
		public CookieContainer CookieContainer { get; set; }

		/// Gets the cookies header (Set-Cookie) of the last request.
		public string Cookies {
			get { return GetHeaderValue("Set-Cookie"); }
		}

		/// Gets the location header for the last request.
		public string Location {
			get { return GetHeaderValue("Location"); }
		}

		/// Gets the status code. When no request is present, <see cref="...
		public HttpStatusCode StatusCode {
			get {
				var result = HttpStatusCode.Gone;

				if (_request != null) {
					var response = base.GetWebResponse(_request) as HttpWebResponse;

					if (response != null) {
						result = response.StatusCode;
					}
				}

				return result;
			}
		}

		/// Gets or sets the setup that is called before the request is d...
		public Action<HttpWebRequest> Setup { get; set; }

		/// Gets the header value.
		public string GetHeaderValue(string headerName) {
			if (_request != null) {
				return base.GetWebResponse(_request)?.Headers?[headerName];
			}

			return null;
		}

		/// Returns a <see cref="T:System.Net.WebRequest" /> object for t...
		protected override WebRequest GetWebRequest(Uri address) {
			_request = base.GetWebRequest(address);

			var httpRequest = _request as HttpWebRequest;

			if (_request != null) {
				httpRequest.AllowAutoRedirect = AutoRedirect;
				httpRequest.CookieContainer = CookieContainer;
				httpRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

				Setup?.Invoke(httpRequest);
			}

			return _request;
		}
	}
}
