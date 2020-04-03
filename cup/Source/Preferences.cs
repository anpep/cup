using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;

namespace cup {
	public class Preferences {
		/// <summary>
		/// Action-HotKey map
		/// </summary>
		public Dictionary<string, string> HotKeys {
			get;
			set;
		} = new Dictionary<string, string>();

		/// <summary>
		/// Image format for screenshots
		/// </summary>
		[JsonConverter(typeof(GuidConverter))]
		public Guid ImageFormat {
			get;
			set;
		} = System.Drawing.Imaging.ImageFormat.Png.Guid;

		/// <summary>
		/// Action type (custom/built-in)
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public ActionType ActionType {
			get;
			set;
		} = ActionType.BuiltInAction;

		/// <summary>
		/// Action name
		/// </summary>
		public string ActionName {
			get;
			set;
		} = "Clipboard";

		/// <summary>
		/// Keep a copy of each screenshot in the local path
		/// </summary>
		public bool KeepLocalCopy {
			get;
			set;
		} = true;

		/// <summary>
		/// Local path for saving screenshots
		/// </summary>
		public string LocalPath {
			get;
			set;
		} = "";

		/// <summary>
		/// Whether or not to display legacy notifications
    /// TODO: Fix notifications on Windows 10 by registering a custom shortcut on the start menu
		/// </summary>
		public bool LegacyNotifications {
			get;
			set;
		} = true;

		/// <summary>
		/// Loads preferences
		/// </summary>
		/// <returns>The Preferences object</returns>
		public static Preferences Load() {
			try {
				Preferences defaults = new Preferences();
				Preferences prefs = JsonConvert.DeserializeObject<Preferences>(File.ReadAllText(Path.Combine(App.AppDirectory, "Preferences.json")));
				App.Logger.WriteLine(LogLevel.Verbose, "validating preferences");

				// validate ActionName
				if (prefs.ActionType == ActionType.BuiltInAction) {
					if ((from type in Assembly.GetExecutingAssembly().GetTypes()
							 where type.Namespace == "cup.Actions" && type.IsSubclassOf(typeof(Action)) && type.Name == prefs.ActionName
							 select type).Count() == 0) {
						App.Logger.WriteLine(LogLevel.Warning, "detected invalid value for `ActionName` - falling back to default");
						prefs.ActionName = "Clipboard";
					}
				} else if (prefs.ActionType == ActionType.CustomAction) {
					if (!App.CustomActions.ContainsKey(prefs.ActionName)) {
						App.Logger.WriteLine(LogLevel.Warning, "detected invalid value for `ActionName` - falling back to default");
						prefs.ActionType = ActionType.BuiltInAction;
						prefs.ActionName = "Clipboard";
					}
				}

				// validate image format
				try {
					new ImageFormat(prefs.ImageFormat);
				} catch {
					App.Logger.WriteLine(LogLevel.Warning, "detected invalid value for `ImageFormat` - falling back to default");
					prefs.ImageFormat = defaults.ImageFormat;
				}

				// local path validation is sort of handled by App.RecreateLocalDirectory()
				// try to save any possible changes
				prefs.Save();

				// return preferenecs
				return prefs;
			} catch {
				App.Logger.WriteLine(LogLevel.Warning, "could not load/deserialize user preferences - falling back to defaults");
				return new Preferences();
			}
		}

		/// <summary>
		/// Saves preferences
		/// </summary>
		/// <returns>Whether or not the operation completed successfully</returns>
		public bool Save() {
			try {
				App.Logger.WriteLine(LogLevel.Verbose, "saving preferences");
				File.WriteAllText(Path.Combine(App.AppDirectory, "Preferences.json"), JsonConvert.SerializeObject(this));
				App.Logger.WriteLine(LogLevel.Informational, "operation completed successfully");
				return true;
			} catch {
				App.Logger.WriteLine(LogLevel.Error, "could not save preferences");
				return false;
			}
		}
	}
}
