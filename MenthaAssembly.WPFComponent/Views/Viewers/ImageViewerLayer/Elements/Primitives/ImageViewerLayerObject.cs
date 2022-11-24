using System.Windows;

namespace MenthaAssembly.Views.Primitives
{
    public abstract class ImageViewerLayerObject : FrameworkElement
    {
        protected internal void GetLayerPosition(double Ix, double Iy, out double Lx, out double Ly)
        {
            if (Parent is ImageViewerLayer Layer)
                Layer.Renderer.GetLayerPosition(Ix, Iy, out Lx, out Ly);

            else if (Parent is ImageViewerLayerObject Object)
                Object.GetLayerPosition(Ix, Iy, out Lx, out Ly);

            else
            {
                Lx = 0d;
                Ly = 0d;
            }
        }

        protected internal void GetImageCoordination(double Lx, double Ly, out double Ix, out double Iy)
        {
            if (Parent is ImageViewerLayer Layer)
                Layer.Renderer.GetImageCoordination(Lx, Ly, out Ix, out Iy);

            else if (Parent is ImageViewerLayerObject Object)
                Object.GetImageCoordination(Lx, Ly, out Ix, out Iy);

            else
            {
                Ix = 0d;
                Iy = 0d;
            }
        }

        protected internal double GetScale()
        {
            if (Parent is ImageViewerLayer Layer)
                return Layer.Renderer.Scale;

            else if (Parent is ImageViewerLayerObject Object)
                return Object.GetScale();

            return 1d;
        }

    }
}