using Newtonsoft.Json.Linq;

namespace cup {
	public class CustomActionCondition {
		/// <summary>
		/// Expected value (JSON and XML-only)
		/// </summary>
		public JToken Expect {
			get;
			set;
		}

		/// <summary>
		/// JSON path/Xpath (JSON and XML-only)
		/// </summary>
		public string For {
			get;
			set;
		}

		/// <summary>
		/// Regex match (Regex-only)
		/// </summary>
		public string Match {
			get;
			set;
		}

		/// <summary>
		/// Error title if condition is not met (optional)
		/// </summary>
		public string ErrorTitle {
			get;
			set;
		}

		/// <summary>
		/// Error description if condition is not met (optional)
		/// </summary>
		public string ErrorDescription {
			get;
			set;
		}
	}
}
