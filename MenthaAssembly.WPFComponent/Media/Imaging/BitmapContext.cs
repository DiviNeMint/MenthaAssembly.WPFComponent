using MenthaAssembly.Media.Imaging;
using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MenthaAssembly
{
    public unsafe class BitmapContext : IImageContext
    {
        public WriteableBitmap Bitmap { get; }

        internal IImageContext Context { get; }
        IImageOperator IImageContext.Operator => Context.Operator;

        public BitmapContext(string Path) : this(new Uri(Path)) { }
        public BitmapContext(Uri Path) : this(new BitmapImage(Path)) { }
        public BitmapContext(BitmapSource Source) : this(new WriteableBitmap(Source)) { }
        public BitmapContext(WriteableBitmap Bitmap)
        {
            this.Bitmap = Bitmap;
            if (PixelFormats.Bgr24.Equals(Bitmap.Format))
            {
                Context = new ImageContext<BGR>(Bitmap.PixelWidth,
                                                Bitmap.PixelHeight,
                                                Bitmap.BackBuffer,
                                                Bitmap.BackBufferStride);
            }
            else if (PixelFormats.Bgr32.Equals(Bitmap.Format))
            {
                Context = new ImageContext<BGRA>(Bitmap.PixelWidth,
                                                 Bitmap.PixelHeight,
                                                 Bitmap.BackBuffer,
                                                 Bitmap.BackBufferStride);
            }
            else if (PixelFormats.Bgra32.Equals(Bitmap.Format) ||
                     PixelFormats.Pbgra32.Equals(Bitmap.Format))
            {
                Context = new ImageContext<BGRA>(Bitmap.PixelWidth,
                                                 Bitmap.PixelHeight,
                                                 Bitmap.BackBuffer,
                                                 Bitmap.BackBufferStride);
            }
            else if (PixelFormats.Rgb24.Equals(Bitmap.Format))
            {
                Context = new ImageContext<RGB>(Bitmap.PixelWidth,
                                                Bitmap.PixelHeight,
                                                Bitmap.BackBuffer,
                                                Bitmap.BackBufferStride);
            }
            else if (PixelFormats.Gray8.Equals(Bitmap.Format))
            {
                Context = new ImageContext<Gray8>(Bitmap.PixelWidth,
                                                  Bitmap.PixelHeight,
                                                  Bitmap.BackBuffer,
                                                  Bitmap.BackBufferStride);
            }
        }

        public int Width => Context.Width;

        public int Height => Context.Height;

        public long Stride => Context.Stride;

        public int BitsPerPixel => Context.BitsPerPixel;

        public int Channels => Context.Channels;

        Type IImageContext.PixelType => Context.PixelType;

        Type IImageContext.StructType => Context.StructType;

        public IntPtr Scan0 => Context.Scan0;
        IntPtr IImageContext.ScanA => throw new NotImplementedException();
        IntPtr IImageContext.ScanR => throw new NotImplementedException();
        IntPtr IImageContext.ScanG => throw new NotImplementedException();
        IntPtr IImageContext.ScanB => throw new NotImplementedException();

        public IImagePalette Palette => Context.Palette;

        public IPixel this[int X, int Y]
        {
            get => Context[X, Y];
            set => Context[X, Y] = value;
        }

        #region Graphic Processing
        public void DrawLine(Point<int> P0, Point<int> P1, IPixel Color)
        {
            Context.DrawLine(P0, P1, Color);
        }
        public void DrawLine(int X0, int Y0, int X1, int Y1, IPixel Color)
        {
            Context.DrawLine(X0, Y0, X1, Y1, Color);
        }
        public void DrawLine(Point<int> P0, Point<int> P1, IImageContext Pen)
        {
            Context.DrawLine(P0, P1, Pen);
        }
        public void DrawLine(int X0, int Y0, int X1, int Y1, IImageContext Pen)
        {
            Context.DrawLine(X0, Y0, X1, Y1, Pen);
        }
        public void DrawLine(Point<int> P0, Point<int> P1, ImageContour Contour, IPixel Fill)
        {
            Context.DrawLine(P0, P1, Contour, Fill);
        }
        public void DrawLine(int X0, int Y0, int X1, int Y1, ImageContour Contour, IPixel Fill)
        {
            Context.DrawLine(X0, Y0, X1, Y1, Contour, Fill);
        }

        public void DrawArc(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, IPixel Color)
        {
            Context.DrawArc(Start, End, Center, Rx, Ry, Clockwise, Color);
        }
        public void DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, IPixel Color)
        {
            Context.DrawArc(Sx, Sy, Ex, Ey, Cx, Cy, Rx, Ry, Clockwise, Color);
        }
        public void DrawArc(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, IImageContext Pen)
        {
            Context.DrawArc(Start, End, Center, Rx, Ry, Clockwise, Pen);
        }
        public void DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, IImageContext Pen)
        {
            Context.DrawArc(Sx, Sy, Ex, Ey, Cx, Cy, Rx, Ry, Clockwise, Pen);
        }
        public void DrawArc(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, ImageContour Contour, IPixel Fill)
        {
            Context.DrawArc(Start, End, Center, Rx, Ry, Clockwise, Contour, Fill);
        }
        public void DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, ImageContour Contour, IPixel Fill)
        {
            Context.DrawArc(Sx, Sy, Ex, Ey, Cx, Cy, Rx, Ry, Clockwise, Contour, Fill);
        }

        public void DrawCurve(IList<int> Points, float Tension, IPixel Color)
        {
            Context.DrawCurve(Points, Tension, Color);
        }
        public void DrawCurve(IList<Point<int>> Points, float Tension, IPixel Color)
        {
            Context.DrawCurve(Points, Tension, Color);
        }
        public void DrawCurve(IList<int> Points, float Tension, IImageContext Pen)
        {
            Context.DrawCurve(Points, Tension, Pen);
        }
        public void DrawCurve(IList<Point<int>> Points, float Tension, IImageContext Pen)
        {
            Context.DrawCurve(Points, Tension, Pen);
        }
        public void DrawCurve(IList<int> Points, float Tension, ImageContour Contour, IPixel Fill)
        {
            Context.DrawCurve(Points, Tension, Contour, Fill);
        }
        public void DrawCurve(IList<Point<int>> Points, float Tension, ImageContour Contour, IPixel Fill)
        {
            Context.DrawCurve(Points, Tension, Contour, Fill);
        }

        public void DrawCurveClosed(IList<int> Points, float Tension, IPixel Color)
        {
            Context.DrawCurveClosed(Points, Tension, Color);
        }
        public void DrawCurveClosed(IList<Point<int>> Points, float Tension, IPixel Color)
        {
            Context.DrawCurveClosed(Points, Tension, Color);
        }
        public void DrawCurveClosed(IList<int> Points, float Tension, IImageContext Pen)
        {
            Context.DrawCurveClosed(Points, Tension, Pen);
        }
        public void DrawCurveClosed(IList<Point<int>> Points, float Tension, IImageContext Pen)
        {
            Context.DrawCurveClosed(Points, Tension, Pen);
        }
        public void DrawCurveClosed(IList<int> Points, float Tension, ImageContour Contour, IPixel Fill)
        {
            Context.DrawCurveClosed(Points, Tension, Contour, Fill);
        }
        public void DrawCurveClosed(IList<Point<int>> Points, float Tension, ImageContour Contour, IPixel Fill)
        {
            Context.DrawCurveClosed(Points, Tension, Contour, Fill);
        }

        public void DrawBezier(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, IPixel Color)
        {
            Context.DrawBezier(X1, Y1, Cx1, Cy1, Cx2, Cy2, X2, Y2, Color);
        }
        public void DrawBezier(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, IImageContext Pen)
        {
            Context.DrawBezier(X1, Y1, Cx1, Cy1, Cx2, Cy2, X2, Y2, Pen);
        }
        public void DrawBezier(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, ImageContour Contour, IPixel Fill)
        {
            Context.DrawBezier(X1, Y1, Cx1, Cy1, Cx2, Cy2, X2, Y2, Contour, Fill);
        }

        public void DrawBeziers(IList<int> Points, IPixel Color)
        {
            Context.DrawBeziers(Points, Color);
        }
        public void DrawBeziers(IList<int> Points, IImageContext Pen)
        {
            Context.DrawBeziers(Points, Pen);
        }
        public void DrawBeziers(IList<int> Points, ImageContour Contour, IPixel Fill)
        {
            Context.DrawBeziers(Points, Contour, Fill);
        }

        public void DrawText(int X, int Y, string Text, int CharSize, IPixel Fill)
        {
            Context.DrawText(X, Y, Text, CharSize, Fill);
        }
        public void DrawText(int X, int Y, string Text, int CharSize, IPixel Fill, double Angle, FontWeightType Weight, bool Italic)
        {
            Context.DrawText(X, Y, Text, CharSize, Fill, Angle, Weight, Italic);
        }
        public void DrawText(int X, int Y, string Text, string FontName, int CharSize, IPixel Fill)
        {
            Context.DrawText(X, Y, Text, FontName, CharSize, Fill);
        }
        public void DrawText(int X, int Y, string Text, string FontName, int CharSize, IPixel Fill, double Angle, FontWeightType Weight, bool Italic)
        {
            Context.DrawText(X, Y, Text, FontName, CharSize, Fill, Angle, Weight, Italic);
        }

        public void DrawTriangle(int X1, int Y1, int X2, int Y2, int X3, int Y3, IPixel Color)
        {
            Context.DrawTriangle(X1, Y1, X2, Y2, X3, Y3, Color);
        }
        public void DrawTriangle(Point<int> P1, Point<int> P2, Point<int> P3, IPixel Color)
        {
            Context.DrawTriangle(P1, P2, P3, Color);
        }
        public void DrawTriangle(int X1, int Y1, int X2, int Y2, int X3, int Y3, IImageContext Pen)
        {
            Context.DrawTriangle(X1, Y1, X2, Y2, X3, Y3, Pen);
        }
        public void DrawTriangle(Point<int> P1, Point<int> P2, Point<int> P3, IImageContext Pen)
        {
            Context.DrawTriangle(P1, P2, P3, Pen);
        }
        public void DrawTriangle(int X1, int Y1, int X2, int Y2, int X3, int Y3, ImageContour Contour, IPixel Fill)
        {
            Context.DrawTriangle(X1, Y1, X2, Y2, X3, Y3, Contour, Fill);
        }
        public void DrawTriangle(Point<int> P1, Point<int> P2, Point<int> P3, ImageContour Contour, IPixel Fill)
        {
            Context.DrawTriangle(P1, P2, P3, Contour, Fill);
        }

        public void DrawRectangle(int X1, int Y1, int X2, int Y2, IPixel Color)
        {
            Context.DrawRectangle(X1, Y1, X2, Y2, Color);
        }
        public void DrawRectangle(Point<int> P1, Point<int> P2, IPixel Color)
        {
            Context.DrawRectangle(P1, P2, Color);
        }
        public void DrawRectangle(int X1, int Y1, int X2, int Y2, IImageContext Pen)
        {
            Context.DrawRectangle(X1, Y1, X2, Y2, Pen);
        }
        public void DrawRectangle(Point<int> P1, Point<int> P2, IImageContext Pen)
        {
            Context.DrawRectangle(P1, P2, Pen);
        }
        public void DrawRectangle(int X1, int Y1, int X2, int Y2, ImageContour Contour, IPixel Fill)
        {
            Context.DrawRectangle(X1, Y1, X2, Y2, Contour, Fill);
        }
        public void DrawRectangle(Point<int> P1, Point<int> P2, ImageContour Contour, IPixel Fill)
        {
            Context.DrawRectangle(P1, P2, Contour, Fill);
        }

        public void DrawQuad(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, IPixel Color)
        {
            Context.DrawQuad(X1, Y1, X2, Y2, X3, Y3, X4, Y4, Color);
        }
        public void DrawQuad(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, IPixel Color)
        {
            Context.DrawQuad(P1, P2, P3, P4, Color);
        }
        public void DrawQuad(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, IImageContext Pen)
        {
            Context.DrawQuad(X1, Y1, X2, Y2, X3, Y3, X4, Y4, Pen);
        }
        public void DrawQuad(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, IImageContext Pen)
        {
            Context.DrawQuad(P1, P2, P3, P4, Pen);
        }
        public void DrawQuad(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, ImageContour Contour, IPixel Fill)
        {
            Context.DrawQuad(X1, Y1, X2, Y2, X3, Y3, X4, Y4, Contour, Fill);
        }
        public void DrawQuad(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, ImageContour Contour, IPixel Fill)
        {
            Context.DrawQuad(P1, P2, P3, P4, Contour, Fill);
        }

        public void DrawEllipse(Bound<int> Bound, IPixel Color)
        {
            Context.DrawEllipse(Bound, Color);
        }
        public void DrawEllipse(Point<int> Center, int Rx, int Ry, IPixel Color)
        {
            Context.DrawEllipse(Center, Rx, Ry, Color);
        }
        public void DrawEllipse(int Cx, int Cy, int Rx, int Ry, IPixel Color)
        {
            Context.DrawEllipse(Cx, Cy, Rx, Ry, Color);
        }
        public void DrawEllipse(Bound<int> Bound, IImageContext Pen)
        {
            Context.DrawEllipse(Bound, Pen);
        }
        public void DrawEllipse(Point<int> Center, int Rx, int Ry, IImageContext Pen)
        {
            Context.DrawEllipse(Center, Rx, Ry, Pen);
        }
        public void DrawEllipse(int Cx, int Cy, int Rx, int Ry, IImageContext Pen)
        {
            Context.DrawEllipse(Cx, Cy, Rx, Ry, Pen);
        }
        public void DrawEllipse(Bound<int> Bound, ImageContour Contour, IPixel Fill)
        {
            Context.DrawEllipse(Bound, Contour, Fill);
        }
        public void DrawEllipse(Point<int> Center, int Rx, int Ry, ImageContour Contour, IPixel Fill)
        {
            Context.DrawEllipse(Center, Rx, Ry, Contour, Fill);
        }
        public void DrawEllipse(int Cx, int Cy, int Rx, int Ry, ImageContour Contour, IPixel Fill)
        {
            Context.DrawEllipse(Cx, Cy, Rx, Ry, Contour, Fill);
        }

        public void FillEllipse(Bound<int> Bound, IPixel Fill)
        {
            Context.FillEllipse(Bound, Fill);
        }
        public void FillEllipse(Point<int> Center, int Rx, int Ry, IPixel Fill)
        {
            Context.FillEllipse(Center, Rx, Ry, Fill);
        }
        public void FillEllipse(int Cx, int Cy, int Rx, int Ry, IPixel Fill)
        {
            Context.FillEllipse(Cx, Cy, Rx, Ry, Fill);
        }

        public void DrawRegularPolygon(Point<int> Center, double Radius, int VertexNum, IPixel Color, double StartAngle)
        {
            Context.DrawRegularPolygon(Center, Radius, VertexNum, Color, StartAngle);
        }
        public void DrawRegularPolygon(int Cx, int Cy, double Radius, int VertexNum, IPixel Color, double StartAngle)
        {
            Context.DrawRegularPolygon(Cx, Cy, Radius, VertexNum, Color, StartAngle);
        }
        public void DrawRegularPolygon(Point<int> Center, double Radius, int VertexNum, IImageContext Pen, double StartAngle)
        {
            Context.DrawRegularPolygon(Center, Radius, VertexNum, Pen, StartAngle);
        }
        public void DrawRegularPolygon(int Cx, int Cy, double Radius, int VertexNum, IImageContext Pen, double StartAngle)
        {
            Context.DrawRegularPolygon(Cx, Cy, Radius, VertexNum, Pen, StartAngle);
        }
        public void DrawRegularPolygon(Point<int> Center, double Radius, int VertexNum, ImageContour Contour, IPixel Fill, double StartAngle)
        {
            Context.DrawRegularPolygon(Center, Radius, VertexNum, Contour, Fill, StartAngle);
        }
        public void DrawRegularPolygon(int Cx, int Cy, double Radius, int VertexNum, ImageContour Contour, IPixel Fill, double StartAngle)
        {
            Context.DrawRegularPolygon(Cx, Cy, Radius, VertexNum, Contour, Fill, StartAngle);
        }

        public void FillPolygon(IEnumerable<Point<int>> Vertices, IPixel Fill, int OffsetX, int OffsetY)
        {
            Context.FillPolygon(Vertices, Fill, OffsetX, OffsetY);
        }
        public void FillPolygon(IEnumerable<int> VerticeDatas, IPixel Fill, int OffsetX, int OffsetY)
        {
            Context.FillPolygon(VerticeDatas, Fill, OffsetX, OffsetY);
        }

        public void DrawStamp(Point<int> Position, IImageContext Stamp)
        {
            Context.DrawStamp(Position, Stamp);
        }
        public void DrawStamp(int X, int Y, IImageContext Stamp)
        {
            Context.DrawStamp(X, Y, Stamp);
        }

        public void FillContour(ImageContour Contour, IPixel Fill, int OffsetX, int OffsetY)
        {
            Context.FillContour(Contour, Fill, OffsetX, OffsetY);
        }

        public void SeedFill(Point<int> SeedPoint, IPixel Fill, ImagePredicate Predicate)
        {
            Context.SeedFill(SeedPoint, Fill, Predicate);
        }
        public void SeedFill(int SeedX, int SeedY, IPixel Fill, ImagePredicate Predicate)
        {
            Context.SeedFill(SeedX, SeedY, Fill, Predicate);
        }

        #endregion

        #region Transform Processing
        public ImageContext<T> Rotate<T>(double Angle, bool Crop) where T : unmanaged, IPixel
        {
            return Context.Rotate<T>(Angle, Crop);
        }
        public ImageContext<T> Rotate<T>(double Angle, bool Crop, ParallelOptions Options) where T : unmanaged, IPixel
        {
            return Context.Rotate<T>(Angle, Crop, Options);
        }

        public ImageContext<T> Resize<T>(int Width, int Height, InterpolationTypes Interpolation) where T : unmanaged, IPixel
        {
            return Context.Resize<T>(Width, Height, Interpolation);
        }
        public ImageContext<T> Resize<T>(int Width, int Height, InterpolationTypes Interpolation, ParallelOptions Options) where T : unmanaged, IPixel
        {
            return Context.Resize<T>(Width, Height, Interpolation, Options);
        }

        public ImageContext<T> Flip<T>(FlipMode Mode) where T : unmanaged, IPixel
        {
            return Context.Flip<T>(Mode);
        }
        public ImageContext<T> Flip<T>(FlipMode Mode, ParallelOptions Options) where T : unmanaged, IPixel
        {
            return Context.Flip<T>(Mode, Options);
        }
        public ImageContext<T, U> Flip<T, U>(FlipMode Mode)
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
        {
            return Context.Flip<T, U>(Mode);
        }
        public ImageContext<T, U> Flip<T, U>(FlipMode Mode, ParallelOptions Options)
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
        {
            return Context.Flip<T, U>(Mode, Options);
        }

        public ImageContext<T> Convolute<T>(ConvoluteKernel Kernel) where T : unmanaged, IPixel
        {
            return Context.Convolute<T>(Kernel);
        }
        public ImageContext<T> Convolute<T>(ConvoluteKernel Kernel, ParallelOptions Options) where T : unmanaged, IPixel
        {
            return Context.Convolute<T>(Kernel, Options);
        }

        public ImageContext<T> Filter<T>(ImageFilter Filter) where T : unmanaged, IPixel 
            => Context.Filter<T>(Filter);
        public ImageContext<T> Filter<T>(ImageFilter Filter, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.Filter<T>(Filter, Options);

        public ImageContext<T> Crop<T>(int X, int Y, int Width, int Height) where T : unmanaged, IPixel
        {
            return Context.Crop<T>(X, Y, Width, Height);
        }
        public ImageContext<T> Crop<T>(int X, int Y, int Width, int Height, ParallelOptions Options) where T : unmanaged, IPixel
        {
            return Context.Crop<T>(X, Y, Width, Height, Options);
        }
        public ImageContext<T, U> Crop<T, U>(int X, int Y, int Width, int Height)
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
        {
            return Context.Crop<T, U>(X, Y, Width, Height);
        }
        public ImageContext<T, U> Crop<T, U>(int X, int Y, int Width, int Height, ParallelOptions Options)
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
        {
            return Context.Crop<T, U>(X, Y, Width, Height, Options);
        }

        public ImageContext<T> Cast<T>() where T : unmanaged, IPixel
        {
            return Context.Cast<T>();
        }
        public ImageContext<T> Cast<T>(ParallelOptions Options) where T : unmanaged, IPixel
        {
            return Context.Cast<T>(Options);
        }
        public ImageContext<T, U> Cast<T, U>()
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
        {
            return Context.Cast<T, U>();
        }
        public ImageContext<T, U> Cast<T, U>(ParallelOptions Options)
            where T : unmanaged, IPixel
            where U : unmanaged, IPixelIndexed
        {
            return Context.Cast<T, U>(Options);
        }

        public void Clear(IPixel Color)
        {
            Context.Clear(Color);
        }
        public void Clear(IPixel Color, ParallelOptions Options)
        {
            Context.Clear(Color, Options);
        }

        #endregion

        #region Buffer Processing
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] Dest0)
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0);
        }
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, ParallelOptions Options)
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0, Options);
        }
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride)
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0, DestStride);
        }
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride, ParallelOptions Options)
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0, DestStride, Options);
        }
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride)
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0, DestOffset, DestStride);
        }
        public void BlockCopy(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride, ParallelOptions Options)
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0, DestOffset, DestStride, Options);
        }
        public void BlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0)
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0);
        }
        public void BlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0, ParallelOptions Options)
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0, Options);
        }
        public void BlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride)
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0, DestStride);
        }
        public void BlockCopy(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride, ParallelOptions Options)
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0, DestStride, Options);
        }
        public void BlockCopy(int X, int Y, int Width, int Height, byte* Dest0)
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0);
        }
        public void BlockCopy(int X, int Y, int Width, int Height, byte* Dest0, ParallelOptions Options)
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0, Options);
        }
        public void BlockCopy(int X, int Y, int Width, int Height, byte* Dest0, long DestStride)
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0, DestStride);
        }
        public void BlockCopy(int X, int Y, int Width, int Height, byte* Dest0, long DestStride, ParallelOptions Options)
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0, DestStride, Options);
        }

        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0) where T : unmanaged, IPixel
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0, Options);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, long DestStride) where T : unmanaged, IPixel
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0, DestStride);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0, DestStride, Options);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, int DestOffset, long DestStride) where T : unmanaged, IPixel
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0, DestOffset, DestStride);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, int DestOffset, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0, DestOffset, DestStride, Options);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0) where T : unmanaged, IPixel
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0, Options);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, long DestStride) where T : unmanaged, IPixel
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0, DestStride);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.BlockCopy(X, Y, Width, Height, Dest0, DestStride, Options);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0) where T : unmanaged, IPixel
        {
            Context.BlockCopy<T>(X, Y, Width, Height, Dest0);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.BlockCopy<T>(X, Y, Width, Height, Dest0, Options);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride) where T : unmanaged, IPixel
        {
            Context.BlockCopy<T>(X, Y, Width, Height, Dest0, DestStride);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.BlockCopy<T>(X, Y, Width, Height, Dest0, DestStride, Options);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride) where T : unmanaged, IPixel
        {
            Context.BlockCopy<T>(X, Y, Width, Height, Dest0, DestOffset, DestStride);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.BlockCopy<T>(X, Y, Width, Height, Dest0, DestOffset, DestStride, Options);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0) where T : unmanaged, IPixel
        {
            Context.BlockCopy<T>(X, Y, Width, Height, Dest0);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.BlockCopy<T>(X, Y, Width, Height, Dest0, Options);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride) where T : unmanaged, IPixel
        {
            Context.BlockCopy<T>(X, Y, Width, Height, Dest0, DestStride);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.BlockCopy<T>(X, Y, Width, Height, Dest0, DestStride, Options);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0) where T : unmanaged, IPixel
        {
            Context.BlockCopy<T>(X, Y, Width, Height, Dest0);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.BlockCopy<T>(X, Y, Width, Height, Dest0, Options);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, long DestStride) where T : unmanaged, IPixel
        {
            Context.BlockCopy<T>(X, Y, Width, Height, Dest0, DestStride);
        }
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
        {
            Context.BlockCopy<T>(X, Y, Width, Height, Dest0, DestStride, Options);
        }

        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB)
        {
            Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB);
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, ParallelOptions Options)
        {
            Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, Options);
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride)
        {
            Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestStride);
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride, ParallelOptions Options)
        {
            Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestStride, Options);
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride)
        {
            Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestOffset, DestStride);
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride, ParallelOptions Options)
        {
            Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestOffset, DestStride, Options);
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB)
        {
            Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB);
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, ParallelOptions Options)
        {
            Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, Options);
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride)
        {
            Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestStride);
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride, ParallelOptions Options)
        {
            Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestStride, Options);
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB)
        {
            Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB);
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, ParallelOptions Options)
        {
            Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, Options);
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, long DestStride)
        {
            Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestStride);
        }
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, long DestStride, ParallelOptions Options)
        {
            Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestStride, Options);
        }

        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB)
        {
            Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB);
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, ParallelOptions Options)
        {
            Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, Options);
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride)
        {
            Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestStride);
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride, ParallelOptions Options)
        {
            Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestStride, Options);
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride)
        {
            Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestOffset, DestStride);
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride, ParallelOptions Options)
        {
            Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestOffset, DestStride, Options);
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB)
        {
            Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB);
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, ParallelOptions Options)
        {
            Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, Options);
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride)
        {
            Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestStride);
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride, ParallelOptions Options)
        {
            Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestStride, Options);
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB)
        {
            Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB);
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, ParallelOptions Options)
        {
            Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, Options);
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, long DestStride)
        {
            Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestStride);
        }
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, long DestStride, ParallelOptions Options)
        {
            Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestStride, Options);
        }

        public void ScanLineCopy(int X, int Y, int Length, byte[] Dest0)
        {
            Context.ScanLineCopy(X, Y, Length, Dest0);
        }
        public void ScanLineCopy(int X, int Y, int Length, byte[] Dest0, int DestOffset)
        {
            Context.ScanLineCopy(X, Y, Length, Dest0, DestOffset);
        }
        public void ScanLineCopy(int X, int Y, int Length, IntPtr Dest0)
        {
            Context.ScanLineCopy(X, Y, Length, Dest0);
        }
        public void ScanLineCopy(int X, int Y, int Length, byte* Dest0)
        {
            Context.ScanLineCopy(X, Y, Length, Dest0);
        }

        public void ScanLineCopy<T>(int X, int Y, int Length, T* Dest0) where T : unmanaged, IPixel
        {
            Context.ScanLineCopy(X, Y, Length, Dest0);
        }
        public void ScanLineCopy<T>(int X, int Y, int Length, T[] Dest0) where T : unmanaged, IPixel
        {
            Context.ScanLineCopy(X, Y, Length, Dest0);
        }
        public void ScanLineCopy<T>(int X, int Y, int Length, T[] Dest0, int DestOffset) where T : unmanaged, IPixel
        {
            Context.ScanLineCopy(X, Y, Length, Dest0, DestOffset);
        }
        public void ScanLineCopy<T>(int X, int Y, int Length, byte[] Dest0) where T : unmanaged, IPixel
        {
            Context.ScanLineCopy<T>(X, Y, Length, Dest0);
        }
        public void ScanLineCopy<T>(int X, int Y, int Length, byte[] Dest0, int DestOffset) where T : unmanaged, IPixel
        {
            Context.ScanLineCopy<T>(X, Y, Length, Dest0, DestOffset);
        }
        public void ScanLineCopy<T>(int X, int Y, int Length, IntPtr Dest0) where T : unmanaged, IPixel
        {
            Context.ScanLineCopy<T>(X, Y, Length, Dest0);
        }
        public void ScanLineCopy<T>(int X, int Y, int Length, byte* Dest0) where T : unmanaged, IPixel
        {
            Context.ScanLineCopy<T>(X, Y, Length, Dest0);
        }

        public void ScanLineCopy3(int X, int Y, int Length, byte[] DestR, byte[] DestG, byte[] DestB)
        {
            Context.ScanLineCopy3(X, Y, Length, DestR, DestG, DestB);
        }
        public void ScanLineCopy3(int X, int Y, int Length, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset)
        {
            Context.ScanLineCopy3(X, Y, Length, DestR, DestG, DestB, DestOffset);
        }
        public void ScanLineCopy3(int X, int Y, int Length, IntPtr DestR, IntPtr DestG, IntPtr DestB)
        {
            Context.ScanLineCopy3(X, Y, Length, DestR, DestG, DestB);
        }
        public void ScanLineCopy3(int X, int Y, int Length, byte* DestR, byte* DestG, byte* DestB)
        {
            Context.ScanLineCopy3(X, Y, Length, DestR, DestG, DestB);
        }

        public void ScanLineCopy4(int X, int Y, int Length, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB)
        {
            Context.ScanLineCopy4(X, Y, Length, DestA, DestR, DestG, DestB);
        }
        public void ScanLineCopy4(int X, int Y, int Length, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset)
        {
            Context.ScanLineCopy4(X, Y, Length, DestA, DestR, DestG, DestB, DestOffset);
        }
        public void ScanLineCopy4(int X, int Y, int Length, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB)
        {
            Context.ScanLineCopy4(X, Y, Length, DestA, DestR, DestG, DestB);
        }
        public void ScanLineCopy4(int X, int Y, int Length, byte* DestA, byte* DestR, byte* DestG, byte* DestB)
        {
            Context.ScanLineCopy4(X, Y, Length, DestA, DestR, DestG, DestB);
        }

        #endregion

        protected bool IsLocked { set; get; }
        public bool TryLock(int Timeout)
        {
            if (Bitmap is null)
                return false;

            if ((Bitmap.Dispatcher.CheckAccess() && Bitmap.TryLock(TimeSpan.FromMilliseconds(Timeout))) ||
                Bitmap.Dispatcher.Invoke(() => Bitmap.TryLock(TimeSpan.FromMilliseconds(Timeout))))
            {
                IsLocked = true;
                return true;
            }

            return false;
        }
        public void Unlock()
        {
            if (Bitmap is null)
                return;

            if (IsLocked)
            {
                if (Bitmap.Dispatcher.CheckAccess())
                    Bitmap.Unlock();
                else
                    Bitmap.Dispatcher.Invoke(() => Bitmap.Unlock());
            }
        }

        public void AddDirtyRect(Int32Rect Rect)
        {
            if (Bitmap is null)
                return;

            if (IsLocked)
                Bitmap.Dispatcher.InvokeSync(() => Bitmap.AddDirtyRect(Rect));
        }
        public void AddDirtyRect(int X, int Y, int Width, int Height)
            => AddDirtyRect(new Int32Rect(X, Y, Width, Height));
        public void AddDirtyRect(Point<int> Point, int Width, int Height)
            => AddDirtyRect(new Int32Rect(Point.X, Point.Y, Width, Height));
        public void AddDirtyRect(int X, int Y, Size<int> Size)
            => AddDirtyRect(new Int32Rect(X, Y, Size.Width, Size.Height));
        public void AddDirtyRect(Point<int> Point, Size<int> Size)
            => AddDirtyRect(new Int32Rect(Point.X, Point.Y, Size.Width, Size.Height));

        public static implicit operator ImageSource(BitmapContext Target) => Target?.Bitmap;
        public static implicit operator BitmapContext(BitmapSource Target) => Target is null ? null : new BitmapContext(Target);
        public static implicit operator BitmapContext(WriteableBitmap Target) => Target is null ? null : new BitmapContext(Target);

    }
}
