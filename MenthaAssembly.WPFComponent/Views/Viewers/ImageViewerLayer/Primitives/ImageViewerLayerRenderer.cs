//#define EnableBlockGrid     // Debug
using MenthaAssembly.Media.Imaging;
using MenthaAssembly.Media.Imaging.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MenthaAssembly.Views.Primitives
{
    internal class ImageViewerLayerRenderer
    {
        private const int RenderBlockSize = 200;
        private const int LessRenderBlockSize = RenderBlockSize - 1;

        public bool HasViewer, HasImage, HasImageContext, HasMarks;
        public double Lw, Lh, Iw, Ih, Scale, Vx, Vy;    // LayerWidth, LayerHeight, ImageWidth, ImageHeight, LayerScale, ViewportX, ViewportY
        public int LBL, LBT, LBR, LBB;                  // LayerBlockLeftIndex, LayerBlockTopIndex, LayerBlockRightIndex, LayerBlockBottomIndex

        private readonly ImageViewerLayer Layer;
        private readonly ImageViewerLayerPresenter LayerPresenter;
        public ImageViewerLayerRenderer(ImageViewerLayer Layer, ImageViewerLayerPresenter LayerPresenter)
        {
            CompositionTarget.Rendering += OnCompositionTargetRendering;

            this.Layer = Layer;
            this.LayerPresenter = LayerPresenter;

            Lw = Lh = Iw = Ih = Scale = Vx = Vy = double.NaN;
            LBL = LBT = LBR = LBB = -1;
        }

        private const int MaxRenderingCount = 3;
        private readonly Dictionary<object, RenderBlock> RenderingBlockTable = [];
        private void OnCompositionTargetRendering(object sender, EventArgs e)
        {
            foreach (object Key in RenderingBlockTable.Keys.Take(MaxRenderingCount))
            {
                RenderBlock Block = RenderingBlockTable[Key];
                RenderingBlockTable.Remove(Key);

                Block.Render();
                RenderBlocks[Key] = Block;
                Layer.InvalidateVisual();
            }
        }

        private bool IsValid = true;
        public void Invalidate()
        {
            if (IsValid && Layer.IsVisible)
                Layer.Invoke(Refresh, DispatcherPriority.Render);
        }

        private bool IsMarksValid = true;
        public void InvalidateMarks()
        {
            if (IsMarksValid)
            {
                IsMarksValid = false;
                Invalidate();
            }
        }

        private RectangleGeometry ClipGeometry;
        private void Refresh()
        {
            IsValid = false;

            try
            {
                HasMarks = Layer.Marks.FirstOrDefault(i => i.Visible) != null;

                if (Layer.Source is ImageSource Image)
                {
                    HasImage = true;
                    HasImageContext = false;

                    if (Layer.Parent is ImageViewer Viewer)
                    {
                        HasViewer = true;
                        if (RefreshSource(Image, Viewer) || !IsMarksValid)
                            Layer.InvalidateVisual();
                    }
                    else
                    {
                        HasViewer = false;
                        if (RefreshSource(Image) || !IsMarksValid)
                            Layer.InvalidateVisual();
                    }
                }

                else if (Layer.SourceContext is IImageContext Context)
                {
                    HasImage = false;
                    HasImageContext = true;

                    if (Layer.Parent is ImageViewer Viewer)
                    {
                        HasViewer = true;
                        RefreshContext(Context, Viewer);
                    }
                    else
                    {
                        HasViewer = false;
                        RefreshContext(Context);
                    }

                    if (!IsMarksValid)
                        Layer.InvalidateVisual();
                }

                else
                {
                    if (HasMarks)
                    {
                        double Lw = Layer.ActualWidth,
                               Lh = Layer.ActualHeight;
                        if (ClipGeometry is null)
                        {
                            ClipGeometry = new RectangleGeometry(new Rect(0d, 0d, Lw, Lh));
                            this.Lw = Lw;
                            this.Lh = Lh;
                        }

                        else if (this.Lw != Lw || this.Lh != Lh)
                        {
                            ClipGeometry.Rect = new Rect(0d, 0d, Lw, Lh);
                            this.Lw = Lw;
                            this.Lh = Lh;
                        }
                    }

                    if (HasImage || HasImageContext)
                    {
                        HasImage = false;
                        HasImageContext = false;
                        Iw = double.NaN;
                        Ih = double.NaN;
                        ImageHash = 0;
                        Layer.InvalidateVisual();
                    }

                    else if (HasMarks)
                        Layer.InvalidateVisual();

                }
            }
            finally
            {
                IsValid = true;
                IsMarksValid = true;
            }
        }

        private Rect Region;
        private int ImageHash;
        private bool RefreshSource(ImageSource Image)
        {
            int ImageHash = Image.GetHashCode();
            bool NewImage = false;
            if (this.ImageHash != ImageHash)
            {
                NewImage = true;
                this.ImageHash = ImageHash;
            }

            bool NewSize = false;
            double Lw = Layer.ActualWidth,
                   Lh = Layer.ActualHeight;
            if (ClipGeometry is null)
            {
                NewSize = true;
                ClipGeometry = new RectangleGeometry(new Rect(0d, 0d, Lw, Lh));
                this.Lw = Lw;
                this.Lh = Lh;
            }

            else if (this.Lw != Lw || this.Lh != Lh)
            {
                NewSize = true;
                ClipGeometry.Rect = new Rect(0d, 0d, Lw, Lh);
                this.Lw = Lw;
                this.Lh = Lh;
            }

            double Iw = Image.Width,
                   Ih = Image.Height;
            if (NewImage || NewSize ||
                this.Iw != Iw || this.Ih != Ih)
            {
                this.Iw = Iw;
                this.Ih = Ih;

                double ScaleX = Lw / Iw,
                       ScaleY = Lh / Ih;

                if (ScaleX < ScaleY)
                {
                    Ih *= ScaleX;

                    Scale = ScaleX;
                    Region.X = 0d;
                    Region.Y = (Lh - Ih) / 2d;
                    Region.Width = Lw;
                    Region.Height = Ih;
                }
                else
                {
                    Iw *= ScaleY;

                    Scale = ScaleY;
                    Region.X = (Lw - Iw) / 2d;
                    Region.Y = 0d;
                    Region.Width = Iw;
                    Region.Height = Lh;
                }

                return true;
            }

            return false;
        }
        private bool RefreshSource(ImageSource Image, ImageViewer Viewer)
        {
            bool NewImage = false;
            int ImageHash = Image.GetHashCode();
            double Iw = Image.Width,
                   Ih = Image.Height;
            if (this.ImageHash != ImageHash)
            {
                NewImage = true;
                this.ImageHash = ImageHash;
                this.Iw = Iw;
                this.Ih = Ih;
            }
            bool NewScale = false;
            double Scale = Viewer.Scale;
            if (this.Scale != Scale)
            {
                NewScale = true;
                this.Scale = Scale;
                LayerPresenter.InvalidateMeasure();
            }

            // Refresh Region Size
            if (NewImage || NewScale)
            {
                Region.Width = Math.Floor(Iw * Scale);
                Region.Height = Math.Floor(Ih * Scale);
            }

            bool NewSize = false;
            double Lw = Layer.ActualWidth,
                   Lh = Layer.ActualHeight;
            if (ClipGeometry is null)
            {
                NewSize = true;
                ClipGeometry = new RectangleGeometry(new Rect(0d, 0d, Lw, Lh));
                this.Lw = Lw;
                this.Lh = Lh;
            }

            else if (this.Lw != Lw || this.Lh != Lh)
            {
                NewSize = true;
                ClipGeometry.Rect = new Rect(0d, 0d, Lw, Lh);
                this.Lw = Lw;
                this.Lh = Lh;
            }

            bool NewRegion = false;
            Rect Viewport = Viewer.Viewport;
            double Vx = Viewport.X,
                   Vy = Viewport.Y;
            if (this.Vx != Vx || this.Vy != Vy)
            {
                NewRegion = true;
                this.Vx = Viewport.X;
                this.Vy = Viewport.Y;
            }

            double ContextX = Viewer.ContextX,
                   ContextY = Viewer.ContextY;
            AlignContextLocation(Viewer, Layer, Iw, Ih, ref ContextX, ref ContextY);

            // Refresh Region Location
            if (NewImage || NewScale || NewRegion)
            {
                Region.X = (ContextX - Vx) * Scale;
                Region.Y = (ContextY - Vy) * Scale;
                return true;
            }

            return NewSize;
        }

        private void RefreshContext(IImageContext Image)
        {
            bool NewThumbnail = false;
            int ImageHash = Image.GetHashCode();
            double Iw = Image.Width,
                   Ih = Image.Height;
            if (this.ImageHash != ImageHash)
            {
                NewThumbnail = true;
                this.ImageHash = ImageHash;
                this.Iw = Iw;
                this.Ih = Ih;
            }

            double Lw = Layer.ActualWidth,
                   Lh = Layer.ActualHeight;
            if (ClipGeometry is null)
            {
                NewThumbnail = true;
                ClipGeometry = new RectangleGeometry(new Rect(0d, 0d, Lw, Lh));
                this.Lw = Lw;
                this.Lh = Lh;
            }

            else if (this.Lw != Lw || this.Lh != Lh)
            {
                NewThumbnail = true;
                ClipGeometry.Rect = new Rect(0d, 0d, Lw, Lh);
                this.Lw = Lw;
                this.Lh = Lh;
            }

            // Refresh Thumbnail
            if (NewThumbnail)
            {
                Scale = Math.Min(Lw / Iw, Lh / Ih);

                double ThumbnailScale = Math.Min(Scale, 1d);
                if (this.ThumbnailScale < ThumbnailScale)
                    RefreshThumbnail(Image, ThumbnailScale);

                // Refresh Region
                double Tw = Math.Floor(Iw * Scale),
                       Th = Math.Floor(Ih * Scale);
                Region = new((Lw - Tw) / 2d, (Lh - Th) / 2d, Tw, Th);

                // Reset LayerBlock Index
                LBL = LBT = LBR = LBB = -1;

                // Refresh RenderBlock
                if (!RenderBlocks.TryGetValue(Thumbnail, out RenderBlock RenderBlock))
                {
                    RenderBlock = new(Thumbnail);
                    RenderBlocks[Thumbnail] = RenderBlock;
                }

                RenderBlock.Region = Region;
                foreach (object Key in RenderBlocks.Keys.Where(i => i != Thumbnail))
                    RenderBlocks.Remove(Key);

                Layer.InvalidateVisual();
            }
        }

        private LayerCanvas RenderCanvas;
        private readonly Dictionary<object, RenderBlock> RenderBlocks = [];
        private void RefreshContext(IImageContext Image, ImageViewer Viewer)
        {
            bool NewImage = false;
            int ImageHash = Image.GetHashCode();
            double Iw = Image.Width,
                   Ih = Image.Height;
            if (this.ImageHash != ImageHash)
            {
                NewImage = true;
                this.ImageHash = ImageHash;
                this.Iw = Iw;
                this.Ih = Ih;

                // Reset Cache
                foreach (LayerCanvas Canvas in RenderCanvases)
                    Canvas.Reset();

                RenderCanvases.Clear();
                RenderCanvas = null;
            }

            bool NewScale = false;
            double Scale = Viewer.Scale;
            if (this.Scale != Scale)
            {
                NewScale = true;
                this.Scale = Scale;
                LayerPresenter.InvalidateMeasure();
            }

            if (NewImage || NewScale)
            {
                // Refresh Region Size
                Region.Width = Math.Floor(Iw * Scale);
                Region.Height = Math.Floor(Ih * Scale);

                RenderCanvas = GetLayerCanvas(Image, Scale);

                // Invalidates all RenderActions
                RenderingBlockTable.Clear();
            }

            bool NewSize = false;
            double Lw = Layer.ActualWidth,
                   Lh = Layer.ActualHeight;
            if (ClipGeometry is null)
            {
                NewSize = true;
                ClipGeometry = new RectangleGeometry(new Rect(0d, 0d, Lw, Lh));
                this.Lw = Lw;
                this.Lh = Lh;
            }
            else if (this.Lw != Lw || this.Lh != Lh)
            {
                NewSize = true;
                ClipGeometry.Rect = new Rect(0d, 0d, Lw, Lh);
                this.Lw = Lw;
                this.Lh = Lh;
            }

            // Refresh Thumbnail
            double MinScale = Viewer.MinScale;
            if (NewImage || ThumbnailScale < MinScale)
                RefreshThumbnail(Image, MinScale);

            bool NewRegion = false;
            Rect Viewport = Viewer.Viewport;
            double Vx = Viewport.X,
                   Vy = Viewport.Y;
            if (this.Vx != Vx || this.Vy != Vy)
            {
                NewRegion = true;
                this.Vx = Viewport.X;
                this.Vy = Viewport.Y;
            }

            double ContextX = Viewer.ContextX,
                   ContextY = Viewer.ContextY;
            AlignContextLocation(Viewer, Layer, Iw, Ih, ref ContextX, ref ContextY);

            bool NewUsage = NewImage || NewScale;
            if (Scale >= 1d)
            {
                LayerCanvas OriginalCanvas = GetLayerCanvas(Image, 1d);
                double Dx = Vx - ContextX,
                       Dy = Vy - ContextY;

                // Refresh Region Location
                Region.X = -Dx * Scale;
                Region.Y = -Dy * Scale;

                int IBcIndex = OriginalCanvas.ColumnLength - 1,
                    IBrIndex = OriginalCanvas.RowLength - 1,
                    LBL = MathHelper.Clamp((int)(Dx / RenderBlockSize), 0, IBcIndex),
                    LBT = MathHelper.Clamp((int)(Dy / RenderBlockSize), 0, IBrIndex),
                    LBR = MathHelper.Clamp((int)((Viewport.Right - ContextX) / RenderBlockSize), 0, IBcIndex),
                    LBB = MathHelper.Clamp((int)((Viewport.Bottom - ContextY) / RenderBlockSize), 0, IBrIndex);
                if (NewSize || this.LBL != LBL || this.LBT != LBT || this.LBR != LBR || this.LBB != LBB)
                {
                    NewUsage = true;
                    NewRegion = true;
                    this.LBL = LBL;
                    this.LBT = LBT;
                    this.LBR = LBR;
                    this.LBB = LBB;
                }

                // Refresh Blocks
                if (NewImage || NewScale || NewRegion)
                {
                    List<LayerBlock> NewBlocks = [];
                    foreach ((LayerBlock Block, Rect Region) in SearchReferenceBlock(Scale, OriginalCanvas, Dx, Dy, LBL, LBT, LBR, LBB))
                    {
                        NewBlocks.Add(Block);
                        if (NewUsage)
                            Block.Usage++;

                        Dictionary<object, RenderBlock> Table = Block.Context is null ? RenderingBlockTable : RenderBlocks;
                        if (!Table.TryGetValue(Block, out RenderBlock RenderBlock))
                        {
                            RenderBlock = new(Block);
                            Table[Block] = RenderBlock;
                        }

                        RenderBlock.Region = Region;
                    }

                    // Remove useless blocks.
                    foreach (object Old in RenderBlocks.Keys.Except(NewBlocks))
                    {
                        RenderBlocks.Remove(Old);
                        RenderingBlockTable.Remove(Old);
                    }

                    Layer.InvalidateVisual();
                }
            }
            else if (Scale <= ThumbnailScale)
            {
                NewUsage = false;
                LBL = LBT = LBR = LBB = -1;

                if (!RenderBlocks.TryGetValue(Thumbnail, out RenderBlock RenderBlock))
                {
                    RenderBlock = new(Thumbnail);
                    RenderBlocks[Thumbnail] = RenderBlock;
                }

                // Refresh Region
                Region.X = (ContextX - Vx) * Scale;
                Region.Y = (ContextY - Vy) * Scale;
                RenderBlock.Region = Region;

                // Remove useless blocks.
                foreach (object Key in RenderBlocks.Keys.Where(i => i != Thumbnail))
                    RenderBlocks.Remove(Key);

                Layer.InvalidateVisual();
            }
            else
            {
                double Dx = (Vx - ContextX) * Scale,
                       Dy = (Vy - ContextY) * Scale;

                // Refresh Region Location
                Region.X = -Dx;
                Region.Y = -Dy;

                int IBcIndex = RenderCanvas.ColumnLength - 1,
                    IBrIndex = RenderCanvas.RowLength - 1,
                    LBL = MathHelper.Clamp((int)(Dx / RenderBlockSize), 0, IBcIndex),
                    LBT = MathHelper.Clamp((int)(Dy / RenderBlockSize), 0, IBrIndex),
                    LBR = MathHelper.Clamp((int)((Viewport.Right - ContextX) * Scale / RenderBlockSize), 0, IBcIndex),
                    LBB = MathHelper.Clamp((int)((Viewport.Bottom - ContextY) * Scale / RenderBlockSize), 0, IBrIndex);
                if (NewSize || this.LBL != LBL || this.LBT != LBT || this.LBR != LBR || this.LBB != LBB)
                {
                    NewUsage = true;
                    NewRegion = true;
                    this.LBL = LBL;
                    this.LBT = LBT;
                    this.LBR = LBR;
                    this.LBB = LBB;
                }

                // Refresh Blocks
                if (NewImage || NewScale || NewRegion)
                {
                    List<LayerBlock> NewBlocks = [];
                    foreach ((LayerBlock Block, Rect Region) in SearchBlock(RenderCanvas, Dx, Dy, LBL, LBT, LBR, LBB))
                    {
                        NewBlocks.Add(Block);
                        if (NewUsage)
                            Block.Usage++;

                        Dictionary<object, RenderBlock> Table = Block.Context is null ? RenderingBlockTable : RenderBlocks;
                        if (!Table.TryGetValue(Block, out RenderBlock RenderBlock))
                        {
                            RenderBlock = new(Block);
                            Table[Block] = RenderBlock;
                        }

                        RenderBlock.Region = Region;
                    }

                    // Remove useless blocks.
                    foreach (object Old in RenderBlocks.Keys.Except(NewBlocks))
                    {
                        RenderBlocks.Remove(Old);
                        RenderingBlockTable.Remove(Old);
                    }

                    Layer.InvalidateVisual();
                }
            }

            // Usage
            if (NewUsage)
            {
                // When scale > 1, the LayerCanvas with scale 1 is used, so temporary images will never be generated.
                foreach (LayerCanvas Canvas in RenderCanvases.Where(i => i.Scale <= 1d))
                    Canvas.RefreshUsage();
            }
        }

        private readonly List<LayerCanvas> RenderCanvases = [];
        private LayerCanvas GetLayerCanvas(IImageContext Image, double Scale)
        {
            if (RenderCanvases.FirstOrDefault(i => i.Scale == Scale) is not LayerCanvas Canvas)
            {
                Canvas = new LayerCanvas(Image, Scale);
                int Index = RenderCanvases.FindIndex(i => i.Scale > Scale);
                if (Index < 0)
                    Index = RenderCanvases.Count;

                RenderCanvases.Insert(Index, Canvas);
            }

            return Canvas;
        }

        /// <summary>
        /// Search RenderBlock.
        /// </summary>
        /// <param name="Target">The LayerCanvas that is actually rendered.</param>
        /// <param name="Dx">The x-coordinate offset from the viewport to the image origin at scale 1 in the viewer.</param>
        /// <param name="Dy">The y-coordinate offset from the viewport to the image origin at scale 1 in the viewer.</param>
        /// <param name="LBL">The x index of the leftmost block of the reference LayerCanvas.</param>
        /// <param name="LBT">The y index of the topmost block of the reference LayerCanvas.</param>
        /// <param name="LBR">The x index of the rightmost block of the reference LayerCanvas.</param>
        /// <param name="LBB">The y index of the bottommost block of the reference LayerCanvas.</param>
        private static IEnumerable<(LayerBlock Block, Rect Region)> SearchBlock(LayerCanvas Target, double Dx, double Dy, int LBL, int LBT, int LBR, int LBB)
        {
            double I0 = Math.Round(LBL * RenderBlockSize - Dx),
                   Iy = Math.Round(LBT * RenderBlockSize - Dy);

            double Ix;
            for (int j = LBT; j <= LBB; j++, Iy += RenderBlockSize)
            {
                Ix = I0;
                for (int i = LBL; i <= LBR; i++, Ix += RenderBlockSize)
                {
                    LayerBlock Block = Target[i, j];
                    Rect Region = new(Ix, Iy, Block.Width, Block.Height);

                    yield return (Block, Region);
                }
            }
        }
        /// <summary>
        /// Search RenderBlock by reference LayerCanvas.
        /// </summary>
        /// <param name="TargetScale">The scale of LayerCanvas rendered in theory.</param>
        /// <param name="Reference">The LayerCanvas that is actually rendered.</param>
        /// <param name="Dx">The x-coordinate offset from the viewport to the image origin at scale 1 in the viewer.</param>
        /// <param name="Dy">The y-coordinate offset from the viewport to the image origin at scale 1 in the viewer.</param>
        /// <param name="LBL">The x index of the leftmost block of the reference LayerCanvas.</param>
        /// <param name="LBT">The y index of the topmost block of the reference LayerCanvas.</param>
        /// <param name="LBR">The x index of the rightmost block of the reference LayerCanvas.</param>
        /// <param name="LBB">The y index of the bottommost block of the reference LayerCanvas.</param>
        private static IEnumerable<(LayerBlock Block, Rect Region)> SearchReferenceBlock(double TargetScale, LayerCanvas Reference, double Dx, double Dy, int LBL, int LBT, int LBR, int LBB)
        {
            double ScaledBlockSize = RenderBlockSize / Reference.Scale * TargetScale,
                   I0 = Math.Round(LBL * ScaledBlockSize - Dx * TargetScale),
                   Iy = Math.Round(LBT * ScaledBlockSize - Dy * TargetScale);

            double Ix;
            for (int j = LBT; j <= LBB; j++, Iy += ScaledBlockSize)
            {
                Ix = I0;
                for (int i = LBL; i <= LBR; i++, Ix += ScaledBlockSize)
                {
                    LayerBlock Block = Reference[i, j];
                    Rect Region = new(Ix, Iy, Block.Width * TargetScale, Block.Height * TargetScale);

                    yield return (Block, Region);
                }
            }
        }

        public double ThumbnailScale;
        public WriteableBitmap Thumbnail;
        private unsafe void RefreshThumbnail(IImageContext Image, double Scale)
        {
            ThumbnailScale = Scale;
            int Iw = (int)Math.Floor(Image.Width * Scale),
                Ih = (int)Math.Floor(Image.Height * Scale);

            if (Thumbnail is null || Thumbnail.PixelWidth < Iw || Thumbnail.PixelHeight < Ih)
                Thumbnail = new WriteableBitmap(Iw, Ih, 96d, 96d, PixelFormats.Bgra32, null);

            try
            {
                Thumbnail.Lock();
                NearestResizePixelAdapter<BGRA> Adapter0 = new(Image, Iw, Ih);

                byte* pDest0 = (byte*)Thumbnail.BackBuffer;
                long Stride = Thumbnail.BackBufferStride;
                _ = Parallel.For(0, Ih, y =>
                {
                    PixelAdapter<BGRA> Adapter = Adapter0.Clone();
                    Adapter.DangerousMove(0, y);

                    BGRA* pDest = (BGRA*)(pDest0 + Stride * y);
                    for (int x = 0; x < Iw; x++, Adapter.DangerousMoveNextX(), pDest++)
                        Adapter.OverrideTo(pDest);
                });
            }
            finally
            {
                Thumbnail.AddDirtyRect(new Int32Rect(0, 0, Iw, Ih));
                Thumbnail.Unlock();
            }
        }

        public void Render(DrawingContext Context)
        {
            if (HasImage)
            {
                ImageSource Source = Layer.Source;
                if (HasViewer)
                {
                    Context.PushClip(ClipGeometry);
                    Context.DrawImage(Source, Region);
                    Context.Pop();

                    if (HasMarks)
                    {
                        TranslatePoint(0d, 0d, out double Ix, out double Iy);
                        Rect Viewport = new(Ix, Iy, Lw / Scale, Lh / Scale);
                        foreach (ImageViewerLayerMark Mark in Layer.Marks.Where(i => i.Visible))
                            RenderMark(Mark, Context, Viewport, Scale);
                    }
                }
                else
                {
                    Context.DrawImage(Source, Region);

                    if (HasMarks)
                    {
                        Context.PushClip(ClipGeometry);

                        TranslatePoint(0d, 0d, out double Ix, out double Iy);
                        Rect Viewport = new(Ix, Iy, Lw / Scale, Lh / Scale);
                        foreach (ImageViewerLayerMark Mark in Layer.Marks.Where(i => i.Visible))
                            RenderMark(Mark, Context, Viewport, Scale);
                    }
                }
            }

            else if (HasImageContext)
            {
                Context.PushClip(ClipGeometry);

                // Guild Line
                GuidelineSet GuideLines = new();
                foreach (Rect Region in RenderBlocks.Values.Select(i => i.Region))
                {
                    double Temp = Region.Left;
                    if (!GuideLines.GuidelinesX.Contains(Temp))
                        GuideLines.GuidelinesX.Add(Temp);

                    Temp = Region.Right;
                    if (!GuideLines.GuidelinesX.Contains(Temp))
                        GuideLines.GuidelinesX.Add(Temp);

                    Temp = Region.Top;
                    if (!GuideLines.GuidelinesY.Contains(Temp))
                        GuideLines.GuidelinesY.Add(Temp);

                    Temp = Region.Bottom;
                    if (!GuideLines.GuidelinesY.Contains(Temp))
                        GuideLines.GuidelinesY.Add(Temp);
                }

                Context.PushGuidelineSet(GuideLines);

                // Image
                foreach (RenderBlock Block in RenderBlocks.Values)
                    Context.DrawImage(Block.Context, Block.Region);

                if (HasMarks)
                {
                    TranslatePoint(0d, 0d, out double Ix, out double Iy);
                    Rect Viewport = new(Ix, Iy, Lw / Scale, Lh / Scale);
                    foreach (ImageViewerLayerMark Mark in Layer.Marks.Where(i => i.Visible))
                        RenderMark(Mark, Context, Viewport, Scale);
                }

#if EnableBlockGrid
                Pen GridPen = new(Brushes.Red, 1d);
                foreach (double X in GuideLines.GuidelinesX)
                    Context.DrawLine(GridPen, new Point(X, 0), new Point(X, Layer.ActualHeight));

                foreach (double Y in GuideLines.GuidelinesY)
                    Context.DrawLine(GridPen, new Point(0, Y), new Point(Layer.ActualWidth, Y));
#endif

            }

            else if (HasMarks &&
                     Layer.Parent is ImageViewer Viewer)
            {
                Context.PushClip(ClipGeometry);

                Rect Viewport = Viewer.Viewport;
                Viewport.X -= Viewer.ContextX;
                Viewport.Y -= Viewer.ContextY;

                foreach (ImageViewerLayerMark Mark in Layer.Marks.Where(i => i.Visible))
                    RenderMark(Mark, Context, Viewport, Viewer.Scale);
            }
        }

        public void RenderMark(ImageViewerLayerMark Mark, DrawingContext Context, Rect Viewport, double Scale)
        {
            ImageSource Visual = Mark.GetVisual();

            double Iw = Visual.Width,
                   Ih = Visual.Height,
                   HIw = Iw / 2d,
                   HIh = Ih / 2d,
                   Lx = Viewport.Left - HIw,
                   Rx = Viewport.Right + HIw,
                   Ty = Viewport.Top - HIh,
                   By = Viewport.Bottom + HIh;

            if (Mark.Zoomable)
            {
                double Factor = MathHelper.Clamp(Scale, Mark.ZoomMinScale, Mark.ZoomMaxScale);
                Iw *= Factor;
                Ih *= Factor;
                HIw *= Factor;
                HIh *= Factor;
            }

            Rect Dirty = new(double.NaN, double.NaN, Iw, Ih);
            Brush Brush = Mark.GetBrush();
            foreach (Point Location in Mark.Locations.Where(i => Lx <= i.X && i.X <= Rx && Ty <= i.Y && i.Y <= By))
            {
                Dirty.X = (Location.X - Viewport.X) * Scale - HIw;
                Dirty.Y = (Location.Y - Viewport.Y) * Scale - HIh;
                Context.DrawRectangle(Brush, null, Dirty);
            }
        }

        /// <summary>
        /// Translates the specified position in this layer to the image coordinate.
        /// </summary>
        /// <param name="Lx">The x-coordinate of the specified position in this layer.</param>
        /// <param name="Ly">The y-coordinate of the specified position in this layer.</param>
        /// <param name="Ix">The x-coordinate in image.</param>
        /// <param name="Iy">The y-coordinate in image.</param>
        public void TranslatePoint(double Lx, double Ly, out double Ix, out double Iy)
        {
            if (HasImage || HasImageContext)
            {
                Ix = (Lx - Region.X) / Scale;
                Iy = (Ly - Region.Y) / Scale;
            }
            else
            {
                Ix = double.NaN;
                Iy = double.NaN;
            }
        }
        /// <summary>
        /// Translates the specified image coordinate to the position in this layer.
        /// </summary>
        /// <param name="Lx">The x-coordinate of the specified position in this layer.</param>
        /// <param name="Ly">The y-coordinate of the specified position in this layer.</param>
        /// <param name="Ix">The x-coordinate in image.</param>
        /// <param name="Iy">The y-coordinate in image.</param>
        public void TranslatePoint(out double Lx, out double Ly, double Ix, double Iy)
        {
            if (HasImage || HasImageContext)
            {
                Lx = Ix * Scale + Region.X;
                Ly = Iy * Scale + Region.Y;
            }
            else
            {
                Lx = double.NaN;
                Ly = double.NaN;
            }
        }
        /// <summary>
        /// Translates the specified position in this layer to the global image coordinate.
        /// </summary>
        /// <param name="Lx">The x-coordinate of the specified position in this layer.</param>
        /// <param name="Ly">The y-coordinate of the specified position in this layer.</param>
        /// <param name="Ix">The x-coordinate in image.</param>
        /// <param name="Iy">The y-coordinate in image.</param>
        public void TranslateGlobalPoint(double Lx, double Ly, out double Ix, out double Iy)
        {
            if (Layer.Parent is ImageViewer Viewer)
            {
                TranslatePoint(Lx, Ly, out Ix, out Iy);
                if (!double.IsNaN(Ix) && !double.IsNaN(Iy))
                    AlignContextLocation(Viewer, Layer, Iw, Ih, ref Ix, ref Iy);
            }
            else
            {
                Ix = double.NaN;
                Iy = double.NaN;
            }
        }

        public static void AlignContextLocation(ImageViewer Viewer, ImageViewerLayer Layer, double Iw, double Ih, ref double Ix, ref double Iy)
        {
            // Horizontal
            switch (Layer.HorizontalAlignment)
            {
                case HorizontalAlignment.Center:
                    {
                        Ix += (Viewer.ContextWidth - Iw) / 2d;
                        break;
                    }
                case HorizontalAlignment.Right:
                    {
                        Ix += Viewer.ContextWidth - Iw;
                        break;
                    }

                case HorizontalAlignment.Left:
                case HorizontalAlignment.Stretch:
                default:
                    break;
            }

            // Vertical
            switch (Layer.VerticalAlignment)
            {
                case VerticalAlignment.Center:
                    {
                        Iy += (Viewer.ContextHeight - Ih) / 2d;
                        break;
                    }
                case VerticalAlignment.Bottom:
                    {
                        Iy += Viewer.ContextHeight - Ih;
                        break;
                    }

                case VerticalAlignment.Top:
                case VerticalAlignment.Stretch:
                default:
                    break;
            }
        }

        private const int MaxRecycleCount = 10;
        private static readonly ConcurrentQueue<WriteableBitmap> Recycler = new();
        private static WriteableBitmap Rent()
            => Recycler.TryDequeue(out WriteableBitmap BlockCanvas) ? BlockCanvas : new WriteableBitmap(RenderBlockSize, RenderBlockSize, 96d, 96d, PixelFormats.Bgra32, null);
        private static void Return(WriteableBitmap BlockCanvas)
        {
            if (Recycler.Count < MaxRecycleCount)
                Recycler.Enqueue(BlockCanvas);
        }

        private sealed class LayerCanvas
        {
            public const int OriginalMaxUsage = 20; // Give the original ratio a higher number of usage
            public const int CommonMaxUsage = 10;

            private readonly LayerBlock[,] Blocks;

            public int Width { get; }

            public int Height { get; }

            public double Scale { get; }

            public int ColumnLength { get; }

            public int RowLength { get; }

            public LayerBlock this[int Column, int Row]
                => Blocks[Column, Row];

            public LayerCanvas(IImageContext Source, double Scale)
            {
                this.Scale = Scale;

                int MaxUsage;
                if (Scale == 1d)
                {
                    Width = Source.Width;
                    Height = Source.Height;
                    MaxUsage = OriginalMaxUsage;
                }
                else
                {
                    Width = (int)(Source.Width * Scale);
                    Height = (int)(Source.Height * Scale);
                    MaxUsage = CommonMaxUsage;
                }

                ColumnLength = (Width + LessRenderBlockSize) / RenderBlockSize;
                RowLength = (Height + LessRenderBlockSize) / RenderBlockSize;

                Blocks = new LayerBlock[ColumnLength, RowLength];
                Initialize(Source, Width, Height, MaxUsage);
            }

            private void Initialize(IImageContext Source, int SIw, int SIh, int MaxUsage)
            {
                double ViewportBlockSize = RenderBlockSize / Scale,
                       Iw = Source.Width,
                       Ih = Source.Height,
                       Vx, NVx, Vy, NVy, Vw, Vh;

                int Sx, NSx, Bw, Bh;

                Vy = 0d;
                NVy = ViewportBlockSize;
                PixelAdapter<BGRA> Adapter = Scale == 1d ? Source.GetAdapter<BGRA>(0, 0) : new NearestResizePixelAdapter<BGRA>(Source, SIw, SIh);
                for (int j = 0, Sy = 0, NSy = RenderBlockSize; j < RowLength; j++, NSy += RenderBlockSize, NVy += ViewportBlockSize)
                {
                    Sx = 0;
                    Bh = NSy <= SIh ? RenderBlockSize : SIh - Sy;
                    if (Bh <= 0)
                        break;

                    Vx = 0d;
                    Vh = NVy <= Ih ? ViewportBlockSize : Ih - Vy;

                    NSx = RenderBlockSize;
                    NVx = ViewportBlockSize;
                    for (int i = 0; i < ColumnLength; i++, NSx += RenderBlockSize, NVx += ViewportBlockSize)
                    {
                        Bw = NSx <= SIw ? RenderBlockSize : SIw - Sx;
                        if (Bw <= 0)
                            break;

                        Vw = NVx <= Iw ? ViewportBlockSize : Iw - Vx;

                        Blocks[i, j] = new(Sx, Sy, Bw, Bh, new Rect(Vx, Vy, Vw, Vh), Adapter, MaxUsage);

                        Sx = NSx;
                        Vx = NVx;
                    }

                    Sy = NSy;
                    Vy = NVy;
                }
            }

            public void RefreshUsage()
            {
                foreach (LayerBlock Block in Blocks)
                {
                    if (Block.Context is not null &&
                        --Block.Usage <= 0)
                        Block.Reset();
                }
            }

            /// <summary>
            /// Destroy Canvas
            /// </summary>
            public void Reset()
            {
                foreach (LayerBlock Block in Blocks)
                    Block.Reset();
            }

            public override bool Equals(object obj)
                => obj is LayerCanvas canvas && Scale == canvas.Scale;
            public override int GetHashCode()
#if NET7_0_OR_GREATER
                => HashCode.Combine(Scale);
#else
                => -16161351 + Scale.GetHashCode();
#endif

        }

        private sealed class LayerBlock(int Sx, int Sy, int Bw, int Bh, Rect Viewport, PixelAdapter<BGRA> Adapter, int MaxUsage)
        {
            private readonly Int32Rect RenderRect = new(0, 0, Bw, Bh);
            private readonly PixelAdapter<BGRA> Adapter = Adapter;
            private readonly int MaxUsage = MaxUsage;

            private WriteableBitmap Canvas;

            /// <summary>
            /// Determines the life cycle.<para/>
            /// It is released when the number &lt;= 0 and reset when <see cref="Render"/>.
            /// </summary>
            public int Usage { get; set; }

            /// <summary>
            /// Image
            /// </summary>
            public BitmapSource Context { get; private set; }

            /// <summary>
            /// Gets the x-coordinate of the block in the scaled image.
            /// </summary>
            public int Sx { get; } = Sx;

            /// <summary>
            /// Gets the y-coordinate of the block in the scaled image.
            /// </summary>
            public int Sy { get; } = Sy;

            /// <summary>
            /// Gets the width of block.
            /// </summary>
            public int Width { get; } = Bw;

            /// <summary>
            /// Gets the height of block.
            /// </summary>
            public int Height { get; } = Bh;

            /// <summary>
            /// Gets the viewport in the original image.
            /// </summary>
            public Rect Viewport { get; } = Viewport;

            private bool IsRendering;

            /// <summary>
            /// Generate Canvas
            /// </summary>
            public unsafe void Render()
            {
                if (Canvas is not null ||
                    IsRendering)
                    return;

                IsRendering = true;
                Usage = MaxUsage;

                try
                {
                    Canvas = Rent();
                    Canvas.Lock();

                    byte* pDest0 = (byte*)Canvas.BackBuffer;
                    long Stride = Canvas.BackBufferStride;

                    PixelAdapter<BGRA> Adapter = this.Adapter.Clone();
                    for (int y = 0; y < Height; y++)
                    {
                        Adapter.DangerousMove(Sx, Sy + y);

                        BGRA* pDest = (BGRA*)(pDest0 + Stride * y);
                        for (int x = 0; x < Width; x++, Adapter.DangerousMoveNextX(), pDest++)
                            Adapter.OverrideTo(pDest);
                    }

                    Canvas.AddDirtyRect(RenderRect);
                    Canvas.Unlock();

                    Context = Canvas.PixelWidth != Width || Canvas.PixelHeight != Height ? new CroppedBitmap(Canvas, RenderRect) : Canvas;
                }
                finally
                {
                    IsRendering = false;
                }
            }

            /// <summary>
            /// Destroy Canvas
            /// </summary>
            public void Reset()
            {
                if (Canvas is null)
                    return;

                Return(Canvas);
                Canvas = null;
                Context = null;
            }

        }

        private sealed class RenderBlock
        {
            public BitmapSource Context { get; private set; }

            public Rect Region { get; set; }

            public bool IsRendered
                => Context != null;

            public RenderBlock(BitmapSource Context)
            {
                this.Context = Context;
            }

            private readonly LayerBlock Block;
            public RenderBlock(LayerBlock Block)
            {
                this.Block = Block;
                Context = Block.Context;
            }

            private bool IsRendering;
            public void Render()
            {
                if (Block is null)
                    return;

                if (IsRendering)
                    return;

                IsRendering = true;
                try
                {
                    Block.Render();
                    Context = Block.Context;
                }
                finally
                {
                    IsRendering = false;
                }
            }

            public override string ToString()
                => $"Region : {Region}, IsRendering : {IsRendering}";

        }

    }
}