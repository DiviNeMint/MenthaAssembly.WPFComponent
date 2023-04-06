using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MenthaAssembly.Views.Primitives
{
    public abstract class ImageViewerLayerObject : FrameworkElement
    {
        public static new readonly DependencyProperty HorizontalAlignmentProperty =
              DependencyProperty.Register("HorizontalAlignment", typeof(HorizontalAlignment), typeof(ImageViewerLayerObject),
                  new FrameworkPropertyMetadata(HorizontalAlignment.Center, FrameworkPropertyMetadataOptions.AffectsParentArrange));
        public new HorizontalAlignment HorizontalAlignment
        {
            get => (HorizontalAlignment)GetValue(HorizontalAlignmentProperty);
            set => SetValue(HorizontalAlignmentProperty, value);
        }

        public static new readonly DependencyProperty VerticalAlignmentProperty =
            DependencyProperty.Register("VerticalAlignment", typeof(VerticalAlignment), typeof(ImageViewerLayerObject),
                new FrameworkPropertyMetadata(VerticalAlignment.Center, FrameworkPropertyMetadataOptions.AffectsParentArrange));
        public new VerticalAlignment VerticalAlignment
        {
            get => (VerticalAlignment)GetValue(VerticalAlignmentProperty);
            set => SetValue(VerticalAlignmentProperty, value);
        }

        /// <summary>
        /// Gets the position in the parent layer of the specified image coordinate.
        /// </summary>
        /// <param name="Ix">The x-coordinate of the specified image coordinate.</param>
        /// <param name="Iy">The y-coordinate of the specified image coordinate.</param>
        /// <param name="Lx">The x-coordinate of the specified point in the parent layer.</param>
        /// <param name="Ly">The y-coordinate of the specified point in the parent layer.</param>
        public void GetLayerPosition(double Ix, double Iy, out double Lx, out double Ly)
        {
            if (this.FindLogicalParents<ImageViewerLayer>().FirstOrDefault() is ImageViewerLayer Layer)
                Layer.Renderer.GetLayerPosition(Ix, Iy, out Lx, out Ly);
            else
            {
                Lx = double.NaN;
                Ly = double.NaN;
            }
        }

        /// <summary>
        /// Gets the position in the element of the specified image coordinate.
        /// </summary>
        /// <param name="Ix">The x-coordinate of the specified image coordinate.</param>
        /// <param name="Iy">The y-coordinate of the specified image coordinate.</param>
        /// <param name="Px">The x-coordinate of the specified point in this element.</param>
        /// <param name="Py">The y-coordinate of the specified point in this element.</param>
        public void GetPosition(double Ix, double Iy, out double Px, out double Py)
        {
            if (this.FindLogicalParents<ImageViewerLayer>().FirstOrDefault() is ImageViewerLayer Layer)
            {
                Layer.Renderer.GetLayerPosition(Ix, Iy, out double Lx, out double Ly);
                Point P = Layer.TranslatePoint(new Point(Lx, Ly), this);
                Px = P.X;
                Py = P.Y;
            }
            else
            {
                Px = double.NaN;
                Py = double.NaN;
            }
        }

        /// <summary>
        /// Gets the image coordinate of the specified position in this object.
        /// </summary>
        /// <param name="Px">The x-coordinate of the specified position in this object.</param>
        /// <param name="Py">The y-coordinate of the specified position in this object.</param>
        /// <param name="Ix">The x-coordinate in image.</param>
        /// <param name="Iy">The y-coordinate in image.</param>
        public void GetImageCoordinate(double Px, double Py, out double Ix, out double Iy)
        {
            if (this.FindLogicalParents<ImageViewerLayer>().FirstOrDefault() is ImageViewerLayer Layer)
            {
                Point LayerPosition = TranslatePoint(new Point(Px, Py), Layer);
                Layer.Renderer.GetImageCoordinate(LayerPosition.X, LayerPosition.Y, out Ix, out Iy);
            }
            else
            {
                Ix = double.NaN;
                Iy = double.NaN;
            }
        }
        /// <summary>
        /// Gets the image coordinate of the mouse position.
        /// </summary>
        /// <param name="Ix">The x-coordinate in image.</param>
        /// <param name="Iy">The y-coordinate in image.</param>
        public void GetImageCoordinate(out double Ix, out double Iy)
        {
            if (this.FindLogicalParents<ImageViewerLayer>().FirstOrDefault() is ImageViewerLayer Layer)
            {
                Point Location = Mouse.GetPosition(Layer);
                Layer.Renderer.GetImageCoordinate(Location.X, Location.Y, out Ix, out Iy);
            }
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
            => this.FindLogicalParents<ImageViewerLayer>().FirstOrDefault() is ImageViewerLayer Layer ? Layer.Renderer.Scale : 1d;

    }
}