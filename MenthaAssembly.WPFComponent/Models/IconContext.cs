using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MenthaAssembly
{
    [ContentProperty("Children")]
    public class IconContext
    {
        public BitmapImage ImageSource { set; get; }

        public Uri UriSource
        {
            get => ImageSource?.UriSource;
            set => ImageSource = new BitmapImage(value);
        }

        public Geometry Geometry { get; set; }

        public DrawingCollection Children { get; } = new DrawingCollection();

        public Size Size { set; get; }

        public Thickness Padding { get; set; }

        public Brush Fill { set; get; }

        public Brush Stroke { set; get; }

        public double StrokeThickness { set; get; }

        public ImageSource GetIcon()
            => (ImageSource)ImageSource ?? new DrawingImage(Children.Count <= 0 ?
                                           new GeometryDrawing(Fill, new Pen(Stroke, StrokeThickness), Geometry) :
                                           (Drawing)new DrawingGroup() { Children = Children });
    }
}
