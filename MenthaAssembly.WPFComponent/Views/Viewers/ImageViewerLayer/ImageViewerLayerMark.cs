using MenthaAssembly.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    [ContentProperty(nameof(Locations))]
    public class ImageViewerLayerMark : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler VisualChanged;

        private bool _Visible = true;
        public bool Visible
        {
            get => _Visible;
            set
            {
                _Visible = value;
                OnVisualChanged();
                OnPropertyChanged();
            }
        }

        private bool _Zoomable = true;
        public bool Zoomable
        {
            get => _Zoomable;
            set
            {
                _Zoomable = value;
                OnVisualChanged();
                OnPropertyChanged();
            }
        }

        private double _ZoomMinScale = 0d;
        public double ZoomMinScale
        {
            get => _ZoomMinScale;
            set
            {
                _ZoomMinScale = Math.Max(value, 0d);
                OnVisualChanged();
                OnPropertyChanged();
            }
        }

        private double _ZoomMaxScale = double.MaxValue;
        public double ZoomMaxScale
        {
            get => _ZoomMaxScale;
            set
            {
                _ZoomMaxScale = value;
                OnVisualChanged();
                OnPropertyChanged();
            }
        }

        public ObservableRangeCollection<Point> Locations { get; }

        private object _Content;
        public object Content
        {
            get => _Content;
            set
            {
                _Content = value;
                InvalidateVisual();
                OnVisualChanged();
                OnPropertyChanged();
            }
        }

        public ImageViewerLayerMark()
        {
            Locations = new();
            Locations.CollectionChanged += OnLocationCollectionChanged;
        }
        public ImageViewerLayerMark(ImageSource Image) : this()
        {
            _Content = Image;
        }
        public ImageViewerLayerMark(IImageContext Image) : this()
        {
            _Content = Image;
        }
        public ImageViewerLayerMark(FrameworkElement Element) : this()
        {
            _Content = Element;
        }

        public virtual void InvalidateVisual()
        {
            CacheVisual = null;
            CacheBrush = null;
        }

        private ImageSource CacheVisual;
        public virtual ImageSource GetVisual()
        {
            if (CacheVisual != null)
                return CacheVisual;

            if (_Content is ImageSource Image)
                CacheVisual = Image;

            else if (_Content is IImageContext Context)
                CacheVisual = Context.ToBitmapSource();

            else if (_Content is FrameworkElement Element)
                CacheVisual = Element.ToBitmapSource();

            return CacheVisual;
        }

        private Brush CacheBrush;
        public virtual Brush GetBrush()
        {
            if (CacheBrush is null &&
                GetVisual() is ImageSource Image)
            {
                CacheBrush = new ImageBrush(Image)
                {
                    AlignmentX = AlignmentX.Left,
                    AlignmentY = AlignmentY.Top
                };
                CacheBrush.Freeze();
            }

            return CacheBrush;
        }

        protected virtual void OnLocationCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            => OnVisualChanged();

        protected void OnPropertyChanged([CallerMemberName] string PropertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));

        protected void OnVisualChanged()
            => VisualChanged?.Invoke(this, EventArgs.Empty);

    }
}