using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Bitmap = System.Drawing.Bitmap;

namespace MenthaAssembly.Devices
{
    public class CursorHelper
    {
        #region Windows API
        [DllImport("user32.dll")]
        private static extern SafeIconHandle CreateIconIndirect(ref IconInfo icon);

        [DllImport("user32.dll", EntryPoint = "CreateIconIndirect")]
        private static extern IntPtr CreateIconIndirect2(ref IconInfo icon);

        [DllImport("user32.dll")]
        private static extern IntPtr CopyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        [DllImport("user32.dll")]
        private static extern bool SetSystemCursor(IntPtr hCursor, CursorID type);

        [DllImport("user32.dll")]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);
        
        private struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        #endregion

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        private class SafeIconHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public SafeIconHandle() : base(true)
            {
            }

            protected override bool ReleaseHandle()
                => DestroyIcon(handle);
        }

        private static DrawingImage _EyedropperCursorImage;
        internal static DrawingImage EyedropperCursorImage
        {
            get
            {
                if (_EyedropperCursorImage is null)
                {
                    // Create CursorImage
                    DrawingGroup ImageDatas = new DrawingGroup();
                    ImageDatas.Children.Add(new GeometryDrawing(Brushes.Transparent, null, Geometry.Parse("M0,0L32,0 32,32 0,32z")));
                    ImageDatas.Children.Add(new GeometryDrawing(Brushes.Black, new Pen(Brushes.White, 1d), Geometry.Parse("M23.650993,4.5C25.773577,4.5 27.5,6.2269645 27.5,8.3494997 27.5,9.378129 27.099771,10.344424 26.372868,11.071375L22.750515,14.693191 23.165054,15.10773C23.369261,15.311888,23.369261,15.642676,23.165054,15.846883L21.074127,17.937811C20.972,18.039888 20.838261,18.090954 20.704527,18.090954 20.570789,18.090954 20.437004,18.039888 20.334925,17.937811L19.651148,17.254034C18.922533,17.958 13.172987,23.575525 12.215317,24.593912 11.726289,25.112001 11.035113,25.409121 10.318404,25.409121 9.6925526,25.409121 9.0871353,25.592403 8.5674791,25.93951L6.3581061,27.412182C6.269259,27.47143 6.1682091,27.5 6.0681396,27.5 5.9333739,27.5 5.7996373,27.447956 5.6985388,27.346857L4.6530995,26.301418C4.4764833,26.125341,4.4499221,25.849144,4.5877752,25.641899L5.9997277,23.524462C6.3866768,22.943008 6.5908833,22.26766 6.5908833,21.570848 6.5908833,20.872517 6.862473,20.216036 7.3565979,19.722403L14.738081,12.340917 14.062196,11.66503C13.857988,11.460873,13.857988,11.130083,14.062196,10.925877L16.15312,8.8349504C16.357279,8.630743,16.688068,8.630743,16.892275,8.8349504L17.306765,9.2494402 20.929117,5.6276245C21.656069,4.9002328,22.622902,4.5,23.650993,4.5z")));
                    ImageDatas.Children.Add(new GeometryDrawing(Brushes.White, null, Geometry.Parse("M16.348545,13.5L18.5,15.70717C17.552536,16.646378,16.208664,17.980757,14.684647,19.5L10.5,19.5z")));
                    _EyedropperCursorImage = new DrawingImage(ImageDatas);
                }
                return _EyedropperCursorImage;
            }
        }
        public static CursorInfo EyedropperCursor
            => CreateCursor(EyedropperCursorImage,
                            SystemParameters.CursorWidth,
                            SystemParameters.CursorHeight,
                            (int)Math.Ceiling(SystemParameters.CursorWidth * 0.125d),
                            (int)Math.Ceiling(SystemParameters.CursorHeight * 0.8125d));


        private static CursorInfo InternalCreateCursor(Bitmap bitmap, int xHotSpot, int yHotSpot)
        {
            IconInfo Info = new IconInfo();
            GetIconInfo(bitmap.GetHicon(), ref Info);

            Info.xHotspot = xHotSpot;
            Info.yHotspot = yHotSpot;
            Info.fIcon = false;

            SafeIconHandle cursorHandle = CreateIconIndirect(ref Info);
            bitmap.Dispose();

            return new CursorInfo(CursorInteropHelper.Create(cursorHandle), cursorHandle);
        }

        public static CursorInfo CreateCursor(UIElement Element, int xHotSpot, int yHotSpot)
            => InternalCreateCursor(CreateBitmap(Element), xHotSpot, yHotSpot);
        public static CursorInfo CreateCursor(DrawingImage Image, double Width, double Height, int xHotSpot, int yHotSpot)
            => CreateCursor(new Image
            {
                Source = Image,
                Stretch = Stretch.Uniform,
                Width = Width,
                Height = Height
            }, xHotSpot, yHotSpot);

        public static void SetGlobalCursor(CursorID Id, CursorInfo Cursor)
            => SetSystemCursor(Cursor.Handle, Id);
        public static void SetGlobalCursor(CursorID Id, UIElement Element, int xHotSpot, int yHotSpot)
            => SetSystemCursor(CreateCursor(Element, xHotSpot, yHotSpot).Handle, Id);
        public static void SetGlobalCursor(CursorID Id, DrawingImage Image, double Width, double Height, int xHotSpot, int yHotSpot)
            => SetSystemCursor(CreateCursor(Image, Width, Height, xHotSpot, yHotSpot).Handle, Id);
        
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

        private static Bitmap CreateBitmap(UIElement Element)
        {
            Element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Element.Arrange(new Rect(new Point(), Element.DesiredSize));

            RenderTargetBitmap RenderBitmap = new RenderTargetBitmap((int)Element.DesiredSize.Width,
                                                                     (int)Element.DesiredSize.Height,
                                                                     96,
                                                                     96,
                                                                     PixelFormats.Pbgra32);
            RenderBitmap.Render(Element);

            PngBitmapEncoder PngEncoder = new PngBitmapEncoder();
            PngEncoder.Frames.Add(BitmapFrame.Create(RenderBitmap));

            using MemoryStream memoryStream = new MemoryStream();
            PngEncoder.Save(memoryStream);

            return new Bitmap(memoryStream);
        }
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


    }
}
