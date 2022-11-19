using System.Collections.Generic;
using System.Windows.Threading;

namespace MenthaAssembly.Views.Primitives
{
    /// <summary>
    /// Manages the canvas update and canvas dirty state.
    /// </summary>
    internal sealed class ImageViewerManager : DispatcherQueue<ImageViewerAction>
    {
        private readonly ImageViewer Viewer;
        public ImageViewerManager(ImageViewer Viewer) : base(Viewer.Dispatcher, DispatcherPriority.Render)
        {
            this.Viewer = Viewer;
        }

        protected override void InternalRun(Queue<ImageViewerAction> Queue)
        {
            ImageViewerAction Last = ImageViewerAction.None;
            while (Queue.Count > 0)
            {
                // Dequeues Action and Checks Last Action.
                ImageViewerAction Curt = Queue.Dequeue();
                if (Last == Curt)
                    continue;

#if NET462
                // Checks Next Action
                if (Queue.Count > 0)
                {
                    ImageViewerAction Next = Queue.Peek();
                    if (Next <= Curt)
                        continue;
                }
#else
                // Checks Next Action
                if (Queue.TryPeek(out ImageViewerAction Next) &&
                    Next <= Curt)
                    continue;
#endif

                switch (Curt)
                {
                    case ImageViewerAction.ContextSize:
                        {
                            Viewer.RefreshContextSize();
                            Queue.Enqueue(ImageViewerAction.ComputeViewBox);
                            break;
                        }
                    case ImageViewerAction.ComputeViewBox:
                        {
                            Size<int> ViewBox = Viewer.ComputeViewBox(out double FitScale);
                            Viewer.SetValue(ImageViewer.ViewBoxPropertyKey, ViewBox);
                            Viewer.MinScale = FitScale;

                            Viewer._Attachments.ForEach(i => i.InvalidateViewBox());
                            Queue.Enqueue(ImageViewerAction.ContextLocation);
                            break;
                        }
                    case ImageViewerAction.ContextLocation:
                        {
                            Viewer.RefreshContextLocation();
                            Queue.Enqueue(ImageViewerAction.ComputeScale);
                            break;
                        }
                    case ImageViewerAction.ComputeScale:
                        {
                            Viewer.Scale = Viewer.ComputeScale();
                            Queue.Enqueue(ImageViewerAction.ComputeViewport);
                            break;
                        }
                    case ImageViewerAction.ComputeViewport:
                        {
                            Viewer.Viewport = Viewer.ComputeViewport();

                            Viewer._Attachments.ForEach(i => i.InvalidateViewport());
                            Queue.Enqueue(ImageViewerAction.RenderCanvas);
                            break;
                        }
                    case ImageViewerAction.RenderCanvas:
                        {
                            foreach (ImageViewerLayer Layer in Viewer.Layers)
                                Layer.InvalidateCanvas();

                            Viewer._Attachments.ForEach(i => i.InvalidateCanvas());
                            break;
                        }
                    case ImageViewerAction.None:
                    default:
                        continue;
                }

                Last = Curt;
            }
        }

    }

    internal enum ImageViewerAction : byte
    {
        None = 0,
        ContextSize = 1,
        ComputeViewBox = 2,
        ContextLocation = 3,
        ComputeScale = 4,
        ComputeViewport = 5,
        RenderCanvas = 6,
    }
}