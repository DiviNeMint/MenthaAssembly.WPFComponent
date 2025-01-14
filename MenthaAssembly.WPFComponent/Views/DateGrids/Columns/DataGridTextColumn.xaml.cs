using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    public class DataGridTextColumn : DataGridBoundColumn
    {
        internal static ComponentResourceKey DefaultElementStyleKey { get; } = new ComponentResourceKey(typeof(DataGridTextColumn), nameof(DefaultElementStyle));
        internal static ComponentResourceKey DefaultEditingElementStyleKey { get; } = new ComponentResourceKey(typeof(DataGridTextColumn), nameof(DefaultEditingElementStyle));

        public static Style DefaultElementStyle
            => (Application.Current.TryFindResource(DefaultElementStyleKey) as Style) ?? System.Windows.Controls.DataGridTextColumn.DefaultElementStyle;

        public static Style DefaultEditingElementStyle
            => (Application.Current.TryFindResource(DefaultEditingElementStyleKey) as Style) ?? System.Windows.Controls.DataGridTextColumn.DefaultEditingElementStyle;

        protected internal override bool AllowEditingMode
            => true;

        /// <summary>
        /// The DependencyProperty for the FontFamily property.
        /// </summary>
        public static readonly DependencyProperty FontFamilyProperty =
            TextElement.FontFamilyProperty.AddOwner(typeof(DataGridTextColumn),
                new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, FrameworkPropertyMetadataOptions.Inherits, NotifyPropertyChangeForRefreshContent));
        /// <summary>
        /// The font family of the desired font.
        /// This will only affect controls whose template uses the property as a parameter. On other controls, the property will do nothing.
        /// </summary>
        public FontFamily FontFamily
        {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        /// <summary>
        /// The DependencyProperty for the FontSize property.
        /// </summary>
        public static readonly DependencyProperty FontSizeProperty =
            TextElement.FontSizeProperty.AddOwner(typeof(DataGridTextColumn),
                new FrameworkPropertyMetadata(SystemFonts.MessageFontSize, FrameworkPropertyMetadataOptions.Inherits, NotifyPropertyChangeForRefreshContent));
        /// <summary>
        /// The size of the desired font.
        /// This will only affect controls whose template uses the property as a parameter. On other controls, the property will do nothing.
        /// </summary>
        [TypeConverter(typeof(FontSizeConverter))]
        [Localizability(LocalizationCategory.None)]
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        /// <summary>
        /// The DependencyProperty for the FontStyle property.
        /// </summary>
        public static readonly DependencyProperty FontStyleProperty =
            TextElement.FontStyleProperty.AddOwner(typeof(DataGridTextColumn),
                new FrameworkPropertyMetadata(SystemFonts.MessageFontStyle, FrameworkPropertyMetadataOptions.Inherits, NotifyPropertyChangeForRefreshContent));
        /// <summary>
        /// The style of the desired font.
        /// This will only affect controls whose template uses the property as a parameter. On other controls, the property will do nothing.
        /// </summary>
        public FontStyle FontStyle
        {
            get => (FontStyle)GetValue(FontStyleProperty);
            set => SetValue(FontStyleProperty, value);
        }

        /// <summary>
        /// The DependencyProperty for the FontWeight property.
        /// </summary>
        public static readonly DependencyProperty FontWeightProperty =
            TextElement.FontWeightProperty.AddOwner(typeof(DataGridTextColumn),
                new FrameworkPropertyMetadata(SystemFonts.MessageFontWeight, FrameworkPropertyMetadataOptions.Inherits, NotifyPropertyChangeForRefreshContent));
        /// <summary>
        /// The weight or thickness of the desired font.
        /// This will only affect controls whose template uses the property as a parameter. On other controls, the property will do nothing.
        /// </summary>
        public FontWeight FontWeight
        {
            get => (FontWeight)GetValue(FontWeightProperty);
            set => SetValue(FontWeightProperty, value);
        }

        /// <summary>
        /// The DependencyProperty for the Foreground property.
        /// </summary>
        public static readonly DependencyProperty ForegroundProperty =
            TextElement.ForegroundProperty.AddOwner(typeof(DataGridTextColumn),
                new FrameworkPropertyMetadata(SystemColors.ControlTextBrush, FrameworkPropertyMetadataOptions.Inherits, NotifyPropertyChangeForRefreshContent));
        /// <summary>
        /// An brush that describes the foreground color.
        /// This will only affect controls whose template uses the property as a parameter. On other controls, the property will do nothing.
        /// </summary>
        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        protected override FrameworkElement GenerateElement(DataGridCell Cell, object DataItem)
        {
            TextBlock Element = new();
            ApplyColumnProperties(false, Cell, Element);
            return Element;
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell Cell, object DataItem)
        {
            TextBox Element = new();
            ApplyColumnProperties(true, Cell, Element);
            return Element;
        }

        private void ApplyColumnProperties(bool IsEditing, DataGridCell Cell, FrameworkElement Element)
        {
            // Text
            Element.ApplyBinding(Binding, IsEditing ? TextBox.TextProperty : TextBlock.TextProperty);

            // Style
            Element.Style = IsEditing ? EditingElementStyle ?? DefaultEditingElementStyle :
                                        ElementStyle ?? DefaultElementStyle;

            // Font series
            this.SyncColumnProperty(Element, TextElement.FontFamilyProperty, FontFamilyProperty);
            this.SyncColumnProperty(Element, TextElement.FontSizeProperty, FontSizeProperty);
            this.SyncColumnProperty(Element, TextElement.FontStyleProperty, FontStyleProperty);
            this.SyncColumnProperty(Element, TextElement.FontWeightProperty, FontWeightProperty);
            this.SyncColumnProperty(Element, TextElement.ForegroundProperty, ForegroundProperty);

            // FlowDirection
            RestoreFlowDirection(Element, Cell);
        }

        protected override void OnInput(CellInputEventArgs e)
        {
            if (DataGridHelper.IsDataGridTextBoxBeginEdit(e))
                e.BeginEdit = true;
        }

    }
}