using Newtonsoft.Json;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using Microsoft.Win32.SafeHandles;
using Microsoft.Win32;

namespace cup {
	public static class App {
		private const int HK_CUP_BASE = 0xBA01;

		private static ContextMenu mContextMenu;
		private static NotifyIcon mNotifyIcon;
		private static Thread mAnimationThread;
		private static bool mIconIsAnimating;

		private static KeyboardHookWindow mKeyboardHookWindow;
		private static Dictionary<int, Action> mActionHooks;

		/// <summary>
		/// Whether to render controls with a classic style (i.e. Windows XP and older) or not.
		/// </summary>
		public static bool UseClassicTheme {
			get;
			private set;
		}

		/// <summary>
		/// The LocalizationManager for the app, which provides culture-variant strings.
		/// </summary>
		public static LocalizationManager LocalizationManager {
			get;
			private set;
		}

		public static string AppDirectory {
			get {
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.ProductName);
			}
		}

		public static Dictionary<string, CustomAction> CustomActions {
			get;
			private set;
		}

		public static Logger Logger {
			get;
			private set;
		}

		public static Preferences Preferences {
			get;
			set;
		}

		/// <summary>
		/// Defines the main application entry point.
		/// </summary>
		/// <param name="args">Command-line arguments passed to the program.</param>
		[STAThread]
		public static void Main(string[] args) {
			Mutex mutex = new Mutex(false, Application.ProductName + Application.ProductVersion.ToString());

			try {
				if (!mutex.WaitOne(0, false)) {
					// another instance of the application may be running
					return;
				}

				if (args.Contains("/verbose")) {
					ConsoleManager.Show();
				}

				Logger = new Logger();
				Logger.WriteLine(LogLevel.Informational, "{0} version {1}", Application.ProductName, Application.ProductVersion);

        Directory.CreateDirectory(AppDirectory);
				FileStream stream = new FileStream(Path.Combine(AppDirectory, Application.ProductName + ".log"), args.Contains("/appendLog") ? FileMode.Append : FileMode.OpenOrCreate);
				Logger.Streams.Add(stream);

#if !DEBUG
				AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs e) {
					Logger.WriteLine(LogLevel.Error, "unhandled exception: {0}", e.ExceptionObject);

					if (e.IsTerminating) {
						Logger.WriteLine(LogLevel.Warning, "CLR terminating");
						Environment.Exit(1);
					}
				};
#endif

				// Disable Windows XP-like text rendering
				Application.SetCompatibleTextRenderingDefault(false);

				// Initialize localization manager
				LocalizationManager = new LocalizationManager();

				// Initialize context menu
				mContextMenu = new ContextMenu(new MenuItem[] {
					new MenuItem(LocalizationManager.GetString("TakeScreenshot"), OnScreenshotClicked) { DefaultItem = true },
					new MenuItem("-"),
					new MenuItem(LocalizationManager.GetString("Preferences"), OnPreferencesClicked),
					new MenuItem(LocalizationManager.GetString("About"), OnAboutClicked),
					new MenuItem("-"),
					new MenuItem(LocalizationManager.GetString("Quit"), OnQuitClicked)
				});

				// Initialize notify icon
				mNotifyIcon = new NotifyIcon() {
					Icon = IconExtensions.ExtractFromAssembly("AppIcon", new Size(16, 16)),
					Text = Application.ProductName,

					ContextMenu = mContextMenu
				};
				mNotifyIcon.MouseClick += delegate (object sender, MouseEventArgs mouseEventArgs) {
					if (mouseEventArgs.Button == MouseButtons.Left) {
						OnScreenshotClicked(sender, mouseEventArgs);
					} else if (mouseEventArgs.Button == MouseButtons.Middle) {
						OnQuitClicked(sender, mouseEventArgs);
					}
				};

				mNotifyIcon.BalloonTipClosed += delegate (object balloonTipEventSender, EventArgs balloonTipEventArgs) {
					StopIconAnimation();
				};

				SetIconFrame(0, 16, GetNotifyIconVariant());
				mNotifyIcon.Visible = true;

				RefreshThemeState();
				SystemEvents.UserPreferenceChanging += (sender, eventArgs) => {
					if (eventArgs.Category == UserPreferenceCategory.General) {
						// just in case!
						RefreshThemeState();
					}
				};

				CustomActions = new Dictionary<string, CustomAction>();
				CreateActionDirectory();
				RegisterCustomActions();
				Preferences = Preferences.Load();
				RecreateLocalDirectory();
				RegisterHotKeys();
				WatchActionDirectory();

				InitializeAssetCache();

				// start application event loop
				Application.Run();
			} finally {
				if (mutex != null) {
					mutex.Close();
					mutex = null;
				}
			}
		}

		/// <summary>
		/// Initializes the asset cache directory
		/// </summary>
		/// <returns>Whether the operation completed successfully</returns>
		private static bool InitializeAssetCache() {
			try {
				// create cache directory
				Directory.CreateDirectory(Path.Combine(AppDirectory, "Cache"));
				Logger.WriteLine(LogLevel.Informational, "asset cache directory is OK");
			} catch {
				Logger.WriteLine(LogLevel.Warning, "error creating asset cache directory - some related features may be unavailable");
				return false;
			}

			try {
				// extract app icon
				Properties.Resources.AppIcon.Save(Path.Combine(AppDirectory, "Cache", "AppIcon.png"), ImageFormat.Png);
				return true;
			} catch {
				Logger.WriteLine(LogLevel.Warning, "error extracting AppIcon");
			}

			return false;
		}

		/// <summary>
		/// Creates the custom action directory
		/// </summary>
		/// <returns>Whether the operation completed successfully</returns>
		private static bool CreateActionDirectory() {
			try {
				Directory.CreateDirectory(Path.Combine(AppDirectory, "Actions"));
				Logger.WriteLine(LogLevel.Informational, "action directory is OK");
				return true;
			} catch {
				Logger.WriteLine(LogLevel.Warning, "error creating action directory - some related features may be unavailable");
			}

			return false;
		}

		/// <summary>
		/// Creates a CustomAction instance and stores it in the CustomActions list, given a file path
		/// </summary>
		/// <param name="filePath">Path of the JSON/SXCU file</param>
		/// <param name="counter">Internal counter for file access errors</param>
		/// <returns></returns>
		private static CustomAction DeserializeAction(string filePath, int counter = 5) {
			Logger.WriteLine(LogLevel.Verbose, "deserializing custom action: {0}", filePath);
			
			try {
				CustomAction action;
				string extension = Path.GetExtension(filePath);

				if (extension == ".json") {
					// cup custom action
					action = JsonConvert.DeserializeObject<CustomAction>(File.ReadAllText(filePath));
					action.ActionID = filePath.GetHashCode().ToString("x");

					if (action.Match != null) {
						if (action.Match.Type == CustomActionMatchType.Regex) {
							if (action.Match.Matches == null || action.Match.Matches.Count < 1) {
								Logger.WriteLine(LogLevel.Warning, "invalid custom action: regex match must contain an expression (`match`) - ignoring");
								return null;
							}
						}

						if (String.IsNullOrEmpty(action.Match.Template)) {
							Logger.WriteLine(LogLevel.Warning, "invalid custom action: a null or empty URL template has been supplied - ignoring");
							return null;
						}
					}
				} else if (extension == ".sxcu") {
					// ShareX Custom Uploader
					action = SxcuConvert.ToCustomAction(File.ReadAllText(filePath));
				} else {
					Logger.WriteLine(LogLevel.Warning, "unrecognized custom action file extension: `{0}` - ignoring action", extension);
					return null;
				}

				Logger.WriteLine(LogLevel.Informational, "action {0} has been deserialized", filePath);
				return action;
			} catch (IOException) {
				Logger.WriteLine(LogLevel.Warning, "caught IOException while trying to access action file - is the file locked? (remaining attempts: {0})", counter - 1);

				// give it some more tries
				if (counter > 0)
					return DeserializeAction(filePath, counter - 1);

				Logger.WriteLine(LogLevel.Warning, "definitely can't access action file - ignoring action");
				return null;
			}
#if !DEBUG
			catch (Exception exception) {
				Logger.WriteLine(LogLevel.Warning, "error processing action file: {0}", exception);
				return null;
			}
#endif  // !DEBUG
		}

		/// <summary>
		/// Registers custom actions found in the action directory
		/// </summary>
		private static void RegisterCustomActions() {
			Logger.WriteLine(LogLevel.Verbose, "discovering and registering custom actions");

			foreach (string path in Directory.EnumerateFiles(Path.Combine(AppDirectory, "Actions"), "*", SearchOption.AllDirectories)) {
				CustomAction action = DeserializeAction(path);

				if (action != null) {
					CustomActions[Path.GetFileNameWithoutExtension(path)] = action;
				}
			}
		}

		/// <summary>
		/// Watches the action directory for changes and performs the pertinent tasks
		/// </summary>
		private static void WatchActionDirectory() {
			FileSystemWatcher watcher = new FileSystemWatcher(Path.Combine(AppDirectory, "Actions")) {
				IncludeSubdirectories = true,
				EnableRaisingEvents = true,
				NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
			};

			watcher.Deleted += delegate (object sender, FileSystemEventArgs args) {
				string key = Path.GetFileNameWithoutExtension(args.Name);
				Logger.WriteLine(LogLevel.Verbose, "action has been deleted: {0}", key);

				if (CustomActions.ContainsKey(key)) {
					CustomActions.Remove(key);
				}

				if (Preferences.ActionType == ActionType.CustomAction && Preferences.ActionName == key) {
					Preferences tempPreferences = new Preferences();

					Preferences.ActionType = tempPreferences.ActionType;
					Preferences.ActionName = tempPreferences.ActionName;

					UnregisterHotKeys();
					RegisterHotKeys();

					Preferences.Save();
				}

				UpdatePreferencesDialog();
			};

			watcher.Changed += delegate (object sender, FileSystemEventArgs args) {
				string key = Path.GetFileNameWithoutExtension(args.Name);
				Logger.WriteLine(LogLevel.Verbose, "action has been updated: {0}", key);

				if (CustomActions.ContainsKey(key)) {
					CustomActions.Remove(key);
				}

				CustomAction action = DeserializeAction(args.FullPath);
				if (action != null)
					CustomActions[key] = action;

				UpdatePreferencesDialog();
			};

			watcher.Renamed += delegate (object sender, RenamedEventArgs args) {
				string key = Path.GetFileNameWithoutExtension(args.Name),
					oldKey = Path.GetFileNameWithoutExtension(args.OldName);

				Logger.WriteLine(LogLevel.Verbose, "action has been renamed: {0} -> {1}", oldKey, key);

				if (CustomActions.ContainsKey(oldKey)) {
					CustomActions.Add(key, CustomActions[oldKey]);
					CustomActions.Remove(oldKey);
				}

				if (Preferences.ActionType == ActionType.CustomAction && Preferences.ActionName == oldKey) {
					Preferences.ActionName = key;
					Logger.WriteLine(LogLevel.Informational, "new user action set: {0} -> {1}", oldKey, key);
					Preferences.Save();
				}

				UpdatePreferencesDialog();
			};

			watcher.Created += delegate (object sender, FileSystemEventArgs args) {
				string key = Path.GetFileNameWithoutExtension(args.Name);
				Logger.WriteLine(LogLevel.Verbose, "created action: {0}", key);

				if (!CustomActions.ContainsKey(key)) {
					Logger.WriteLine(LogLevel.Verbose, "unregistering previous conflicting action");
					CustomAction action = DeserializeAction(args.FullPath);

					if (action != null)
						CustomActions[key] = action;

					UpdatePreferencesDialog();
				};
			};

			new Thread(new ThreadStart(() => {
				try {
					watcher.WaitForChanged(WatcherChangeTypes.All);
					Logger.WriteLine(LogLevel.Verbose, "watching action directory");
				} catch {
					Logger.WriteLine(LogLevel.Warning, "failed to watch action directory");
				}
			})).Start();
		}

		/// <summary>
		/// Initiates an action
		/// </summary>
		/// <param name="action">The Action instance</param>
		private static void InitiateAction(Action action) {
			if (CropDialog.IsOpen) {
				Logger.WriteLine(LogLevel.Warning, "crop dialog already open - ignoring");
				return;
			}

			using (CropDialog cropDialog = new CropDialog()) {
				if (cropDialog.ShowDialog() == DialogResult.OK) {
					Thread thread = new Thread(new ThreadStart(delegate {
						using (Bitmap bitmap = GDI.CaptureBitmapFromScreen(cropDialog.Area)) {
							// Disable context menu items so no concurrent captures can be made
							mContextMenu.MenuItems[0].Enabled = false;
							mContextMenu.MenuItems[1].Enabled = false;

							// Perform the action and display its notification
							action.DisplayNotification(action.Process(bitmap));

							// Re-enable the context menu items again
							mContextMenu.MenuItems[0].Enabled = true;
							mContextMenu.MenuItems[1].Enabled = true;
						}
					}));

					thread.SetApartmentState(ApartmentState.STA);
					thread.Start();
				}
			}
		}

		/// <summary>
		/// Event handler triggered when the "Take screenshot" context menu item gets activated.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">Event arguments.</param>
		private static void OnScreenshotClicked(object sender, EventArgs e) {
			// initiate default action
			InitiateAction(Action.GetAction(Preferences.ActionType, Preferences.ActionName));
		}

		/// <summary>
		/// HACK: Updates Preferences dialog
		/// </summary>
		private static void UpdatePreferencesDialog() {
			if (!PreferencesDialog.IsOpen)
				return;

			PreferencesDialog dialog = (from form in Application.OpenForms.Cast<Form>()
																	where form.Name == "PreferencesDialog"
																	select form as PreferencesDialog).First();

			if (dialog == null)
				return;

			dialog.Invoke(new MethodInvoker(() => {
				dialog.InitializeStyles();
				dialog.PopulateActions();
			}));
		}

		/// <summary>
		/// HACK: Updates About dialog
		/// </summary>
		private static void UpdateAboutDialog() {
			if (!AboutDialog.IsOpen)
				return;

			AboutDialog dialog = (from form in Application.OpenForms.Cast<Form>()
														where form.Name == "AboutDialog"
														select form as AboutDialog).First();

			if (dialog == null)
				return;

			dialog.Invoke(new MethodInvoker(() => {
				dialog.InitializeStyles();
			}));
		}

		/// <summary>
		/// Event handler triggered when the "Preferences" context menu item gets activated.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">Event arguments.</param>
		private static void OnPreferencesClicked(object sender, EventArgs e) {
			if (PreferencesDialog.IsOpen) {
				Logger.WriteLine(LogLevel.Warning, "preferences dialog already open - ignoring");
				return;
			}

			using (PreferencesDialog preferencesDialog = new PreferencesDialog()) {
				preferencesDialog.ShowDialog();
			}
		}

		/// <summary>
		/// Event handler triggered when the "About" context menu item gets activated.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">Event arguments.</param>
		private static void OnAboutClicked(object sender, EventArgs e) {
			if (AboutDialog.IsOpen) {
				Logger.WriteLine(LogLevel.Warning, "about dialog already open - ignoring");
				return;
			}

			using (AboutDialog preferencesDialog = new AboutDialog()) {
				preferencesDialog.ShowDialog();
			}
		}

		/// <summary>
		/// Event handler triggered when the "Quit" context menu item gets activated.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">Event arguments.</param>
		private static void OnQuitClicked(object sender, EventArgs e) {
			StopIconAnimation();

			UnregisterHotKeys();

			mNotifyIcon.Visible = false;
			mNotifyIcon.Dispose();
			Environment.Exit(0);
		}

		public static void SetIconFrame(int frame, int squareSize = 16, int variant = 0) {
			try {
				using (Bitmap bitmap = new Bitmap(squareSize, squareSize)) {
					using (Graphics graphics = Graphics.FromImage(bitmap)) {
						graphics.DrawImage(Properties.Resources.TrayIconStrip, new Rectangle(0, 0, squareSize, squareSize), new Rectangle(frame * squareSize, variant * squareSize, squareSize, squareSize), GraphicsUnit.Pixel);
					}

					mNotifyIcon.Icon = Icon.FromHandle(bitmap.GetHicon());
				}
			} catch { }
		}

		/// <summary>
		/// Starts tray icon frame animation asynchronously given some values.
		/// </summary>
		/// <param name="from">Initial frame index.</param>
		/// <param name="to">Last frame index.</param>
		/// <param name="delay">Delay between frames.</param>
		/// <param name="squareSize">The side value for each frame</param>
		/// <param name="variant">Animation variant (sprite map row)</param>
		public static void PerformIconAnimation(int from, int to, int delay = 50, int squareSize = 16, int variant = 0) {
			// Don't do anything if another animation is currently in progress.
			if (mIconIsAnimating)
				return;

			mIconIsAnimating = true;

			mAnimationThread = new Thread(new ThreadStart(() => {
				using (Bitmap bitmap = new Bitmap(16, 16)) {
					using (Graphics graphics = Graphics.FromImage(bitmap)) {
						while (mIconIsAnimating) {
							for (int frame = from; frame < to; frame++) {
								// Draw frame
								graphics.DrawImage(Properties.Resources.TrayIconStrip, new Rectangle(0, 0, squareSize, squareSize), new Rectangle(frame * squareSize, variant * squareSize, squareSize, squareSize), GraphicsUnit.Pixel);

								// Change tray icon
								mNotifyIcon.Icon = Icon.FromHandle(bitmap.GetHicon());

								// Clear graphics so icons don't get stacked on top of each other.
								graphics.Clear(Color.Transparent);

								// Sleep for a bit before the next frame is rendered
								Thread.Sleep(delay);
							}
						}
					}
				}
			}));

			mAnimationThread.Start();
		}

		/// <summary>
		/// Stops tray icon animation, if any, and resets the notify icon.
		/// </summary>
		public static void StopIconAnimation() {
			mIconIsAnimating = false;

			if (mAnimationThread != null) {
				mAnimationThread.Abort();
				mAnimationThread = null;
			}

			SetIconFrame(0, 16, GetNotifyIconVariant());
		}

		/// <summary>
		/// Returns the tray icon variant based on the current platform version.
		/// </summary>
		public static int GetNotifyIconVariant() {
			if (Environment.OSVersion.Version.Major < 6) {
				// Windows XP (5.1) and older (first row)
				return 0;
			} else if (Environment.OSVersion.Version.Major >= 10) {
				// Windows 10 (10.0, fourth row)
				return 3;
			} else if (Environment.OSVersion.Version.Minor >= 2) {
				// Windows 8 and fist Windows 10 Previews (>= 6.2, third row)
				return 2;
			}

			// Windows Vista and 7 (6.0, 6.1), second row (default)
			return 1;
		}

		/// <summary>
		/// Adjust theme for older platforms and for the classic Windows theme
		/// </summary>
		public static void RefreshThemeState() {
			bool previousValue = UseClassicTheme;
			UseClassicTheme = Environment.OSVersion.Version.Major < 6 || !NativeMethods.IsThemeActive();
			Application.EnableVisualStyles();
			Logger.WriteLine(LogLevel.Debug, "using classic theme: {0}", UseClassicTheme);

			if (previousValue != UseClassicTheme) {
				// update dialog styles
				UpdatePreferencesDialog();
				UpdateAboutDialog();
			}
		}

		/// <summary>
		/// Registers hot keys
		/// </summary>
		public static void RegisterHotKeys() {
			mActionHooks = new Dictionary<int, Action>();
			mKeyboardHookWindow = new KeyboardHookWindow();

			int hotKeyId = HK_CUP_BASE;
			List<string> keysToRemove = new List<string>();

			foreach (KeyValuePair<string, string> hotKeyData in Preferences.HotKeys) {
				try {
					Action action = Action.GetAction(CustomActions.ContainsKey(hotKeyData.Key) ? ActionType.CustomAction : ActionType.BuiltInAction, hotKeyData.Key, true);
					HotKey hotKey = HotKey.Parse(hotKeyData.Value);

					if (hotKey != HotKey.Nil) {
						if (mKeyboardHookWindow.RegisterHotKey(hotKeyId, hotKey)) {
							mActionHooks.Add(hotKeyId++, action);
							Logger.WriteLine(LogLevel.Informational, "registered hotkey 0x{0:X2}: {1}, {2}", hotKeyId, hotKeyData.Key, hotKey);
						} else {
							Logger.WriteLine(LogLevel.Warning, "unable to register hotkey 0x{0:X2} - ignoring", hotKeyId);
						}
					}
				} catch {
					// may be an invalid key value pair - remove kebab
					Logger.WriteLine(LogLevel.Warning, "invalid hot key detected - removing");
					keysToRemove.Add(hotKeyData.Key);
				}
			}

			foreach (string key in keysToRemove)
				Preferences.HotKeys.Remove(key);

			if (keysToRemove.Count > 0)
				Preferences.Save();

			mKeyboardHookWindow.OnKeyboardHookTriggered += OnKeyboardHookTriggered;
		}

		/// <summary>
		/// Unregisters previously reigstered hotkeys, if any
		/// </summary>
		public static void UnregisterHotKeys() {
			if (mKeyboardHookWindow == null)
				return;

			foreach (KeyValuePair<int, Action> hook in mActionHooks) {
				mKeyboardHookWindow.UnregisterHotKey(hook.Key);
			}

			mActionHooks.Clear();
			mKeyboardHookWindow.OnKeyboardHookTriggered -= OnKeyboardHookTriggered;
		}

		/// <summary>
		/// Event triggered when a hot key message has been received
		/// </summary>
		/// <param name="sender">The sender of the event</param>
		/// <param name="e">Arguments for this event</param>
		private static void OnKeyboardHookTriggered(object sender, KeyboardHookTriggeredEventArgs e) {
			Logger.WriteLine(LogLevel.Debug, "keyboard hook 0x{0:X2} triggered", e.ID);

			if (!mActionHooks.ContainsKey(e.ID)) {
				Logger.WriteLine(LogLevel.Warning, "unrecognized keyboard hook - ignoring");
				return;
			}

			if (mContextMenu.MenuItems[0].Enabled) {
				InitiateAction(mActionHooks[e.ID]);
			} else {
				Logger.WriteLine(LogLevel.Warning, "attempted to take screenshot while processing previous one - ignoring");
			}
		}

		/// <summary>
		/// Displays a notification bubble with the specified icon and text
		/// </summary>
		/// <param name="icon">The icon to be shown</param>
		/// <param name="instruction">The instruction text</param>
		/// <param name="text">The notification body</param>
		/// <param name="timeout">The time to wait, in milliseconds, for the notification to be hidden</param>
		public static void ShowNotification(ToolTipIcon icon, string instruction, string text, int timeout = 5000, System.Action callback = null) {
			Logger.WriteLine(LogLevel.Verbose, "displaying balloon tip");
			mNotifyIcon.ShowBalloonTip(timeout, instruction, text, icon);

			if (callback != null) {
				mNotifyIcon.Tag = new EventHandler(delegate (object sender, EventArgs e) {
					// Detach event handler and call the callback
					mNotifyIcon.BalloonTipClicked -= (EventHandler)mNotifyIcon.Tag;
					callback();
				});

				// Attach event handler for detaching itself before the callback gets called
				mNotifyIcon.BalloonTipClicked += (EventHandler)mNotifyIcon.Tag;
			}
		}

		public static void ShowNotificationEx(string picturePath, string logoPath, string title, string description, string url) {
			ToastContent content = new ToastContent() {
				ActivationType = ToastActivationType.Protocol,
				Duration = ToastDuration.Short,
				Launch = url == null ? new Uri(picturePath).AbsolutePath : url,
				Audio = new ToastAudio() { Silent = true },
				Visual = new ToastVisual() {
					BindingGeneric = new ToastBindingGeneric() {
						Attribution = new ToastGenericAttributionText() { Text = url },
						AppLogoOverride = new ToastGenericAppLogo() { Source = logoPath },
						Children = {
							new AdaptiveText() { Text = title, HintStyle = AdaptiveTextStyle.Title },
							new AdaptiveText() { Text = description, HintStyle = AdaptiveTextStyle.Body },
							new AdaptiveImage() { Source = new Uri(picturePath).AbsoluteUri, HintAlign = AdaptiveImageAlign.Center, HintCrop = AdaptiveImageCrop.None, HintRemoveMargin = true }
						}
					}
				}
			};

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(content.GetContent());

			Logger.WriteLine(LogLevel.Debug, "toast markup:\n{0}", content.GetContent());

			ToastNotification toast = new ToastNotification(doc) {
				Group = Application.ProductName + Application.ProductVersion.ToString(),
				Priority = ToastNotificationPriority.High,
				ExpirationTime = new DateTimeOffset(DateTime.Now).AddSeconds(5)
			};

			Logger.WriteLine(LogLevel.Verbose, "displaying toast notification");
			ToastNotificationManager.CreateToastNotifier(Application.ProductName).Show(toast);

			// HACK: delete the temporary image for display in 5 seconds
			new Thread(new ThreadStart(() => {
				if (picturePath != null) {
					Thread.Sleep(5000);
					File.Delete(picturePath);
				}
			})).Start();
		}

		/// <summary>
		/// Creates the "Screenshots" directory if it has been deleted.
		/// </summary>
		public static bool RecreateLocalDirectory() {
			Logger.WriteLine(LogLevel.Verbose, "recreating local directory");

			string directory = Preferences.LocalPath;

			if (String.IsNullOrEmpty(directory))
				directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), LocalizationManager.GetString("LocalDirectoryName"));

			try {
				// create directory
				if (!Directory.Exists(directory)) {
					Directory.CreateDirectory(directory);
					Logger.WriteLine(LogLevel.Warning, "could not create directory: {0}", directory);
				}

				// test write permissions
				string tempFilePath = Path.Combine(directory, "Write permission test.txt");
				File.WriteAllText(tempFilePath, "Hello, world!");
				File.Delete(tempFilePath);
				Logger.WriteLine(LogLevel.Informational, "local directory is OK");
				return true;
			} catch {
				// could not create directory or not writable
				Logger.WriteLine(LogLevel.Warning, "local directory is not accessible");

				if (String.IsNullOrEmpty(Preferences.LocalPath)) {
					Logger.WriteLine(LogLevel.Warning, "could not create default local directory or it's not accessible - disabling local saving");

					Preferences.KeepLocalCopy = false;
					Preferences.Save();
				} else {
					Logger.WriteLine(LogLevel.Verbose, "attempting to create default directory");
					Preferences.LocalPath = "";
					Preferences.Save();
					return RecreateLocalDirectory();
				}

				return false;
			}
		}

		/// <summary>
		/// Opens the specified file in the Windows File Explorer
		/// </summary>
		/// <param name="fileName">The file name to be selected</param>
		public static void OpenInExplorer(string fileName) {
			Process.Start("explorer", "/select,\"" + fileName.Replace("\"", "\\\"") + "\"");
		}

		/// <summary>
		/// Animates the notify icon using the spinner animation according to the current platform variant
		/// </summary>
		public static void ShowIconSpinner() {
			switch (GetNotifyIconVariant()) {
				case 0:
					PerformIconAnimation(10, 14, 50, 16, 0); // Frames 10-14 (Windows XP)
					break;

				case 2:
					PerformIconAnimation(10, 14, 50, 16, 2); // Frames 10-14 (Windows 8)
					break;

				case 3:
					PerformIconAnimation(13, 19, 50, 16, 3); // Frames 13-19 (Windows 10)
					break;

				default:
					PerformIconAnimation(10, 27, 50, 16, 1); // Frames 10-27 (Windows Vista/7, default)
					break;
			}
		}

		/// <summary>
		/// Changes the frame for the specified amount of time with the variation for the current platform
		/// <param name="duration">The duration, in milliseconds, of the frame</param>
		/// </summary>
		public static void SetTimedFrame(int frame, int duration = 5000) {
			new Thread(new ThreadStart(delegate {
				SetIconFrame(frame, 16, GetNotifyIconVariant());
				Thread.Sleep(duration);
				SetIconFrame(0, 16, GetNotifyIconVariant());
			})).Start();
		}
	}
}
