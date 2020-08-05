using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Bitmap = System.Drawing.Bitmap;

namespace MenthaAssembly.Devices
{
    public static class CursorHelper
    {
        #region Windows API
        [DllImport("user32.dll")]
        private static extern SafeIconHandle CreateIconIndirect(ref IconInfo icon);

        [DllImport("user32.dll")]
        private static extern IntPtr CopyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetIconInfo(IntPtr hIcon, out IconInfo pIconInfo);

        [DllImport("user32.dll")]
        private static extern bool SetSystemCursor(IntPtr hCursor, CursorID type);

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        private struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        private class SafeIconHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public SafeIconHandle() : base(true) { }

            protected override bool ReleaseHandle()
                => DestroyIcon(handle);
        }

        #endregion

        public static CursorResource Resource
            => CursorResource.Instance;

        public static CursorInfo EyedropperCursor
            => CreateCursor(Resource.Eyedropper, 1, 21, 0);

        public static CursorInfo GrabHandCursor
            => CreateCursor(Resource.GrabHand, 9, 10, 0);

        public static CursorInfo RotateCursor
            => CreateCursor(Resource.RotateArrow, 13, 13, 0);

        public static CursorInfo Rotate45Cursor
            => CreateCursor(Resource.RotateArrow, 13, 13, 45d);

        public static CursorInfo Rotate90Cursor
            => CreateCursor(Resource.RotateArrow, 13, 13, 90d);

        public static CursorInfo Rotate135Cursor
            => CreateCursor(Resource.RotateArrow, 13, 13, 135d);

        public static CursorInfo Rotate180Cursor
            => CreateCursor(Resource.RotateArrow, 13, 13, 180d);

        public static CursorInfo Rotate225Cursor
            => CreateCursor(Resource.RotateArrow, 13, 13, 225d);

        public static CursorInfo Rotate270Cursor
            => CreateCursor(Resource.RotateArrow, 13, 13, 270d);

        public static CursorInfo Rotate315Cursor
            => CreateCursor(Resource.RotateArrow, 13, 13, 315d);

        private static CursorInfo InternalCreateCursor(Bitmap bitmap, int xHotSpot, int yHotSpot)
        {
            GetIconInfo(bitmap.GetHicon(), out IconInfo Info);

            Info.fIcon = false;
            Info.xHotspot = xHotSpot;
            Info.yHotspot = yHotSpot;

            SafeIconHandle cursorHandle = CreateIconIndirect(ref Info);
            bitmap.Dispose();

            return new CursorInfo(CursorInteropHelper.Create(cursorHandle), cursorHandle);
        }

        public static CursorInfo CreateCursor(UIElement Element, int xHotSpot, int yHotSpot)
            => InternalCreateCursor(ImageHelper.CreateBitmap(Element, (int)SystemParameters.CursorWidth, (int)SystemParameters.CursorHeight), xHotSpot, yHotSpot);
        public static CursorInfo CreateCursor(DrawingImage Image, int xHotSpot, int yHotSpot, double Angle)
            => InternalCreateCursor(ImageHelper.CreateBitmap(Image, (int)SystemParameters.CursorWidth, (int)SystemParameters.CursorHeight, Angle), xHotSpot, yHotSpot);

        public static void SetAllGlobalCursor(CursorInfo Cursor)
        {
            if (Cursor is null)
            {
                SystemParametersInfo(87, 0, IntPtr.Zero, 2);
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
