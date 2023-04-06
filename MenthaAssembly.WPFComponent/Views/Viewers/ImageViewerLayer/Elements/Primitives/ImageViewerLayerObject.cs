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
        /// Gets the position in the parent layer of the specified image coordination.
        /// </summary>
        /// <param name="Ix">The x-coordinate of the specified image coordination.</param>
        /// <param name="Iy">The y-coordinate of the specified image coordination.</param>
        /// <param name="Lx">The x-coordinate of the parent layer.</param>
        /// <param name="Ly">The y-coordinate of the parent layer.</param>
        protected internal void GetLayerPosition(double Ix, double Iy, out double Lx, out double Ly)
        {
            if (this.FindLogicalParents<ImageViewerLayer>().FirstOrDefault() is ImageViewerLayer Layer)
                //if (this.FindVisualParents<ImageViewerLayer>().FirstOrDefault() is ImageViewerLayer Layer)
                Layer.Renderer.GetLayerPosition(Ix, Iy, out Lx, out Ly);

            //if (Parent is ImageViewerLayer Layer)
            //    Layer.Renderer.GetLayerPosition(Ix, Iy, out Lx, out Ly);

            //else if (Parent is ImageViewerLayerObject Object)
            //    Object.GetLayerPosition(Ix, Iy, out Lx, out Ly);

            else
            {
                Lx = double.NaN;
                Ly = double.NaN;
            }
        }

        /// <summary>
        /// Gets the image coordination of the specified position in this object.
        /// </summary>
        /// <param name="Px">The x-coordinate of the specified position in this object.</param>
        /// <param name="Py">The y-coordinate of the specified position in this object.</param>
        /// <param name="Ix">The x-coordinate in image.</param>
        /// <param name="Iy">The y-coordinate in image.</param>
        protected internal void GetImageCoordination(double Px, double Py, out double Ix, out double Iy)
        {
            if (this.FindLogicalParents<ImageViewerLayer>().FirstOrDefault() is ImageViewerLayer Layer)
            {
                Point LayerPosition = TranslatePoint(new Point(Px, Py), Layer);
                Layer.Renderer.GetImageCoordination(LayerPosition.X, LayerPosition.Y, out Ix, out Iy);
            }

            //if (Parent is ImageViewerLayer Layer)
            //    Layer.Renderer.GetImageCoordination(Tx, Ty, out Ix, out Iy);

            //else if (Parent is ImageViewerLayerObject Object)
            //    Object.GetImageCoordination(Tx, Ty, out Ix, out Iy);

            else
            {
                Ix = double.NaN;
                Iy = double.NaN;
            }
        }
        /// <summary>
        /// Gets the image coordination of the mouse position.
        /// </summary>
        /// <param name="Ix">The x-coordinate in image.</param>
        /// <param name="Iy">The y-coordinate in image.</param>
        protected internal void GetImageCoordination(out double Ix, out double Iy)
        {
            if (this.FindVisualParents<ImageViewerLayer>().FirstOrDefault() is ImageViewerLayer Layer)
            {
                Point Location = Mouse.GetPosition(Layer);
                Layer.Renderer.GetImageCoordination(Location.X, Location.Y, out Ix, out Iy);
            }

            //if (Parent is ImageViewerLayer Layer)
            //{
            //    Point Location = Mouse.GetPosition(Layer);
            //    Layer.Renderer.GetImageCoordination(Location.X, Location.Y, out Ix, out Iy);
            //}

            //else if (Parent is ImageViewerLayerObject Object)
            //    Object.GetImageCoordination(out Ix, out Iy);

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
        {
            if (this.FindLogicalParents<ImageViewerLayer>().FirstOrDefault() is ImageViewerLayer Layer)
                return Layer.Renderer.Scale;

            //if (Parent is ImageViewerLayer Layer)
            //    return Layer.Renderer.Scale;

            //else if (Parent is ImageViewerLayerObject Object)
            //    return Object.GetScale();

            return 1d;
        }

    }
}