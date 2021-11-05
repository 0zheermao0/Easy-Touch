/*
 * Code by Mark Belles
 * see http://www.codeproject.com/csharp/Screen_Capturing.asp
*/

using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ComponentModel;
using LaraNara.Win32Calls;

namespace LaraNara.PowerPointDump
{
    /// <summary>
    /// Provides methods for capturing windows, and window icons as bitmap images
    /// </summary>
    public class ScreenCapturing
    {
        /// <summary>
        /// Gets a large icon from the window as a Bitmap 
        /// </summary>
        public static Bitmap GetWindowLargeIconAsBitmap(int handle)
        {
            try
            {

                int result;
                IntPtr hWnd = new IntPtr(handle);
                Win32.SendMessageTimeout(hWnd, Win32.WM_GETICON, Win32.ICON_BIG, 0, Win32.SMTO_ABORTIFHUNG, 1000, out result);

                IntPtr hIcon = new IntPtr(result);

                if (hIcon == IntPtr.Zero)
                {
                    result = Win32.GetClassLong(hWnd, Win32.GCL_HICON);
                    hIcon = new IntPtr(result);
                }

                if (hIcon == IntPtr.Zero)
                {
                    Win32.SendMessageTimeout(hWnd, Win32.WM_QUERYDRAGICON, 0, 0, Win32.SMTO_ABORTIFHUNG, 1000, out result);
                    hIcon = new IntPtr(result);
                }

                if (hIcon == IntPtr.Zero)
                    return null;
                else
                    return Bitmap.FromHicon(hIcon);
            }
            catch (System.Exception)
            {
                //				System.Diagnostics.Trace.WriteLine(systemException);
            }
            return null;
        }

        /// <summary>
        /// Gets a small icon from the window as a Bitmap
        /// </summary>
        public static Bitmap GetWindowSmallIconAsBitmap(int handle)
        {
            try
            {
                int result;
                IntPtr hWnd = new IntPtr(handle);
                Win32.SendMessageTimeout(hWnd, Win32.WM_GETICON, Win32.ICON_SMALL, 0, Win32.SMTO_ABORTIFHUNG, 1000, out result);

                IntPtr hIcon = new IntPtr(result);

                if (hIcon == IntPtr.Zero)
                {
                    result = Win32.GetClassLong(hWnd, Win32.GCL_HICONSM);
                    hIcon = new IntPtr(result);
                }

                if (hIcon == IntPtr.Zero)
                {
                    Win32.SendMessageTimeout(hWnd, Win32.WM_QUERYDRAGICON, 0, 0, Win32.SMTO_ABORTIFHUNG, 1000, out result);
                    hIcon = new IntPtr(result);
                }

                if (hIcon == IntPtr.Zero)
                    return null;
                else
                    return Bitmap.FromHicon(hIcon);
            }
            catch (System.Exception)
            {
                //				System.Diagnostics.Trace.WriteLine(systemException);
            }
            return null;
        }

        /// <summary>
        /// Gets a Bitmap of the window (aka. screen capture)
        /// </summary>
        public static Bitmap GetPrimaryDesktopWindowCaptureAsBitmap()
        {
            // create a graphics object from the window handle
            Graphics gfxWindow = Graphics.FromHwnd(IntPtr.Zero);

            // create a bitmap from the visible clipping bounds of the graphics object from the window
            Bitmap bitmap = new Bitmap((int)gfxWindow.VisibleClipBounds.Width, (int)gfxWindow.VisibleClipBounds.Height, gfxWindow);

            // create a graphics object from the bitmap
            Graphics gfxBitmap = Graphics.FromImage(bitmap);

            // get a device context for the window
            IntPtr hdcWindow = gfxWindow.GetHdc();

            // get a device context for the bitmap
            IntPtr hdcBitmap = gfxBitmap.GetHdc();

            // bitblt the window to the bitmap
            Win32.BitBlt(hdcBitmap, 0, 0, bitmap.Width, bitmap.Height, hdcWindow, 0, 0, (int)Win32.TernaryRasterOperations.SRCCOPY);

            // release the bitmap's device context
            gfxBitmap.ReleaseHdc(hdcBitmap);

            // release the window's device context
            gfxWindow.ReleaseHdc(hdcWindow);

            // dispose of the bitmap's graphics object
            gfxBitmap.Dispose();

            // dispose of the window's graphics object
            gfxWindow.Dispose();

            // return the bitmap of the window
            return bitmap;
        }

        public static Bitmap GetDesktopWindowCaptureAsBitmap()
        {
            Rectangle rcScreen = Rectangle.Empty;
            Screen[] screens = Screen.AllScreens;

            // Create a rectangle encompassing all screens...
            foreach (Screen screen in screens)
                rcScreen = Rectangle.Union(rcScreen, screen.Bounds);
            //			System.Diagnostics.Trace.WriteLine(rcScreen);

            // Create a composite bitmap of the size of all screens...
            Bitmap finalBitmap = new Bitmap(rcScreen.Width, rcScreen.Height);

            // Get a graphics object for the composite bitmap and initialize it...
            Graphics g = Graphics.FromImage(finalBitmap);
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            g.FillRectangle(SystemBrushes.Desktop, 0, 0, rcScreen.Width - rcScreen.X, rcScreen.Height - rcScreen.Y);

            // Get an HDC for the composite area...
            IntPtr hdcDestination = g.GetHdc();

            // Now, loop through screens, BitBlting each to the composite HDC created above...
            foreach (Screen screen in screens)
            {
                // Create DC for each source monitor...
                IntPtr hdcSource = Win32.CreateDC(IntPtr.Zero, screen.DeviceName, IntPtr.Zero, IntPtr.Zero);

                // Blt the source directly to the composite destination...
                int xDest = screen.Bounds.X - rcScreen.X;
                int yDest = screen.Bounds.Y - rcScreen.Y;
                //				bool success = BitBlt(hdcDestination, xDest, yDest, screen.Bounds.Width, screen.Bounds.Height, hdcSource, 0, 0, (int)TernaryRasterOperations.SRCCOPY);
                bool success = Win32.StretchBlt(hdcDestination, xDest, yDest, screen.Bounds.Width, screen.Bounds.Height, hdcSource, 0, 0, screen.Bounds.Width, screen.Bounds.Height, (int)Win32.TernaryRasterOperations.SRCCOPY);
                //				System.Diagnostics.Trace.WriteLine(screen.Bounds);
                if (!success)
                {
                    System.ComponentModel.Win32Exception win32Exception = new System.ComponentModel.Win32Exception();
                    System.Diagnostics.Trace.WriteLine(win32Exception);
                }

                // Cleanup source HDC...
                Win32.DeleteDC(hdcSource);
            }

            // Cleanup destination HDC and Graphics...
            g.ReleaseHdc(hdcDestination);
            g.Dispose();

            //			IntPtr hDC = GetDC(IntPtr.Zero);
            //			Graphics gDest = Graphics.FromHdc(hDC);
            //			gDest.DrawImage(finalBitmap, 0, 0, 640, 480);
            //			gDest.Dispose();
            //			ReleaseDC(IntPtr.Zero, hDC);

            // Return composite bitmap which will become our Form's PictureBox's image...
            return finalBitmap;
        }

        /// <summary>
        /// Gets a Bitmap of the window (aka. screen capture)
        /// </summary>
        public static Bitmap GetWindowCaptureAsBitmap(int handle)
        {
            IntPtr hWnd = new IntPtr(handle);
            Win32.Rect rc = new Win32.Rect();
            if (!Win32.GetWindowRect(hWnd, ref rc))
                return null;

            Win32.WindowInfo wi = new Win32.WindowInfo();
            wi.size = Marshal.SizeOf(wi);

            if (!Win32.GetWindowInfo(hWnd, ref wi))
                return null;

            // create a bitmap from the visible clipping bounds of the graphics object from the window
            Bitmap bitmap = new Bitmap(rc.Width, rc.Height);

            // create a graphics object from the bitmap
            Graphics gfxBitmap = Graphics.FromImage(bitmap);

            // get a device context for the bitmap
            IntPtr hdcBitmap = gfxBitmap.GetHdc();

            // get a device context for the window
            IntPtr hdcWindow = Win32.GetWindowDC(hWnd);

            // bitblt the window to the bitmap
            Win32.BitBlt(hdcBitmap, 0, 0, rc.Width, rc.Height, hdcWindow, 0, 0, (int)Win32.TernaryRasterOperations.SRCCOPY);

            // release the bitmap's device context
            gfxBitmap.ReleaseHdc(hdcBitmap);

            Win32.ReleaseDC(hWnd, hdcWindow);

            // dispose of the bitmap's graphics object
            gfxBitmap.Dispose();

            // return the bitmap of the window
            return bitmap;
        }

        /// <summary>
        /// Convert a bitmap to a byte array
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static byte[] GetBytes(Bitmap bmp)
        {
            if (bmp == null)
                return new byte[0];

            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            // maybe 
            // bmp.Dispose();

            return stream.GetBuffer();
        }

        public static byte[] GetBytes(Icon icon)
        {
            if (icon == null)
                return new byte[0];

            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            icon.Save(stream);
            return stream.GetBuffer();
        }

        public static Icon GetIcon(byte[] bytes)
        {
            if (bytes == null)
                return null;

            if (bytes.Length == 0)
                return null;

            System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);
            Icon icon = new Icon(stream);
            stream.Close();
            try
            {
                return (Icon)icon.Clone();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                icon.Dispose();
            }
            return null;
        }

        /// <summary>
        /// Converts a byte array to a bitmap
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Bitmap GetBitmap(byte[] bytes)
        {
            if (bytes == null)
                return null;

            if (bytes.Length == 0)
                return null;

            System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);
            Bitmap b = new Bitmap(stream);
            stream.Close();
            try
            {
                return (Bitmap)b.Clone();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                b.Dispose();
            }
            return null;
        }

        /// <summary>
        /// Gets a byte array of a bitmap for the large icon for the window specified by the handle 
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static byte[] GetWindowLargeIconAsByteArray(int handle)
        {
            return ScreenCapturing.GetBytes(ScreenCapturing.GetWindowLargeIconAsBitmap(handle));
        }

        /// <summary>
        /// Gets a byte array of a bitmap for the small icon for the window specified by the handle 
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static byte[] GetWindowSmallIconAsByteArray(int handle)
        {
            return ScreenCapturing.GetBytes(ScreenCapturing.GetWindowSmallIconAsBitmap(handle));
        }

        /// <summary>
        /// Gets a byte array of a bitmap for the desktop window
        /// </summary>
        /// <returns></returns>
        public static byte[] GetDesktopWindowCaptureAsByteArray()
        {
            return ScreenCapturing.GetBytes(ScreenCapturing.GetDesktopWindowCaptureAsBitmap());
        }

        /// <summary>
        /// Gets a byte array of a bitmap for the window specified by the handle 
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        public static byte[] GetWindowCaptureAsByteArray(int handle)
        {
            return ScreenCapturing.GetBytes(ScreenCapturing.GetWindowCaptureAsBitmap(handle));
        }

        /// <summary>
        /// Calculates the total screen size, for all attached monitors, by unioning each monitor's bounds together
        /// </summary>
        /// <returns></returns>
        public static Rectangle CalculateTotalScreenSize()
        {
            Rectangle rcScreen = Rectangle.Empty;
            Screen[] screens = Screen.AllScreens;

            // Create a rectangle encompassing all screens...
            foreach (Screen screen in screens)
                rcScreen = Rectangle.Union(rcScreen, screen.Bounds);

            return rcScreen;
        }
    }

}
