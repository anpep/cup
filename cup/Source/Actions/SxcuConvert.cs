using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace cup {
	public static class SxcuConvert {
		public static CustomAction ToCustomAction(string json) {
			SharexUploader uploader = JsonConvert.DeserializeObject<SharexUploader>(json);

			if (String.IsNullOrWhiteSpace(uploader.FileFormName)) {
				App.Logger.WriteLine(LogLevel.Error, "invalid ShareX uploader: no FileFormName field found");
				return null;
			}

			CustomAction customAction = new CustomAction() {
				ActionID = json.GetHashCode().ToString("x"),
				Method = uploader.RequestType,
				URL = uploader.RequestURL,
				Name = uploader.FileFormName,
				Headers = uploader.Headers
			};

			App.Logger.WriteLine(LogLevel.Verbose, "trying to build match for custom action");
			if (!String.IsNullOrWhiteSpace(uploader.URL)) {
				Regex expr = new Regex(@"\$(?:(xml|json)\:(.+?))|(?:regex\:([0-9]+)\,?([0-9]+)?)\$", RegexOptions.Compiled);
				MatchCollection matches = expr.Matches(uploader.URL);

				// select 1 for each distinct guard type in this template - if the result count is greater than 1 this uploader is mixing guards, which is not supported
				if ((from Match m in matches
						 group m by m.Groups[1].Value into _
						 select 1).ToArray().Length > 1) {
					App.Logger.WriteLine(LogLevel.Error, "unsupported uploader: distinct guard types are being mixed up");
					return null;
				}

				// try to convert matches
				customAction.Match = new CustomActionMatch();

				switch (matches[0].Groups[0].Value) {
					case "json":
						customAction.Match.Type = CustomActionMatchType.JSON;
						customAction.Match.Template = expr.Replace(uploader.URL, new MatchEvaluator((Match match) => {
							return "{" + match.Groups[2] + "}";
						}));
						break;

					case "xml":
						customAction.Match.Type = CustomActionMatchType.XML;
						customAction.Match.Template = expr.Replace(uploader.URL, new MatchEvaluator((Match match) => {
							return "{" + match.Groups[2] + "}";
						}));
						break;

					case "regex":
						customAction.Match.Type = CustomActionMatchType.Regex;
						customAction.Match.Matches = uploader.RegexList;
						customAction.Match.Template = expr.Replace(uploader.URL, new MatchEvaluator((Match match) => {
							return "{" + match.Groups[1] + " " + match.Groups[2] + "}";
						}));

						break;
				}
			} else {
				App.Logger.WriteLine(LogLevel.Warning, "no URL template found on ShareX uploader - assuming redirection");
			}

			return customAction;
		}
	}
}
