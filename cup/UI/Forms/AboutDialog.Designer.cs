namespace cup {
	partial class AboutDialog {
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
			this.appLink = new cup.LinkLabelEx();
			this.descriptionLabel = new System.Windows.Forms.Label();
			this.versionLabel = new System.Windows.Forms.Label();
			this.appNameLabel = new System.Windows.Forms.Label();
			this.appIcon = new System.Windows.Forms.PictureBox();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.appIcon)).BeginInit();
			this.SuspendLayout();
			// 
			// appLink
			// 
			this.appLink.ForeColor = System.Drawing.SystemColors.HotTrack;
			this.appLink.HoverColor = System.Drawing.Color.Empty;
			this.appLink.Location = new System.Drawing.Point(96, 204);
			this.appLink.Name = "appLink";
			this.appLink.RegularColor = System.Drawing.Color.Empty;
			this.appLink.Size = new System.Drawing.Size(1, 1);
			this.appLink.TabIndex = 9;
			this.appLink.Click += new System.EventHandler(this.appLink_Click);
			// 
			// descriptionLabel
			// 
			this.descriptionLabel.AutoEllipsis = true;
			this.descriptionLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.descriptionLabel.Location = new System.Drawing.Point(12, 155);
			this.descriptionLabel.Name = "descriptionLabel";
			this.descriptionLabel.Size = new System.Drawing.Size(260, 48);
			this.descriptionLabel.TabIndex = 8;
			this.descriptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// versionLabel
			// 
			this.versionLabel.AutoEllipsis = true;
			this.versionLabel.ForeColor = System.Drawing.Color.Gray;
			this.versionLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.versionLabel.Location = new System.Drawing.Point(78, 122);
			this.versionLabel.Name = "versionLabel";
			this.versionLabel.Size = new System.Drawing.Size(128, 32);
			this.versionLabel.TabIndex = 7;
			this.versionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// appNameLabel
			// 
			this.appNameLabel.AutoEllipsis = true;
			this.appNameLabel.Font = new System.Drawing.Font("Segoe UI", 12F);
			this.appNameLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))));
			this.appNameLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.appNameLabel.Location = new System.Drawing.Point(78, 89);
			this.appNameLabel.Name = "appNameLabel";
			this.appNameLabel.Size = new System.Drawing.Size(128, 32);
			this.appNameLabel.TabIndex = 6;
			this.appNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// appIcon
			// 
			this.appIcon.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.appIcon.Location = new System.Drawing.Point(118, 40);
			this.appIcon.MinimumSize = new System.Drawing.Size(48, 48);
			this.appIcon.Name = "appIcon";
			this.appIcon.Size = new System.Drawing.Size(48, 48);
			this.appIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.appIcon.TabIndex = 5;
			this.appIcon.TabStop = false;
			// 
			// toolTip
			// 
			this.toolTip.AutomaticDelay = 100;
			// 
			// AboutDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.ClientSize = new System.Drawing.Size(294, 275);
			this.Controls.Add(this.appLink);
			this.Controls.Add(this.descriptionLabel);
			this.Controls.Add(this.versionLabel);
			this.Controls.Add(this.appNameLabel);
			this.Controls.Add(this.appIcon);
			this.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(300, 300);
			this.Name = "AboutDialog";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.TopMost = true;
			((System.ComponentModel.ISupportInitialize)(this.appIcon)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private LinkLabelEx appLink;
		private System.Windows.Forms.Label descriptionLabel;
		private System.Windows.Forms.Label versionLabel;
		private System.Windows.Forms.Label appNameLabel;
		private System.Windows.Forms.PictureBox appIcon;
		private System.Windows.Forms.ToolTip toolTip;
	}
}