using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace DolphinDBForExcelWPFLib
{
   static class Util
    {
        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter,
                   int x, int y, int width, int height, uint flags);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hwnd, uint msg,
                   IntPtr wParam, IntPtr lParam);

        const int GWL_EXSTYLE = -20;
        const int WS_EX_DLGMODALFRAME = 0x0001;
        const int SWP_NOSIZE = 0x0001;
        const int SWP_NOMOVE = 0x0002;
        const int SWP_NOZORDER = 0x0004;
        const int SWP_FRAMECHANGED = 0x0020;
        const uint WM_SETICON = 0x0080;

        public static void RemoveWindowIcon(Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;

            SendMessage(hwnd, WM_SETICON, new IntPtr(1), IntPtr.Zero);
            SendMessage(hwnd, WM_SETICON, IntPtr.Zero, IntPtr.Zero);

            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_DLGMODALFRAME);

            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE |
                  SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }

        public static bool ParseServerStr(string s,out string host,out int port)
        {
            host = "";
            port = -1;

            string[] se = s.Split(':');
            if (se.Length != 2)
                return false;

            if (string.IsNullOrEmpty(se[0]))
                return false;

            if (!int.TryParse(se[1], out port))
                return false;

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                return false;

            host = se[0];
            return true;
        }

        public static string ConvTimeSpanToString(TimeSpan span)
        {
            string s = null;
            if (span == null)
                return null;

            int ms;
            int seconds;
            int minutes;
            int hours;
            int days;

            ms = Convert.ToInt32(span.TotalMilliseconds);

            s = (ms % 1000).ToString() + "ms";
            if ((seconds = ms / 1000) == 0)
                return s;
            s = (seconds % 60).ToString() + "s " + s;

            if ((minutes = seconds / 60) == 0)
                return s;
            s = (minutes % 60).ToString() + "m " + s;

            if ((hours = minutes / 60) == 0)
                return s;
            s = (hours % 60).ToString() + "h " + s;

            if ((days = hours / 24) == 0)
                return s;
            s = (days % 60).ToString() + "d " + s;
            return s;
        }

        public static void ShowErrorMessageBox(string msg)
        {
            MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

}
