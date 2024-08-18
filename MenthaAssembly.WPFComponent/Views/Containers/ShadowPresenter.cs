using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    /// <summary>
    /// Copy from Microsoft.Windows.Themes.SystemDropShadowChrome
    /// </summary>
    public sealed class ShadowPresenter : Decorator
    {
        /// <summary>
        /// DependencyProperty for <see cref="Color" /> property.
        /// </summary>
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(nameof(Color), typeof(Color), typeof(ShadowPresenter),
                new FrameworkPropertyMetadata(Color.FromArgb(0x71, 0x00, 0x00, 0x00), FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(ClearBrushes)));
        /// <summary>
        /// The Color property defines the Color used to fill the shadow region.
        /// </summary>
        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        /// <summary>
        /// DependencyProperty for <see cref="CornerRadius" /> property.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(ShadowPresenter),
                new FrameworkPropertyMetadata(new CornerRadius(), FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(ClearBrushes)), new ValidateValueCallback(IsCornerRadiusValid));
        /// <summary>
        /// The CornerRadius property defines the CornerRadius of the object casting the shadow.
        /// </summary>
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        static ShadowPresenter()
        {
            MarginProperty.OverrideMetadata(typeof(ShadowPresenter),
                new FrameworkPropertyMetadata(new Thickness(0d, 0d, 5d, 5d), FrameworkPropertyMetadataOptions.AffectsMeasure));
        }

        private static bool IsCornerRadiusValid(object value)
        {
            CornerRadius cr = (CornerRadius)value;
            return !(cr.TopLeft < 0.0 || cr.TopRight < 0.0 || cr.BottomLeft < 0.0 || cr.BottomRight < 0.0 ||
                     double.IsNaN(cr.TopLeft) || double.IsNaN(cr.TopRight) || double.IsNaN(cr.BottomLeft) || double.IsNaN(cr.BottomRight) ||
                     double.IsInfinity(cr.TopLeft) || double.IsInfinity(cr.TopRight) || double.IsInfinity(cr.BottomLeft) || double.IsInfinity(cr.BottomRight));
        }
        private static void ClearBrushes(DependencyObject o, DependencyPropertyChangedEventArgs e)
            => ((ShadowPresenter)o)._brushes = null;

        private const double ShadowDepth = 5;
        protected override void OnRender(DrawingContext Context)
        {
            CornerRadius cornerRadius = CornerRadius;
            Rect Bounds = new(new Point(ShadowDepth, ShadowDepth),
                              new Size(RenderSize.Width, RenderSize.Height));

            Color color = Color;
            if (Bounds.Width > 0 && Bounds.Height > 0 && color.A > 0)
            {
                // The shadow is drawn with a dark center the size of the shadow bounds
                // deflated by shadow depth on each side.
                double centerWidth = Bounds.Right - Bounds.Left - 2 * ShadowDepth;
                double centerHeight = Bounds.Bottom - Bounds.Top - 2 * ShadowDepth;

                // Clamp corner radii to be less than 1/2 the side of the inner shadow bounds 
                double maxRadius = Math.Min(centerWidth * 0.5, centerHeight * 0.5);
                cornerRadius.TopLeft = Math.Min(cornerRadius.TopLeft, maxRadius);
                cornerRadius.TopRight = Math.Min(cornerRadius.TopRight, maxRadius);
                cornerRadius.BottomLeft = Math.Min(cornerRadius.BottomLeft, maxRadius);
                cornerRadius.BottomRight = Math.Min(cornerRadius.BottomRight, maxRadius);

                // Get the brushes for the 9 regions
                Brush[] brushes = GetBrushes(color, cornerRadius);

                // Snap grid to device pixels
                double centerTop = Bounds.Top + ShadowDepth;
                double centerLeft = Bounds.Left + ShadowDepth;
                double centerRight = Bounds.Right - ShadowDepth;
                double centerBottom = Bounds.Bottom - ShadowDepth;

                // Because of different corner radii there are 6 potential x (or y) lines to snap to
                double[] guidelineSetX = [ centerLeft,
                                           centerLeft + cornerRadius.TopLeft,
                                           centerRight - cornerRadius.TopRight,
                                           centerLeft + cornerRadius.BottomLeft,
                                           centerRight - cornerRadius.BottomRight,
                                           centerRight];

                double[] guidelineSetY = [ centerTop,
                                           centerTop + cornerRadius.TopLeft,
                                           centerTop + cornerRadius.TopRight,
                                           centerBottom - cornerRadius.BottomLeft,
                                           centerBottom - cornerRadius.BottomRight,
                                           centerBottom];

                Context.PushGuidelineSet(new GuidelineSet(guidelineSetX, guidelineSetY));

                // The corner rectangles are drawn drawn ShadowDepth pixels bigger to 
                // account for the blur
                cornerRadius.TopLeft += ShadowDepth;
                cornerRadius.TopRight += ShadowDepth;
                cornerRadius.BottomLeft += ShadowDepth;
                cornerRadius.BottomRight += ShadowDepth;


                // Draw Top row
                Rect topLeft = new(Bounds.Left, Bounds.Top, cornerRadius.TopLeft, cornerRadius.TopLeft);
                Context.DrawRectangle(brushes[TopLeft], null, topLeft);

                double topWidth = guidelineSetX[2] - guidelineSetX[1];
                if (topWidth > 0)
                {
                    Rect top = new(guidelineSetX[1], Bounds.Top, topWidth, ShadowDepth);
                    Context.DrawRectangle(brushes[Top], null, top);
                }

                Rect topRight = new(guidelineSetX[2], Bounds.Top, cornerRadius.TopRight, cornerRadius.TopRight);
                Context.DrawRectangle(brushes[TopRight], null, topRight);

                // Middle row
                double leftHeight = guidelineSetY[3] - guidelineSetY[1];
                if (leftHeight > 0)
                {
                    Rect left = new(Bounds.Left, guidelineSetY[1], ShadowDepth, leftHeight);
                    Context.DrawRectangle(brushes[Left], null, left);
                }

                double rightHeight = guidelineSetY[4] - guidelineSetY[2];
                if (rightHeight > 0)
                {
                    Rect right = new(guidelineSetX[5], guidelineSetY[2], ShadowDepth, rightHeight);
                    Context.DrawRectangle(brushes[Right], null, right);
                }

                // Bottom row
                Rect bottomLeft = new(Bounds.Left, guidelineSetY[3], cornerRadius.BottomLeft, cornerRadius.BottomLeft);
                Context.DrawRectangle(brushes[BottomLeft], null, bottomLeft);

                double bottomWidth = guidelineSetX[4] - guidelineSetX[3];
                if (bottomWidth > 0)
                {
                    Rect bottom = new(guidelineSetX[3], guidelineSetY[5], bottomWidth, ShadowDepth);
                    Context.DrawRectangle(brushes[Bottom], null, bottom);
                }

                Rect bottomRight = new(guidelineSetX[4], guidelineSetY[4], cornerRadius.BottomRight, cornerRadius.BottomRight);
                Context.DrawRectangle(brushes[BottomRight], null, bottomRight);


                // Fill Center

                // Because the heights of the top/bottom rects and widths of the left/right rects are fixed
                // and the corner rects are drawn with the size of the corner, the center 
                // may not be a square.  In this case, create a path to fill the area

                // When the target object's corner radius is 0, only need to draw one rect
                if (cornerRadius.TopLeft == ShadowDepth &&
                    cornerRadius.TopLeft == cornerRadius.TopRight &&
                    cornerRadius.TopLeft == cornerRadius.BottomLeft &&
                    cornerRadius.TopLeft == cornerRadius.BottomRight)
                {
                    // All corners of target are 0, render one large rectangle
                    Rect center = new(guidelineSetX[0], guidelineSetY[0], centerWidth, centerHeight);
                    Context.DrawRectangle(brushes[Center], null, center);
                }
                else
                {
                    // If the corner radius is TL=2, TR=1, BL=0, BR=2 the following shows the shape that needs to be created.
                    //             _________________
                    //            |                 |_
                    //         _ _|                   |
                    //        |                       |
                    //        |                    _ _|
                    //        |                   |   
                    //        |___________________| 
                    // The missing corners of the shape are filled with the radial gradients drawn above

                    // Define shape counter clockwise
                    PathFigure figure = new();
                    if (cornerRadius.TopLeft > ShadowDepth)
                    {
                        figure.StartPoint = new Point(guidelineSetX[1], guidelineSetY[0]);
                        figure.Segments.Add(new LineSegment(new Point(guidelineSetX[1], guidelineSetY[1]), true));
                        figure.Segments.Add(new LineSegment(new Point(guidelineSetX[0], guidelineSetY[1]), true));
                    }
                    else
                    {
                        figure.StartPoint = new Point(guidelineSetX[0], guidelineSetY[0]);
                    }

                    if (cornerRadius.BottomLeft > ShadowDepth)
                    {
                        figure.Segments.Add(new LineSegment(new Point(guidelineSetX[0], guidelineSetY[3]), true));
                        figure.Segments.Add(new LineSegment(new Point(guidelineSetX[3], guidelineSetY[3]), true));
                        figure.Segments.Add(new LineSegment(new Point(guidelineSetX[3], guidelineSetY[5]), true));
                    }
                    else
                    {
                        figure.Segments.Add(new LineSegment(new Point(guidelineSetX[0], guidelineSetY[5]), true));
                    }

                    if (cornerRadius.BottomRight > ShadowDepth)
                    {
                        figure.Segments.Add(new LineSegment(new Point(guidelineSetX[4], guidelineSetY[5]), true));
                        figure.Segments.Add(new LineSegment(new Point(guidelineSetX[4], guidelineSetY[4]), true));
                        figure.Segments.Add(new LineSegment(new Point(guidelineSetX[5], guidelineSetY[4]), true));
                    }
                    else
                    {
                        figure.Segments.Add(new LineSegment(new Point(guidelineSetX[5], guidelineSetY[5]), true));
                    }


                    if (cornerRadius.TopRight > ShadowDepth)
                    {
                        figure.Segments.Add(new LineSegment(new Point(guidelineSetX[5], guidelineSetY[2]), true));
                        figure.Segments.Add(new LineSegment(new Point(guidelineSetX[2], guidelineSetY[2]), true));
                        figure.Segments.Add(new LineSegment(new Point(guidelineSetX[2], guidelineSetY[0]), true));
                    }
                    else
                    {
                        figure.Segments.Add(new LineSegment(new Point(guidelineSetX[5], guidelineSetY[0]), true));
                    }

                    figure.IsClosed = true;
                    figure.Freeze();

                    PathGeometry geometry = new();
                    geometry.Figures.Add(figure);
                    geometry.Freeze();

                    Context.DrawGeometry(brushes[Center], null, geometry);
                }

                Context.Pop();
            }
        }

        #region Private Properties

        private Brush[] GetBrushes(Color c, CornerRadius cornerRadius)
        {
            if (_commonBrushes == null)
            {
                lock (_resourceAccess)
                {
                    if (_commonBrushes == null)
                    {
                        // Assume that the first render of DropShadow uses the most common color for the app.
                        // This breaks down if (a) the first Shadow is customized, or 
                        // (b) ButtonChrome becomes more broadly used than just on system controls.
                        _commonBrushes = CreateBrushes(c, cornerRadius);
                        _commonCornerRadius = cornerRadius;
                    }
                }
            }

            if (c == ((SolidColorBrush)_commonBrushes[Center]).Color &&
                cornerRadius == _commonCornerRadius)
            {
                // clear local brushes - use common
                _brushes = null;
                return _commonBrushes;
            }

            // need to create local brushes
            _brushes ??= CreateBrushes(c, cornerRadius);
            return _brushes;
        }

        // Create common gradient stop collection for gradient brushes
        private static GradientStopCollection CreateStops(Color c, double cornerRadius)
        {
            // Scale stops to lie within 0 and 1
            double gradientScale = 1 / (cornerRadius + ShadowDepth);

            GradientStopCollection gsc = [new GradientStop(c, (0.5 + cornerRadius) * gradientScale)];

            // Create gradient stops based on the Win32 dropshadow fall off
            Color stopColor = c;
            stopColor.A = (byte)(.74336 * c.A);
            gsc.Add(new GradientStop(stopColor, (1.5 + cornerRadius) * gradientScale));

            stopColor.A = (byte)(.38053 * c.A);
            gsc.Add(new GradientStop(stopColor, (2.5 + cornerRadius) * gradientScale));

            stopColor.A = (byte)(.12389 * c.A);
            gsc.Add(new GradientStop(stopColor, (3.5 + cornerRadius) * gradientScale));

            stopColor.A = (byte)(.02654 * c.A);
            gsc.Add(new GradientStop(stopColor, (4.5 + cornerRadius) * gradientScale));

            stopColor.A = 0;
            gsc.Add(new GradientStop(stopColor, (5 + cornerRadius) * gradientScale));

            gsc.Freeze();

            return gsc;
        }

        // Creates an array of brushes needed to render this 
        private static Brush[] CreateBrushes(Color c, CornerRadius cornerRadius)
        {
            Brush[] brushes = new Brush[9];

            // Create center brush
            brushes[Center] = new SolidColorBrush(c);
            brushes[Center].Freeze();

            // Sides
            GradientStopCollection sideStops = CreateStops(c, 0);
            LinearGradientBrush top = new(sideStops, new Point(0, 1), new Point(0, 0));
            top.Freeze();
            brushes[Top] = top;

            LinearGradientBrush left = new(sideStops, new Point(1, 0), new Point(0, 0));
            left.Freeze();
            brushes[Left] = left;

            LinearGradientBrush right = new(sideStops, new Point(0, 0), new Point(1, 0));
            right.Freeze();
            brushes[Right] = right;

            LinearGradientBrush bottom = new(sideStops, new Point(0, 0), new Point(0, 1));
            bottom.Freeze();
            brushes[Bottom] = bottom;

            // Corners

            // Use side stops if the corner radius is 0
            GradientStopCollection topLeftStops = cornerRadius.TopLeft == 0 ? sideStops : CreateStops(c, cornerRadius.TopLeft);
            RadialGradientBrush topLeft = new(topLeftStops)
            {
                RadiusX = 1,
                RadiusY = 1,
                Center = new Point(1, 1),
                GradientOrigin = new Point(1, 1)
            };
            topLeft.Freeze();
            brushes[TopLeft] = topLeft;

            // Reuse previous stops if corner radius is the same as side or top left
            GradientStopCollection topRightStops = cornerRadius.TopRight == 0
                ? sideStops
                : cornerRadius.TopRight == cornerRadius.TopLeft ? topLeftStops : CreateStops(c, cornerRadius.TopRight);
            RadialGradientBrush topRight = new(topRightStops)
            {
                RadiusX = 1,
                RadiusY = 1,
                Center = new Point(0, 1),
                GradientOrigin = new Point(0, 1)
            };
            topRight.Freeze();
            brushes[TopRight] = topRight;

            // Reuse previous stops if corner radius is the same as any of the previous radii
            GradientStopCollection bottomLeftStops = cornerRadius.BottomLeft == 0
                ? sideStops
                : cornerRadius.BottomLeft == cornerRadius.TopLeft
                ? topLeftStops
                : cornerRadius.BottomLeft == cornerRadius.TopRight ? topRightStops : CreateStops(c, cornerRadius.BottomLeft);
            RadialGradientBrush bottomLeft = new(bottomLeftStops)
            {
                RadiusX = 1,
                RadiusY = 1,
                Center = new Point(1, 0),
                GradientOrigin = new Point(1, 0)
            };
            bottomLeft.Freeze();
            brushes[BottomLeft] = bottomLeft;

            // Reuse previous stops if corner radius is the same as any of the previous radii
            GradientStopCollection bottomRightStops = cornerRadius.BottomRight == 0
                ? sideStops
                : cornerRadius.BottomRight == cornerRadius.TopLeft
                ? topLeftStops
                : cornerRadius.BottomRight == cornerRadius.TopRight
                ? topRightStops
                : cornerRadius.BottomRight == cornerRadius.BottomLeft ? bottomLeftStops : CreateStops(c, cornerRadius.BottomRight);
            RadialGradientBrush bottomRight = new(bottomRightStops)
            {
                RadiusX = 1,
                RadiusY = 1,
                Center = new Point(0, 0),
                GradientOrigin = new Point(0, 0)
            };
            bottomRight.Freeze();
            brushes[BottomRight] = bottomRight;

            return brushes;
        }

        private const int TopLeft = 0;
        private const int Top = 1;
        private const int TopRight = 2;
        private const int Left = 3;
        private const int Center = 4;
        private const int Right = 5;
        private const int BottomLeft = 6;
        private const int Bottom = 7;
        private const int BottomRight = 8;

        // 9 brushes:
        //  0 TopLeft       1 Top       2 TopRight  
        //  3 Left          4 Center    5 Right
        //  6 BottomLeft    7 Bottom    8 BottomRight
        private static Brush[] _commonBrushes;
        private static CornerRadius _commonCornerRadius;
        private static object _resourceAccess = new();

        // Local brushes if our color is not the common color
        private Brush[] _brushes;

        #endregion

    }
}