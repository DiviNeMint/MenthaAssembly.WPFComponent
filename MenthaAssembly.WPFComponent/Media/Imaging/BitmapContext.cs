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

        public BitmapContext(WriteableBitmap Bitmap)
        {
            this.Bitmap = Bitmap;
            if (PixelFormats.Bgr24.Equals(Bitmap.Format))
            {
                Context = new ImageContext<BGR>(Bitmap.PixelWidth, Bitmap.PixelHeight, Bitmap.BackBuffer, Bitmap.BackBufferStride);
            }
            else if (PixelFormats.Bgr32.Equals(Bitmap.Format) ||
                     PixelFormats.Bgra32.Equals(Bitmap.Format) ||
                     PixelFormats.Pbgra32.Equals(Bitmap.Format))
            {
                Context = new ImageContext<BGRA>(Bitmap.PixelWidth, Bitmap.PixelHeight, Bitmap.BackBuffer, Bitmap.BackBufferStride);
            }
            else if (PixelFormats.Rgb24.Equals(Bitmap.Format))
            {
                Context = new ImageContext<RGB>(Bitmap.PixelWidth, Bitmap.PixelHeight, Bitmap.BackBuffer, Bitmap.BackBufferStride);
            }
            else if (PixelFormats.Gray8.Equals(Bitmap.Format))
            {
                Context = new ImageContext<Gray8>(Bitmap.PixelWidth, Bitmap.PixelHeight, Bitmap.BackBuffer, Bitmap.BackBufferStride);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public int Width => Context.Width;

        public int Height => Context.Height;

        public long Stride => Context.Stride;

        public int BitsPerPixel => Context.BitsPerPixel;

        public Type PixelType => Context.PixelType;

        public IntPtr[] Scan0 => Context.Scan0;

        public IReadOnlyPixel this[int X, int Y]
        {
            get => Context[X, Y];
            set => Context[X, Y] = value;
        }

        #region Graphic Processing
        public void DrawLine(Point<int> P0, Point<int> P1, IImageContext Pen)
            => Context.DrawLine(P0, P1, Pen);
        public void DrawLine(Point<int> P0, Point<int> P1, IImageContext Pen, BlendMode Blend)
            => Context.DrawLine(P0, P1, Pen, Blend);

        public void DrawLine(int X0, int Y0, int X1, int Y1, IImageContext Pen)
            => Context.DrawLine(X0, Y0, X1, Y1, Pen);
        public void DrawLine(int X0, int Y0, int X1, int Y1, IImageContext Pen, BlendMode Blend)
            => Context.DrawLine(X0, Y0, X1, Y1, Pen, Blend);

        public void DrawLine<T>(Point<int> P0, Point<int> P1, T Color) where T : unmanaged, IPixel
            => Context.DrawLine(P0, P1, Color);
        public void DrawLine<T>(Point<int> P0, Point<int> P1, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawLine(P0, P1, Color, Blend);

        public void DrawLine<T>(int X0, int Y0, int X1, int Y1, T Color) where T : unmanaged, IPixel
            => Context.DrawLine(X0, Y0, X1, Y1, Color);
        public void DrawLine<T>(int X0, int Y0, int X1, int Y1, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawLine(X0, Y0, X1, Y1, Color, Blend);

        public void DrawLine<T>(Point<int> P0, Point<int> P1, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => Context.DrawLine(P0, P1, Contour, Fill);
        public void DrawLine<T>(Point<int> P0, Point<int> P1, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawLine(P0, P1, Contour, Fill, Blend);

        public void DrawLine<T>(int X0, int Y0, int X1, int Y1, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => Context.DrawLine(X0, Y0, X1, Y1, Contour, Fill);
        public void DrawLine<T>(int X0, int Y0, int X1, int Y1, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawLine(X0, Y0, X1, Y1, Contour, Fill, Blend);

        public void DrawArc(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, IImageContext Pen)
            => Context.DrawArc(Start, End, Center, Rx, Ry, Clockwise, Pen);
        public void DrawArc(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, IImageContext Pen, BlendMode Blend)
            => Context.DrawArc(Start, End, Center, Rx, Ry, Clockwise, Pen, Blend);

        public void DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, IImageContext Pen)
            => Context.DrawArc(Sx, Sy, Ex, Ey, Cx, Cy, Rx, Ry, Clockwise, Pen);
        public void DrawArc(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, IImageContext Pen, BlendMode Blend)
            => Context.DrawArc(Sx, Sy, Ex, Ey, Cx, Cy, Rx, Ry, Clockwise, Pen, Blend);

        public void DrawArc<T>(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, T Color) where T : unmanaged, IPixel
            => Context.DrawArc(Start, End, Center, Rx, Ry, Clockwise, Color);
        public void DrawArc<T>(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawArc(Start, End, Center, Rx, Ry, Clockwise, Color, Blend);

        public void DrawArc<T>(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, T Color) where T : unmanaged, IPixel
            => Context.DrawArc(Sx, Sy, Ex, Ey, Cx, Cy, Rx, Ry, Clockwise, Color);
        public void DrawArc<T>(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawArc(Sx, Sy, Ex, Ey, Cx, Cy, Rx, Ry, Clockwise, Color, Blend);

        public void DrawArc<T>(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => Context.DrawArc(Start, End, Center, Rx, Ry, Clockwise, Contour, Fill);
        public void DrawArc<T>(Point<int> Start, Point<int> End, Point<int> Center, int Rx, int Ry, bool Clockwise, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawArc(Start, End, Center, Rx, Ry, Clockwise, Contour, Fill, Blend);

        public void DrawArc<T>(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => Context.DrawArc(Sx, Sy, Ex, Ey, Cx, Cy, Rx, Ry, Clockwise, Contour, Fill);
        public void DrawArc<T>(int Sx, int Sy, int Ex, int Ey, int Cx, int Cy, int Rx, int Ry, bool Clockwise, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawArc(Sx, Sy, Ex, Ey, Cx, Cy, Rx, Ry, Clockwise, Contour, Fill, Blend);

        public void DrawCurve(IList<int> Points, float Tension, IImageContext Pen)
            => Context.DrawCurve(Points, Tension, Pen);
        public void DrawCurve(IList<int> Points, float Tension, IImageContext Pen, BlendMode Blend)
            => Context.DrawCurve(Points, Tension, Pen, Blend);

        public void DrawCurve(IList<Point<int>> Points, float Tension, IImageContext Pen)
            => Context.DrawCurve(Points, Tension, Pen);
        public void DrawCurve(IList<Point<int>> Points, float Tension, IImageContext Pen, BlendMode Blend)
            => Context.DrawCurve(Points, Tension, Pen, Blend);

        public void DrawCurve<T>(IList<int> Points, float Tension, T Color) where T : unmanaged, IPixel
            => Context.DrawCurve(Points, Tension, Color);
        public void DrawCurve<T>(IList<int> Points, float Tension, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawCurve(Points, Tension, Color, Blend);

        public void DrawCurve<T>(IList<Point<int>> Points, float Tension, T Color) where T : unmanaged, IPixel
            => Context.DrawCurve(Points, Tension, Color);
        public void DrawCurve<T>(IList<Point<int>> Points, float Tension, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawCurve(Points, Tension, Color, Blend);

        public void DrawCurve<T>(IList<int> Points, float Tension, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => Context.DrawCurve(Points, Tension, Contour, Fill);
        public void DrawCurve<T>(IList<int> Points, float Tension, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawCurve(Points, Tension, Contour, Fill, Blend);

        public void DrawCurve<T>(IList<Point<int>> Points, float Tension, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => Context.DrawCurve(Points, Tension, Contour, Fill);
        public void DrawCurve<T>(IList<Point<int>> Points, float Tension, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawCurve(Points, Tension, Contour, Fill, Blend);

        public void DrawCurveClosed(IList<int> Points, float Tension, IImageContext Pen)
            => Context.DrawCurveClosed(Points, Tension, Pen);
        public void DrawCurveClosed(IList<int> Points, float Tension, IImageContext Pen, BlendMode Blend)
            => Context.DrawCurveClosed(Points, Tension, Pen, Blend);

        public void DrawCurveClosed(IList<Point<int>> Points, float Tension, IImageContext Pen)
            => Context.DrawCurveClosed(Points, Tension, Pen);
        public void DrawCurveClosed(IList<Point<int>> Points, float Tension, IImageContext Pen, BlendMode Blend)
            => Context.DrawCurveClosed(Points, Tension, Pen, Blend);

        public void DrawCurveClosed<T>(IList<int> Points, float Tension, T Color) where T : unmanaged, IPixel
            => Context.DrawCurveClosed(Points, Tension, Color);
        public void DrawCurveClosed<T>(IList<int> Points, float Tension, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawCurveClosed(Points, Tension, Color, Blend);

        public void DrawCurveClosed<T>(IList<Point<int>> Points, float Tension, T Color) where T : unmanaged, IPixel
            => Context.DrawCurveClosed(Points, Tension, Color);
        public void DrawCurveClosed<T>(IList<Point<int>> Points, float Tension, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawCurveClosed(Points, Tension, Color, Blend);

        public void DrawCurveClosed<T>(IList<int> Points, float Tension, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => Context.DrawCurveClosed(Points, Tension, Contour, Fill);
        public void DrawCurveClosed<T>(IList<int> Points, float Tension, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawCurveClosed(Points, Tension, Contour, Fill, Blend);

        public void DrawCurveClosed<T>(IList<Point<int>> Points, float Tension, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => Context.DrawCurveClosed(Points, Tension, Contour, Fill);
        public void DrawCurveClosed<T>(IList<Point<int>> Points, float Tension, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawCurveClosed(Points, Tension, Contour, Fill, Blend);

        public void DrawBezier(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, IImageContext Pen)
            => Context.DrawBezier(X1, Y1, Cx1, Cy1, Cx2, Cy2, X2, Y2, Pen);
        public void DrawBezier(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, IImageContext Pen, BlendMode Blend)
            => Context.DrawBezier(X1, Y1, Cx1, Cy1, Cx2, Cy2, X2, Y2, Pen, Blend);

        public void DrawBezier<T>(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, T Color) where T : unmanaged, IPixel
            => Context.DrawBezier(X1, Y1, Cx1, Cy1, Cx2, Cy2, X2, Y2, Color);
        public void DrawBezier<T>(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawBezier(X1, Y1, Cx1, Cy1, Cx2, Cy2, X2, Y2, Color, Blend);

        public void DrawBezier<T>(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => Context.DrawBezier(X1, Y1, Cx1, Cy1, Cx2, Cy2, X2, Y2, Contour, Fill);
        public void DrawBezier<T>(int X1, int Y1, int Cx1, int Cy1, int Cx2, int Cy2, int X2, int Y2, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawBezier(X1, Y1, Cx1, Cy1, Cx2, Cy2, X2, Y2, Contour, Fill, Blend);

        public void DrawBeziers(IList<int> Points, IImageContext Pen)
            => Context.DrawBeziers(Points, Pen);
        public void DrawBeziers(IList<int> Points, IImageContext Pen, BlendMode Blend)
            => Context.DrawBeziers(Points, Pen, Blend);

        public void DrawBeziers<T>(IList<int> Points, T Color) where T : unmanaged, IPixel
            => Context.DrawBeziers(Points, Color);
        public void DrawBeziers<T>(IList<int> Points, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawBeziers(Points, Color, Blend);

        public void DrawBeziers<T>(IList<int> Points, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => Context.DrawBeziers(Points, Contour, Fill);
        public void DrawBeziers<T>(IList<int> Points, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawBeziers(Points, Contour, Fill, Blend);

        public void DrawTriangle(int X1, int Y1, int X2, int Y2, int X3, int Y3, IImageContext Pen)
            => Context.DrawTriangle(X1, Y1, X2, Y2, X3, Y3, Pen);
        public void DrawTriangle(int X1, int Y1, int X2, int Y2, int X3, int Y3, IImageContext Pen, BlendMode Blend)
            => Context.DrawTriangle(X1, Y1, X2, Y2, X3, Y3, Pen, Blend);

        public void DrawTriangle(Point<int> P1, Point<int> P2, Point<int> P3, IImageContext Pen)
            => Context.DrawTriangle(P1, P2, P3, Pen);
        public void DrawTriangle(Point<int> P1, Point<int> P2, Point<int> P3, IImageContext Pen, BlendMode Blend)
            => Context.DrawTriangle(P1, P2, P3, Pen, Blend);

        public void DrawTriangle<T>(int X1, int Y1, int X2, int Y2, int X3, int Y3, T Color) where T : unmanaged, IPixel
            => Context.DrawTriangle(X1, Y1, X2, Y2, X3, Y3, Color);
        public void DrawTriangle<T>(int X1, int Y1, int X2, int Y2, int X3, int Y3, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawTriangle(X1, Y1, X2, Y2, X3, Y3, Color, Blend);

        public void DrawTriangle<T>(Point<int> P1, Point<int> P2, Point<int> P3, T Color) where T : unmanaged, IPixel
            => Context.DrawTriangle(P1, P2, P3, Color);
        public void DrawTriangle<T>(Point<int> P1, Point<int> P2, Point<int> P3, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawTriangle(P1, P2, P3, Color, Blend);

        public void DrawTriangle<T>(int X1, int Y1, int X2, int Y2, int X3, int Y3, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => Context.DrawTriangle(X1, Y1, X2, Y2, X3, Y3, Contour, Fill);
        public void DrawTriangle<T>(int X1, int Y1, int X2, int Y2, int X3, int Y3, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawTriangle(X1, Y1, X2, Y2, X3, Y3, Contour, Fill, Blend);

        public void DrawTriangle<T>(Point<int> P1, Point<int> P2, Point<int> P3, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => Context.DrawTriangle(P1, P2, P3, Contour, Fill);
        public void DrawTriangle<T>(Point<int> P1, Point<int> P2, Point<int> P3, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawTriangle(P1, P2, P3, Contour, Fill, Blend);

        public void DrawRectangle(int X1, int Y1, int X2, int Y2, IImageContext Pen)
            => Context.DrawRectangle(X1, Y1, X2, Y2, Pen);
        public void DrawRectangle(int X1, int Y1, int X2, int Y2, IImageContext Pen, BlendMode Blend)
            => Context.DrawRectangle(X1, Y1, X2, Y2, Pen, Blend);

        public void DrawRectangle(Point<int> P1, Point<int> P2, IImageContext Pen)
            => Context.DrawRectangle(P1, P2, Pen);
        public void DrawRectangle(Point<int> P1, Point<int> P2, IImageContext Pen, BlendMode Blend)
            => Context.DrawRectangle(P1, P2, Pen, Blend);

        public void DrawRectangle<T>(int X1, int Y1, int X2, int Y2, T Color) where T : unmanaged, IPixel
            => Context.DrawRectangle(X1, Y1, X2, Y2, Color);
        public void DrawRectangle<T>(int X1, int Y1, int X2, int Y2, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawRectangle(X1, Y1, X2, Y2, Color, Blend);

        public void DrawRectangle<T>(Point<int> P1, Point<int> P2, T Color) where T : unmanaged, IPixel
            => Context.DrawRectangle(P1, P2, Color);
        public void DrawRectangle<T>(Point<int> P1, Point<int> P2, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawRectangle(P1, P2, Color, Blend);

        public void DrawRectangle<T>(int X1, int Y1, int X2, int Y2, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => Context.DrawRectangle(X1, Y1, X2, Y2, Contour, Fill);
        public void DrawRectangle<T>(int X1, int Y1, int X2, int Y2, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawRectangle(X1, Y1, X2, Y2, Contour, Fill, Blend);

        public void DrawRectangle<T>(Point<int> P1, Point<int> P2, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => Context.DrawRectangle(P1, P2, Contour, Fill);
        public void DrawRectangle<T>(Point<int> P1, Point<int> P2, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawRectangle(P1, P2, Contour, Fill, Blend);

        public void DrawQuad(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, IImageContext Pen)
            => Context.DrawQuad(X1, Y1, X2, Y2, X3, Y3, X4, Y4, Pen);
        public void DrawQuad(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, IImageContext Pen, BlendMode Blend)
            => Context.DrawQuad(X1, Y1, X2, Y2, X3, Y3, X4, Y4, Pen, Blend);

        public void DrawQuad(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, IImageContext Pen)
            => Context.DrawQuad(P1, P2, P3, P4, Pen);
        public void DrawQuad(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, IImageContext Pen, BlendMode Blend)
            => Context.DrawQuad(P1, P2, P3, P4, Pen, Blend);

        public void DrawQuad<T>(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, T Color) where T : unmanaged, IPixel
            => Context.DrawQuad(X1, Y1, X2, Y2, X3, Y3, X4, Y4, Color);
        public void DrawQuad<T>(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawQuad(X1, Y1, X2, Y2, X3, Y3, X4, Y4, Color, Blend);

        public void DrawQuad<T>(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, T Color) where T : unmanaged, IPixel
            => Context.DrawQuad(P1, P2, P3, P4, Color);
        public void DrawQuad<T>(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawQuad(P1, P2, P3, P4, Color, Blend);

        public void DrawQuad<T>(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => Context.DrawQuad(X1, Y1, X2, Y2, X3, Y3, X4, Y4, Contour, Fill);
        public void DrawQuad<T>(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawQuad(X1, Y1, X2, Y2, X3, Y3, X4, Y4, Contour, Fill, Blend);

        public void DrawQuad<T>(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => Context.DrawQuad(P1, P2, P3, P4, Contour, Fill);
        public void DrawQuad<T>(Point<int> P1, Point<int> P2, Point<int> P3, Point<int> P4, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawQuad(P1, P2, P3, P4, Contour, Fill, Blend);

        public void DrawEllipse(Bound<int> Bound, IImageContext Pen)
            => Context.DrawEllipse(Bound, Pen);
        public void DrawEllipse(Bound<int> Bound, IImageContext Pen, BlendMode Blend)
            => Context.DrawEllipse(Bound, Pen, Blend);

        public void DrawEllipse(Point<int> Center, int Rx, int Ry, IImageContext Pen)
            => Context.DrawEllipse(Center, Rx, Ry, Pen);
        public void DrawEllipse(Point<int> Center, int Rx, int Ry, IImageContext Pen, BlendMode Blend)
            => Context.DrawEllipse(Center, Rx, Ry, Pen, Blend);

        public void DrawEllipse(int Cx, int Cy, int Rx, int Ry, IImageContext Pen)
            => Context.DrawEllipse(Cx, Cy, Rx, Ry, Pen);
        public void DrawEllipse(int Cx, int Cy, int Rx, int Ry, IImageContext Pen, BlendMode Blend)
            => Context.DrawEllipse(Cx, Cy, Rx, Ry, Pen, Blend);

        public void DrawEllipse<T>(Bound<int> Bound, T Color) where T : unmanaged, IPixel
            => Context.DrawEllipse(Bound, Color);
        public void DrawEllipse<T>(Bound<int> Bound, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawEllipse(Bound, Color, Blend);

        public void DrawEllipse<T>(Point<int> Center, int Rx, int Ry, T Color) where T : unmanaged, IPixel
            => Context.DrawEllipse(Center, Rx, Ry, Color);
        public void DrawEllipse<T>(Point<int> Center, int Rx, int Ry, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawEllipse(Center, Rx, Ry, Color, Blend);

        public void DrawEllipse<T>(int Cx, int Cy, int Rx, int Ry, T Color) where T : unmanaged, IPixel
            => Context.DrawEllipse(Cx, Cy, Rx, Ry, Color);
        public void DrawEllipse<T>(int Cx, int Cy, int Rx, int Ry, T Color, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawEllipse(Cx, Cy, Rx, Ry, Color, Blend);

        public void DrawEllipse<T>(Bound<int> Bound, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => Context.DrawEllipse(Bound, Contour, Fill);
        public void DrawEllipse<T>(Bound<int> Bound, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawEllipse(Bound, Contour, Fill, Blend);

        public void DrawEllipse<T>(Point<int> Center, int Rx, int Ry, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => Context.DrawEllipse(Center, Rx, Ry, Contour, Fill);
        public void DrawEllipse<T>(Point<int> Center, int Rx, int Ry, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawEllipse(Center, Rx, Ry, Contour, Fill, Blend);

        public void DrawEllipse<T>(int Cx, int Cy, int Rx, int Ry, ImageContour Contour, T Fill) where T : unmanaged, IPixel
            => Context.DrawEllipse(Cx, Cy, Rx, Ry, Contour, Fill);
        public void DrawEllipse<T>(int Cx, int Cy, int Rx, int Ry, ImageContour Contour, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawEllipse(Cx, Cy, Rx, Ry, Contour, Fill, Blend);

        public void FillEllipse<T>(Bound<int> Bound, T Fill) where T : unmanaged, IPixel
            => Context.FillEllipse(Bound, Fill);
        public void FillEllipse<T>(Bound<int> Bound, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.FillEllipse(Bound, Fill, Blend);

        public void FillEllipse<T>(Point<int> Center, int Rx, int Ry, T Fill) where T : unmanaged, IPixel
            => Context.FillEllipse(Center, Rx, Ry, Fill);
        public void FillEllipse<T>(Point<int> Center, int Rx, int Ry, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.FillEllipse(Center, Rx, Ry, Fill, Blend);

        public void FillEllipse<T>(int Cx, int Cy, int Rx, int Ry, T Fill) where T : unmanaged, IPixel
            => Context.FillEllipse(Cx, Cy, Rx, Ry, Fill);
        public void FillEllipse<T>(int Cx, int Cy, int Rx, int Ry, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.FillEllipse(Cx, Cy, Rx, Ry, Fill, Blend);

        public void DrawRegularPolygon(Point<int> Center, double Radius, int VertexNum, IImageContext Pen, double StartAngle)
            => Context.DrawRegularPolygon(Center, Radius, VertexNum, Pen, StartAngle);
        public void DrawRegularPolygon(Point<int> Center, double Radius, int VertexNum, IImageContext Pen, BlendMode Blend, double StartAngle)
            => Context.DrawRegularPolygon(Center, Radius, VertexNum, Pen, Blend, StartAngle);

        public void DrawRegularPolygon(int Cx, int Cy, double Radius, int VertexNum, IImageContext Pen, double StartAngle)
            => Context.DrawRegularPolygon(Cx, Cy, Radius, VertexNum, Pen, StartAngle);
        public void DrawRegularPolygon(int Cx, int Cy, double Radius, int VertexNum, IImageContext Pen, BlendMode Blend, double StartAngle)
            => Context.DrawRegularPolygon(Cx, Cy, Radius, VertexNum, Pen, Blend, StartAngle);

        public void DrawRegularPolygon<T>(Point<int> Center, double Radius, int VertexNum, T Color, double StartAngle) where T : unmanaged, IPixel
            => Context.DrawRegularPolygon(Center, Radius, VertexNum, Color, StartAngle);
        public void DrawRegularPolygon<T>(Point<int> Center, double Radius, int VertexNum, T Color, BlendMode Blend, double StartAngle) where T : unmanaged, IPixel
            => Context.DrawRegularPolygon(Center, Radius, VertexNum, Color, Blend, StartAngle);

        public void DrawRegularPolygon<T>(int Cx, int Cy, double Radius, int VertexNum, T Color, double StartAngle) where T : unmanaged, IPixel
            => Context.DrawRegularPolygon(Cx, Cy, Radius, VertexNum, Color, StartAngle);
        public void DrawRegularPolygon<T>(int Cx, int Cy, double Radius, int VertexNum, T Color, BlendMode Blend, double StartAngle) where T : unmanaged, IPixel
            => Context.DrawRegularPolygon(Cx, Cy, Radius, VertexNum, Color, Blend, StartAngle);

        public void DrawRegularPolygon<T>(Point<int> Center, double Radius, int VertexNum, ImageContour Contour, T Fill, double StartAngle) where T : unmanaged, IPixel
            => Context.DrawRegularPolygon(Center, Radius, VertexNum, Contour, Fill, StartAngle);
        public void DrawRegularPolygon<T>(Point<int> Center, double Radius, int VertexNum, ImageContour Contour, T Fill, BlendMode Blend, double StartAngle) where T : unmanaged, IPixel
            => Context.DrawRegularPolygon(Center, Radius, VertexNum, Contour, Fill, Blend, StartAngle);

        public void DrawRegularPolygon<T>(int Cx, int Cy, double Radius, int VertexNum, ImageContour Contour, T Fill, double StartAngle) where T : unmanaged, IPixel
            => Context.DrawRegularPolygon(Cx, Cy, Radius, VertexNum, Contour, Fill, StartAngle);
        public void DrawRegularPolygon<T>(int Cx, int Cy, double Radius, int VertexNum, ImageContour Contour, T Fill, BlendMode Blend, double StartAngle) where T : unmanaged, IPixel
            => Context.DrawRegularPolygon(Cx, Cy, Radius, VertexNum, Contour, Fill, Blend, StartAngle);

        public void DrawStamp(Point<int> Position, IImageContext Stamp)
            => Context.DrawStamp(Position, Stamp);
        public void DrawStamp(Point<int> Position, IImageContext Stamp, BlendMode Blend)
            => Context.DrawStamp(Position, Stamp, Blend);

        public void DrawStamp(int X, int Y, IImageContext Stamp)
            => Context.DrawStamp(X, Y, Stamp);
        public void DrawStamp(int X, int Y, IImageContext Stamp, BlendMode Blend)
            => Context.DrawStamp(X, Y, Stamp, Blend);

        public void FillPolygon<T>(IEnumerable<Point<int>> Vertices, T Fill, int OffsetX, int OffsetY) where T : unmanaged, IPixel
            => Context.FillPolygon(Vertices, Fill, OffsetX, OffsetY);
        public void FillPolygon<T>(IEnumerable<Point<int>> Vertices, T Fill, int OffsetX, int OffsetY, BlendMode Blend) where T : unmanaged, IPixel
            => Context.FillPolygon(Vertices, Fill, OffsetX, OffsetY, Blend);

        public void FillPolygon<T>(IEnumerable<int> VerticeDatas, T Fill, int OffsetX, int OffsetY) where T : unmanaged, IPixel
            => Context.FillPolygon(VerticeDatas, Fill, OffsetX, OffsetY);
        public void FillPolygon<T>(IEnumerable<int> VerticeDatas, T Fill, int OffsetX, int OffsetY, BlendMode Blend) where T : unmanaged, IPixel
            => Context.FillPolygon(VerticeDatas, Fill, OffsetX, OffsetY, Blend);

        public void FillContour<T>(IImageContour Contour, T Fill, double OffsetX, double OffsetY) where T : unmanaged, IPixel
            => Context.FillContour(Contour, Fill, OffsetX, OffsetY);
        public void FillContour<T>(IImageContour Contour, T Fill, double OffsetX, double OffsetY, BlendMode Blend) where T : unmanaged, IPixel
            => Context.FillContour(Contour, Fill, OffsetX, OffsetY, Blend);

        public void SeedFill<T>(Point<int> SeedPoint, T Fill, ImagePredicate Predicate) where T : unmanaged, IPixel
            => Context.SeedFill(SeedPoint, Fill, Predicate);
        public void SeedFill<T>(Point<int> SeedPoint, T Fill, BlendMode Blend, ImagePredicate Predicate) where T : unmanaged, IPixel
            => Context.SeedFill(SeedPoint, Fill, Blend, Predicate);

        public void SeedFill<T>(int SeedX, int SeedY, T Fill, ImagePredicate Predicate) where T : unmanaged, IPixel
            => Context.SeedFill(SeedX, SeedY, Fill, Predicate);
        public void SeedFill<T>(int SeedX, int SeedY, T Fill, BlendMode Blend, ImagePredicate Predicate) where T : unmanaged, IPixel
            => Context.SeedFill(SeedX, SeedY, Fill, Blend, Predicate);

        public void DrawText<T>(int X, int Y, string Text, int CharSize, T Fill) where T : unmanaged, IPixel
            => Context.DrawText(X, Y, Text, CharSize, Fill);
        public void DrawText<T>(int X, int Y, string Text, int CharSize, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawText(X, Y, Text, CharSize, Fill, Blend);

        public void DrawText<T>(int X, int Y, string Text, int CharSize, T Fill, double Angle, FontWeightType Weight, bool Italic) where T : unmanaged, IPixel
            => Context.DrawText(X, Y, Text, CharSize, Fill, Angle, Weight, Italic);
        public void DrawText<T>(int X, int Y, string Text, int CharSize, T Fill, BlendMode Blend, double Angle, FontWeightType Weight, bool Italic) where T : unmanaged, IPixel
            => Context.DrawText(X, Y, Text, CharSize, Fill, Blend, Angle, Weight, Italic);

        public void DrawText<T>(int X, int Y, string Text, string FontName, int CharSize, T Fill) where T : unmanaged, IPixel
            => Context.DrawText(X, Y, Text, FontName, CharSize, Fill);
        public void DrawText<T>(int X, int Y, string Text, string FontName, int CharSize, T Fill, BlendMode Blend) where T : unmanaged, IPixel
            => Context.DrawText(X, Y, Text, FontName, CharSize, Fill, Blend);

        public void DrawText<T>(int X, int Y, string Text, string FontName, int CharSize, T Fill, double Angle, FontWeightType Weight, bool Italic) where T : unmanaged, IPixel
            => Context.DrawText(X, Y, Text, FontName, CharSize, Fill, Angle, Weight, Italic);
        public void DrawText<T>(int X, int Y, string Text, string FontName, int CharSize, T Fill, BlendMode Blend, double Angle, FontWeightType Weight, bool Italic) where T : unmanaged, IPixel
            => Context.DrawText(X, Y, Text, FontName, CharSize, Fill, Blend, Angle, Weight, Italic);

        #endregion

        #region Transform Processing
        public ImageContext<T> Rotate<T>(double Angle, InterpolationTypes Interpolation) where T : unmanaged, IPixel
            => Context.Rotate<T>(Angle, Interpolation);
        public ImageContext<T> Rotate<T>(double Angle, InterpolationTypes Interpolation, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.Rotate<T>(Angle, Interpolation, Options);

        public ImageContext<T> Resize<T>(int Width, int Height, InterpolationTypes Interpolation) where T : unmanaged, IPixel
            => Context.Resize<T>(Width, Height, Interpolation);
        public ImageContext<T> Resize<T>(int Width, int Height, InterpolationTypes Interpolation, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.Resize<T>(Width, Height, Interpolation, Options);

        public ImageContext<T> Flip<T>(FlipMode Mode) where T : unmanaged, IPixel
            => Context.Flip<T>(Mode);
        public ImageContext<T> Flip<T>(FlipMode Mode, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.Flip<T>(Mode, Options);

        public ImageContext<T> Convolute<T>(ConvoluteKernel Kernel) where T : unmanaged, IPixel
            => Context.Convolute<T>(Kernel);
        public ImageContext<T> Convolute<T>(ConvoluteKernel Kernel, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.Convolute<T>(Kernel, Options);

        public ImageContext<T> Filter<T>(ImageFilter Filter) where T : unmanaged, IPixel
            => Context.Filter<T>(Filter);
        public ImageContext<T> Filter<T>(ImageFilter Filter, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.Filter<T>(Filter, Options);

        public ImageContext<T> Quantizate<T>(QuantizationTypes Type, int Count) where T : unmanaged, IPixel
            => Context.Quantizate<T>(Type, Count);
        public ImageContext<T> Quantizate<T>(QuantizationTypes Type, int Count, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.Quantizate<T>(Type, Count, Options);

        public ImageContext<T> Binarize<T>(ImageThreshold Threshold) where T : unmanaged, IPixel
            => Context.Binarize<T>(Threshold);
        public ImageContext<T> Binarize<T>(ImageThreshold Threshold, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.Binarize<T>(Threshold, Options);
        public ImageContext<T> Binarize<T>(ImagePredicate Predicate) where T : unmanaged, IPixel
            => Context.Binarize<T>(Predicate);
        public ImageContext<T> Binarize<T>(ImagePredicate Predicate, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.Binarize<T>(Predicate, Options);
        public ImageContext<T, U> Binarize<T, U>(ImageThreshold Threshold) where T : unmanaged, IPixel where U : unmanaged, IPixelIndexed
            => Context.Binarize<T, U>(Threshold);
        public ImageContext<T, U> Binarize<T, U>(ImageThreshold Threshold, ParallelOptions Options) where T : unmanaged, IPixel where U : unmanaged, IPixelIndexed
            => Context.Binarize<T, U>(Threshold, Options);
        public ImageContext<T, U> Binarize<T, U>(ImagePredicate Predicate) where T : unmanaged, IPixel where U : unmanaged, IPixelIndexed
            => Context.Binarize<T, U>(Predicate);
        public ImageContext<T, U> Binarize<T, U>(ImagePredicate Predicate, ParallelOptions Options) where T : unmanaged, IPixel where U : unmanaged, IPixelIndexed
            => Context.Binarize<T, U>(Predicate, Options);

        public ImageContext<T> Crop<T>(int X, int Y, int Width, int Height) where T : unmanaged, IPixel
            => Context.Crop<T>(X, Y, Width, Height);
        public ImageContext<T> Crop<T>(int X, int Y, int Width, int Height, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.Crop<T>(X, Y, Width, Height, Options);

        public ImageContext<T> Cast<T>() where T : unmanaged, IPixel
            => Context.Cast<T>();
        public ImageContext<T> Cast<T>(ParallelOptions Options) where T : unmanaged, IPixel
            => Context.Cast<T>(Options);
        public ImageContext<T, U> Cast<T, U>() where T : unmanaged, IPixel where U : unmanaged, IPixelIndexed
            => Context.Cast<T, U>();
        public ImageContext<T, U> Cast<T, U>(ParallelOptions Options) where T : unmanaged, IPixel where U : unmanaged, IPixelIndexed
            => Context.Cast<T, U>(Options);

        public void Clear<T>(T Color) where T : unmanaged, IPixel
            => Context.Clear(Color);
        public void Clear<T>(T Color, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.Clear(Color, Options);

        #endregion

        #region Buffer Processing
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0) where T : unmanaged, IPixel
            => Context.BlockCopy(X, Y, Width, Height, Dest0);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.BlockCopy(X, Y, Width, Height, Dest0, Options);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, long DestStride) where T : unmanaged, IPixel
            => Context.BlockCopy(X, Y, Width, Height, Dest0, DestStride);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.BlockCopy(X, Y, Width, Height, Dest0, DestStride, Options);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, int DestOffset, long DestStride) where T : unmanaged, IPixel
            => Context.BlockCopy(X, Y, Width, Height, Dest0, DestOffset, DestStride);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T[] Dest0, int DestOffset, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.BlockCopy(X, Y, Width, Height, Dest0, DestOffset, DestStride, Options);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0) where T : unmanaged, IPixel
            => Context.BlockCopy(X, Y, Width, Height, Dest0);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.BlockCopy(X, Y, Width, Height, Dest0, Options);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, long DestStride) where T : unmanaged, IPixel
            => Context.BlockCopy(X, Y, Width, Height, Dest0, DestStride);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, T* Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.BlockCopy(X, Y, Width, Height, Dest0, DestStride, Options);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0) where T : unmanaged, IPixel
            => Context.BlockCopy<T>(X, Y, Width, Height, Dest0);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.BlockCopy<T>(X, Y, Width, Height, Dest0, Options);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride) where T : unmanaged, IPixel
            => Context.BlockCopy<T>(X, Y, Width, Height, Dest0, DestStride);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.BlockCopy<T>(X, Y, Width, Height, Dest0, DestStride, Options);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride) where T : unmanaged, IPixel
            => Context.BlockCopy<T>(X, Y, Width, Height, Dest0, DestOffset, DestStride);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte[] Dest0, int DestOffset, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.BlockCopy<T>(X, Y, Width, Height, Dest0, DestOffset, DestStride, Options);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0) where T : unmanaged, IPixel
            => Context.BlockCopy<T>(X, Y, Width, Height, Dest0);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.BlockCopy<T>(X, Y, Width, Height, Dest0, Options);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride) where T : unmanaged, IPixel
            => Context.BlockCopy<T>(X, Y, Width, Height, Dest0, DestStride);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, IntPtr Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.BlockCopy<T>(X, Y, Width, Height, Dest0, DestStride, Options);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0) where T : unmanaged, IPixel
            => Context.BlockCopy<T>(X, Y, Width, Height, Dest0);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.BlockCopy<T>(X, Y, Width, Height, Dest0, Options);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, long DestStride) where T : unmanaged, IPixel
            => Context.BlockCopy<T>(X, Y, Width, Height, Dest0, DestStride);
        public void BlockCopy<T>(int X, int Y, int Width, int Height, byte* Dest0, long DestStride, ParallelOptions Options) where T : unmanaged, IPixel
            => Context.BlockCopy<T>(X, Y, Width, Height, Dest0, DestStride, Options);

        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB)
            => Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB);
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, ParallelOptions Options)
            => Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, Options);
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride)
            => Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestStride);
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride, ParallelOptions Options)
            => Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestStride, Options);
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride)
            => Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestOffset, DestStride);
        public void BlockCopy3(int X, int Y, int Width, int Height, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride, ParallelOptions Options)
            => Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestOffset, DestStride, Options);
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB)
            => Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB);
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, ParallelOptions Options)
            => Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, Options);
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride)
            => Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestStride);
        public void BlockCopy3(int X, int Y, int Width, int Height, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride, ParallelOptions Options)
            => Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestStride, Options);
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB)
            => Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB);
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, ParallelOptions Options)
            => Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, Options);
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, long DestStride)
            => Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestStride);
        public void BlockCopy3(int X, int Y, int Width, int Height, byte* DestR, byte* DestG, byte* DestB, long DestStride, ParallelOptions Options)
            => Context.BlockCopy3(X, Y, Width, Height, DestR, DestG, DestB, DestStride, Options);

        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB)
            => Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB);
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, ParallelOptions Options)
            => Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, Options);
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride)
            => Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestStride);
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, long DestStride, ParallelOptions Options)
            => Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestStride, Options);
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride)
            => Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestOffset, DestStride);
        public void BlockCopy4(int X, int Y, int Width, int Height, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset, long DestStride, ParallelOptions Options)
            => Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestOffset, DestStride, Options);
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB)
            => Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB);
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, ParallelOptions Options)
            => Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, Options);
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride)
            => Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestStride);
        public void BlockCopy4(int X, int Y, int Width, int Height, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB, long DestStride, ParallelOptions Options)
            => Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestStride, Options);
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB)
            => Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB);
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, ParallelOptions Options)
            => Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, Options);
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, long DestStride)
            => Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestStride);
        public void BlockCopy4(int X, int Y, int Width, int Height, byte* DestA, byte* DestR, byte* DestG, byte* DestB, long DestStride, ParallelOptions Options)
            => Context.BlockCopy4(X, Y, Width, Height, DestA, DestR, DestG, DestB, DestStride, Options);

        public void ScanLineCopy<T>(int X, int Y, int Length, T* Dest0) where T : unmanaged, IPixel
            => Context.ScanLineCopy(X, Y, Length, Dest0);
        public void ScanLineCopy<T>(int X, int Y, int Length, T[] Dest0) where T : unmanaged, IPixel
            => Context.ScanLineCopy(X, Y, Length, Dest0);
        public void ScanLineCopy<T>(int X, int Y, int Length, T[] Dest0, int DestOffset) where T : unmanaged, IPixel
            => Context.ScanLineCopy(X, Y, Length, Dest0, DestOffset);
        public void ScanLineCopy<T>(int X, int Y, int Length, byte[] Dest0) where T : unmanaged, IPixel
            => Context.ScanLineCopy<T>(X, Y, Length, Dest0);
        public void ScanLineCopy<T>(int X, int Y, int Length, byte[] Dest0, int DestOffset) where T : unmanaged, IPixel
            => Context.ScanLineCopy<T>(X, Y, Length, Dest0, DestOffset);
        public void ScanLineCopy<T>(int X, int Y, int Length, IntPtr Dest0) where T : unmanaged, IPixel
            => Context.ScanLineCopy<T>(X, Y, Length, Dest0);
        public void ScanLineCopy<T>(int X, int Y, int Length, byte* Dest0) where T : unmanaged, IPixel
            => Context.ScanLineCopy<T>(X, Y, Length, Dest0);

        public void ScanLineCopy3(int X, int Y, int Length, byte[] DestR, byte[] DestG, byte[] DestB)
            => Context.ScanLineCopy3(X, Y, Length, DestR, DestG, DestB);
        public void ScanLineCopy3(int X, int Y, int Length, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset)
            => Context.ScanLineCopy3(X, Y, Length, DestR, DestG, DestB, DestOffset);
        public void ScanLineCopy3(int X, int Y, int Length, IntPtr DestR, IntPtr DestG, IntPtr DestB)
            => Context.ScanLineCopy3(X, Y, Length, DestR, DestG, DestB);
        public void ScanLineCopy3(int X, int Y, int Length, byte* DestR, byte* DestG, byte* DestB)
            => Context.ScanLineCopy3(X, Y, Length, DestR, DestG, DestB);

        public void ScanLineCopy4(int X, int Y, int Length, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB)
            => Context.ScanLineCopy4(X, Y, Length, DestA, DestR, DestG, DestB);
        public void ScanLineCopy4(int X, int Y, int Length, byte[] DestA, byte[] DestR, byte[] DestG, byte[] DestB, int DestOffset)
            => Context.ScanLineCopy4(X, Y, Length, DestA, DestR, DestG, DestB, DestOffset);
        public void ScanLineCopy4(int X, int Y, int Length, IntPtr DestA, IntPtr DestR, IntPtr DestG, IntPtr DestB)
            => Context.ScanLineCopy4(X, Y, Length, DestA, DestR, DestG, DestB);
        public void ScanLineCopy4(int X, int Y, int Length, byte* DestA, byte* DestR, byte* DestG, byte* DestB)
            => Context.ScanLineCopy4(X, Y, Length, DestA, DestR, DestG, DestB);

        #endregion

        public PixelAdapter<T> GetAdapter<T>(int X, int Y) where T : unmanaged, IPixel
            => Context.GetAdapter<T>(X, Y);
        public IPixelAdapter GetAdapter(int X, int Y)
            => Context.GetAdapter(X, Y);

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
                Bitmap.Invoke(() => Bitmap.AddDirtyRect(Rect));
        }
        public void AddDirtyRect(int X, int Y, int Width, int Height)
            => AddDirtyRect(new Int32Rect(X, Y, Width, Height));
        public void AddDirtyRect(Point<int> Point, int Width, int Height)
            => AddDirtyRect(new Int32Rect(Point.X, Point.Y, Width, Height));
        public void AddDirtyRect(int X, int Y, Size<int> Size)
            => AddDirtyRect(new Int32Rect(X, Y, Size.Width, Size.Height));
        public void AddDirtyRect(Point<int> Point, Size<int> Size)
            => AddDirtyRect(new Int32Rect(Point.X, Point.Y, Size.Width, Size.Height));

        public IImageContext Clone()
            => Context.Clone();
        object ICloneable.Clone()
            => Clone();

        public static implicit operator ImageSource(BitmapContext Target) => Target?.Bitmap;
        public static implicit operator BitmapContext(WriteableBitmap Target) => Target is null ? null : new BitmapContext(Target);

    }
}
