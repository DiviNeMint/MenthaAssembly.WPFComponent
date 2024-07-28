using System.Linq;
using System.Windows;

namespace MenthaAssembly.Views.Primitives
{
    public abstract class ImageViewerLayerObject : FrameworkElement
    {
        //public static new readonly DependencyProperty HorizontalAlignmentProperty =
        //      DependencyProperty.Register("HorizontalAlignment", typeof(HorizontalAlignment), typeof(ImageViewerLayerObject),
        //          new FrameworkPropertyMetadata(HorizontalAlignment.Center, FrameworkPropertyMetadataOptions.AffectsParentArrange));
        //public new HorizontalAlignment HorizontalAlignment
        //{
        //    get => (HorizontalAlignment)GetValue(HorizontalAlignmentProperty);
        //    set => SetValue(HorizontalAlignmentProperty, value);
        //}

        //public static new readonly DependencyProperty VerticalAlignmentProperty =
        //    DependencyProperty.Register("VerticalAlignment", typeof(VerticalAlignment), typeof(ImageViewerLayerObject),
        //        new FrameworkPropertyMetadata(VerticalAlignment.Center, FrameworkPropertyMetadataOptions.AffectsParentArrange));
        //public new VerticalAlignment VerticalAlignment
        //{
        //    get => (VerticalAlignment)GetValue(VerticalAlignmentProperty);
        //    set => SetValue(VerticalAlignmentProperty, value);
        //}

        static ImageViewerLayerObject()
        {
            VerticalAlignmentProperty.OverrideMetadata(typeof(ImageViewerLayerObject), new FrameworkPropertyMetadata(VerticalAlignment.Center, FrameworkPropertyMetadataOptions.AffectsParentArrange));
            HorizontalAlignmentProperty.OverrideMetadata(typeof(ImageViewerLayerObject), new FrameworkPropertyMetadata(HorizontalAlignment.Center, FrameworkPropertyMetadataOptions.AffectsParentArrange));
        }

        /// <summary>
        /// Translates the specified image coordinate to the position in the specified element.
        /// </summary>
        /// <param name="RelativeTo">The specified element.</param>
        /// <param name="Px">The x-coordinate of the point in this element.</param>
        /// <param name="Py">The y-coordinate of the point in this element.</param>
        /// <param name="Ix">The x-coordinate of the specified image coordinate.</param>
        /// <param name="Iy">The y-coordinate of the specified image coordinate.</param>
        public void TranslatePoint(UIElement RelativeTo, out double Px, out double Py, double Ix, double Iy)
        {
            if (GetParentLayer() is ImageViewerLayer Layer)
                Layer.TranslatePoint(RelativeTo, out Px, out Py, Ix, Iy);
            else
            {
                Px = double.NaN;
                Py = double.NaN;
            }
        }
        /// <summary>
        /// Translates the specified image coordinate to the position in this object.
        /// </summary>
        /// <param name="Lx">The x-coordinate of the point in this object.</param>
        /// <param name="Ly">The y-coordinate of the point in this object.</param>
        /// <param name="Ix">The x-coordinate of the specified image coordinate.</param>
        /// <param name="Iy">The y-coordinate of the specified image coordinate.</param>
        public void TranslatePoint(out double Lx, out double Ly, double Ix, double Iy)
        {
            if (GetParentLayer() is ImageViewerLayer Layer)
                Layer.TranslatePoint(this, out Lx, out Ly, Ix, Iy);
            else
            {
                Lx = double.NaN;
                Ly = double.NaN;
            }
        }
        /// <summary>
        /// Translates the specified position in the specified element to the image coordinate.
        /// </summary>
        /// <param name="RelativeFrom">The specified element.</param>
        /// <param name="Px">The x-coordinate of the specified point in this element.</param>
        /// <param name="Py">The y-coordinate of the specified point in this element.</param>
        /// <param name="Ix">The x-coordinate of the image coordinate.</param>
        /// <param name="Iy">The y-coordinate of the image coordinate.</param>
        public void TranslatePoint(UIElement RelativeFrom, double Px, double Py, out double Ix, out double Iy)
        {
            if (GetParentLayer() is ImageViewerLayer Layer)
                Layer.TranslatePoint(RelativeFrom, Px, Py, out Ix, out Iy);
            else
            {
                Ix = double.NaN;
                Iy = double.NaN;
            }
        }
        /// <summary>
        /// Translates the specified position in this object to the image coordinate.
        /// </summary>
        /// <param name="Px">The x-coordinate of the specified point in this object.</param>
        /// <param name="Py">The y-coordinate of the specified point in this object.</param>
        /// <param name="Ix">The x-coordinate of the image coordinate.</param>
        /// <param name="Iy">The y-coordinate of the image coordinate.</param>
        public void TranslatePoint(double Px, double Py, out double Ix, out double Iy)
        {
            if (GetParentLayer() is ImageViewerLayer Layer)
                Layer.TranslatePoint(this, Px, Py, out Ix, out Iy);
            else
            {
                Ix = double.NaN;
                Iy = double.NaN;
            }
        }

        /// <summary>
        /// Gets the scale of the ImageViewer.
        /// </summary>
        protected internal double GetScale()
            => GetParentLayer()?.Renderer.Scale ?? 1d;

        /// <summary>
        /// Gets the parent layer of this object.
        /// </summary>
        protected internal ImageViewerLayer GetParentLayer()
            => this.FindLogicalParents<ImageViewerLayer>().FirstOrDefault();

    }
}