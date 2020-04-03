using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace cup {
	public partial class AboutDialog : Form {
		/// <summary>
		/// Determines whether there's another assigned instance of this Form or not.
		/// </summary>
		public static bool IsOpen {
			get;
			private set;
		}

		/// <summary>
		/// Initializer method for this class.
		/// </summary>
		public AboutDialog() {
			InitializeComponent();
			InitializeStyles();

			appIcon.Image = IconExtensions.ExtractFromAssembly("AppIcon", appIcon.Size).ToBitmap();

			// Set localized strings
			Text = App.LocalizationManager.GetString("About");
			appNameLabel.Text = Application.ProductName;
			versionLabel.Text = String.Join(".", Application.ProductVersion.Split('.').Take(2));
			descriptionLabel.Text = App.LocalizationManager.GetString("AppDescription");
			appLink.Text = App.LocalizationManager.GetString("AppURL");
			appLink.Left = (Width - appLink.Width) / 2;

			toolTip.SetToolTip(versionLabel, Application.ProductVersion.ToString() + Environment.NewLine + ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), true)[0]).Value);
		}

		/// <summary>
		/// Modify control and form styles accordingly.
		/// </summary>
		public void InitializeStyles() {
			Font = SystemFonts.MessageBoxFont;

			if (App.UseClassicTheme) {
				BackColor = SystemColors.Control;
				appNameLabel.Font = new Font(Font, FontStyle.Bold);
				appNameLabel.ForeColor = SystemColors.ControlText;
			} else {
				BackColor = SystemColors.Window;
				appNameLabel.Font = new Font(Font.FontFamily, 12, FontStyle.Regular);
				appNameLabel.ForeColor = Color.FromArgb(0x003399);
			}
		}

		/// <summary>
		/// Processes Windows messages
		/// </summary>
		/// <param name="msg">The message being received</param>
		protected override void WndProc(ref Message msg) {
			switch (msg.Msg) {
				case NativeMethods.WM_THEMECHANGED:
					// Modify control styles to adapt to the new theme
					App.RefreshThemeState();
					InitializeStyles(); 
					break;

				default:
					break;
			}

			base.WndProc(ref msg);
		}

		/// <summary>
		/// Raises the Click event for appLink
		/// </summary>
		/// <param name="sender">The object that triggered the event</param>
		/// <param name="e">Arguments for this event</param>
		private void appLink_Click(object sender, EventArgs e) {
			Process.Start("http://" + appLink.Text);
		}

		/// <summary>
		/// Raises the Form.FormClosed event
		/// </summary>
		/// <param name="e">Arguments for this event</param>
		protected override void OnFormClosed(FormClosedEventArgs e) {
			AboutDialog.IsOpen = false;
			base.OnFormClosed(e);
		}

		/// <summary>
		/// Raises the Form.Shown event
		/// </summary>
		/// <param name="e">Arguments for this event</param>
		protected override void OnShown(EventArgs e) {
			AboutDialog.IsOpen = true;
			base.OnShown(e);
		}
	}
}
