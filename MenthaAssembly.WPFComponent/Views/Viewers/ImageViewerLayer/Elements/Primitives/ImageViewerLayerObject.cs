using System.Windows;

namespace MenthaAssembly.Views.Primitives
{
    public abstract class ImageViewerLayerObject : FrameworkElement
    {

        protected void GetImageStatus(out double ImageLayerX, out double ImageLayerY, out double Scale)
        {
            if (Parent is ImageViewerLayer Layer)
            {
                Layer.Renderer.GetLayerPosition(0d, 0d, out ImageLayerX, out ImageLayerY);
                Scale = Layer.Renderer.Scale;
            }

            else if (Parent is ImageViewerLayerObject Object)
                Object.GetImageStatus(out ImageLayerX, out ImageLayerY, out Scale);

            else
            {
                ImageLayerX = 0d;
                ImageLayerY = 0d;
                Scale = 1d;
            }
        }

        protected double GetScale()
        {
            if (Parent is ImageViewerLayer Layer)
                return Layer.Renderer.Scale;

            else if (Parent is ImageViewerLayerObject Object)
                return Object.GetScale();

            return 1d;
        }

    }
}