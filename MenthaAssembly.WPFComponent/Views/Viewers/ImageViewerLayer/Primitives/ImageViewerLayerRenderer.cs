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
        private const double RenderInterval = 20d;
        private readonly DispatcherTimer Timer;

        private readonly ConcurrentDictionary<BlockIndex, Action> BlockRenderActionTable = new();
        public bool HasViewer, HasImage, HasImageContext, HasMarks;
        public double Lw, Lh, Iw, Ih, Scale, Vx, Vy;   // LayerWidth, LayerHeight, ImageWidth, ImageHeight, LayerScale, ViewportX, ViewportY
        public int IBc, IBr, LBL, LBT, LBR, LBB;       // ImageBlockColumn, ImageBlockRow, LayerBlockLeftIndex, LayerBlockTopIndex, LayerBlockRightIndex, LayerBlockBottomIndex
        public WriteableBitmap Thumbnail;
        public double ThumbnailScale;

        private readonly ImageViewerLayer Layer;
        private readonly ImageViewerLayerPresenter LayerPresenter;
        public ImageViewerLayerRenderer(ImageViewerLayer Layer, ImageViewerLayerPresenter LayerPresenter)
        {
            Timer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(RenderInterval),
            };
            Timer.Tick += OnRenderTimerTick;

            this.Layer = Layer;
            this.LayerPresenter = LayerPresenter;

            Lw = Lh = Iw = Ih = Scale = Vx = Vy = double.NaN;
            IBc = IBr = LBL = LBT = LBR = LBB = -1;
        }

        private void OnRenderTimerTick(object sender, EventArgs e)
        {
            try
            {
                ICollection<BlockIndex> Keys = BlockRenderActionTable.Keys;
                while (Keys.Count > 0)
                {
                    foreach (BlockIndex Index in Keys)
                        if (BlockRenderActionTable.TryRemove(Index, out Action RenderAction))
                            RenderAction.Invoke();

                    Keys = BlockRenderActionTable.Keys;
                }
            }
            finally
            {
                Timer.Stop();
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
        private readonly Dictionary<BlockIndex, ImageViewerLayerRenderBlock> RenderBlocks = new();
        private unsafe void RefreshContext(IImageContext Image)
        {
            bool NewImage = false,
                 NewThumbnail = false;
            int ImageHash = Image.GetHashCode();
            double Iw = Image.Width,
                   Ih = Image.Height;
            if (this.ImageHash != ImageHash)
            {
                NewImage = true;
                NewThumbnail = true;
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
                NewThumbnail = true;
                ClipGeometry = new RectangleGeometry(new Rect(0d, 0d, Lw, Lh));
                this.Lw = Lw;
                this.Lh = Lh;
            }

            else if (this.Lw != Lw || this.Lh != Lh)
            {
                NewSize = true;
                NewThumbnail = true;
                ClipGeometry.Rect = new Rect(0d, 0d, Lw, Lh);
                this.Lw = Lw;
                this.Lh = Lh;
            }

            // Refresh Thumbnail
            if (NewThumbnail)
            {
                double TScale = Math.Max(Iw / Lw, Ih / Lh);
                if (TScale > 1d)
                {
                    do
                    {
                        TScale /= 1.5d;
                    } while (TScale > 1d);

                    TScale = TScale * Lw / Iw;
                }
                else
                {
                    TScale = 1d;
                }

                if (NewImage || TScale / ThumbnailScale > 1.5d)
                    RefreshThumbnail(Image, TScale);
            }

            if (NewImage || NewSize)
            {
                BlockIndex Index = new();
                bool IsBlockInvalid = false;
                double ScaleX = Lw / Iw,
                       ScaleY = Lh / Ih,
                       SIx, SIy, SIw, SIh;
                if (ScaleX < ScaleY)
                {
                    IsBlockInvalid = NewImage || !ScaleX.Equals(Scale);
                    Scale = ScaleX;
                    Ih *= ScaleX;

                    SIx = 0d;
                    SIy = Math.Round((Lh - Ih) / 2d);
                    SIw = Lw;
                    SIh = Ih;
                }
                else
                {
                    IsBlockInvalid = NewImage || !ScaleY.Equals(Scale);
                    Scale = ScaleY;
                    Iw *= ScaleY;

                    SIx = Math.Round((Lw - Iw) / 2d);
                    SIy = 0d;
                    SIw = Iw;
                    SIh = Lh;
                }

                int IBc = (int)Math.Ceiling(SIw / RenderBlockSize),
                    IBr = (int)Math.Ceiling(SIh / RenderBlockSize);

                if (IsBlockInvalid)
                {
                    // Invalidates all blocks.
                    ICollection<WriteableBitmap> CacheCanvas = this.CacheCanvas.Values;
                    if (CacheCanvas.Count > 0)
                    {
                        this.CacheCanvas.Clear();
                        foreach (WriteableBitmap Canvas in CacheCanvas)
                            InvalidateCanvas(Canvas);
                    }

                    if (RenderBlocks.Count > 0)
                        foreach (ImageViewerLayerRenderBlock Block in RenderBlocks.Values)
                            InvalidateCanvas(Block);

                    // Invalidates all RenderActions
                    BlockRenderActionTable.Clear();
                }

                // Checks that enough blocks.
                for (int i = this.IBc + 1; i <= IBc; i++)
                    for (int j = 0; j <= this.IBr; j++)
                        RenderBlocks.Add(new(i, j), new ImageViewerLayerRenderBlock());

                for (int j = this.IBr + 1; j <= IBr; j++)
                    for (int i = 0; i <= IBc; i++)
                        RenderBlocks.Add(new(i, j), new ImageViewerLayerRenderBlock());

                // Refresh BlockInfos
                LBL = LBT = 0;
                LBR = IBc - 1;
                LBB = IBr - 1;
                this.IBc = Math.Max(this.IBc, IBc);
                this.IBr = Math.Max(this.IBr, IBr);

                // Refresh Blocks
                int IntSIw = (int)SIw,
                    IntSIh = (int)SIh,
                    Sx, Sy = 0,
                    Bw, Bh;

                double Ix,
                       Iy = SIy,
                       ThumbnailFactor = ThumbnailScale / Scale;

                NearestResizePixelAdapter<BGRA> Adapter0 = new(Image, IntSIw, IntSIh);
                Index.Row = 0;
                for (int j = LBT; j <= LBB; j++, Index.Row++, Sy += RenderBlockSize, Iy += RenderBlockSize)
                {
                    Sx = 0;
                    Ix = SIx;
                    Bh = Sy + RenderBlockSize <= IntSIh ? RenderBlockSize : IntSIh - Sy;
                    if (Bh <= 0)
                        break;

                    Index.Column = 0;
                    for (int i = LBL; i <= LBR; i++, Index.Column++, Sx += RenderBlockSize, Ix += RenderBlockSize)
                    {
                        Bw = Sx + RenderBlockSize <= IntSIw ? RenderBlockSize : IntSIw - Sx;
                        if (Bw <= 0)
                            break;

                        ImageViewerLayerRenderBlock Block = RenderBlocks[Index];
                        Block.Region.X = Ix;
                        Block.Region.Y = Iy;
                        Block.Region.Width = Bw;
                        Block.Region.Height = Bh;

                        if (Block.Canvas is not WriteableBitmap Canvas)
                        {
                            Int32Rect ThumbnailRegion = new((int)(Sx * ThumbnailFactor),
                                                            (int)(Sy * ThumbnailFactor),
                                                            (int)(Bw * ThumbnailFactor),
                                                            (int)(Bh * ThumbnailFactor));
                            if (Block.Bitmap is not CroppedBitmap Thumbnail ||
                                !this.Thumbnail.Equals(Thumbnail.Source) ||
                                !Thumbnail.SourceRect.Equals(ThumbnailRegion))
                                Block.Bitmap = new CroppedBitmap(this.Thumbnail, ThumbnailRegion);

                            if (ThumbnailScale != 1d && !Block.IsRendering)
                            {
                                BlockIndex ActionIndex = Index;
                                int TSx = Sx,
                                    TSy = Sy,
                                    TBw = Bw,
                                    TBh = Bh;
                                void RenderAction()
                                {
                                    try
                                    {
                                        Block.IsRendering = true;
                                        if (!DequeueCacheCanvas(ActionIndex, out Canvas))
                                        {
                                            Canvas = GetCanvas();
                                            Canvas.Lock();

                                            byte* pDest0 = (byte*)Canvas.BackBuffer;
                                            long Stride = Canvas.BackBufferStride;

                                            PixelAdapter<BGRA> Adapter = Adapter0.Clone();
                                            for (int y = 0; y < TBh; y++)
                                            {
                                                Adapter.DangerousMove(TSx, TSy + y);

                                                BGRA* pDest = (BGRA*)(pDest0 + Stride * y);
                                                for (int x = 0; x < TBw; x++, Adapter.DangerousMoveNextX(), pDest++)
                                                    Adapter.OverrideTo(pDest);
                                            }

                                            Canvas.AddDirtyRect(DirtyRect);
                                            Canvas.Unlock();
                                        }

                                        Block.Canvas = Canvas;
                                        Block.Bitmap = TBw == RenderBlockSize && TBh == RenderBlockSize ? Canvas :
                                                                                                          new CroppedBitmap(Block.Canvas, new Int32Rect(0, 0, TBw, TBh));

                                        Layer.InvalidateVisual();
                                    }
                                    finally
                                    {
                                        Block.IsRendering = false;
                                    }
                                }

                                BlockRenderActionTable.AddOrUpdate(ActionIndex, RenderAction, (o, v) => RenderAction);
                                Timer.Start();
                            }
                        }
                    }
                }

                Layer.InvalidateVisual();
            }
        }
        private unsafe void RefreshContext(IImageContext Image, ImageViewer Viewer)
        {
            bool NewImage = false,
                 NewThumbnail = false;
            int ImageHash = Image.GetHashCode();
            double Iw = Image.Width,
                   Ih = Image.Height;
            if (this.ImageHash != ImageHash)
            {
                NewImage = true;
                NewThumbnail = true;
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
                ICollection<WriteableBitmap> CacheCanvas = this.CacheCanvas.Values;
                if (CacheCanvas.Count > 0)
                {
                    this.CacheCanvas.Clear();
                    foreach (WriteableBitmap Canvas in CacheCanvas)
                        InvalidateCanvas(Canvas);
                }

                if (RenderBlocks.Count > 0)
                    foreach (ImageViewerLayerRenderBlock Block in RenderBlocks.Values)
                        InvalidateCanvas(Block);

                // Invalidates all RenderActions
                BlockRenderActionTable.Clear();

                // Checks that enough blocks.
                for (int i = this.IBc + 1; i <= IBc; i++)
                    for (int j = 0; j <= this.IBr; j++)
                        RenderBlocks.Add(new(i, j), new ImageViewerLayerRenderBlock());

                for (int j = this.IBr + 1; j <= IBr; j++)
                    for (int i = 0; i <= IBc; i++)
                        RenderBlocks.Add(new(i, j), new ImageViewerLayerRenderBlock());

                this.IBc = Math.Max(this.IBc, IBc);
                this.IBr = Math.Max(this.IBr, IBr);
            }

            bool NewSize = false;
            double Lw = Layer.ActualWidth,
                   Lh = Layer.ActualHeight;
            if (ClipGeometry is null)
            {
                NewSize = true;
                NewThumbnail = true;
                ClipGeometry = new RectangleGeometry(new Rect(0d, 0d, Lw, Lh));
                this.Lw = Lw;
                this.Lh = Lh;
            }
            else if (this.Lw != Lw || this.Lh != Lh)
            {
                NewSize = true;
                NewThumbnail = true;
                ClipGeometry.Rect = new Rect(0d, 0d, Lw, Lh);
                this.Lw = Lw;
                this.Lh = Lh;
            }

            // Refresh Thumbnail
            if (NewThumbnail)
            {
                double TScale = Math.Max(Iw / Lw, Ih / Lh);
                if (TScale > 1d)
                {
                    do
                    {
                        TScale /= 1.5d;
                    } while (TScale > 1d);

                    TScale = TScale * Lw / Iw;
                }
                else
                {
                    TScale = 1d;
                }

                if (NewImage || TScale / ThumbnailScale > 1.5d)
                    RefreshThumbnail(Image, TScale);
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
                this.LBL = LBL;
                this.LBT = LBT;
                this.LBR = LBR;
                this.LBB = LBB;

                int ExL = LBL - 1,
                    ExR = LBR + 1,
                    ExT = LBT - 1,
                    ExB = LBB + 1;

                foreach (KeyValuePair<BlockIndex, ImageViewerLayerRenderBlock> BlockInfo in RenderBlocks.Where(i => i.Value.Canvas != null))
                {
                    BlockIndex Index = BlockInfo.Key;
                    if (ExL <= Index.Column && Index.Column <= ExR)
                    {
                        if (ExT == Index.Row || Index.Row == ExB)
                        {
                            // OnBorder
                            EnqueueCacheCanvas(BlockInfo.Value, Index);
                            continue;
                        }
                    }
                    else
                    {
                        // Outside
                        InvalidateCanvas(BlockInfo.Value);
                        continue;
                    }

                    if (ExT <= Index.Row && Index.Row <= ExB)
                    {
                        if (ExL == Index.Column || Index.Column == ExR)
                        {
                            // OnBorder
                            EnqueueCacheCanvas(BlockInfo.Value, Index);
                            continue;
                        }
                    }
                    else
                    {
                        // Outside
                        InvalidateCanvas(BlockInfo.Value);
                        continue;
                    }
                }
            }

            // Refresh Blocks
            if (NewImage || NewScale || NewRegion)
            {
                BlockIndex Index = new();
                int S0 = LBL * RenderBlockSize,
                    Sy = LBT * RenderBlockSize,
                    IntSIw = (int)SIw,
                    IntSIh = (int)SIh,
                    Sx, Bw, Bh;

                double I0 = Math.Round(S0 - Dx),
                       Ix,
                       Iy = Math.Round(Sy - Dy),
                       ThumbnailFactor = ThumbnailScale / Scale;

                int ThumbIw = Thumbnail.PixelWidth,
                    ThumbIh = Thumbnail.PixelHeight,
                    ThumbBSize = (int)(RenderBlockSize * ThumbnailFactor),
                    ThumbS0 = (int)(S0 * ThumbnailFactor),
                    ThumbSy = (int)(Sy * ThumbnailFactor),
                    ThumbSx, ThumbBw, ThumbBh;

                NearestResizePixelAdapter<BGRA> Adapter0 = new(Image, IntSIw, IntSIh);
                Index.Row = LBT;
                for (int j = LBT; j <= LBB; j++, Index.Row++, Sy += RenderBlockSize, Iy += RenderBlockSize, ThumbSy += ThumbBSize)
                {
                    Sx = S0;
                    ThumbSx = ThumbS0;
                    Ix = I0;
                    Bh = Sy + RenderBlockSize <= IntSIh ? RenderBlockSize : IntSIh - Sy;
                    ThumbBh = ThumbSy + ThumbBSize <= ThumbIh ? ThumbBSize : ThumbIh - ThumbSy;
                    if (Bh <= 0)
                        break;

                    Index.Column = LBL;
                    for (int i = LBL; i <= LBR; i++, Index.Column++, Sx += RenderBlockSize, Ix += RenderBlockSize, ThumbSx += ThumbBSize)
                    {
                        Bw = Sx + RenderBlockSize <= IntSIw ? RenderBlockSize : IntSIw - Sx;
                        ThumbBw = ThumbSx + ThumbBSize <= ThumbIw ? ThumbBSize : ThumbIw - ThumbSx;
                        if (Bw <= 0)
                            break;

                        ImageViewerLayerRenderBlock Block = RenderBlocks[Index];
                        Block.Region.X = Ix;
                        Block.Region.Y = Iy;
                        Block.Region.Width = Bw;
                        Block.Region.Height = Bh;

                        if (Block.Canvas is not WriteableBitmap Canvas)
                        {
                            Int32Rect ThumbnailRegion = new(ThumbSx, ThumbSy, ThumbBw, ThumbBh);
                            if (Block.Bitmap is not CroppedBitmap Thumbnail ||
                                !this.Thumbnail.Equals(Thumbnail.Source) ||
                                !Thumbnail.SourceRect.Equals(ThumbnailRegion))
                                Block.Bitmap = new CroppedBitmap(this.Thumbnail, ThumbnailRegion);

                            if (ThumbnailScale != 1d && !Block.IsRendering)
                            {
                                BlockIndex ActionIndex = Index;
                                int TSx = Sx,
                                    TSy = Sy,
                                    TBw = Bw,
                                    TBh = Bh;
                                void RenderAction()
                                {
                                    try
                                    {
                                        Block.IsRendering = true;
                                        if (!DequeueCacheCanvas(ActionIndex, out Canvas))
                                        {
                                            Canvas = GetCanvas();
                                            Canvas.Lock();

                                            byte* pDest0 = (byte*)Canvas.BackBuffer;
                                            long Stride = Canvas.BackBufferStride;

                                            PixelAdapter<BGRA> Adapter = Adapter0.Clone();
                                            for (int y = 0; y < TBh; y++)
                                            {
                                                Adapter.DangerousMove(TSx, TSy + y);

                                                BGRA* pDest = (BGRA*)(pDest0 + Stride * y);
                                                for (int x = 0; x < TBw; x++, Adapter.DangerousMoveNextX(), pDest++)
                                                    Adapter.OverrideTo(pDest);
                                            }

                                            Canvas.AddDirtyRect(DirtyRect);
                                            Canvas.Unlock();
                                        }

                                        Block.Canvas = Canvas;
                                        Block.Bitmap = TBw == RenderBlockSize && TBh == RenderBlockSize ? Canvas :
                                                                                                          new CroppedBitmap(Block.Canvas, new Int32Rect(0, 0, TBw, TBh));

                                        Layer.InvalidateVisual();
                                    }
                                    finally
                                    {
                                        Block.IsRendering = false;
                                    }
                                }

                                BlockRenderActionTable.AddOrUpdate(ActionIndex, RenderAction, (o, v) => RenderAction);
                                Timer.Start();
                            }
                        }
                    }
                }

                Layer.InvalidateVisual();
            }
        }

        private unsafe void RefreshThumbnail(IImageContext Image, double Scale)
        {
            ThumbnailScale = Scale;
            int Iw = (int)Math.Round(Image.Width * Scale),
                Ih = (int)Math.Round(Image.Height * Scale);

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

        private readonly ConcurrentDictionary<BlockIndex, WriteableBitmap> CacheCanvas = new();
        private void EnqueueCacheCanvas(ImageViewerLayerRenderBlock Block, BlockIndex Index)
        {
            if (!Block.IsRendering && Block.Canvas != null)
            {
                EnqueueCacheCanvas(Block.Canvas, Index);
                Block.Canvas = null;
            }
        }
        private void EnqueueCacheCanvas(WriteableBitmap Canvas, BlockIndex Index)
        {
            if (Index.Equals(BlockIndex.Invalid))
                InvalidCanvas.Enqueue(Canvas);
            else
                CacheCanvas.AddOrUpdate(Index, Canvas, (o, v) => Canvas);
        }
        private bool DequeueCacheCanvas(BlockIndex Index, out WriteableBitmap Canvas)
            => CacheCanvas.TryRemove(Index, out Canvas);

        private readonly ConcurrentQueue<WriteableBitmap> InvalidCanvas = new();
        private void InvalidateCanvas(ImageViewerLayerRenderBlock Block)
        {
            if (!Block.IsRendering && Block.Canvas != null)
            {
                InvalidateCanvas(Block.Canvas);
                Block.Canvas = null;
            }

            Block.Bitmap = null;
        }
        private void InvalidateCanvas(WriteableBitmap Canvas)
            => InvalidCanvas.Enqueue(Canvas);
        private WriteableBitmap GetCanvas()
            => InvalidCanvas.TryDequeue(out WriteableBitmap Invalid) ? Invalid :
                                                                       new WriteableBitmap(RenderBlockSize, RenderBlockSize, 96d, 96d, PixelFormats.Bgra32, null);

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
                BlockIndex Index = new(LBL, LBT);

                // Guild Line
                GuidelineSet GuideLines = new();
                for (int i = LBL; i <= LBR; i++, Index.Column++)
                {
                    Block = RenderBlocks[Index];
                    GuideLines.GuidelinesX.Add(Block.Region.Left);
                }
                GuideLines.GuidelinesX.Add(Block.Region.Right);

                for (int j = LBT; j <= LBB; j++, Index.Row++)
                {
                    Block = RenderBlocks[Index];
                    GuideLines.GuidelinesY.Add(Block.Region.Top);
                }
                GuideLines.GuidelinesY.Add(Block.Region.Bottom);

                Context.PushGuidelineSet(GuideLines);

                // Image
                Index.Row = LBT;
                for (int j = LBT; j <= LBB; j++, Index.Row++)
                {
                    Index.Column = LBL;
                    for (int i = LBL; i <= LBR; i++, Index.Column++)
                    {
                        Block = RenderBlocks[Index];
                        Context.DrawImage(Block.Bitmap, Block.Region);
                        //if (!Block.IsRendering && Block.Canvas != null)
                        //{
                        //}
                        //else
                        //{
                        //    Context.DrawRectangle(Brushes.Red, null, Block.Region);
                        //}
                    }
                }

                if (HasMarks)
                {
                    TranslatePoint(0d, 0d, out double Ix, out double Iy);
                    Rect Viewport = new(Ix, Iy, Lw / Scale, Lh / Scale);
                    foreach (ImageViewerLayerMark Mark in Layer.Marks.Where(i => i.Visible))
                        RenderMark(Mark, Context, Viewport, Scale);
                }

#if EnableBlockGrid

                // EnableBlockGrid
                Pen GridPen = new Pen(Brushes.Red, 1d);
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
                BlockIndex Index = new();
                if (HasViewer)
                {
                    Index.Row = LBT;
                    Index.Column = LBL;
                    Rect Region = RenderBlocks[Index].Region;
                    Ix = (LBL * RenderBlockSize + Lx - Region.X) / Scale;
                    Iy = (LBT * RenderBlockSize + Ly - Region.Y) / Scale;
                }
                else
                {
                    Rect Region = RenderBlocks[Index].Region;
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
                BlockIndex Index = new();
                if (HasViewer)
                {
                    Index.Row = LBT;
                    Index.Column = LBL;
                    Rect Region = RenderBlocks[Index].Region;
                    Lx = Ix * Scale - LBL * RenderBlockSize + Region.X;
                    Ly = Iy * Scale - LBT * RenderBlockSize + Region.Y;
                }
                else
                {
                    Rect Region = RenderBlocks[Index].Region;
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

            public bool IsRendering;

            public override string ToString()
                => $"Region : {Region}, IsRendering : {IsRendering}";

        }

        private struct BlockIndex
        {
            public static BlockIndex Invalid { get; } = new BlockIndex(int.MinValue, int.MinValue);

            public int Column { set; get; }

            public int Row { set; get; }

            public BlockIndex(int Column, int Row)
            {
                this.Column = Column;
                this.Row = Row;
            }

            public bool Inside(int Left, int Top, int Right, int Bottom)
                => Left <= Column && Column <= Right && Top <= Row && Row <= Bottom;

            public bool OnBorder(int Left, int Top, int Right, int Bottom)
                => Left <= Column && Column <= Right ? Top == Row || Row == Bottom :
                                                       Top <= Row && Row <= Bottom && (Left == Column || Column == Right);

            public override int GetHashCode()
            {
                int hashCode = 656739706;
                hashCode = hashCode * -1521134295 + Column.GetHashCode();
                hashCode = hashCode * -1521134295 + Row.GetHashCode();
                return hashCode;
            }

            public override bool Equals(object obj)
                => obj is BlockIndex Target && Column == Target.Column && Row == Target.Row;

            public override string ToString()
                => $"Column : {Column}, Row : {Row}";

        }

    }
}