#if !NET5_0_OR_GREATER
using System.Security.Permissions;
#endif
using MenthaAssembly.Win32;
using Microsoft.Win32.SafeHandles;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using static MenthaAssembly.Win32.Graphic;
using static MenthaAssembly.Win32.System;

namespace MenthaAssembly.Devices
{
    public static class CursorHelper
    {
        #region Windows API
#if !NET5_0_OR_GREATER
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
#endif
        private class SafeIconHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public SafeIconHandle() : base(true) { }
            public SafeIconHandle(IntPtr Handle) : base(true)
            {
                base.SetHandle(Handle);
            }

            protected override bool ReleaseHandle()
                => DestroyIcon(handle);
        }

        #endregion

        public static CursorResource Resource
            => CursorResource.Instance;

        public static CursorInfo EyedropperCursor
            => CreateCursor(Resource.Eyedropper, 22, 22, 1, 21, 0);

        public static CursorInfo GrabHandCursor
            => CreateCursor(Resource.GrabHand, 20, 20, 9, 10, 0);

        public static CursorInfo RotateCursor
            => CreateCursor(Resource.RotateArrow, 26, 26, 13, 13, 0);

        public static CursorInfo Rotate45Cursor
            => CreateCursor(Resource.RotateArrow, 26, 26, 13, 13, 45d);

        public static CursorInfo Rotate90Cursor
            => CreateCursor(Resource.RotateArrow, 26, 26, 13, 13, 90d);

        public static CursorInfo Rotate135Cursor
            => CreateCursor(Resource.RotateArrow, 26, 26, 13, 13, 135d);

        public static CursorInfo Rotate180Cursor
            => CreateCursor(Resource.RotateArrow, 26, 26, 13, 13, 180d);

        public static CursorInfo Rotate225Cursor
            => CreateCursor(Resource.RotateArrow, 26, 26, 13, 13, 225d);

        public static CursorInfo Rotate270Cursor
            => CreateCursor(Resource.RotateArrow, 26, 26, 13, 13, 270d);

        public static CursorInfo Rotate315Cursor
            => CreateCursor(Resource.RotateArrow, 26, 26, 13, 13, 315d);

        public static CursorInfo CreateCursor(UIElement Element, int xHotSpot, int yHotSpot)
            => CreateCursor(Element, (int)SystemParameters.CursorWidth, (int)SystemParameters.CursorHeight, xHotSpot, yHotSpot, 1d);
        public static CursorInfo CreateCursor(UIElement Element, int Width, int Height, int xHotSpot, int yHotSpot)
            => CreateCursor(Element, Width, Height, xHotSpot, yHotSpot, 1d);
        public static CursorInfo CreateCursor(UIElement Element, int Width, int Height, int xHotSpot, int yHotSpot, double Opacity)
        {
            IntPtr HIcon = Element.CreateHIcon(Width, Height, xHotSpot, yHotSpot, Opacity);
            SafeIconHandle HCursor = new(HIcon);

            return new CursorInfo(CursorInteropHelper.Create(HCursor), HCursor);
        }

        public static CursorInfo CreateCursor(DrawingImage Image, int xHotSpot, int yHotSpot, double Angle)
            => CreateCursor(Image, (int)SystemParameters.CursorWidth, (int)SystemParameters.CursorHeight, xHotSpot, yHotSpot, Angle, 1d);
        public static CursorInfo CreateCursor(DrawingImage Image, int Width, int Height, int xHotSpot, int yHotSpot, double Angle)
            => CreateCursor(Image, Width, Height, xHotSpot, yHotSpot, Angle, 1d);
        public static CursorInfo CreateCursor(DrawingImage Image, int Width, int Height, int xHotSpot, int yHotSpot, double Angle, double Opacity)
        {
            double CursorX = Math.Round(xHotSpot * Math.Max(Image.Width, Width) / Image.Width),
                   CursorY = Math.Round(yHotSpot * Math.Max(Image.Height, Height) / Image.Height);

            IntPtr HIcon = Image.CreateHIcon(Width, Height, Angle, (int)CursorX, (int)CursorY, Opacity);
            SafeIconHandle HCursor = new(HIcon);

            return new CursorInfo(CursorInteropHelper.Create(HCursor), HCursor);
        }

        public static void SetAllGlobalCursor(CursorInfo Cursor)
        {
            if (Cursor is null)
            {
                SystemParametersInfo(SystemParameterActionType.SetCursors, 0, IntPtr.Zero, SystemParameterInfoFlag.SendChange);
                return;
            }

            foreach (CursorID ID in Enum.GetValues(typeof(CursorID)))
                SetSystemCursor(CopyIcon(Cursor.Handle), ID);

        }

        public static void SetGlobalCursor(CursorID Id, CursorInfo Cursor)
            => SetSystemCursor(Cursor.Handle, Id);
        public static void SetGlobalCursor(CursorID Id, UIElement Element, int xHotSpot, int yHotSpot)
            => SetSystemCursor(CreateCursor(Element, xHotSpot, yHotSpot).Handle, Id);
        public static void SetGlobalCursor(CursorID Id, DrawingImage Image, int xHotSpot, int yHotSpot, double Angle)
            => SetSystemCursor(CreateCursor(Image, xHotSpot, yHotSpot, Angle).Handle, Id);

    }
}