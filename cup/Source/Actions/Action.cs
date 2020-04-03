using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace cup {
	/// <summary>
	/// Manipulates files and image data.
	/// </summary>
	public class Action {
		/// <summary>
		/// Get an Action instance from the provided action data
		/// </summary>
		/// <param name="type">Action type</param>
		/// <param name="name">Action name</param>
		/// <param name="fail">Whether to throw an exception or fall back to default action</param>
		/// <returns>An Action instance, always</returns>
		public static Action GetAction(ActionType type, string name, bool fail = false) {
			if (type == ActionType.CustomAction) {
				if (App.CustomActions.ContainsKey(name)) {
					return App.CustomActions[name];
				} else {
					App.Logger.WriteLine(LogLevel.Error, "custom action `{0}` is not registered", name);
				}
			} else if (type == ActionType.BuiltInAction) {
				try {
					return Activator.CreateInstance(Type.GetType("cup.Actions." + name)) as Action;
				} catch {
					App.Logger.WriteLine(LogLevel.Error, "activator could not create an instance of the provided built-in action: `{0}`", name);
				}
			}

			if (fail)
				throw new ArgumentException("no such action");

			App.Logger.WriteLine(LogLevel.Warning, "defaulting to Clipboard built-in action");
			return new Actions.Clipboard();
		}

		/// <summary>
		/// Process the screenshot
		/// </summary>
		/// <param name="screenshot">Screenshot bitmap</param>
		/// <returns>Always returns an ActionResult instance</returns>
		public virtual ActionResult Process(Bitmap screenshot) {
			ActionResult result = new ActionResult();
			ImageFormat format = new ImageFormat(App.Preferences.ImageFormat);
			string extension = ImageFormatExtensions.GetExtension(format);

			if (App.Preferences.KeepLocalCopy) {
				string localPath = Path.Combine(App.Preferences.LocalPath, DateTime.Now.ToString("dd-MM-yyyy hh.mm.ss")) + extension;

				try {
					screenshot.Save(localPath, format);
					result.LocalPath = localPath;
					App.Logger.WriteLine(LogLevel.Informational, "local copy of screenshot is OK");
				} catch {
					App.Logger.WriteLine(LogLevel.Warning, "could not save local copy of screenshot");
				}
			}

			if (String.IsNullOrEmpty(result.LocalPath)) {
				try {
					string temporaryPath = Path.GetTempFileName().Replace(".tmp", extension);
					screenshot.Save(temporaryPath, format);
					result.TemporaryPath = temporaryPath;
					App.Logger.WriteLine(LogLevel.Informational, "saved in temporary path");
				} catch {
					App.Logger.WriteLine(LogLevel.Error, "could not save screenshot in temporary path");
				}
			}

			return result;
		}

		/// <summary>
		/// Displays notifications about the current action
		/// </summary>
		/// <param name="result">An ActionResult instance returned by Action.Process()</param>
		/// <returns>Whether or not the notification was shown</returns>
		public virtual bool DisplayNotification(ActionResult result) {
			if (String.IsNullOrEmpty(result.LocalPath))  // file was not saved
				return false;

			try {
				if (App.Preferences.LegacyNotifications) {
					throw new Exception();  // HACK: this will fall back to legacy notifications
				}

				// show modern notification
				App.ShowNotificationEx(result.LocalPath ?? result.TemporaryPath, Path.Combine(App.AppDirectory, "Cache", "AppIcon.png"), App.LocalizationManager.GetString("FileSavedInstruction"), App.LocalizationManager.GetString("FileSavedBody"),
					result.RemoteUri?.AbsoluteUri);
			} catch {
				// show classic notification
				App.ShowNotification(ToolTipIcon.Info, App.LocalizationManager.GetString("FileSavedInstruction"), App.LocalizationManager.GetString("FileSavedBody"), 5000, () =>
					App.OpenInExplorer(result.LocalPath));
			}

			return true;
		}
	}
}
