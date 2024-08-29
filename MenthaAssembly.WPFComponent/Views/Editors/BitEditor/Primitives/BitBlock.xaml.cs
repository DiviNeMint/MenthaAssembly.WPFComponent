using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MenthaAssembly.Views.Primitives
{
    public sealed class BitBlock : FrameworkElement
    {
        public static readonly RoutedEvent ClickEvent = ButtonBase.ClickEvent.AddOwner(typeof(BitBlock));
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        public static readonly DependencyProperty BackgroundProperty =
            Panel.BackgroundProperty.AddOwner(typeof(BitBlock));
        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public static readonly DependencyProperty ForegroundProperty =
            TextElement.ForegroundProperty.AddOwner(typeof(BitBlock));
        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            TextBox.TextProperty.AddOwner(typeof(BitBlock),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, e) =>
                    {
                        if (d is BitBlock This)
                            This.InvalidateText();
                    }));
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty IsSetProperty =
            DependencyProperty.Register(nameof(IsSet), typeof(bool), typeof(BitBlock),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) =>
                    {
                        if (d is BitBlock This && This.Parent is BitEditor Editor)
                            Editor.InvalidateVisual();
                    }));
        public bool IsSet
        {
            get => (bool)GetValue(IsSetProperty);
            set => SetValue(IsSetProperty, value);
        }

        public int Index { get; }

        internal BitBlock(int Index)
        {
            this.Index = Index;
        }
        static BitBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BitBlock), new FrameworkPropertyMetadata(typeof(BitBlock)));
        }

        private bool IsLeftMouseDown;
        private Point MousePosition;
        private double MouseMoveDelta;
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _ = CaptureMouse();
                IsLeftMouseDown = true;
                MousePosition = e.GetPosition(this);
                MouseMoveDelta = 0d;
                e.Handled = true;
            }
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsLeftMouseDown)
            {
                e.Handled = true;
                Point Position = e.GetPosition(this);
                double Dx = Position.X - MousePosition.X,
                       Dy = Position.Y - MousePosition.Y;

                if (MouseMoveDelta <= 25d)
                    MouseMoveDelta += Dx * Dx + Dy * Dy;

                MousePosition = Position;
            }
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (IsLeftMouseDown)
            {
                IsLeftMouseDown = false;
                ReleaseMouseCapture();

                if (MouseMoveDelta <= 25)
                    OnClick();
            }
        }

        private void OnClick()
        {
            RoutedEventArgs e = new(ClickEvent, this);
            RaiseEvent(e);

            if (!e.Handled)
                IsSet = !IsSet;
        }

        protected override void OnRender(DrawingContext Context)
        {
            Brush Background, Foreground;
            if (IsSet)
            {
                Background = this.Foreground;
                Foreground = this.Background;
            }
            else
            {
                Background = this.Background;
                Foreground = this.Foreground;
            }

            Size Size = RenderSize;
            double Bw = Size.Width,
                   Bh = Size.Height;

            // Background
            Context.DrawRectangle(Background, null, new(Size));

            // Text
            if (GetFormattedText(Foreground) is FormattedText Text)
                Context.DrawText(Text, new Point((Bw - Text.Width) / 2d, (Bh - Text.Height) / 2d));
        }

        private Brush TextBrush;
        private FormattedText FormattedText = null;
        private FormattedText GetFormattedText(Brush Foreground)
        {
            if (FormattedText != null)
            {
                if (TextBrush != Foreground)
                {
                    FormattedText.SetForegroundBrush(Foreground);
                    TextBrush = Foreground;
                }

                return FormattedText;
            }

            // Font
            FontFamily FontFamily = TextElement.GetFontFamily(this);
            FontStyle FontStyle = TextElement.GetFontStyle(this);
            FontWeight FontWeight = TextElement.GetFontWeight(this);
            FontStretch FontStretch = TextElement.GetFontStretch(this);
            double FontSize = TextElement.GetFontSize(this);

            Typeface Typeface = new(FontFamily, FontStyle, FontWeight, FontStretch);

            // Text
            string Text = this.Text;

#pragma warning disable CS0618 // 類型或成員已經過時
            FormattedText = new(string.IsNullOrEmpty(Text) ? $"{Index + 1}" : Text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Typeface, FontSize, Foreground);
#pragma warning restore CS0618 // 類型或成員已經過時

            TextBrush = Foreground;
            return FormattedText;
        }
        public void InvalidateText()
            => FormattedText = null;

    }
}