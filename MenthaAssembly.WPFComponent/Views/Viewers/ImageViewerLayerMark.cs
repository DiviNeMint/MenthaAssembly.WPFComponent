using MenthaAssembly.Media.Imaging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    [ContentProperty(nameof(CenterLocations))]
    public class ImageViewerLayerMark : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _Zoomable = true;
        public bool Zoomable
        {
            get => _Zoomable;
            set
            {
                _Zoomable = value;
                this.OnPropertyChanged();
            }
        }

        private double _ZoomMinScale = 0d;
        public double ZoomMinScale
        {
            get => _ZoomMinScale;
            set
            {
                _ZoomMinScale = Math.Max(value, 0d);
                this.OnPropertyChanged();
            }
        }

        private double _ZoomMaxScale = double.MaxValue;
        public double ZoomMaxScale
        {
            get => _ZoomMaxScale;
            set
            {
                _ZoomMaxScale = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<Point> CenterLocations { get; } = new ObservableCollection<Point>();

        private object _Visual;
        public object Visual
        {
            get => _Visual;
            set
            {
                _Visual = value;
                InvalidateVisualContext();
                OnPropertyChanged();
            }
        }

        public virtual void InvalidateVisualContext()
            => CacheVisualContext = null;

        private IImageContext CacheVisualContext;
        protected internal virtual IImageContext CreateVisualContext()
        {
            if (CacheVisualContext != null)
                return CacheVisualContext;

            if (_Visual is IImageContext Context)
                CacheVisualContext = Context;

            else if (_Visual is ImageSource Image)
                CacheVisualContext = Image.ToImageContext();

            else if (_Visual is FrameworkElement Element)
                CacheVisualContext = Element.ToBitmapSource()
                                            .ToImageContext();

            return CacheVisualContext;
        }

        protected void OnPropertyChanged([CallerMemberName] string PropertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));

    }
}
