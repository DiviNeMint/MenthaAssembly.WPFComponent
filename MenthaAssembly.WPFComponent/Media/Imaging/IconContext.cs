using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MenthaAssembly
{
    [ContentProperty("Children")]
    public class IconContext
    {
        private ImageSource _ImageSource;
        public ImageSource ImageSource
        {
            get => _ImageSource ?? new DrawingImage(Children.Count <= 0 ?
                                   new GeometryDrawing(Fill, new Pen(Stroke, StrokeThickness), Geometry) :
                                   (Drawing)Drawings);
            set => _ImageSource = value;
        }

        public Uri UriSource
        {
            get => (_ImageSource as BitmapImage)?.UriSource;
            set => _ImageSource = new BitmapImage(value);
        }

        public Geometry Geometry { get; set; }

        protected DrawingGroup Drawings { set; get; } = new DrawingGroup();
        public DrawingCollection Children => Drawings.Children;

        public Size Size { set; get; } = Size.Empty;

        public Thickness Padding { get; set; }

        public Brush Fill { set; get; }

        public Brush Stroke { set; get; }

        public double StrokeThickness { set; get; }

        public IconContext()
        {
        }

        public IconContext(IEnumerable<Drawing> Drawings)
        {
            foreach (Drawing item in Drawings)
                Children.Add(item);
        }

        public IconContext(IEnumerable<Geometry> Geometries, Brush Fill, Brush Stroke, double StrokeThickness)
        {
            foreach (Geometry item in Geometries)
                Children.Add(new GeometryDrawing(Fill, new Pen(Stroke, StrokeThickness), item));
        }
    }
}
