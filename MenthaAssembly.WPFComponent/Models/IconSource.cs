using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Composition;
using System.Windows.Media.Imaging;

namespace MenthaAssembly
{
    public class IconSource
    {
        public BitmapImage ImageSource { set; get; }

        public Uri UriSource
        {
            get => ImageSource?.UriSource;
            set => ImageSource = new BitmapImage(value);
        }

        public Geometry Geometry { get; set; }

        public Size Size { set; get; }

        public Thickness Padding { get; set; }

        public Brush Fill { set; get; }

        public Brush Stroke { set; get; }

        public double StrokeThickness { set; get; }

    }
}
