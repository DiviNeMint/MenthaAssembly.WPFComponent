using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace MenthaAssembly.Views
{
    [StyleTypedProperty(Property = "MenuStyleStyle", StyleTargetType = typeof(PopupMenuItem))]
    public class PopupMenu : ItemsControl
    {
        public static readonly RoutedEvent OpenedEvent =
            EventManager.RegisterRoutedEvent("Opened", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PropertyEditor));
        public event RoutedEventHandler Opened
        {
            add { AddHandler(OpenedEvent, value); }
            remove { RemoveHandler(OpenedEvent, value); }
        }

        public static readonly RoutedEvent ClosedEvent =
            EventManager.RegisterRoutedEvent("Closed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PropertyEditor));
        public event RoutedEventHandler Closed
        {
            add { AddHandler(ClosedEvent, value); }
            remove { RemoveHandler(ClosedEvent, value); }
        }

        public static readonly DependencyProperty AllowsTransparencyProperty =
            Popup.AllowsTransparencyProperty.AddOwner(typeof(PopupMenu), new PropertyMetadata(true));
        public bool AllowsTransparency
        {
            get => (bool)GetValue(AllowsTransparencyProperty);
            set => SetValue(AllowsTransparencyProperty, value);
        }

        public static readonly DependencyProperty IsOpenProperty =
            Popup.IsOpenProperty.AddOwner(typeof(PopupMenu), new PropertyMetadata(false));
        [Category("Appearance")]
        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public static readonly DependencyProperty StaysOpenProperty =
            Popup.StaysOpenProperty.AddOwner(typeof(PopupMenu), new PropertyMetadata(false));
        [Category("Behavior")]
        public bool StaysOpen
        {
            get => (bool)GetValue(StaysOpenProperty);
            set => SetValue(StaysOpenProperty, value);
        }

        public static readonly DependencyProperty PopupAnimationProperty =
            Popup.PopupAnimationProperty.AddOwner(typeof(PopupMenu), new PropertyMetadata(PopupAnimation.Fade));
        [Category("Appearance")]
        public PopupAnimation PopupAnimation
        {
            get => (PopupAnimation)GetValue(PopupAnimationProperty);
            set => SetValue(PopupAnimationProperty, value);
        }

        public static readonly DependencyProperty PlacementTargetProperty =
            Popup.PlacementTargetProperty.AddOwner(typeof(PopupMenu));
        [Category("Layout")]
        public UIElement PlacementTarget
        {
            get => (UIElement)GetValue(PlacementTargetProperty);
            set => SetValue(PlacementTargetProperty, value);
        }

        public static readonly DependencyProperty VerticalOffsetProperty =
            Popup.VerticalOffsetProperty.AddOwner(typeof(PopupMenu));
        [Category("Layout")]
        [TypeConverter(typeof(LengthConverter))]
        public double VerticalOffset
        {
            get => (double)GetValue(VerticalOffsetProperty);
            set => SetValue(VerticalOffsetProperty, value);
        }

        public static readonly DependencyProperty HorizontalOffsetProperty =
            Popup.HorizontalOffsetProperty.AddOwner(typeof(PopupMenu));
        [Category("Layout")]
        [TypeConverter(typeof(LengthConverter))]
        public double HorizontalOffset
        {
            get => (double)GetValue(HorizontalOffsetProperty);
            set => SetValue(HorizontalOffsetProperty, value);
        }

        public static readonly DependencyProperty PlacementProperty =
            Popup.PlacementProperty.AddOwner(typeof(PopupMenu));
        [Category("Layout")]
        public PlacementMode Placement
        {
            get => (PlacementMode)GetValue(PlacementProperty);
            set => SetValue(PlacementProperty, value);
        }

        public static readonly DependencyProperty PlacementRectangleProperty =
            Popup.PlacementRectangleProperty.AddOwner(typeof(PopupMenu));
        public Rect PlacementRectangle
        {
            get => (Rect)GetValue(PlacementRectangleProperty);
            set => SetValue(PlacementRectangleProperty, value);
        }

        public static readonly DependencyProperty CustomPopupPlacementCallbackProperty =
            Popup.CustomPopupPlacementCallbackProperty.AddOwner(typeof(PopupMenu));
        [Category("Layout")]
        public CustomPopupPlacementCallback CustomPopupPlacementCallback
        {
            get => (CustomPopupPlacementCallback)GetValue(CustomPopupPlacementCallbackProperty);
            set => SetValue(CustomPopupPlacementCallbackProperty, value);
        }

        static PopupMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PopupMenu), new FrameworkPropertyMetadata(typeof(PopupMenu)));
        }

        protected Popup PART_Popup;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.Parent is Panel Panel)
            {
                PART_Popup = new Popup();
                PART_Popup.SetBinding(AllowsTransparencyProperty, new Binding(nameof(AllowsTransparency)) { Source = this });
                PART_Popup.SetBinding(PopupAnimationProperty, new Binding(nameof(PopupAnimation)) { Source = this });
                PART_Popup.SetBinding(StaysOpenProperty, new Binding(nameof(StaysOpen)) { Source = this });

                PART_Popup.Opened += (s, e) => OnOpened(e);
                PART_Popup.Closed += (s, e) =>
                {
                    if (IsOpen)
                        IsOpen = false;

                    OnClosed(e);
                };

                Panel.Children.Remove(this);
                Popup.CreateRootPopup(PART_Popup, this);
                Panel.Children.Add(PART_Popup);
            }
        }

        protected virtual void OnClosed(EventArgs e)
            => RaiseEvent(new RoutedEventArgs(ClosedEvent, this));

        protected virtual void OnOpened(EventArgs e)
        {
            if (this.ActualHeight is 0d ||
                this.ActualWidth is 0d)
                this.InvalidateMeasure();

            RaiseEvent(new RoutedEventArgs(OpenedEvent, this));
        }

        protected override bool IsItemItsOwnContainerOverride(object Item)
            => Item is PopupMenuItem || Item is Separator;

        protected override DependencyObject GetContainerForItemOverride()
            => new PopupMenuItem();

    }
}
