using MenthaAssembly.Devices;
using System;
using System.Windows;
using System.Windows.Interop;

namespace MenthaAssembly
{
    public static class WindowHelper
    {
        private struct MinMaxInfo
        {
            public Int32Point ptReserved;
            public Int32Size ptMaxSize;
            public Int32Point ptMaxPosition;
            public Int32Size ptMinTrackSize;
            public Int32Size ptMaxTrackSize;
        };

        public static void WindowFix(this Window window)
        {
            IntPtr mWindowHandle = new WindowInteropHelper(window).Handle;
            HwndSource.FromHwnd(mWindowHandle).AddHook(new HwndSourceHook(WindowProc));
        }

        private unsafe static IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    // Fix Window Size
                    if (Screen.Current is ScreenInfo Info)
                    {
                        MinMaxInfo* pInfo = (MinMaxInfo*)lParam;
                        pInfo->ptMaxPosition = new Int32Point(Info.WorkArea.Left - Info.Bound.Left,
                                                              Info.WorkArea.Left - Info.Bound.Left);

                        pInfo->ptMaxSize = new Int32Size(Info.WorkArea.Right - Info.WorkArea.Left,
                                                         Info.WorkArea.Bottom - Info.WorkArea.Top);

                        //Position relative pour notre fenêtre
                        //MinMaxInfo lMmi = Marshal.PtrToStructure<MinMaxInfo>(lParam);
                        //lMmi.ptMaxPosition = new Int32Point(Info.WorkArea.Left - Info.Bound.Left,
                        //                                    Info.WorkArea.Left - Info.Bound.Left);
                        //lMmi.ptMaxSize = new Int32Size(Info.WorkArea.Right - Info.WorkArea.Left,
                        //                               Info.WorkArea.Bottom - Info.WorkArea.Top);

                        //Marshal.StructureToPtr(lMmi, lParam, true);
                    }
                    break;
            }

            return IntPtr.Zero;
        }

    }
}
