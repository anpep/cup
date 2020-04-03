using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;

namespace cup {
	/// <summary>
	/// Provides access to localization functionalities based on the user preferences.
	/// </summary>
	public class LocalizationManager {
		private ResourceManager mResourceManager;

		/// <summary>
		/// Initializes resource manager for the specified language.
		/// If not specified or non-existant, the system language is used.
		/// If the system language is not supported, english is used by default.
		/// </summary>
		/// <param name="languageCode">The two-letter ISO 639-1 language code</param>
		public LocalizationManager(string languageCode = null) {
			App.Logger.WriteLine(LogLevel.Verbose, "initializing LocalizationManager");

			if (languageCode == null) {
				languageCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
				App.Logger.WriteLine(LogLevel.Warning, "no user-specified language - using OS default: {0}", languageCode);
			}

			languageCode = languageCode.ToLower();

			try {
				// Create resource manager for the user language
				mResourceManager = new ResourceManager("cup.Languages." + languageCode, Assembly.GetExecutingAssembly());
				mResourceManager.GetString("");
				App.Logger.WriteLine(LogLevel.Informational, "ResourceManager for locale `{0}` is OK", languageCode);
			} catch {
				// Language is not supported, use english by default
				mResourceManager = new ResourceManager("cup.Languages.en", Assembly.GetExecutingAssembly());
				App.Logger.WriteLine(LogLevel.Warning, "ResourceManager test failed (does the resource file for locale `{0}` exist?) - falling back to `en`", languageCode);
			}
		}

		/// <summary>
		/// Retrieves an string for the current user language.
		/// </summary>
		/// <returns>The string or, if it's not found, the string name.</returns>
		public string GetString(string name) {
			string @string = mResourceManager.GetString(name);

			if (@string == null) {
				App.Logger.WriteLine(LogLevel.Warning, "untranslated string `{0}`", name);
				return name;
			}

			return @string;
		}
	}
}
