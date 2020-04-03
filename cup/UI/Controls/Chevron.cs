using System;
using System.Drawing;
using System.Windows.Forms;

namespace cup {
	public class Chevron : Control {
		private Bitmap normalCollapsedBitmap = new Bitmap(19, 19);
		private Bitmap hoverCollapsedBitmap = new Bitmap(19, 19);
		private Bitmap downCollapsedBitmap = new Bitmap(19, 19);

		private Bitmap normalExpandedBitmap = new Bitmap(19, 19);
		private Bitmap hoverExpandedBitmap = new Bitmap(19, 19);
		private Bitmap downExpandedBitmap = new Bitmap(19, 19);

		private int state = 0;
		private bool expanded = false;

		public bool Expanded {
			get {
				return expanded;
			}

			set {
				expanded = value;
				Invalidate();
			}
		}

		public override string Text {
			get {
				return base.Text;
			}

			set {
				base.Text = value;
				Invalidate();
			}
		}

		public Chevron() {
			int column = 0;

			if (Environment.OSVersion.Version.Major >= 6)
				column++;

			if (Environment.OSVersion.Version >= new Version(6, 2))
				column++;

			using (Graphics graphics = Graphics.FromImage(normalExpandedBitmap))
				graphics.DrawImage(Properties.Resources.ChevronStrip, new Rectangle(0, 0, 19, 19), new Rectangle(19 * column, 0, 19, 19), GraphicsUnit.Pixel);

			using (Graphics graphics = Graphics.FromImage(hoverExpandedBitmap))
				graphics.DrawImage(Properties.Resources.ChevronStrip, new Rectangle(0, 0, 19, 19), new Rectangle(19 * column, 19, 19, 19), GraphicsUnit.Pixel);

			using (Graphics graphics = Graphics.FromImage(downExpandedBitmap))
				graphics.DrawImage(Properties.Resources.ChevronStrip, new Rectangle(0, 0, 19, 19), new Rectangle(19 * column, 38, 19, 19), GraphicsUnit.Pixel);

			using (Graphics graphics = Graphics.FromImage(normalCollapsedBitmap))
				graphics.DrawImage(Properties.Resources.ChevronStrip, new Rectangle(0, 0, 19, 19), new Rectangle(19 * column, 57, 19, 19), GraphicsUnit.Pixel);

			using (Graphics graphics = Graphics.FromImage(hoverCollapsedBitmap))
				graphics.DrawImage(Properties.Resources.ChevronStrip, new Rectangle(0, 0, 19, 19), new Rectangle(19 * column, 76, 19, 19), GraphicsUnit.Pixel);

			using (Graphics graphics = Graphics.FromImage(downCollapsedBitmap))
				graphics.DrawImage(Properties.Resources.ChevronStrip, new Rectangle(0, 0, 19, 19), new Rectangle(19 * column, 95, 19, 19), GraphicsUnit.Pixel);

			SetStyle(ControlStyles.AllPaintingInWmPaint
					| ControlStyles.SupportsTransparentBackColor
					| ControlStyles.OptimizedDoubleBuffer
					| ControlStyles.ResizeRedraw
					| ControlStyles.UserPaint
					| ControlStyles.FixedHeight
					| ControlStyles.FixedWidth, true);

			SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, false);
		}

		protected override void OnMouseLeave(EventArgs e) {
			state = 0;
			Invalidate();

			base.OnMouseLeave(e);
		}

		protected override void OnMouseMove(MouseEventArgs mEventArgs) {
			state = 1;
			Invalidate();

			base.OnMouseMove(mEventArgs);
		}

		protected override void OnMouseDown(MouseEventArgs mEventArgs) {
			state = 2;
			Invalidate();

			base.OnMouseDown(mEventArgs);
		}

		protected override void OnMouseUp(MouseEventArgs mEventArgs) {
			state = 0;
			Expanded = !Expanded;

			if (Expanded)
				OnExpand(this, (EventArgs)mEventArgs);
			else
				OnCollapse(this, (EventArgs)mEventArgs);

			base.OnMouseUp(mEventArgs);
		}

		protected override void OnPaint(PaintEventArgs e) {
			if (!Expanded && state == 0)
				e.Graphics.DrawImage(normalCollapsedBitmap, 6, 3);
			else if (!Expanded && state == 1)
				e.Graphics.DrawImage(hoverCollapsedBitmap, 6, 3);
			else if (!Expanded && state == 2)
				e.Graphics.DrawImage(downCollapsedBitmap, 6, 3);
			else if (state == 0)
				e.Graphics.DrawImage(normalExpandedBitmap, 6, 3);
			else if (state == 1)
				e.Graphics.DrawImage(hoverExpandedBitmap, 6, 3);
			else if (state == 2)
				e.Graphics.DrawImage(downExpandedBitmap, 6, 3);

			TextRenderer.DrawText(e.Graphics, Text, SystemFonts.MessageBoxFont, new Point(27, 5), SystemColors.ControlText, Color.Transparent, TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

			base.OnPaint(e);
		}

		protected override void WndProc(ref Message message) {
			if (message.Msg == NativeMethods.WM_SETCURSOR) {
				NativeMethods.SetCursor(NativeMethods.LoadCursor(IntPtr.Zero, new IntPtr(NativeMethods.IDC_HAND)));
				message.Result = IntPtr.Zero;

				return;
			}

			base.WndProc(ref message);
		}

		public event EventHandler Expand;
		public event EventHandler Collapse;

		private void OnExpand(object sender, EventArgs e) {
			Expand?.Invoke(sender, e);
		}

		private void OnCollapse(object sender, EventArgs e) {
			Collapse?.Invoke(sender, e);
		}
	}
}
