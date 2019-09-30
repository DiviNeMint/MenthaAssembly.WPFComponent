using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MenthaAssembly
{
    public static class User32
    {
        [DllImport("user32")]
        public static extern bool GetCursorPos(out Point lpPoint);

        [DllImport("user32", SetLastError = true)]
        public static extern IntPtr MonitorFromPoint(Point pt, MonitorOptions dwFlags);

        [DllImport("user32")]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, MonitorInfo lpmi);


        [DllImport("user32")]
        public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);



        #region Enums    
        public enum MonitorOptions : uint
        {
            Monitor_DefaultToNull = 0,
            Monitor_DefaultToPrimary = 1,
            Monitor_DefaultToNearest = 2
        }

        public enum WindowCompositionAttribute
        {
            WCA_Accent_Policy = 19,
        }

        public enum AccentState
        {
            Accent_Disabled,
            Accent_Enable_Gradient,
            Accent_Enable_TransparentGradient,
            Accent_Enable_BlurBehind,
            Accent_Invalid_State,
        }
        #endregion

        #region Structs
        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            public int X { set; get; }

            public int Y { set; get; }

            public Point(int X, int Y)
            {
                this.X = X;
                this.Y = Y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left { set; get; }

            public int Top { set; get; }

            public int Right { set; get; }

            public int Bottom { set; get; }

            public Rect(int Left, int Top, int Right, int Bottom)
            {
                this.Left = Left;
                this.Top = Top;
                this.Right = Right;
                this.Bottom = Bottom;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class MonitorInfo
        {
            public int cbSize = Marshal.SizeOf(typeof(MonitorInfo));
            public Rect rcMonitor = new Rect();
            public Rect rcWork = new Rect();
            public int dwFlags = 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute { set; get; }
            public IntPtr Data { set; get; }
            public int SizeOfData { set; get; }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AccentPolicy
        {
            public AccentState AccentState { set; get; }
            public int AccentFlags { set; get; }
            public int GradientColor { set; get; }
            public int AnimationId { set; get; }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MinMaxInfo
        {
            public Point ptReserved;
            public Point ptMaxSize;
            public Point ptMaxPosition;
            public Point ptMinTrackSize;
            public Point ptMaxTrackSize;
        };

        #endregion

    }
}
