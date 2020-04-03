namespace cup {
	partial class PreferencesDialog {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
      this.components = new System.ComponentModel.Container();
      this.mainInstruction = new System.Windows.Forms.Label();
      this.screenshotLabel = new System.Windows.Forms.Label();
      this.actionComboBox = new System.Windows.Forms.ComboBox();
      this.actionLabel = new System.Windows.Forms.Label();
      this.saveLocallyCheckBox = new System.Windows.Forms.CheckBox();
      this.screenshotFormatLabel = new System.Windows.Forms.Label();
      this.screenshotFormatComboBox = new System.Windows.Forms.ComboBox();
      this.autostartCheckBox = new System.Windows.Forms.CheckBox();
      this.advancedSettingsPanel = new System.Windows.Forms.Panel();
      this.legacyNotificationsCheckBox = new System.Windows.Forms.CheckBox();
      this.openActionsDirectory = new cup.LinkLabelEx();
      this.screenshotHotKeyTextBox = new System.Windows.Forms.TextBox();
      this.changeDirectoryLink = new cup.LinkLabelEx();
      this.advancedSettingsChevron = new cup.Chevron();
      this.shortcutWarningToolTip = new System.Windows.Forms.ToolTip(this.components);
      this.advancedSettingsPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // mainInstruction
      // 
      this.mainInstruction.AutoSize = true;
      this.mainInstruction.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
      this.mainInstruction.Location = new System.Drawing.Point(10, 9);
      this.mainInstruction.Name = "mainInstruction";
      this.mainInstruction.Size = new System.Drawing.Size(64, 13);
      this.mainInstruction.TabIndex = 0;
      this.mainInstruction.Text = "Preferences";
      // 
      // screenshotLabel
      // 
      this.screenshotLabel.Location = new System.Drawing.Point(12, 66);
      this.screenshotLabel.Name = "screenshotLabel";
      this.screenshotLabel.Size = new System.Drawing.Size(113, 23);
      this.screenshotLabel.TabIndex = 1;
      this.screenshotLabel.Text = "Screenshot:";
      this.screenshotLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // actionComboBox
      // 
      this.actionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.actionComboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.actionComboBox.FormattingEnabled = true;
      this.actionComboBox.Location = new System.Drawing.Point(134, 41);
      this.actionComboBox.Name = "actionComboBox";
      this.actionComboBox.Size = new System.Drawing.Size(196, 21);
      this.actionComboBox.TabIndex = 7;
      this.actionComboBox.SelectedIndexChanged += new System.EventHandler(this.actionComboBox_SelectedIndexChanged);
      this.actionComboBox.Format += new System.Windows.Forms.ListControlConvertEventHandler(this.actionComboBox_Format);
      // 
      // actionLabel
      // 
      this.actionLabel.Location = new System.Drawing.Point(12, 41);
      this.actionLabel.Name = "actionLabel";
      this.actionLabel.Size = new System.Drawing.Size(113, 23);
      this.actionLabel.TabIndex = 8;
      this.actionLabel.Text = "Action:";
      this.actionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // saveLocallyCheckBox
      // 
      this.saveLocallyCheckBox.AutoSize = true;
      this.saveLocallyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.saveLocallyCheckBox.Location = new System.Drawing.Point(12, 106);
      this.saveLocallyCheckBox.Name = "saveLocallyCheckBox";
      this.saveLocallyCheckBox.Size = new System.Drawing.Size(211, 18);
      this.saveLocallyCheckBox.TabIndex = 9;
      this.saveLocallyCheckBox.Text = "Keep a local copy of each screenshot";
      this.saveLocallyCheckBox.UseVisualStyleBackColor = true;
      this.saveLocallyCheckBox.CheckedChanged += new System.EventHandler(this.saveLocallyCheckBox_CheckedChanged);
      // 
      // screenshotFormatLabel
      // 
      this.screenshotFormatLabel.Location = new System.Drawing.Point(12, 12);
      this.screenshotFormatLabel.Name = "screenshotFormatLabel";
      this.screenshotFormatLabel.Size = new System.Drawing.Size(113, 23);
      this.screenshotFormatLabel.TabIndex = 11;
      this.screenshotFormatLabel.Text = "Screenshot format:";
      this.screenshotFormatLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // screenshotFormatComboBox
      // 
      this.screenshotFormatComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.screenshotFormatComboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.screenshotFormatComboBox.FormattingEnabled = true;
      this.screenshotFormatComboBox.Location = new System.Drawing.Point(134, 12);
      this.screenshotFormatComboBox.Name = "screenshotFormatComboBox";
      this.screenshotFormatComboBox.Size = new System.Drawing.Size(196, 21);
      this.screenshotFormatComboBox.TabIndex = 10;
      this.screenshotFormatComboBox.SelectedIndexChanged += new System.EventHandler(this.screenshotFormatComboBox_SelectedIndexChanged);
      this.screenshotFormatComboBox.Format += new System.Windows.Forms.ListControlConvertEventHandler(this.screenshotFormatComboBox_Format);
      // 
      // autostartCheckBox
      // 
      this.autostartCheckBox.AutoSize = true;
      this.autostartCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.autostartCheckBox.Location = new System.Drawing.Point(12, 132);
      this.autostartCheckBox.Name = "autostartCheckBox";
      this.autostartCheckBox.Size = new System.Drawing.Size(194, 18);
      this.autostartCheckBox.TabIndex = 12;
      this.autostartCheckBox.Text = "Start the application when I sign in";
      this.autostartCheckBox.UseVisualStyleBackColor = true;
      this.autostartCheckBox.CheckedChanged += new System.EventHandler(this.autostartCheckBox_CheckedChanged);
      // 
      // advancedSettingsPanel
      // 
      this.advancedSettingsPanel.Controls.Add(this.legacyNotificationsCheckBox);
      this.advancedSettingsPanel.Controls.Add(this.openActionsDirectory);
      this.advancedSettingsPanel.Controls.Add(this.screenshotFormatLabel);
      this.advancedSettingsPanel.Controls.Add(this.screenshotFormatComboBox);
      this.advancedSettingsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.advancedSettingsPanel.Location = new System.Drawing.Point(0, 239);
      this.advancedSettingsPanel.Name = "advancedSettingsPanel";
      this.advancedSettingsPanel.Size = new System.Drawing.Size(344, 98);
      this.advancedSettingsPanel.TabIndex = 14;
      // 
      // legacyNotificationsCheckBox
      // 
      this.legacyNotificationsCheckBox.AutoSize = true;
      this.legacyNotificationsCheckBox.Enabled = false;
      this.legacyNotificationsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.legacyNotificationsCheckBox.Location = new System.Drawing.Point(12, 44);
      this.legacyNotificationsCheckBox.Name = "legacyNotificationsCheckBox";
      this.legacyNotificationsCheckBox.Size = new System.Drawing.Size(152, 18);
      this.legacyNotificationsCheckBox.TabIndex = 19;
      this.legacyNotificationsCheckBox.Text = "Show legacy notifications";
      this.legacyNotificationsCheckBox.UseVisualStyleBackColor = true;
      this.legacyNotificationsCheckBox.CheckedChanged += new System.EventHandler(this.legacyNotificationsCheckBox_CheckedChanged);
      // 
      // openActionsDirectory
      // 
      this.openActionsDirectory.ForeColor = System.Drawing.SystemColors.HotTrack;
      this.openActionsDirectory.HoverColor = System.Drawing.Color.Empty;
      this.openActionsDirectory.Location = new System.Drawing.Point(12, 70);
      this.openActionsDirectory.Name = "openActionsDirectory";
      this.openActionsDirectory.RegularColor = System.Drawing.Color.Empty;
      this.openActionsDirectory.Size = new System.Drawing.Size(123, 14);
      this.openActionsDirectory.TabIndex = 18;
      this.openActionsDirectory.Text = "Open actions directory...";
      this.openActionsDirectory.Click += new System.EventHandler(this.openActionsDirectory_Click);
      // 
      // screenshotHotKeyTextBox
      // 
      this.screenshotHotKeyTextBox.BackColor = System.Drawing.SystemColors.Window;
      this.screenshotHotKeyTextBox.Location = new System.Drawing.Point(134, 68);
      this.screenshotHotKeyTextBox.Name = "screenshotHotKeyTextBox";
      this.screenshotHotKeyTextBox.ReadOnly = true;
      this.screenshotHotKeyTextBox.Size = new System.Drawing.Size(196, 20);
      this.screenshotHotKeyTextBox.TabIndex = 15;
      this.screenshotHotKeyTextBox.Enter += new System.EventHandler(this.screenshotHotKeyTextBox_Enter);
      this.screenshotHotKeyTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.screenshotHotKeyTextBox_KeyDown);
      this.screenshotHotKeyTextBox.Leave += new System.EventHandler(this.screenshotHotKeyTextBox_Leave);
      // 
      // changeDirectoryLink
      // 
      this.changeDirectoryLink.ForeColor = System.Drawing.SystemColors.HotTrack;
      this.changeDirectoryLink.HoverColor = System.Drawing.Color.Empty;
      this.changeDirectoryLink.Location = new System.Drawing.Point(274, 106);
      this.changeDirectoryLink.Name = "changeDirectoryLink";
      this.changeDirectoryLink.RegularColor = System.Drawing.Color.Empty;
      this.changeDirectoryLink.Size = new System.Drawing.Size(54, 14);
      this.changeDirectoryLink.TabIndex = 17;
      this.changeDirectoryLink.Text = "Change...";
      this.changeDirectoryLink.Click += new System.EventHandler(this.changeDirectoryLink_Click);
      // 
      // advancedSettingsChevron
      // 
      this.advancedSettingsChevron.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.advancedSettingsChevron.Expanded = false;
      this.advancedSettingsChevron.Location = new System.Drawing.Point(0, 211);
      this.advancedSettingsChevron.Margin = new System.Windows.Forms.Padding(0);
      this.advancedSettingsChevron.Name = "advancedSettingsChevron";
      this.advancedSettingsChevron.Size = new System.Drawing.Size(344, 28);
      this.advancedSettingsChevron.TabIndex = 13;
      this.advancedSettingsChevron.Text = "Advanced settings";
      this.advancedSettingsChevron.Expand += new System.EventHandler(this.advancedSettingsChevron_Expand);
      this.advancedSettingsChevron.Collapse += new System.EventHandler(this.advancedSettingsChevron_Collapse);
      // 
      // shortcutWarningToolTip
      // 
      this.shortcutWarningToolTip.AutomaticDelay = 0;
      this.shortcutWarningToolTip.IsBalloon = true;
      this.shortcutWarningToolTip.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Warning;
      this.shortcutWarningToolTip.UseAnimation = false;
      this.shortcutWarningToolTip.UseFading = false;
      // 
      // PreferencesDialog
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
      this.BackColor = System.Drawing.Color.White;
      this.ClientSize = new System.Drawing.Size(344, 337);
      this.Controls.Add(this.advancedSettingsChevron);
      this.Controls.Add(this.advancedSettingsPanel);
      this.Controls.Add(this.changeDirectoryLink);
      this.Controls.Add(this.screenshotHotKeyTextBox);
      this.Controls.Add(this.autostartCheckBox);
      this.Controls.Add(this.saveLocallyCheckBox);
      this.Controls.Add(this.actionLabel);
      this.Controls.Add(this.actionComboBox);
      this.Controls.Add(this.screenshotLabel);
      this.Controls.Add(this.mainInstruction);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.KeyPreview = true;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "PreferencesDialog";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "cup";
      this.TopMost = true;
      this.advancedSettingsPanel.ResumeLayout(false);
      this.advancedSettingsPanel.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label mainInstruction;
		private System.Windows.Forms.Label screenshotLabel;
		private System.Windows.Forms.ComboBox actionComboBox;
		private System.Windows.Forms.Label actionLabel;
		private System.Windows.Forms.CheckBox saveLocallyCheckBox;
		private System.Windows.Forms.Label screenshotFormatLabel;
		private System.Windows.Forms.ComboBox screenshotFormatComboBox;
		private System.Windows.Forms.CheckBox autostartCheckBox;
		private Chevron advancedSettingsChevron;
		private System.Windows.Forms.Panel advancedSettingsPanel;
		private System.Windows.Forms.TextBox screenshotHotKeyTextBox;
		private LinkLabelEx changeDirectoryLink;
		private LinkLabelEx openActionsDirectory;
		private System.Windows.Forms.CheckBox legacyNotificationsCheckBox;
		private System.Windows.Forms.ToolTip shortcutWarningToolTip;
	}
}