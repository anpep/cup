using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace cup {
	public class SharexUploader {
		/// <summary>
		/// HTTP request method
		/// </summary>
		public string RequestType {
			get;
			set;
		}

		/// <summary>
		/// HTTP request URL
		/// </summary>
		public string RequestURL {
			get;
			set;
		}

		/// <summary>
		/// File parameter name
		/// </summary>
		public string FileFormName {
			get;
			set;
		}

		/// <summary>
		/// HTTP POST parameters
		/// </summary>
		public Dictionary<string, string> Arguments {
			get;
			set;
		}

		/// <summary>
		/// HTTP headers
		/// </summary>
		public Dictionary<string, string> Headers {
			get;
			set;
		}

		/// <summary>
		/// URL template
		/// </summary>
		public string URL {
			get;
			set;
		}


		/// <summary>
		/// Regular expression list
		/// </summary>
		public List<string> RegexList {
			get;
			set;
		}
	}
}
