using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Controls;
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

        //[DllImport("user32.dll", EntryPoint = "GetCursorInfo")]
        //private static extern bool GetCursorInfo(out CURSORINFO pci);

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

        //private struct CURSORINFO
        //{
        //    public int cbSize;          // Specifies the size, in bytes, of the structure. 
        //    public int flags;           // Specifies the cursor state. This parameter can be one of the following values:
        //    public IntPtr hCursor;      // Handle to the cursor. 
        //    public Int32Point Position; // The screen coordinates of the cursor.
        //}

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
            => CreateCursor(Resource.Eyedropper, 1, 21);

        public static CursorInfo GrabHandCursor
            => CreateCursor(Resource.GrabHand, 9, 10);

        public static CursorInfo RotateCursor
            => CreateCursor(Resource.RotateArrow, 13, 13);

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

            Info.xHotspot = xHotSpot;
            Info.yHotspot = yHotSpot;
            Info.fIcon = false;

            SafeIconHandle cursorHandle = CreateIconIndirect(ref Info);
            bitmap.Dispose();

            return new CursorInfo(CursorInteropHelper.Create(cursorHandle), cursorHandle);
        }

        public static CursorInfo CreateCursor(UIElement Element, int xHotSpot, int yHotSpot)
            => InternalCreateCursor(ImageHelper.CreateBitmap(Element, (int)SystemParameters.CursorWidth, (int)SystemParameters.CursorHeight), xHotSpot, yHotSpot);
        public static CursorInfo CreateCursor(DrawingImage Image, int xHotSpot, int yHotSpot, double Angle = 0d)
            => CreateCursor(new Image
            {
                Source = Image,
                Stretch = Stretch.UniformToFill,
                Width = Image.Width,
                Height = Image.Height,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = Angle != 0 ? new RotateTransform(Angle) : null
            }, xHotSpot, yHotSpot);

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
        public static void SetGlobalCursor(CursorID Id, DrawingImage Image, int xHotSpot, int yHotSpot)
            => SetSystemCursor(CreateCursor(Image, xHotSpot, yHotSpot).Handle, Id);

        //private static Bitmap CreateBitmap(DrawingImage Image)
        //{
        //    // Convert To Visual
        //    DrawingVisual Visual = new DrawingVisual();
        //    DrawingContext drawingContext = Visual.RenderOpen();
        //    drawingContext.DrawImage(Image, new Rect(new Point(0, 0), new Size(Image.Width, Image.Height)));
        //    drawingContext.Close();

        //    // Convert To BitmapSource
        //    RenderTargetBitmap RenderBitmap = new RenderTargetBitmap((int)Image.Width, (int)Image.Height, 96, 96, PixelFormats.Pbgra32);
        //    RenderBitmap.Render(Visual);

        //    // Convert To Png
        //    PngBitmapEncoder PngEncoder = new PngBitmapEncoder();
        //    PngEncoder.Frames.Add(BitmapFrame.Create(RenderBitmap));
        //    using MemoryStream memoryStream = new MemoryStream();
        //    PngEncoder.Save(memoryStream);

        //    return new Bitmap(memoryStream);
        //}

        //public static Int32Size CursorSize
        //{
        //    get
        //    {
        //        CURSORINFO ci = new CURSORINFO();
        //        ci.CbSize = Marshal.SizeOf(ci);

        //        if (GetCursorInfo(out ci) &&
        //            ci.Flags == 1)
        //        {
        //            IntPtr Hicon = CopyIcon(ci.HCursor);
        //            if (GetIconInfo(Hicon, out IconInfo icInfo))
        //            {
        //                Bitmap bmp = Bitmap.FromHbitmap(icInfo.hbmMask);

        //                int x = 0,
        //                    y = 0;

        //                for (int i = 0; i < bmp.Width; i++)
        //                {
        //                    for (int j = 0; j < bmp.Height; j++)
        //                    {
        //                        System.Drawing.Color a = bmp.GetPixel(i, j);
        //                        if (a.R == 0 && a.G == 0 && a.B == 0)
        //                        {
        //                            if (i > x)
        //                                x = i;

        //                            if (j > y)
        //                                y = j;
        //                        }
        //                    }
        //                }

        //                bmp.Dispose();
        //                if (Hicon != IntPtr.Zero)
        //                    DestroyIcon(Hicon);

        //                if (icInfo.hbmColor != IntPtr.Zero)
        //                    DeleteObject(icInfo.hbmColor);

        //                if (icInfo.hbmMask != IntPtr.Zero)
        //                    DeleteObject(icInfo.hbmMask);

        //                if (ci.HCursor != IntPtr.Zero)
        //                    DeleteObject(ci.HCursor);

        //                return new Int32Size(x, y);
        //            }

        //            if (Hicon != IntPtr.Zero)
        //                DestroyIcon(Hicon);

        //            if (icInfo.hbmColor != IntPtr.Zero)
        //                DeleteObject(icInfo.hbmColor);

        //            if (icInfo.hbmMask != IntPtr.Zero)
        //                DeleteObject(icInfo.hbmMask);
        //        }

        //        if (ci.HCursor != IntPtr.Zero)
        //            DeleteObject(ci.HCursor);

        //        return Int32Size.Empty;
        //    }
        //}

    }

}
