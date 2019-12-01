using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using static MenthaAssembly.User32;

namespace MenthaAssembly
{
    public static class WindowHelper
    {
        public static void WindowFix(this Window window)
        {
            IntPtr mWindowHandle = new WindowInteropHelper(window).Handle;
            HwndSource.FromHwnd(mWindowHandle).AddHook(new HwndSourceHook(WindowProc));
        }

        private static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WmGetMinMaxInfo(hwnd, lParam);
                    break;
            }

            return IntPtr.Zero;
        }


        private static void WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            GetCursorPos(out User32.Point lMousePosition);
            IntPtr lCurrentScreen = MonitorFromPoint(lMousePosition, MonitorOptions.Monitor_DefaultToNearest);


            MonitorInfo lCurrentScreenInfo = new MonitorInfo();
            if (GetMonitorInfo(lCurrentScreen, lCurrentScreenInfo) == false)
                return;

            //Position relative pour notre fenêtre
            MinMaxInfo lMmi = Marshal.PtrToStructure<MinMaxInfo>(lParam);
            lMmi.ptMaxPosition.X = lCurrentScreenInfo.rcWork.Left - lCurrentScreenInfo.rcMonitor.Left;
            lMmi.ptMaxPosition.Y = lCurrentScreenInfo.rcWork.Top - lCurrentScreenInfo.rcMonitor.Top;
            lMmi.ptMaxSize.X = lCurrentScreenInfo.rcWork.Right - lCurrentScreenInfo.rcWork.Left;
            lMmi.ptMaxSize.Y = lCurrentScreenInfo.rcWork.Bottom - lCurrentScreenInfo.rcWork.Top;

            Marshal.StructureToPtr(lMmi, lParam, true);
        }

    }
}
