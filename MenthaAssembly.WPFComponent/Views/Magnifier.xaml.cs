using MenthaAssembly.Devices;
using MenthaAssembly.Media.Imaging;
using MenthaAssembly.Win32;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MenthaAssembly.Views
{
    public class Magnifier : Control
    {
        public event EventHandler<ChangedEventArgs<double>> ZoomFactorChanged;

        public static readonly DependencyProperty CornerRadiusProperty = Border.CornerRadiusProperty.AddOwner(typeof(Magnifier));
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty ZoomFactorProperty =
                DependencyProperty.Register("ZoomFactor", typeof(double), typeof(Magnifier), new PropertyMetadata(2d,
                    (d, e) =>
                    {
                        if (d is Magnifier This && This.IsLoaded)
                            This.OnZoomFactorChanged(e.ToChangedEventArgs<double>());
                    },
                    (d, v) =>
                    {
                        return v is double value ? value < 1d ? 1d : value : (object)2d;
                    }));
        public double ZoomFactor
        {
            get => (double)GetValue(ZoomFactorProperty);
            set => SetValue(ZoomFactorProperty, value);
        }

        public static readonly DependencyProperty CrossLineBrushProperty =
                DependencyProperty.Register("CrossLineBrush", typeof(Brush), typeof(Magnifier), new PropertyMetadata(Brushes.Red));
        public Brush CrossLineBrush
        {
            get => (Brush)GetValue(CrossLineBrushProperty);
            set => SetValue(CrossLineBrushProperty, value);
        }

        static Magnifier()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Magnifier), new FrameworkPropertyMetadata(typeof(Magnifier)));
        }

        protected Border PART_Root;
        protected DispatcherTimer MagnifierTimer = null;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_Root") is Border Root)
                PART_Root = Root;

            void StartGrab()
            {
                MagnifierTimer ??= new DispatcherTimer(TimeSpan.FromMilliseconds(50d), DispatcherPriority.Normal, (s, e) => OnRefreshImage(), Dispatcher);   // 20 FPS
                MagnifierTimer.Start();
            }

            IsVisibleChanged += (s, e) =>
            {
                if (e.NewValue is true)
                    StartGrab();
                else
                    MagnifierTimer?.Stop();
            };

            if (IsVisible && !DesignerProperties.GetIsInDesignMode(this))
                StartGrab();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            CalculateScreenshot(out Screenshot_MouseOffsetX, out Screenshot_MouseOffsetY, out Screenshot_Width, out Screenshot_Height);
        }

        protected virtual void OnZoomFactorChanged(ChangedEventArgs<double> e)
        {
            ZoomFactorChanged?.Invoke(this, e);

            if (e.Handled)
                return;

            CalculateScreenshot(out Screenshot_MouseOffsetX, out Screenshot_MouseOffsetY, out Screenshot_Width, out Screenshot_Height);
        }

        protected virtual void CalculateScreenshot(out int OffsetX, out int OffsetY, out int Width, out int Height)
        {
            Thickness Border = BorderThickness;
            double Factor = ZoomFactor;
            Width = (int)Math.Round(ActualWidth / Factor - Border.Left - Border.Right);
            Height = (int)Math.Round(ActualHeight / Factor - Border.Top - Border.Bottom);
            OffsetX = Width >> 1;
            OffsetY = Height >> 1;
        }

        private int Screenshot_MouseOffsetX,
                    Screenshot_MouseOffsetY,
                    Screenshot_Width,
                    Screenshot_Height;
        protected virtual void OnRefreshImage()
        {
            Point<int> Position = GlobalMouse.Position;
            ImageContext<BGR> Screenshot = Desktop.Screenshot(Position.X - Screenshot_MouseOffsetX, Position.Y - Screenshot_MouseOffsetY, Screenshot_Width, Screenshot_Height);

            if (Screenshot is null)
                return;

            PART_Root.Background = new ImageBrush(BitmapSource.Create(Screenshot_Width, Screenshot_Height, 96, 96, PixelFormats.Bgr24, null, Screenshot.Scan0[0], (int)(Screenshot.Stride * Screenshot.Height), (int)Screenshot.Stride));
        }

    }
}