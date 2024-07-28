using MenthaAssembly.Views.Primitives;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    public sealed class ImageViewerLayerTextElement : ImageViewerLayerShape
    {
        private const FrameworkPropertyMetadataOptions AffectsTextRender = AffectsRenderAndMeasure |
                                                                           FrameworkPropertyMetadataOptions.AffectsParentArrange |
                                                                           FrameworkPropertyMetadataOptions.Inherits;

        public static readonly DependencyProperty FontFamilyProperty =
            TextElement.FontFamilyProperty.AddOwner(typeof(ImageViewerLayerTextElement),
                new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, AffectsTextRender, OnTextGeometryChanged));
        public FontFamily FontFamily
        {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        public static readonly DependencyProperty FontStyleProperty =
            TextElement.FontStyleProperty.AddOwner(typeof(ImageViewerLayerTextElement),
                new FrameworkPropertyMetadata(SystemFonts.MessageFontStyle, AffectsTextRender, OnTextGeometryChanged));
        public FontStyle FontStyle
        {
            get => (FontStyle)GetValue(FontStyleProperty);
            set => SetValue(FontStyleProperty, value);
        }

        public static readonly DependencyProperty FontWeightProperty =
            TextElement.FontWeightProperty.AddOwner(typeof(ImageViewerLayerTextElement),
                new FrameworkPropertyMetadata(SystemFonts.MessageFontWeight, AffectsTextRender, OnTextGeometryChanged));
        public FontWeight FontWeight
        {
            get => (FontWeight)GetValue(FontWeightProperty);
            set => SetValue(FontWeightProperty, value);
        }

        public static readonly DependencyProperty FontStretchProperty =
            TextElement.FontStretchProperty.AddOwner(typeof(ImageViewerLayerTextElement),
                new FrameworkPropertyMetadata(FontStretches.Normal, AffectsTextRender, OnTextGeometryChanged));
        public FontStretch FontStretch
        {
            get => (FontStretch)GetValue(FontStretchProperty);
            set => SetValue(FontStretchProperty, value);
        }

        public static readonly DependencyProperty FontSizeProperty =
            TextElement.FontSizeProperty.AddOwner(typeof(ImageViewerLayerTextElement),
                new FrameworkPropertyMetadata(SystemFonts.MessageFontSize, AffectsTextRender, OnTextGeometryChanged));
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ImageViewerLayerTextElement),
                new FrameworkPropertyMetadata(null, AffectsTextRender, OnTextGeometryChanged));
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        static ImageViewerLayerTextElement()
        {
            VerticalAlignmentProperty.OverrideMetadata(typeof(ImageViewerLayerTextElement), new FrameworkPropertyMetadata(VerticalAlignment.Top, FrameworkPropertyMetadataOptions.AffectsParentArrange));
            HorizontalAlignmentProperty.OverrideMetadata(typeof(ImageViewerLayerTextElement), new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.AffectsParentArrange));
        }

        protected override Size MeasureOverride(Size AvailableSize)
        {
            double Cw = Width,
                   Ch = Height;
            if (GetFormattedText() is FormattedText FormattedText)
            {
                Cw = double.IsNaN(Cw) || double.IsInfinity(Cw) ? FormattedText.Width : Math.Min(Cw, FormattedText.Width);
                Ch = double.IsNaN(Ch) || double.IsInfinity(Ch) ? FormattedText.Height : Math.Min(Ch, FormattedText.Height);
            }

            if (double.IsNaN(Cw) || double.IsInfinity(Cw) || double.IsNaN(Ch) || double.IsInfinity(Ch))
            {
                Cw = 0d;
                Ch = 0d;
            }
            else if (Zoomable)
            {
                double Scale = GetScale();
                if (!double.IsNaN(Scale))
                {
                    Cw *= Scale;
                    Ch *= Scale;
                }
            }

            ZoomedDesiredSize = new Size(Cw, Ch);
            return ZoomedDesiredSize;
        }

        protected override void OnRender(DrawingContext Context)
        {
            double Scale = GetScale();
            if (double.IsNaN(Scale) || double.IsNaN(Scale))
                return;

            if (GetTextGeometry() is Geometry Geometry)
            {
                //// Guild Line
                //GuidelineSet GuideLines = new();

                //bool UseGuideLine = false;
                //Size ControlSize = ZoomedDesiredSize;
                //double Cw = ControlSize.Width,
                //       Ch = ControlSize.Height;
                //if (!double.IsNaN(Cw) && !double.IsInfinity(Cw) && Cw != 0d)
                //{
                //    GuideLines.GuidelinesX.Add(0d);
                //    GuideLines.GuidelinesX.Add(Cw);
                //    UseGuideLine = true;
                //}

                //if (!double.IsNaN(Ch) && !double.IsInfinity(Ch) && Ch != 0d)
                //{
                //    GuideLines.GuidelinesY.Add(0d);
                //    GuideLines.GuidelinesY.Add(Cw);
                //    UseGuideLine = true;
                //}

                //if (UseGuideLine)
                //    Context.PushGuidelineSet(GuideLines);

                if (Zoomable)
                    Geometry.Transform = new ScaleTransform(Scale, Scale);

                Context.DrawGeometry(Fill, GetPen(), Geometry);
            }

            //// Draw the text highlight based on the properties that are set.
            //if (Highlight == true)
            //    Context.DrawGeometry(null, new System.Windows.Media.Pen(Stroke, StrokeThickness), _textHighLightGeometry);
        }

        private static void OnTextGeometryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageViewerLayerTextElement This)
            {
                This.InvalidateTextGeometry();
                This.InvalidateFormattedText();
            }
        }

        private Geometry TextGeometry = null;
        private Geometry GetTextGeometry()
        {
            string Text = this.Text;
            if (string.IsNullOrEmpty(Text))
                return null;

            if (TextGeometry is null &&
                GetFormattedText() is FormattedText FormattedText)
            {
                // Build the geometry object that represents the text.
                TextGeometry = FormattedText.BuildGeometry(new Point());

                //// Build the geometry object that represents the text highlight.
                //if (Highlight == true)
                //    _textHighLightGeometry = formattedText.BuildHighlightGeometry(new Point());
            }

            return TextGeometry;
        }
        private void InvalidateTextGeometry()
            => TextGeometry = null;

        private FormattedText FormattedText = null;
        private FormattedText GetFormattedText()
        {
            string Text = this.Text;
            if (string.IsNullOrEmpty(Text))
                return null;

            if (FormattedText is null)
            {
                Typeface Typeface = new(FontFamily, FontStyle, FontWeight, FontStretch);
#pragma warning disable CS0618 // 類型或成員已經過時
                // The brush does not matter since we use the geometry of the text.
                FormattedText = new(Text, CultureInfo.CurrentUICulture, FlowDirection, Typeface, FontSize, Brushes.Black, null, TextFormattingMode.Display);
#pragma warning restore CS0618 // 類型或成員已經過時
            }

            return FormattedText;
        }
        private void InvalidateFormattedText()
            => FormattedText = null;

    }
}