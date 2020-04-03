using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;

namespace cup {
	public static class ImageFormatExtensions {
		public static string GetExtension(this ImageFormat imageFormat) {
			return (from encoder in ImageCodecInfo.GetImageEncoders()
						  where encoder.FormatID == imageFormat.Guid
						  select encoder.FilenameExtension).First().Split(';')[0].Trim('*').ToLower();
		}
  }
}
