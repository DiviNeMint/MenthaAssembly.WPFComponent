using MenthaAssembly.Devices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    public class ColorBox : Control
    {
        public static readonly RoutedEvent ColorChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(ColorChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler<RoutedPropertyChangedEventArgs<Color?>>), typeof(ColorBox));
        public event RoutedEventHandler ColorChanged
        {
            add => AddHandler(ColorChangedEvent, value);
            remove => RemoveHandler(ColorChangedEvent, value);
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(ColorBox), new PropertyMetadata(false,
                (d, e) =>
                {
                    if (d is ColorBox ThisBox)
                        ThisBox.OnIsOpenChanged(e.ToChangedEventArgs<bool>());
                }));
        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public static readonly DependencyProperty ColorProperty =
            ColorEditor.ColorProperty.AddOwner(typeof(ColorBox), new PropertyMetadata(null,
                (d, e) =>
                {
                    if (d is ColorBox ThisBox)
                        ThisBox.OnColorChanged(e.ToRoutedPropertyChangedEventArgs<Color?>(ColorChangedEvent));
                }));
        public Color? Color
        {
            get => (Color?)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        static ColorBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorBox), new FrameworkPropertyMetadata(typeof(ColorBox)));
        }

        private ColorEditor PART_ColorEditor;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_ColorEditor") is ColorEditor PART_ColorEditor)
                this.PART_ColorEditor = PART_ColorEditor;
        }

        protected virtual void OnIsOpenChanged(ChangedEventArgs<bool> e)
        {
            if (e.NewValue)
                GlobalMouse.MouseDown += OnGlobalMouseDown;
            else
                GlobalMouse.MouseDown -= OnGlobalMouseDown;

            void OnGlobalMouseDown(GlobalMouseEventArgs e)
            {
                if (!IsMouseOver &&
                    (!PART_ColorEditor?.IsColorCapturing ?? true))
                {
                    GlobalMouse.MouseDown -= OnGlobalMouseDown;
                    IsOpen = false;
                }
            }
        }

        protected virtual void OnColorChanged(RoutedPropertyChangedEventArgs<Color?> e)
            => RaiseEvent(e);

    }
}