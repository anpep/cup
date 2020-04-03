using System.Drawing;

namespace cup.Actions {
	public class Clipboard : Action {
		/// <summary>
		/// Process the screenshot
		/// </summary>
		/// <param name="screenshot">Screenshot bitmap</param>
		/// <returns>Always returns an ActionResult instance</returns>
		public override ActionResult Process(Bitmap screenshot) {
			System.Windows.Forms.Clipboard.SetImage(screenshot);
			return base.Process(screenshot);
		}
	}
}
