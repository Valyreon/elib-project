using MahApps.Metro.IconPacks;
using System.Windows;
using System.Windows.Controls;

namespace ElibWpf.CustomComponents
{
    public class TextBoxWithSymbol : TextBox
    {
        public static DependencyProperty IconNameProperty;
        public static DependencyProperty IconSizeProperty;
        public static DependencyProperty IconMarginProperty;
        public static DependencyProperty TextboxPaddingProperty;
        public static DependencyProperty WatermarkTextProperty;
        public static DependencyProperty EnterCommandProperty;

        static TextBoxWithSymbol()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBoxWithSymbol), new FrameworkPropertyMetadata(typeof(TextBoxWithSymbol)));
            IconNameProperty = DependencyProperty.Register("IconName", typeof(PackIconFontAwesomeKind), typeof(TextBoxWithSymbol));
            IconSizeProperty = DependencyProperty.Register("IconSize", typeof(double), typeof(TextBoxWithSymbol));
            WatermarkTextProperty = DependencyProperty.Register("WatermarkText", typeof(string), typeof(TextBoxWithSymbol));
            IconMarginProperty = DependencyProperty.Register("IconMargin", typeof(System.Windows.Thickness), typeof(TextBoxWithSymbol));
            TextboxPaddingProperty = DependencyProperty.Register("TextboxPadding", typeof(System.Windows.Thickness), typeof(TextBoxWithSymbol));
            EnterCommandProperty = DependencyProperty.Register("EnterCommand", typeof(System.Windows.Input.ICommand), typeof(TextBoxWithSymbol));
        }

        public PackIconFontAwesomeKind IconName
        {
            get { return (PackIconFontAwesomeKind)base.GetValue(IconNameProperty); }
            set { base.SetValue(IconNameProperty, value); }
        }

        public double IconSize
        {
            get { return (double)base.GetValue(IconSizeProperty); }
            set { base.SetValue(IconSizeProperty, value); }
        }

        public System.Windows.Thickness IconMargin
        {
            get => (System.Windows.Thickness)base.GetValue(IconMarginProperty);
            set => base.SetValue(IconMarginProperty, value);
        }

        public System.Windows.Thickness TextboxPadding
        {
            get => (System.Windows.Thickness)base.GetValue(TextboxPaddingProperty);
            set => base.SetValue(TextboxPaddingProperty, value);
        }

        public string WatermarkText
        {
            get { return (string)base.GetValue(WatermarkTextProperty); }
            set { base.SetValue(WatermarkTextProperty, value); }
        }

        public System.Windows.Input.ICommand EnterCommand
        {
            get => (System.Windows.Input.ICommand)base.GetValue(EnterCommandProperty);
            set => base.SetValue(EnterCommandProperty, value);
        }
    }
}