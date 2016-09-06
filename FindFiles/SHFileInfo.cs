using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Interop;
using System.IO;

namespace FindFiles
{
	public class SHFileInfo
	{
		public static BitmapImage GetBitmap(string filePath)
		{
			SHFILEINFO shinfo = new SHFILEINFO();

			IntPtr hImg = SHGetFileInfo(
				filePath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo),
				SHGFI_ICON | SHGFI_LARGEICON);// Win32.SHGFI_SMALLICON);

			Icon appIcon = Icon.FromHandle(shinfo.hIcon);
			Bitmap bitmap = appIcon.ToBitmap();
			//Bitmap bitmap = System.Drawing.Icon.FromHandle(hImg).ToBitmap();
			IntPtr hBitmap = bitmap.GetHbitmap();

			var bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
				hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

			PngBitmapEncoder encoder = new PngBitmapEncoder();
			MemoryStream memoryStream = new MemoryStream();
			BitmapImage bImg = new BitmapImage();

			encoder.Frames.Add(BitmapFrame.Create(bs));
			encoder.Save(memoryStream);

			bImg.BeginInit();
			bImg.StreamSource = new MemoryStream(memoryStream.ToArray());
			bImg.EndInit();

			memoryStream.Close();

			return bImg;
		}

		public const uint SHGFI_ICON = 0x100;
		public const uint SHGFI_LARGEICON = 0x0; // 'Large icon
		public const uint SHGFI_SMALLICON = 0x1; // 'Small icon

		[DllImport("shell32.dll")]
		public static extern IntPtr SHGetFileInfo(
			string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

		[StructLayout(LayoutKind.Sequential)]
		public struct SHFILEINFO
		{
			public IntPtr hIcon;
			public IntPtr iIcon;
			public uint dwAttributes;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szDisplayName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		};
	}
}
