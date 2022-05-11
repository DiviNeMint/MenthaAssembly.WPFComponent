using MenthaAssembly.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MenthaAssembly.Views.Primitives
{
    public abstract class ImageViewerBase : Control
    {
        public ParallelOptions RenderParallelOptions { set; get; }

        internal virtual Size<int> ViewBox { set; get; }

        internal Rect InternalViewport { set; get; }

        internal double InternalScale { set; get; }

        public Int32Rect ContextRect
            => new Int32Rect(ContextX, ContextY, ContextWidth, ContextHeight);

        internal int ContextX = 0,
                     ContextY = 0,
                     ContextWidth = 0,
                     ContextHeight = 0;
        protected void UpdateContextLocation()
        {
            ContextX = Math.Max((ViewBox.Width - ContextWidth) >> 1, 0);
            ContextY = Math.Max((ViewBox.Height - ContextHeight) >> 1, 0);
        }
        protected void UpdateContextSize(IEnumerable<ImageViewerLayer> Layers)
        {
            ContextWidth = 0;
            ContextHeight = 0;
            foreach (IImageContext Image in Layers.Where(i => i?.SourceContext != null)
                                                  .Select(i => i.SourceContext))
            {
                ContextWidth = Math.Max(ContextWidth, Image.Width);
                ContextHeight = Math.Max(ContextHeight, Image.Height);
            }
        }

        internal double DisplayAreaWidth,
                        DisplayAreaHeight;
        protected override Size MeasureOverride(Size Constraint)
        {
            if (VisualChildrenCount > 0 &&
                GetVisualChild(0) is UIElement Child)
                Child.Measure(Constraint);

            double W = Constraint.Width,
                   H = Constraint.Height;
            if (double.IsInfinity(W) || W == 0d ||
                double.IsInfinity(H) || H == 0d)
            {
                DisplayAreaWidth = 0d;
                DisplayAreaHeight = 0d;
            }
            else
            {
                DisplayAreaWidth = Math.Max(W - (BorderThickness.Left + BorderThickness.Right), 0d);
                DisplayAreaHeight = Math.Max(H - (BorderThickness.Top + BorderThickness.Bottom), 0d);
            }

            return Constraint;
        }
    }
}