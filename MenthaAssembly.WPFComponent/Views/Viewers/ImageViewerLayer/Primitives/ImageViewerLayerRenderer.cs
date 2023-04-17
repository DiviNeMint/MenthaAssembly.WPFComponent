using MenthaAssembly.Media.Imaging;
using MenthaAssembly.Media.Imaging.Utils;
using System;
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

        public bool HasViewer, HasImage, HasImageContext, HasMarks;
        public double Lw, Lh, Iw, Ih, Scale, Vx, Vy;   // LayerWidth, LayerHeight, ImageWidth, ImageHeight, LayerScale, ViewportX, ViewportY
        public int IBc, IBr, LBL, LBT, LBR, LBB;       // ImageBlockColumn, ImageBlockRow, LayerBlockLeftIndex, LayerBlockTopIndex, LayerBlockRightIndex, LayerBlockBottomIndex

        private readonly ImageViewerLayer Layer;
        private readonly ImageViewerLayerPresenter LayerPresenter;
        public ImageViewerLayerRenderer(ImageViewerLayer Layer, ImageViewerLayerPresenter LayerPresenter)
        {
            this.Layer = Layer;
            this.LayerPresenter = LayerPresenter;

            Lw = Lh = Iw = Ih = Scale = Vx = Vy = double.NaN;
            IBc = IBr = LBL = LBT = LBR = LBB = 0;
        }

        private bool IsValid = true;
        public void Invalidate()
        {
            if (IsValid && Layer.IsVisible)
            {
                Dispatcher Dispatcher = Layer.Dispatcher;
                if (Dispatcher.CheckAccess())
                    Refresh();
                else
#if NET462
                    Dispatcher.BeginInvoke(new Action(Refresh), DispatcherPriority.Render);
#else
                    Dispatcher.BeginInvoke(Refresh, DispatcherPriority.Render);
#endif
            }
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

        private ScaleTransform ScaleTransform;
        private bool RefreshSource(ImageSource Image, ImageViewer Viewer)
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

            bool NewScale = false;
            double LCx = Lw / 2d,
                   LCy = Lh / 2d,
                   Scale = Viewer.Scale;
            if (ScaleTransform is null)
            {
                NewScale = true;
                this.Scale = Scale;
                ScaleTransform = new ScaleTransform(Scale, Scale, LCx, LCy);
                LayerPresenter.InvalidateMeasure();
            }
            else
            {
                if (NewSize)
                {
                    ScaleTransform.CenterX = LCx;
                    ScaleTransform.CenterY = LCy;
                }

                if (this.Scale != Scale)
                {
                    NewScale = true;
                    this.Scale = Scale;
                    ScaleTransform.ScaleX = Scale;
                    ScaleTransform.ScaleY = Scale;
                    LayerPresenter.InvalidateMeasure();
                }
            }

            double Tx = LCx - Viewer.ViewportCx + Viewer.ContextX,
                   Ty = LCy - Viewer.ViewportCy + Viewer.ContextY,
                   Iw = Image.Width,
                   Ih = Image.Height;

            AlignContextLocation(Viewer, Layer, Iw, Ih, ref Tx, ref Ty);

            bool NewLocation = false;
            if (this.Iw != Iw ||
                this.Ih != Ih)
            {
                NewLocation = true;
                this.Iw = Iw;
                this.Ih = Ih;
                Region.X = Tx;
                Region.Y = Ty;
                Region.Width = Iw;
                Region.Height = Ih;
            }

            else if (Region.X != Tx ||
                     Region.Y != Ty)
            {
                NewLocation = true;
                Region.X = Tx;
                Region.Y = Ty;
            }

            return NewImage || NewSize || NewScale || NewLocation;
        }

        private readonly Int32Rect DirtyRect = new(0, 0, RenderBlockSize, RenderBlockSize);
        private readonly Dictionary<int, ImageViewerLayerRenderBlock> RenderBlocks = new();
        private unsafe void RefreshContext(IImageContext Image)
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

            if (NewImage || NewSize)
            {
                double ScaleX = Lw / Iw,
                       ScaleY = Lh / Ih,
                       SIx, SIy, SIw, SIh;
                if (ScaleX < ScaleY)
                {
                    Scale = ScaleX;
                    Ih *= ScaleX;

                    SIx = 0d;
                    SIy = Math.Round((Lh - Ih) / 2d);
                    SIw = Lw;
                    SIh = Ih;
                }
                else
                {
                    Scale = ScaleY;
                    Iw *= ScaleY;

                    SIx = Math.Round((Lw - Iw) / 2d);
                    SIy = 0d;
                    SIw = Iw;
                    SIh = Lh;
                }

                int IBc = (int)Math.Ceiling(SIw / RenderBlockSize),
                    IBr = (int)Math.Ceiling(SIh / RenderBlockSize);

                // Invalidates all blocks.
                foreach (WriteableBitmap Canvas in RecyclableCanvas.Values)
                    EnqueueBlockCanvas(Canvas, -1);

                RecyclableCanvas.Clear();

                foreach (ImageViewerLayerRenderBlock Block in RenderBlocks.Values)
                    EnqueueBlockCanvas(Block, -1);

                // Checks that enough blocks.
                int LastCount = Math.Max(this.IBc * this.IBr, RenderBlocks.Count),
                    Count = IBc * IBr;

                for (; LastCount < Count; LastCount++)
                    RenderBlocks.Add(LastCount, new ImageViewerLayerRenderBlock());

                // Refresh BlockInfos
                LBL = LBT = 0;
                LBR = IBc - 1;
                LBB = IBr - 1;
                this.IBc = IBc;
                this.IBr = IBr;

                // Refresh Blocks
                int IntSIw = (int)SIw,
                    IntSIh = (int)SIh,
                    Index = 0,
                    Sx, Sy = 0,
                    Bw, Bh;

                double Ix,
                       Iy = SIy;

                bool IsRefresh = false;
                NearestResizePixelAdapter<BGRA> Adapter0 = new(Image, IntSIw, IntSIh);
                for (int j = 0; j < IBr; j++, Sy += RenderBlockSize, Iy += RenderBlockSize)
                {
                    Sx = 0;
                    Ix = SIx;
                    Bh = Sy + RenderBlockSize <= IntSIh ? RenderBlockSize : IntSIh - Sy;
                    if (Bh <= 0)
                        break;

                    for (int i = 0; i < IBc; i++, Index++, Sx += RenderBlockSize, Ix += RenderBlockSize)
                    {
                        Bw = Sx + RenderBlockSize <= IntSIw ? RenderBlockSize : IntSIw - Sx;
                        if (Bw <= 0)
                            break;

                        ImageViewerLayerRenderBlock Block = RenderBlocks[Index];
                        if (!Block.IsValid)
                        {
                            try
                            {
                                Block.IsRendering = true;

                                WriteableBitmap Canvas = Block.Canvas;
                                if (Canvas is null)
                                {
                                    if (!TryGetCacheCanvas(Index, out Canvas))
                                    {
                                        Canvas = GetCanvas();
                                        Canvas.Lock();

                                        byte* pDest0 = (byte*)Canvas.BackBuffer;
                                        long Stride = Canvas.BackBufferStride;
                                        _ = Parallel.For(0, Bh, y =>
                                        {
                                            PixelAdapter<BGRA> Adapter = Adapter0.Clone();
                                            Adapter.InternalMove(Sx, Sy + y);

                                            BGRA* pDest = (BGRA*)(pDest0 + Stride * y);
                                            for (int x = 0; x < Bw; x++, Adapter.InternalMoveNext(), pDest++)
                                                Adapter.OverrideTo(pDest);
                                        });

                                        Canvas.AddDirtyRect(DirtyRect);
                                        Canvas.Unlock();
                                    }

                                    Block.Canvas = Canvas;
                                }

                                Block.Bitmap = Bw == RenderBlockSize && Bh == RenderBlockSize ? Canvas :
                                                                                                new CroppedBitmap(Block.Canvas, new Int32Rect(0, 0, Bw, Bh));
                                Layer.InvalidateVisual();
                            }
                            finally
                            {
                                Block.IsRendering = false;
                                IsRefresh = true;
                            }
                        }

                        Block.Region.X = Ix;
                        Block.Region.Y = Iy;
                        Block.Region.Width = Bw;
                        Block.Region.Height = Bh;
                    }
                }

                if (!IsRefresh)
                    Layer.InvalidateVisual();

                // Free some unused canvas.
                int UnusedCount = UnusedCanvas.Count >> 1;
                for (int i = 0; i < UnusedCount; i++)
                    _ = UnusedCanvas.Dequeue();
            }
        }
        private unsafe void RefreshContext(IImageContext Image, ImageViewer Viewer)
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

            int IBc = this.IBc,
                IBr = this.IBr;
            double SIw = Math.Round(Iw * Scale),
                   SIh = Math.Round(Ih * Scale);
            if (NewImage || NewScale)
            {
                IBc = (int)Math.Ceiling(SIw / RenderBlockSize);
                IBr = (int)Math.Ceiling(SIh / RenderBlockSize);

                // Invalidates all blocks.
                if (RecyclableCanvas.Count > 0)
                {
                    foreach (WriteableBitmap Canvas in RecyclableCanvas.Values)
                        EnqueueBlockCanvas(Canvas, -1);

                    RecyclableCanvas.Clear();
                }

                if (RenderBlocks.Count > 0)
                    foreach (ImageViewerLayerRenderBlock Block in RenderBlocks.Values)
                        EnqueueBlockCanvas(Block, -1);

                // Checks that enough blocks.
                int LastCount = Math.Max(this.IBc * this.IBr, RenderBlocks.Count),
                    Count = IBc * IBr;
                for (; LastCount < Count; LastCount++)
                    RenderBlocks.Add(LastCount, new ImageViewerLayerRenderBlock());

                this.IBc = IBc;
                this.IBr = IBr;
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

            double Dx = (Vx - ContextX) * Scale,
                   Dy = (Vy - ContextY) * Scale;

            int IBcIndex = IBc - 1,
                IBrIndex = IBr - 1,
                LBL = MathHelper.Clamp((int)(Dx / RenderBlockSize), 0, IBcIndex),
                LBT = MathHelper.Clamp((int)(Dy / RenderBlockSize), 0, IBrIndex),
                LBR = MathHelper.Clamp((int)((Viewport.Right - ContextX) * Scale / RenderBlockSize), 0, IBcIndex),
                LBB = MathHelper.Clamp((int)((Viewport.Bottom - ContextY) * Scale / RenderBlockSize), 0, IBrIndex);
            if (NewSize || this.LBL != LBL || this.LBT != LBT || this.LBR != LBR || this.LBB != LBB)
            {
                NewRegion = true;

                if (!NewImage && !NewScale)
                {
                    // Recyclable blocks
                    int j = 0,
                        Index = 0;

                    // Up
                    for (; j < LBT; j++)
                        for (int i = 0; i < IBc; i++, Index++)
                            EnqueueBlockCanvas(RenderBlocks[Index], Index);

                    for (; j <= LBB; j++)
                    {
                        // Left
                        for (int i = 0; i < LBL; i++, Index++)
                            EnqueueBlockCanvas(RenderBlocks[Index], Index);

                        // Right
                        Index += LBR - LBL + 1;
                        for (int i = LBR + 1; i < IBc; i++, Index++)
                            EnqueueBlockCanvas(RenderBlocks[Index], Index);
                    }

                    // Bottom
                    for (; j < IBr; j++)
                        for (int i = 0; i < IBc; i++, Index++)
                            EnqueueBlockCanvas(RenderBlocks[Index], Index);
                }

                this.LBL = LBL;
                this.LBT = LBT;
                this.LBR = LBR;
                this.LBB = LBB;
            }

            // Refresh Blocks
            if (NewImage || NewScale || NewRegion)
            {
                int Index0 = LBT * IBc + LBL,
                    Index, Sx,
                    S0 = LBL * RenderBlockSize,
                    Sy = LBT * RenderBlockSize,
                    IntSIw = (int)SIw,
                    IntSIh = (int)SIh,
                    Bw, Bh;

                double I0 = Math.Round(S0 - Dx),
                       Ix,
                       Iy = Math.Round(Sy - Dy);

                bool IsRefresh = false;
                NearestResizePixelAdapter<BGRA> Adapter0 = new(Image, IntSIw, IntSIh);
                for (int j = LBT; j <= LBB; j++, Index0 += IBc, Sy += RenderBlockSize, Iy += RenderBlockSize)
                {
                    Index = Index0;
                    Sx = S0;
                    Ix = I0;
                    Bh = Sy + RenderBlockSize <= IntSIh ? RenderBlockSize : IntSIh - Sy;
                    if (Bh <= 0)
                        break;

                    for (int i = LBL; i <= LBR; i++, Index++, Sx += RenderBlockSize, Ix += RenderBlockSize)
                    {
                        Bw = Sx + RenderBlockSize <= IntSIw ? RenderBlockSize : IntSIw - Sx;
                        if (Bw <= 0)
                            break;

                        ImageViewerLayerRenderBlock Block = RenderBlocks[Index];
                        if (!Block.IsValid)
                        {
                            try
                            {
                                Block.IsRendering = true;

                                WriteableBitmap Canvas = Block.Canvas;
                                if (Canvas is null)
                                {
                                    if (!TryGetCacheCanvas(Index, out Canvas))
                                    {
                                        Canvas = GetCanvas();
                                        Canvas.Lock();

                                        byte* pDest0 = (byte*)Canvas.BackBuffer;
                                        long Stride = Canvas.BackBufferStride;
                                        _ = Parallel.For(0, Bh, y =>
                                        {
                                            PixelAdapter<BGRA> Adapter = Adapter0.Clone();
                                            Adapter.InternalMove(Sx, Sy + y);

                                            BGRA* pDest = (BGRA*)(pDest0 + Stride * y);
                                            for (int x = 0; x < Bw; x++, Adapter.InternalMoveNext(), pDest++)
                                                Adapter.OverrideTo(pDest);
                                        });

                                        Canvas.AddDirtyRect(DirtyRect);
                                        Canvas.Unlock();
                                    }

                                    Block.Canvas = Canvas;
                                }

                                Block.Bitmap = Bw == RenderBlockSize && Bh == RenderBlockSize ? Canvas :
                                                                                                new CroppedBitmap(Block.Canvas, new Int32Rect(0, 0, Bw, Bh));
                                if (NewScale)
                                    Layer.Dispatcher.Invoke(Layer.InvalidateVisual, DispatcherPriority.ApplicationIdle);
                                else
                                    Layer.InvalidateVisual();
                            }
                            finally
                            {
                                Block.IsRendering = false;
                                IsRefresh = true;
                            }
                        }

                        Block.Region.X = Ix;
                        Block.Region.Y = Iy;
                        Block.Region.Width = Bw;
                        Block.Region.Height = Bh;
                    }
                }

                if (!IsRefresh)
                {
                    if (NewScale)
                        Layer.Dispatcher.Invoke(Layer.InvalidateVisual, DispatcherPriority.ApplicationIdle);
                    else
                        Layer.InvalidateVisual();
                }
            }

            // Free some unused canvas.
            int UnusedCount = UnusedCanvas.Count >> 1;
            for (int i = 0; i < UnusedCount; i++)
                _ = UnusedCanvas.Dequeue();
        }

        private readonly Dictionary<int, WriteableBitmap> RecyclableCanvas = new();
        private readonly Queue<WriteableBitmap> UnusedCanvas = new();
        private void EnqueueBlockCanvas(ImageViewerLayerRenderBlock Block, int Index)
        {
            if (!Block.IsRendering && Block.Canvas != null)
            {
                EnqueueBlockCanvas(Block.Canvas, Index);
                Block.Canvas = null;
            }
        }
        private void EnqueueBlockCanvas(WriteableBitmap Canvas, int Index)
        {
            if (Index == -1)
                UnusedCanvas.Enqueue(Canvas);
            else
                RecyclableCanvas[Index] = Canvas;
        }
        private bool TryGetCacheCanvas(int Index, out WriteableBitmap Canvas)
        {
            if (RecyclableCanvas.TryGetValue(Index, out Canvas))
            {
                _ = RecyclableCanvas.Remove(Index);
                return true;
            }

            return false;
        }
        private WriteableBitmap GetCanvas()
        {
#if NET462
            if (UnusedCanvas.Count > 0)
            {
                WriteableBitmap Canvas = UnusedCanvas.Dequeue();
                return Canvas;
            }
#else
            if (UnusedCanvas.TryDequeue(out WriteableBitmap Canvas))
                return Canvas;
#endif

            if (RecyclableCanvas.Count > 0)
            {
                KeyValuePair<int, WriteableBitmap> Datas = RecyclableCanvas.LastOrDefault();
                _ = RecyclableCanvas.Remove(Datas.Key);
                return Datas.Value;
            }

            return new WriteableBitmap(RenderBlockSize, RenderBlockSize, 96d, 96d, PixelFormats.Bgra32, null);
        }

        public void Render(DrawingContext Context)
        {
            if (HasImage)
            {
                ImageSource Source = Layer.Source;
                if (HasViewer)
                {
                    Context.PushClip(ClipGeometry);
                    Context.PushTransform(ScaleTransform);
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

                ImageViewerLayerRenderBlock Block = null;
                int Index0 = LBT * IBc + LBL,
                    Index = Index0;

                // Guild Line
                GuidelineSet GuideLines = new GuidelineSet();
                for (int i = LBL; i <= LBR; i++, Index++)
                {
                    Block = RenderBlocks[Index];
                    GuideLines.GuidelinesX.Add(Block.Region.Left);
                }
                GuideLines.GuidelinesX.Add(Block.Region.Right);

                Index = Index0;
                for (int j = LBT; j <= LBB; j++, Index += IBc)
                {
                    Block = RenderBlocks[Index];
                    GuideLines.GuidelinesY.Add(Block.Region.Top);
                }
                GuideLines.GuidelinesY.Add(Block.Region.Bottom);

                Context.PushGuidelineSet(GuideLines);

                // Image
                for (int j = LBT; j <= LBB; j++, Index0 += IBc)
                {
                    Index = Index0;
                    for (int i = LBL; i <= LBR; i++, Index++)
                    {
                        Block = RenderBlocks[Index];
                        if (Block.IsValid)
                            Context.DrawImage(Block.Bitmap, Block.Region);
                    }
                }

                if (HasMarks)
                {
                    TranslatePoint(0d, 0d, out double Ix, out double Iy);
                    Rect Viewport = new(Ix, Iy, Lw / Scale, Lh / Scale);
                    foreach (ImageViewerLayerMark Mark in Layer.Marks.Where(i => i.Visible))
                        RenderMark(Mark, Context, Viewport, Scale);
                }
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
            if (HasImage)
            {
                if (HasViewer)
                {
                    double HLw = Lw / 2d,
                           HLh = Lh / 2d;
                    Ix = (Lx - HLw) / Scale + HLw - Region.X;
                    Iy = (Ly - HLh) / Scale + HLh - Region.Y;
                }
                else
                {
                    Ix = (Lx - Region.X) / Scale;
                    Iy = (Ly - Region.Y) / Scale;
                }
            }

            else if (HasImageContext)
            {
                if (HasViewer)
                {
                    Rect Region = RenderBlocks[LBT * IBc + LBL].Region;
                    Ix = (LBL * RenderBlockSize + Lx - Region.X) / Scale;
                    Iy = (LBT * RenderBlockSize + Ly - Region.Y) / Scale;
                }
                else
                {
                    Rect Region = RenderBlocks[0].Region;
                    Ix = (Lx - Region.X) / Scale;
                    Iy = (Ly - Region.Y) / Scale;
                }
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
            if (HasImage)
            {
                if (HasViewer)
                {
                    double HLw = Lw / 2d,
                           HLh = Lh / 2d;

                    Lx = (Ix - HLw + Region.X) * Scale + HLw;
                    Ly = (Iy - HLh + Region.Y) * Scale + HLh;
                }
                else
                {
                    Lx = Ix * Scale + Region.X;
                    Ly = Iy * Scale + Region.Y;
                }
            }

            else if (HasImageContext)
            {
                if (HasViewer)
                {
                    Rect Region = RenderBlocks[LBT * IBc + LBL].Region;
                    Lx = Ix * Scale - LBL * RenderBlockSize + Region.X;
                    Ly = Iy * Scale - LBT * RenderBlockSize + Region.Y;
                }
                else
                {
                    Rect Region = RenderBlocks[0].Region;
                    Lx = Ix * Scale + Region.X;
                    Ly = Iy * Scale + Region.Y;
                }
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

        private sealed class ImageViewerLayerRenderBlock
        {
            public WriteableBitmap Canvas;

            public BitmapSource Bitmap;

            public Rect Region;

            public bool IsValid
                => !IsRendering && Canvas != null;

            public bool IsRendering;

            public override string ToString()
                => $"IsValid : {IsValid}";

        }

    }
}