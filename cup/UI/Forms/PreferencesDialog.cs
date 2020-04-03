using Microsoft.Win32;
using Ookii.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Windows.Forms;

namespace cup {
	public partial class PreferencesDialog : Form {
		/// <summary>
		/// Determines whether there's another assigned instance of this Form or not.
		/// </summary>
		public static bool IsOpen {
			get;
			private set;
		}

		private readonly ImageFormat[] IMAGE_FORMATS = { ImageFormat.Png, ImageFormat.Jpeg, ImageFormat.Gif };
		private (ActionType, string)[] mActions;

		private RegistryKey mStartupRegistryKey;

		/// <summary>
		/// Initializer method for this class.
		/// </summary>
		public PreferencesDialog() {
			InitializeComponent();
			InitializeStyles();

			// Set localized strings
			Text = Application.ProductName;
			mainInstruction.Text = App.LocalizationManager.GetString("Preferences");

			screenshotLabel.Text = App.LocalizationManager.GetString("ScreenshotHotKey");
			actionLabel.Text = App.LocalizationManager.GetString("Action");

			saveLocallyCheckBox.Text = App.LocalizationManager.GetString("SaveLocally");
			autostartCheckBox.Text = App.LocalizationManager.GetString("Autostart");

			advancedSettingsChevron.Text = App.LocalizationManager.GetString("AdvancedSettings");
			screenshotFormatLabel.Text = App.LocalizationManager.GetString("ScreenshotFormat");

			legacyNotificationsCheckBox.Text = App.LocalizationManager.GetString("LegacyNotifications");
			openActionsDirectory.Text = App.LocalizationManager.GetString("OpenActionsDirectory");

			PopulateActions();

			// Screenshot format
			foreach (ImageFormat format in IMAGE_FORMATS)
				screenshotFormatComboBox.Items.Add(format);

			// Read settings
			if (App.Preferences.HotKeys.ContainsKey(App.Preferences.ActionName)) {
				screenshotHotKeyTextBox.Text = HotKey.Parse(App.Preferences.HotKeys[App.Preferences.ActionName]).ToString();
			} else {
				screenshotHotKeyTextBox.Text = App.LocalizationManager.GetString("NilHotKey");
			}

			saveLocallyCheckBox.Checked = App.Preferences.KeepLocalCopy && App.RecreateLocalDirectory();
			autostartCheckBox.Enabled = OpenStartupRegistryKey();
			autostartCheckBox.Checked = IsAppOnStartupKey();
			screenshotFormatComboBox.SelectedItem = new ImageFormat(App.Preferences.ImageFormat);

			if (Environment.OSVersion.Version.Major < 10 || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor <= 1)) {
				// older than Windows 10 (10.0) or older than Windows 8 (6.2)
				legacyNotificationsCheckBox.Visible = false;
			} else {
				legacyNotificationsCheckBox.Checked = App.Preferences.LegacyNotifications;
			}

			// Collapse dialog
			Height -= advancedSettingsPanel.Height;
			advancedSettingsPanel.Visible = false;
		}

		/// <summary>
		/// Modify control and form styles accordingly.
		/// </summary>
		public void InitializeStyles() {
			Font = SystemFonts.MessageBoxFont;

			if (App.UseClassicTheme) {
				BackColor = SystemColors.Control;
				mainInstruction.Font = new Font(Font, FontStyle.Bold);
				mainInstruction.ForeColor = SystemColors.ControlText;
			} else {
				BackColor = SystemColors.Window;
				mainInstruction.Font = new Font(Font.FontFamily, 12, FontStyle.Regular);
				mainInstruction.ForeColor = Color.FromArgb(0x003399);
			}
		}

		/// <summary>
		/// Returns whether or not the app has a value on the Startup registry key
		/// </summary>
		/// <returns>Whether or not the operation completed successfully</returns>
		private bool IsAppOnStartupKey() {
			try {
				return mStartupRegistryKey.GetValue(Application.ProductName, null).ToString() == Application.ExecutablePath;
			} catch {
				return false;
			}
		}

		/// <summary>
		/// Creates or deletes the value on the Startup registry key
		/// </summary>
		/// <returns>Whether or not the operation completed successfully</returns>
		private bool ToggleAppOnStartup() {
			bool onStartupKey = IsAppOnStartupKey();

			try {
				if (onStartupKey) {
					mStartupRegistryKey.DeleteValue(Application.ProductName);
				} else {
					mStartupRegistryKey.SetValue(Application.ProductName, Application.ExecutablePath, RegistryValueKind.String);
				}

				return !onStartupKey;
			} catch {
				return onStartupKey;
			}
		}

		/// <summary>
		/// Opens the Startup registry key
		/// </summary>
		/// <returns>Whether or not the operation completed successfully</returns>
		private bool OpenStartupRegistryKey() {
			try {
				mStartupRegistryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", RegistryKeyPermissionCheck.ReadWriteSubTree);
				App.Logger.WriteLine(LogLevel.Informational, "operation completed successfully");
				return true;
			} catch {
				App.Logger.WriteLine(LogLevel.Warning, "could not open startup registry key - some related features may be unavailable");
				return false;
			}
		}

		/// <summary>
		/// Populate actions and custom actions
		/// </summary>
		public void PopulateActions() {
			// get all actions as an array of (ActionType, string) tuples
			mActions = (from type in Assembly.GetExecutingAssembly().GetTypes()
									where type.Namespace == "cup.Actions" && type.IsSubclassOf(typeof(Action))
									select (ActionType.BuiltInAction, type.Name)).Concat(from customAction in App.CustomActions.Keys
																																			 select (ActionType.CustomAction, customAction)).ToArray();

			App.Logger.WriteLine(LogLevel.Debug, "found {0} action(s)", mActions.Length);

			actionComboBox.Items.Clear();
			actionComboBox.Items.AddRange(Array.ConvertAll(mActions, x => (object)x));

			if (actionComboBox.SelectedIndex < 0) {
				actionComboBox.SelectedIndex = 0;
				App.Logger.WriteLine(LogLevel.Warning, "the previously selected action is no longer available - falling back to 0");
			}

		}

		/// <summary>
		/// Displays the keys pressed by the user.
		/// </summary>
		/// <param name="sender">Sender object.</param>
		/// <param name="e">Arguments for this event.</param>
		private void screenshotHotKeyTextBox_KeyDown(object sender, KeyEventArgs e) {
			e.SuppressKeyPress = true;

			HotKey hotKey = e.Modifiers == Keys.None ? HotKey.Nil : new HotKey(e.Modifiers, e.KeyCode);
			string encodedHotKey = hotKey.Encode();
			string currentAction = (((ActionType, string))actionComboBox.SelectedItem).Item2;

			if (App.Preferences.HotKeys.ContainsValue(encodedHotKey)) {
				hotKey = HotKey.Nil;

				SystemSounds.Asterisk.Play();
				shortcutWarningToolTip.ToolTipTitle = App.LocalizationManager.GetString("DuplicateHotKeyTitle");
				shortcutWarningToolTip.Show(String.Format(App.LocalizationManager.GetString("DuplicateHotKeyDescription"), (from hk in App.Preferences.HotKeys
																																																										where hk.Value == encodedHotKey
																																																										select hk.Key).First()), this,
																																																										screenshotHotKeyTextBox.Location.X + screenshotHotKeyTextBox.Width - 24,
																																																										screenshotHotKeyTextBox.Location.Y - 32);
			} else if (hotKey != HotKey.Nil) {
				App.Preferences.HotKeys[currentAction] = encodedHotKey;
			} else {
				App.Preferences.HotKeys.Remove(currentAction);
			}


			screenshotHotKeyTextBox.Text = hotKey.ToString();
		}

		/// <summary>
		/// Triggered when the Save locally checkbox's state has been changed
		/// </summary>
		/// <param name="sender">Sender of the event</param>
		/// <param name="e">Arguments for this event</param>
		private void saveLocallyCheckBox_CheckedChanged(object sender, EventArgs e) {
			saveLocallyCheckBox.Checked = App.RecreateLocalDirectory();
			App.Preferences.KeepLocalCopy = saveLocallyCheckBox.Checked;
		}

		/// <summary>
		/// Triggered when the action combo box's selection has been changed.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Arguments for this event.</param>
		private void actionComboBox_SelectedIndexChanged(object sender, EventArgs e) {
			App.Preferences.ActionType = mActions[actionComboBox.SelectedIndex].Item1;
			App.Preferences.ActionName = mActions[actionComboBox.SelectedIndex].Item2;

			App.UnregisterHotKeys();
			App.RegisterHotKeys();

			if (App.Preferences.HotKeys.ContainsKey(App.Preferences.ActionName)) {
				screenshotHotKeyTextBox.Text = HotKey.Parse(App.Preferences.HotKeys[App.Preferences.ActionName]).ToString();
			} else {
				screenshotHotKeyTextBox.Text = App.LocalizationManager.GetString("NilHotKey");
			}
		}

		/// <summary>
		/// Triggered when the image format combo box's selection has been changed.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Arguments for this event.</param>
		private void screenshotFormatComboBox_SelectedIndexChanged(object sender, EventArgs e) {
			App.Preferences.ImageFormat = (screenshotFormatComboBox.SelectedItem as ImageFormat).Guid;
		}

		/// <summary>
		/// Raises the Form.Shown event.
		/// </summary>
		/// <param name="e">Arguments for this event.</param>
		protected override void OnShown(EventArgs e) {
			IsOpen = true;
			base.OnShown(e);
		}

		/// <summary>
		/// Raises the Form.FormClosing event.
		/// </summary>
		/// <param name="e">Arguments for this event.</param>
		protected override void OnFormClosing(FormClosingEventArgs e) {
			IsOpen = false;
			Hide();
			App.Preferences.Save();
			App.UnregisterHotKeys();
			App.RegisterHotKeys();
			App.Logger.WriteLine(LogLevel.Debug, "settings saved");
			base.OnFormClosing(e);
		}

		/// <summary>
		/// Collapse the dialog when the advanced settings panel is being collapsed
		/// </summary>
		/// <param name="sender">Sender of the event</param>
		/// <param name="e">Arguments for this event</param>
		private void advancedSettingsChevron_Collapse(object sender, EventArgs e) {
			advancedSettingsPanel.Visible = false;

			if (App.UseClassicTheme) {
				Height -= advancedSettingsPanel.Height;
			} else {
				int newHeight = Height - advancedSettingsPanel.Height;

				while (Height > newHeight) {
					Height -= 4;
					Top += 2;
				}
			}
		}

		/// <summary>
		/// Expand the dialog when the advanced settings panel is being expanded.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Arguments for this event.</param>
		private void advancedSettingsChevron_Expand(object sender, EventArgs e) {
			if (App.UseClassicTheme) {
				Height += advancedSettingsPanel.Height;
			} else {
				int newHeight = Height + advancedSettingsPanel.Height;

				while (Height < newHeight) {
					Height += 4;
					Top -= 2;
				}
			}

			advancedSettingsPanel.Visible = true;
		}

		/// <summary>
		/// Opens the actions directory for the user to manage them
		/// </summary>
		/// <param name="sender">Sender of this event</param>
		/// <param name="e">Arguments for this event</param>
		private void openActionsDirectory_Click(object sender, EventArgs e) {
			Process.Start(Path.Combine(App.AppDirectory, "Actions"));
		}

		/// <summary>
		/// Formats each action name in the action combo box
		/// </summary>
		/// <param name="sender">Sender of this event</param>
		/// <param name="e">Arguments for this event</param>
		private void actionComboBox_Format(object sender, ListControlConvertEventArgs e) {
			// HACK: deconstruction odyssey!
			(ActionType actionType, string name) = ((ActionType, string))(e.Value ?? (ActionType.BuiltInAction, ""));

			if (actionType == ActionType.BuiltInAction) {
				e.Value = $"{App.LocalizationManager.GetString(name)} {App.LocalizationManager.GetString("BuiltIn")}";
			} else {
				e.Value = name;
			}
		}

		/// <summary>
		/// Toggles the autostart option
		/// </summary>
		/// <param name="sender">Sender of this event</param>
		/// <param name="e">Arguments for this event</param>
		private void autostartCheckBox_CheckedChanged(object sender, EventArgs e) {
			autostartCheckBox.Checked = ToggleAppOnStartup();
		}

		/// <summary>
		/// Unregister hotkeys so they are not triggered while the focus is on the hotkey input
		/// </summary>
		/// <param name="sender">Sender of this event</param>
		/// <param name="e">Arguments for this event</param>
		private void screenshotHotKeyTextBox_Enter(object sender, EventArgs e) {
			App.UnregisterHotKeys();
		}

		/// <summary>
		/// Re-registers hotkeys after leaving focus on the hotkey input
		/// </summary>
		/// <param name="sender">Sender of this event</param>
		/// <param name="e">Arguments for this event</param>
		private void screenshotHotKeyTextBox_Leave(object sender, EventArgs e) {
			App.RegisterHotKeys();
		}

		/// <summary>
		/// Toggles legacy notifications setting
		/// </summary>
		/// <param name="sender">Sender of this event</param>
		/// <param name="e">Arguments for this event</param>
		private void legacyNotificationsCheckBox_CheckedChanged(object sender, EventArgs e) {
			App.Preferences.LegacyNotifications = legacyNotificationsCheckBox.Checked;
		}

		/// <summary>
		/// Triggered when the "Change directory..." link is clicked
		/// </summary>
		/// <param name="sender">Sender of this event</param>
		/// <param name="e">Arguments for this event</param>
		private void changeDirectoryLink_Click(object sender, EventArgs e) {
			try {
				using (VistaFolderBrowserDialog folderBrowserDialog = new VistaFolderBrowserDialog() { SelectedPath = App.Preferences.LocalPath }) {
					if (folderBrowserDialog.ShowDialog(this) == DialogResult.OK) {
						App.Preferences.LocalPath = folderBrowserDialog.SelectedPath;
						App.Logger.WriteLine(LogLevel.Verbose, "updated local directory: `{0}`", App.Preferences.LocalPath);
					}
				}
			} catch (OverflowException) {
				// HACK: VistaFolderBrowserDialog has an integer overflow bug which I'm not gonna solve 'cause I'm too lazy - come to daddy OverflowException!
			}
		}

		/// <summary>
		/// Converts image formats human-readable strings
		/// </summary>
		/// <param name="sender">Sender of this event</param>
		/// <param name="e">Arguments for this event</param>
		private void screenshotFormatComboBox_Format(object sender, ListControlConvertEventArgs e) {
			e.Value = (e.ListItem as ImageFormat).ToString().ToUpperInvariant();
		}
	}
}
