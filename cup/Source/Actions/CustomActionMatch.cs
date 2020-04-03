using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cup {
	public class CustomActionMatch {
		/// <summary>
		/// Match type
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public CustomActionMatchType Type {
			get;
			set;
		}

		/// <summary>
		/// Template string
		/// </summary>
		public string Template {
			get;
			set;
		}

		/// <summary>
		/// Regex for extracting the URL from the response body (Regex-only)
		/// </summary>
		public List<string> Matches {
			get;
			set;
		}

		/// <summary>
		/// Preconditions for the response to be considered successful (optional)
		/// </summary>
		public List<CustomActionCondition> Conditions {
			get;
			set;
		}
	}
}
