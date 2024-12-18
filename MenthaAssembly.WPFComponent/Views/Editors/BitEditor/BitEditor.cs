﻿using MenthaAssembly.Views.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MenthaAssembly.Views
{
    public class BitEditor : FrameworkElement
    {
        private static readonly FrameworkPropertyMetadataOptions RenderMetadataOption = FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender;
        private static readonly FrameworkPropertyMetadataOptions MeasureMetadataOption = FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender;

        public static readonly RoutedEvent BitChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(BitChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler<BitChangedEventArgs>), typeof(BitEditor));
        public event RoutedEventHandler<BitChangedEventArgs> BitChanged
        {
            add => AddHandler(BitChangedEvent, value);
            remove => RemoveHandler(BitChangedEvent, value);
        }

        public static readonly DependencyProperty SeparatorProperty =
            DependencyProperty.Register(nameof(Separator), typeof(Brush), typeof(BitEditor),
                new FrameworkPropertyMetadata(Brushes.White, RenderMetadataOption,
                    (d, e) =>
                    {
                        if (d is BitEditor This)
                            This.InvalidateSeparatorPen();
                    }));
        public Brush Separator
        {
            get => (Brush)GetValue(SeparatorProperty);
            set => SetValue(SeparatorProperty, value);
        }

        public static readonly DependencyProperty StrokeProperty =
            Shape.StrokeProperty.AddOwner(typeof(BitEditor),
                new FrameworkPropertyMetadata(Brushes.DimGray, RenderMetadataOption,
                    (d, e) =>
                    {
                        if (d is BitEditor This)
                            This.InvalidateStrokePen();
                    }));
        public Brush Stroke
        {
            get => (Brush)GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        public static readonly DependencyProperty StrokeThicknessProperty =
            Shape.StrokeThicknessProperty.AddOwner(typeof(BitEditor),
                new FrameworkPropertyMetadata(1d, MeasureMetadataOption,
                    (d, e) =>
                    {
                        if (d is BitEditor This)
                            This.InvalidatePen();
                    }));
        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public static readonly DependencyProperty FontFamilyProperty =
            TextElement.FontFamilyProperty.AddOwner(typeof(BitEditor));
        public FontFamily FontFamily
        {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }
        public static readonly DependencyProperty FontStyleProperty =
            TextElement.FontStyleProperty.AddOwner(typeof(BitEditor));
        public FontStyle FontStyle
        {
            get => (FontStyle)GetValue(FontStyleProperty);
            set => SetValue(FontStyleProperty, value);
        }

        public static readonly DependencyProperty FontWeightProperty =
            TextElement.FontWeightProperty.AddOwner(typeof(BitEditor));
        public FontWeight FontWeight
        {
            get => (FontWeight)GetValue(FontWeightProperty);
            set => SetValue(FontWeightProperty, value);
        }

        public static readonly DependencyProperty FontStretchProperty =
            TextElement.FontStretchProperty.AddOwner(typeof(BitEditor));
        public FontStretch FontStretch
        {
            get => (FontStretch)GetValue(FontStretchProperty);
            set => SetValue(FontStretchProperty, value);
        }

        public static readonly DependencyProperty FontSizeProperty =
            TextElement.FontSizeProperty.AddOwner(typeof(BitEditor));
        [TypeConverter(typeof(FontSizeConverter))]
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static readonly DependencyProperty BitStyleProperty =
            DependencyProperty.Register(nameof(BitStyle), typeof(Style), typeof(BitEditor), new FrameworkPropertyMetadata(null, MeasureMetadataOption));
        public Style BitStyle
        {
            get => (Style)GetValue(BitStyleProperty);
            set => SetValue(BitStyleProperty, value);
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(IBitEditorSource), typeof(BitEditor),
                new FrameworkPropertyMetadata(null, MeasureMetadataOption,
                    (d, e) =>
                    {
                        if (d is BitEditor This)
                            This.OnSourceChanged(e.ToChangedEventArgs<IBitEditorSource>());
                    }));
        public IBitEditorSource Source
        {
            get => (IBitEditorSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        protected override int VisualChildrenCount
            => Children.Count;

        private readonly UIElementCollection Children;
        public BitEditor()
        {
            Children = new(this, this);
            for (int i = 0; i < 8; i++)
                Children.Add(PrepareBitBlock(i));
        }

        private readonly BitGridRow Owner;
        internal BitEditor(BitGridRow Owner) : this()
        {
            this.Owner = Owner;
        }

        private void OnSourceChanged(ChangedEventArgs<IBitEditorSource> e)
        {
            if (e.NewValue is IBitEditorSource New)
            {
                int Count = Children.Count,
                    NewCount = New.Count;
                if (Count != NewCount)
                {
                    if (NewCount > Count)
                    {
                        for (int i = Count; i < NewCount; i++)
                            Children.Add(PrepareBitBlock(i));
                    }
                    else
                    {
                        for (int i = NewCount; i < Count; i++)
                        {
                            BitBlock Block = (BitBlock)Children[NewCount];
                            Children.RemoveAt(NewCount);
                            ClearBitBlock(Block);
                        }
                    }

                    InvalidateMeasure();
                }
            }
        }

        protected override Visual GetVisualChild(int Index)
            => Children[Index];

        private BitBlock PrepareBitBlock(int Index)
        {
            BitBlock Block = new(Index);

            // DataContext
            Block.SetBinding(DataContextProperty, new Binding(nameof(Source)) { Source = this });

            // IsSet
            Block.SetBinding(BitBlock.IsSetProperty, new Binding($"[{Index}]"));

            // Font
            Block.SetBinding(TextElement.FontFamilyProperty, new Binding(nameof(FontFamily)) { Source = this });
            Block.SetBinding(TextElement.FontStyleProperty, new Binding(nameof(FontStyle)) { Source = this });
            Block.SetBinding(TextElement.FontWeightProperty, new Binding(nameof(FontWeight)) { Source = this });
            Block.SetBinding(TextElement.FontStretchProperty, new Binding(nameof(FontStretch)) { Source = this });
            Block.SetBinding(TextElement.FontSizeProperty, new Binding(nameof(FontSize)) { Source = this });

            // Style
            Block.SetBinding(StyleProperty, new Binding(nameof(BitStyle)) { Source = this });

            // Event
            Block.Click += OnBitBlockClick;
            Block.IsMouseDirectlyOverChanged += OnBitBlockIsMouseDirectlyOverChanged;

            return Block;
        }
        private void ClearBitBlock(BitBlock Block)
        {
            // Binding
            BindingOperations.ClearAllBindings(Block);

            // Event
            Block.Click -= OnBitBlockClick;
            Block.IsMouseDirectlyOverChanged -= OnBitBlockIsMouseDirectlyOverChanged;
        }

        private void OnBitBlockClick(object sender, RoutedEventArgs e)
        {
            if (sender is BitBlock Block)
            {
                int Index = Block.Index;
                bool IsChecked = Block.IsSet;
                BitChangedEventArgs Arg = new(Index, IsChecked, BitChangedEvent, Block);
                RaiseEvent(Arg);

                if (Arg.Handled)
                    e.Handled = true;
            }
        }
        private void OnBitBlockIsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Owner?.Owner?.Owner is BitGrid Grid &&
                sender is BitBlock Block)
                Grid.SetMouseOverAdorner(Block, e.NewValue is true);
        }

        private readonly Dictionary<int, Rect> Locations = [];
        private Size RenderBlocksSize;
        protected override Size MeasureOverride(Size AvailableSize)
        {
            double Bw = 0d,
                   Bh = 0d;

            DpiScale Dpi = VisualTreeHelper.GetDpi(this);
            double Thickness = StrokeThickness,
                   Tx = Thickness / Dpi.DpiScaleX,
                   Ty = Thickness / Dpi.DpiScaleY,
                   Nx = Tx;

            int Count = Children.Count;
            for (int i = 0; i < Count; i++)
            {
                if (Children[i] is BitBlock Block)
                {
                    // Size
                    if (i == 0)
                    {
                        Block.Measure(AvailableSize);
                        Size BlockSize = Block.DesiredSize;
                        Bw = BlockSize.Width;
                        Bh = BlockSize.Height;
                    }

                    // Locations
                    Locations[i] = new(Nx, Ty, Bw, Bh);

                    // Next X
                    Nx += Bw + Tx;
                }
            }

            RenderBlocksSize = new Size(Nx, Bh + Ty * 2d);
            return RenderBlocksSize;
        }

        protected override Size ArrangeOverride(Size FinalSize)
        {
            int Count = Children.Count;
            for (int i = 0; i < Count; i++)
                Children[i].Arrange(Locations[i]);

            return base.ArrangeOverride(FinalSize);
        }

        protected override void OnRender(DrawingContext Context)
        {
            int Count = Children.Count;
            if (Count <= 0)
                return;

            Size Size = RenderBlocksSize;
            double Thickness = StrokeThickness,
                   HalfThickness = Thickness * 0.5d,
                   Rw = Size.Width,
                   Rh = Size.Height;

            // Border
            Context.DrawRectangle(Stroke, GetStrokePen(), new Rect(HalfThickness, HalfThickness, Rw - Thickness, Rh - Thickness));

            // Separator
            Point Start = new(),
                  End = new(Start.X, Rh);

            if (GetPrevRowSource() is IBitEditorSource PrevSource && PrevSource.Count > 0)
            {
                Point RightTop = new();

                bool LastBit = Children[0] is BitBlock FirstBlock && FirstBlock.IsSet;
                if (LastBit && PrevSource[0])
                {
                    RightTop.X = Count < 2 ? Rw : (Locations[0].Right + Locations[1].Left) / 2d;
                    Context.DrawLine(GetSeparatorPen(), Start, RightTop);
                }

                for (int i = 1; i < Count; i++)
                {
                    bool Bit = Children[i] is BitBlock Block && Block.IsSet;
                    if (Bit)
                    {
                        bool SetStart = false;
                        if (LastBit)
                        {
                            Start.X = (Locations[i - 1].Right + Locations[i].Left) / 2d;
                            End.X = Start.X;
                            Context.DrawLine(GetSeparatorPen(), Start, End);

                            SetStart = true;
                        }

                        if (i < PrevSource.Count && PrevSource[i])
                        {
                            // Checks if Start has been updated 
                            if (!SetStart)
                                Start.X = (Locations[i - 1].Right + Locations[i].Left) / 2d;

                            int j = i + 1;
                            RightTop.X = Count <= j ? Rw : (Locations[i].Right + Locations[j].Left) / 2d;
                            Context.DrawLine(GetSeparatorPen(), Start, RightTop);
                        }
                    }

                    LastBit = Bit;
                }
            }
            else
            {
                bool LastBit = Children[0] is BitBlock FirstBlock && FirstBlock.IsSet;
                for (int i = 1; i < Count; i++)
                {
                    bool Bit = Children[i] is BitBlock Block && Block.IsSet;
                    if (LastBit && Bit)
                    {
                        Start.X = (Locations[i - 1].Right + Locations[i].Left) / 2d;
                        End.X = Start.X;
                        Context.DrawLine(GetSeparatorPen(), Start, End);
                    }

                    LastBit = Bit;
                }
            }
        }

        private Pen StrokePen;
        protected virtual Pen GetStrokePen()
        {
            if (StrokePen != null)
                return StrokePen;

            double Thickness = StrokeThickness;
            if (double.IsInfinity(Thickness) || Thickness is 0d || double.IsNaN(Thickness))
                return null;

            Brush Brush = Stroke;
            if (Brush is null || Brush.Equals(Brushes.Transparent))
                return null;

            // This pen is internal to the system and
            // must not participate in freezable treeness
            StrokePen = new Pen
            {
                Thickness = Math.Abs(Thickness),
                Brush = Brush,
            };

            StrokePen.Freeze();
            return StrokePen;
        }

        private Pen SeparatorPen;
        protected virtual Pen GetSeparatorPen()
        {
            if (SeparatorPen != null)
                return SeparatorPen;

            double Thickness = StrokeThickness;
            if (double.IsInfinity(Thickness) || Thickness is 0d || double.IsNaN(Thickness))
                return null;

            Brush Brush = Separator;
            if (Brush is null || Brush.Equals(Brushes.Transparent))
                return null;

            // This pen is internal to the system and
            // must not participate in freezable treeness
            SeparatorPen = new Pen
            {
                Thickness = Math.Abs(Thickness),
                Brush = Brush,
            };

            SeparatorPen.Freeze();
            return SeparatorPen;
        }

        public void InvalidatePen()
        {
            InvalidateStrokePen();
            InvalidateSeparatorPen();
        }

        private void InvalidateStrokePen()
            => StrokePen = null;
        private void InvalidateSeparatorPen()
            => SeparatorPen = null;

        private IBitEditorSource GetPrevRowSource()
        {
            if (Owner is not BitGridRow Row ||
                Row.Owner is not BitGridPresenter GridPresenter)
                return null;

            ItemContainerGenerator RowGenerator = GridPresenter.ItemContainerGenerator;
            int RowIndex = RowGenerator.IndexFromContainer(Owner);
            if (RowIndex-- <= 0)
                return null;

            if (RowGenerator.Items[RowIndex] is not IBitRowSource PrevRowSource)
                return null;

            int EditorIndex = Row.ItemContainerGenerator.IndexFromContainer(this);
            return PrevRowSource.Skip(EditorIndex).FirstOrDefault();
        }
        internal void InvalidateNextRowVisual()
        {
            if (Owner is not BitGridRow Row ||
                Row.Owner is not BitGridPresenter GridPresenter)
                return;

            ItemContainerGenerator RowGenerator = GridPresenter.ItemContainerGenerator;
            int RowIndex = RowGenerator.IndexFromContainer(Owner) + 1;
            if (RowGenerator.ContainerFromIndex(RowIndex) is BitGridRow NextRow)
            {
                int EditorIndex = Row.ItemContainerGenerator.IndexFromContainer(this);
                if (EditorIndex > -1 && NextRow.ItemContainerGenerator.ContainerFromIndex(EditorIndex) is BitEditor NextRowRditor)
                    NextRowRditor.InvalidateVisual();
            }
        }

    }
}