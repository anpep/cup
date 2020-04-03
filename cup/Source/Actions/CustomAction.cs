using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace cup {
	public class CustomAction : Action {
		/// <summary>
		/// HTTP request method
		/// </summary>
		public string Method {
			get;
			set;
		}

		/// <summary>
		/// HTTP request URL
		/// </summary>
		public string URL {
			get;
			set;
		}

		/// <summary>
		/// Custom icon URL (optional)
		/// </summary>
		public string IconURL {
			get;
			set;
		}

		/// <summary>
		/// Action ID (ignored, set by App.DeserializeCustomAction)
		/// </summary>
		public string ActionID {
			get;
			set;
		}

		/// <summary>
		/// Custom HTTP headers
		/// </summary>
		public Dictionary<string, string> Headers {
			get;
			set;
		}

		/// <summary>
		/// Custom HTTP POST parameters
		/// </summary>
		public Dictionary<string, string> Params {
			get;
			set;
		}

		/// <summary>
		/// File parameter name
		/// </summary>
		public string Name {
			get;
			set;
		}

		/// <summary>
		/// Matching rules for server response (optional)
		/// </summary>
		public CustomActionMatch Match {
			get;
			set;
		}

		/// <summary>
		/// Maximum file size, in bytes (optional)
		/// </summary>
		public long MaximumFileSize {
			get;
			set;
		}

		/// <summary>
		/// Apply template for JSON result
		/// </summary>
		/// <param name="obj">JSON object</param>
		/// <returns>The URL</returns>
		private Uri ApplyTemplate(JObject obj) {
			if (String.IsNullOrEmpty(Match.Template))
				return null;

			return new Uri(new Regex(@"\{(.+)\}").Replace(Match.Template, new MatchEvaluator((Match match) => {
				return obj.SelectToken(match.Groups[1].Value).Value<string>();
			})));
		}

		/// <summary>
		/// Apply template for XML result
		/// </summary>
		/// <param name="doc">XML document</param>
		/// <returns>The URL</returns>
		private Uri ApplyTemplate(XmlDocument doc) {
			if (String.IsNullOrEmpty(Match.Template))
				return null;

			return new Uri(new Regex(@"\{(.+)\}").Replace(Match.Template, new MatchEvaluator((Match match) => {
				return doc.SelectNodes(match.Groups[1].Value)[0].Value;
			})));
		}

		/// <summary>
		/// Apply template for regular expression match
		/// </summary>
		/// <param name="res">Response text</param>
		/// <returns>The URL</returns>
		private Uri ApplyTemplate(string res) {
			if (String.IsNullOrEmpty(Match.Template))
				return null;

			Dictionary<int, MatchCollection> matches = new Dictionary<int, MatchCollection>();

			return new Uri(new Regex(@"\{([0-9]+)?[^a-zA-Z0-9_]+([a-zA-Z0-9_]+)\}").Replace(Match.Template, new MatchEvaluator((Match match) => {
				int matchIndex = 0;
				if (match.Groups[0].Value.Length > 0) {
					// optional match index supplied
					matchIndex = Int32.Parse(match.Groups[0].Value);
				}

				// add this match to the collection
				if (!matches.ContainsKey(matchIndex)) {
					matches[matchIndex] = new Regex(Match.Matches[matchIndex], RegexOptions.Compiled | RegexOptions.ECMAScript).Matches(res);
				}

				if (Int32.TryParse(match.Groups[1].Value, out int groupNumber)) {
					// group number specified
					return matches[matchIndex][0].Groups[groupNumber].Value;
				} else {
					// group name specified
					return matches[matchIndex][0].Groups[match.Groups[1].Value].Value;
				}
			})));
		}



		/// <summary>
		/// Uploads and process the screenshot
		/// </summary>
		/// <param name="screenshot">The screenshot bitmap</param>
		/// <returns>Always returns an ActionResult</returns>
		public override ActionResult Process(Bitmap screenshot) {
			// perform basic operations on the screenshot
			ActionResult result = base.Process(screenshot);

			// hold my beer
			App.ShowIconSpinner();

			try {
				// upload our shit
				string response = Upload(result.LocalPath ?? result.TemporaryPath);

				if (Match == null) {
					// no matching rules are provided by this custom action - assume the plaintext of the response contains the URI
					result.RemoteUri = new Uri(response);
				} else if (Match.Type == CustomActionMatchType.JSON) {
					// this action provides JSON matching rules - deserialize the response
					JObject obj = JsonConvert.DeserializeObject<JObject>(response);

					if (Match.Conditions != null) {
						// also it provides some preconditions that must be met before taking the URL
						foreach (CustomActionCondition condition in Match.Conditions) {
							if (condition.For == null) {
								App.Logger.WriteLine(LogLevel.Warning, "condition validation unsuccessful: invalid value for `For` was supplied - ignoring");
								continue;
							}

							if (!obj.SelectToken(condition.For).Equals(condition.Expect)) {
								// unmet condition - deal with it
								throw new Exception() {
									Data = {
										{ "condition", condition }
									}
								};
							}
						}
					}

					// access the specified JSON path to retrieve the URL
					result.RemoteUri = ApplyTemplate(obj);
				} else if (Match.Type == CustomActionMatchType.XML) {
					// this action provides XML matching rules - deserialize the response
					XmlDocument doc = new XmlDocument();
					doc.LoadXml(response);

					if (Match.Conditions != null) {
						// also it provides some preconditions that must be met before taking the URL
						foreach (CustomActionCondition condition in Match.Conditions) {
							if (condition.For == null) {
								App.Logger.WriteLine(LogLevel.Warning, "condition validation unsuccessful: invalid value for `For` was supplied - ignoring");
								continue;
							}

							if (doc.SelectSingleNode(condition.For).Value != condition.Expect.ToString()) {
								// unmet condition - deal with it
								throw new Exception() {
									Data = {
										{ "condition", condition }
									}
								};
							}
						}
					}

					// access the specified Xpath to retrieve tkhe URL
					result.RemoteUri = ApplyTemplate(doc);
				} else if (Match.Type == CustomActionMatchType.Regex) {
					// REGEXXXX UGGGGGH KILL ME PLEASE
					if (Match.Conditions != null) {
						foreach (CustomActionCondition condition in Match.Conditions) {
							if (condition.Match == null) {
								App.Logger.WriteLine(LogLevel.Warning, "condition validation unsuccessful: invalid value for `Match` was supplied - ignoring");
								continue;
							}

							if (!new Regex(condition.Match).IsMatch(response)) {
								// unmet condition
								throw new Exception() {
									Data = {
										{ "condition", condition }
									}
								};
							}
						}
					}

					result.RemoteUri = ApplyTemplate(response);
				}

				// sanity check: make sure the URI doesn't look suspicious
				if (result.RemoteUri != null && (result.RemoteUri.IsFile || result.RemoteUri.IsUnc || result.RemoteUri.IsLoopback)) {
					App.Logger.WriteLine(LogLevel.Error, "returned URI is potentially unsafe - not acceptable");
					throw new Exception();
				}
			} catch (Exception exception) {
				// display warning badge by moving to the second frame
				App.StopIconAnimation();
				App.SetTimedFrame(1);

				// if the "condition" key is present in the exception data, try to take the error strings from there
				string errorTitle, errorDescription;
				if (exception.Data.Contains("condition")) {
					CustomActionCondition condition = exception.Data["condition"] as CustomActionCondition;
					App.Logger.WriteLine(LogLevel.Warning, "unmet condition - expectation failed for: `{0}`", condition.For);

					errorTitle = condition.ErrorTitle;
					errorDescription = condition.ErrorDescription;
				} else {
					errorTitle = App.LocalizationManager.GetString("UploadErrorInstruction");
					errorDescription = App.LocalizationManager.GetString("UploadErrorBody");
				}

				// notify the error
				App.ShowNotification(ToolTipIcon.Warning, errorTitle, errorDescription);

				// just in case!
				result.RemoteUri = null;
			}

			App.StopIconAnimation();
			return result;
		}

		/// <summary>
		/// Performs the file upload
		/// </summary>
		/// <param name="fileName">Local file name</param>
		/// <returns>The server response string</returns>
		public string Upload(string fileName) {
			long fileSize = new FileInfo(fileName).Length;

			if (MaximumFileSize > 0 && fileSize > MaximumFileSize) {
				App.Logger.WriteLine(LogLevel.Error, "validation failed: local file exceeded the maximum size for this action: {0} > {1}", fileSize, MaximumFileSize);
				throw new OverflowException();
			}

			using (FileStream fileStream = new FileStream(fileName, FileMode.Open)) {
				using (HttpClient client = new HttpClient()) {
					using (MultipartFormDataContent formData = new MultipartFormDataContent()) {
						if (Params != null) {
							// add parameters
							foreach (KeyValuePair<string, string> param in Params) {
								formData.Add(new StringContent(param.Value), param.Key);
								App.Logger.WriteLine(LogLevel.Debug, "added parameter: {0}", param.Key);
							}
						}

						if (Method == null) {
							Method = "post";
							App.Logger.WriteLine(LogLevel.Warning, "request method was not specified - falling back to POST");
						}

						// add file data
						formData.Add(new StreamContent(fileStream), Name, DateTime.Now.Ticks.ToString("x") + Path.GetExtension(fileName));
						App.Logger.WriteLine(LogLevel.Verbose, "added file data - waiting for synchronous response");

						// send file
						HttpResponseMessage result = client.SendAsync(new HttpRequestMessage(new HttpMethod(Method.ToUpper()), URL) {
							Content = formData
						}, HttpCompletionOption.ResponseContentRead).Result;

						if (result.IsSuccessStatusCode) {
							App.Logger.WriteLine(LogLevel.Informational, "operation completed successfully");
							string body = result.Content.ReadAsStringAsync().Result;

							if (Match == null) {
								// no matching mechanisms are provided by this action
								if (!result.Headers.Contains("Location")) {
									// try to find some URLs in response body
									foreach (Match match in new Regex(@"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)").Matches(body)) {
										if (Uri.IsWellFormedUriString(match.Value, UriKind.Absolute)) {
											App.Logger.WriteLine(LogLevel.Verbose, "taking first well-formed URI");
											return match.Value;
										}
									}

									App.Logger.WriteLine(LogLevel.Error, "incongruential response - got no Location header and no URLs were found in plaintext");
									throw new HttpRequestException();
								}

								// redirect, maybe? idk man
								App.Logger.WriteLine(LogLevel.Verbose, "got redirect - taking its URL");
								return result.Headers.Location.ToString();
							}

							// return the response body and let the Perform() action deal with the result
							return body;
						} else {
							App.Logger.WriteLine(LogLevel.Error, "unsuccessful response - got status {0}", result.StatusCode);
							throw new HttpRequestException();
						}
					}
				}
			}
		}

		/// <summary>
		/// Displays notifications about the current action
		/// </summary>
		/// <param name="result">An ActionResult instance returned by Action.Process()</param>
		/// <returns>Whether or not the notification was shown</returns>
		public override bool DisplayNotification(ActionResult result) {
			if (result.RemoteUri == null)  // file was not uploaded for some reason
				return base.DisplayNotification(result);

			Clipboard.SetText(result.RemoteUri.ToString());
			App.Logger.WriteLine(LogLevel.Debug, "copied URL to clipboard");

			try {
				if (App.Preferences.LegacyNotifications) {
					throw new Exception();  // HACK: this will fall back to legacy notifications
				}

				if (IconURL != null) {
					// this action contains a custom icon
					string iconFilePath;

					try {
						Uri uri = new Uri(IconURL);

						if (uri.IsFile) {
							iconFilePath = uri.GetLeftPart(UriPartial.Authority | UriPartial.Path);
							App.Logger.WriteLine(LogLevel.Verbose, "set notification file to local path: `{0}`", iconFilePath);
						} else {
							string localIconPath = Path.Combine(App.AppDirectory, "Cache", $"ActionIcon_{ActionID}.png");

							if (!File.Exists(localIconPath)) {
								App.Logger.WriteLine(LogLevel.Warning, "remote notification icon is not yet cached - attempting to download the image");

								using (HttpClient client = new HttpClient()) {
									new Bitmap(new Bitmap(client.GetStreamAsync(IconURL).Result, true) as Image, new Size(144, 144)).Save(localIconPath, ImageFormat.Png);
									App.Logger.WriteLine(LogLevel.Informational, "operation completed successfully");
								}
							}

							iconFilePath = localIconPath;
						}
					} catch {
						App.Logger.WriteLine(LogLevel.Warning, "could not retrieve remote file or is not a valid image - falling back to AppIcon");
						iconFilePath = Path.Combine(App.AppDirectory, "Cache", "AppIcon.png");
					}

					// show modern notification
					App.ShowNotificationEx(result.LocalPath ?? result.TemporaryPath ?? result.RemoteUri?.AbsoluteUri, iconFilePath, App.LocalizationManager.GetString("FileUploadedInstruction"), App.LocalizationManager.GetString("FileUploadedBody"),
						result.RemoteUri?.AbsoluteUri);
				}
			} catch {
				// show classic notification
				App.ShowNotification(ToolTipIcon.Info, App.LocalizationManager.GetString("FileUploadedInstruction"), App.LocalizationManager.GetString("FileUploadedBody"), 5000, () => {
					if (result.RemoteUri == null)
						App.OpenInExplorer(result.LocalPath ?? result.TemporaryPath);
					else
						System.Diagnostics.Process.Start(result.RemoteUri.AbsoluteUri);
				});
			}

			return true;
		}
	}
}
