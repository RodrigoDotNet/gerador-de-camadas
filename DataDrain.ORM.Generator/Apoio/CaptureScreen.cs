using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using DataDrain.ORM.Generator.Apoio.API;

namespace DataDrain.ORM.Generator.Apoio
{
    public class CaptureScreen
    {

        public struct SIZE
        {
            public int cx;
            public int cy;
        }

        /// <summary>
        /// Recupera a imagem da aplicação atual
        /// </summary>
        /// <param name="procName"></param>
        /// <returns></returns>
        public static Bitmap CaptureApplication(string procName)
        {
            try
            {
                var proc = Process.GetProcessesByName(procName)[0];
                var rect = new User32.Rect();
                User32.GetWindowRect(proc.MainWindowHandle, ref rect);

                var width = rect.right - rect.left;
                var height = rect.bottom - rect.top;

                var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                var graphics = Graphics.FromImage(bmp);
                graphics.CopyFromScreen(rect.left, rect.top, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);

                return bmp;
            }
            catch (Exception)
            {
                return new Bitmap(0, 0);
            }
        }

        /// <summary>
        /// Retorna a imagem do Desktop
        /// </summary>
        /// <returns></returns>
        public static Bitmap CaptureDesktop()
        {
            SIZE size;
            var hDC = Win32Stuff.GetDC(Win32Stuff.GetDesktopWindow());
            var hMemDC = GDIStuff.CreateCompatibleDC(hDC);

            size.cx = Win32Stuff.GetSystemMetrics(Win32Stuff.SM_CXSCREEN);

            size.cy = Win32Stuff.GetSystemMetrics(Win32Stuff.SM_CYSCREEN);

            var hBitmap = GDIStuff.CreateCompatibleBitmap(hDC, size.cx, size.cy);

            if (hBitmap != IntPtr.Zero)
            {
                var hOld = GDIStuff.SelectObject(hMemDC, hBitmap);

                GDIStuff.BitBlt(hMemDC, 0, 0, size.cx, size.cy, hDC, 0, 0, GDIStuff.SRCCOPY);

                GDIStuff.SelectObject(hMemDC, hOld);
                GDIStuff.DeleteDC(hMemDC);
                Win32Stuff.ReleaseDC(Win32Stuff.GetDesktopWindow(), hDC);
                var bmp = System.Drawing.Image.FromHbitmap(hBitmap);
                GDIStuff.DeleteObject(hBitmap);
                GC.Collect();
                return bmp;
            }
            return null;

        }

        public static Image CaptureDesktopWithCursor()
        {
            var cursorX = 0;
            var cursorY = 0;

            Image desktopBmp = CaptureDesktop();
            Image cursorBmp = CaptureCursor(ref cursorX, ref cursorY);
            if (desktopBmp != null)
            {
                if (cursorBmp != null)
                {
                    var r = new Rectangle(cursorX, cursorY, cursorBmp.Width, cursorBmp.Height);
                    var g = Graphics.FromImage(desktopBmp);
                    g.DrawImage(cursorBmp, r);
                    g.Flush();

                    return desktopBmp;
                }
                return desktopBmp;
            }
            return null;
        }

        static Bitmap CaptureCursor(ref int x, ref int y)
        {
            var ci = new Win32Stuff.CURSORINFO();
            ci.cbSize = Marshal.SizeOf(ci);
            if (Win32Stuff.GetCursorInfo(out ci))
            {
                if (ci.flags == Win32Stuff.CURSOR_SHOWING)
                {
                    var hicon = Win32Stuff.CopyIcon(ci.hCursor);
                    Win32Stuff.ICONINFO icInfo;
                    if (Win32Stuff.GetIconInfo(hicon, out icInfo))
                    {
                        x = ci.ptScreenPos.x - icInfo.xHotspot;
                        y = ci.ptScreenPos.y - icInfo.yHotspot;

                        var ic = Icon.FromHandle(hicon);
                        var bmp = ic.ToBitmap();
                        return bmp;
                    }
                }
            }

            return null;
        }
    }

    internal class User32
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);
    }

}
