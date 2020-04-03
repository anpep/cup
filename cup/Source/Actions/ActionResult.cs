using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cup {
	/// <summary>
	/// ActionResult is returned by Action.Perform() and contains screenshot metadata
	/// </summary>
	public class ActionResult {
		/// <summary>
		/// Local path for the screenshot (may be null)
		/// </summary>
		public string LocalPath {
			get;
			set;
		}

		/// <summary>
		/// Temporary path for the screenshot (may be null)
		/// </summary>
		public string TemporaryPath {
			get;
			set;
		}

		/// <summary>
		/// Remote URI for the screenshot (may be null)
		/// </summary>
		public Uri RemoteUri {
			get;
			set;
		}

		/// <summary>
		/// Screenshot bitmap
		/// </summary>
		public Bitmap Image {
			get;
			set;
		}
	}
}
