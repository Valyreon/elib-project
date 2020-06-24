using MahApps.Metro.IconPacks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBoxWithSymbol),
                new FrameworkPropertyMetadata(typeof(TextBoxWithSymbol)));
            IconNameProperty =
                DependencyProperty.Register("IconName", typeof(PackIconFontAwesomeKind), typeof(TextBoxWithSymbol));
            IconSizeProperty = DependencyProperty.Register("IconSize", typeof(double), typeof(TextBoxWithSymbol));
            WatermarkTextProperty =
                DependencyProperty.Register("WatermarkText", typeof(string), typeof(TextBoxWithSymbol));
            IconMarginProperty =
                DependencyProperty.Register("IconMargin", typeof(Thickness), typeof(TextBoxWithSymbol));
            TextboxPaddingProperty =
                DependencyProperty.Register("TextboxPadding", typeof(Thickness), typeof(TextBoxWithSymbol));
            EnterCommandProperty =
                DependencyProperty.Register("EnterCommand", typeof(ICommand), typeof(TextBoxWithSymbol));
        }

        public ICommand EnterCommand
        {
            get => (ICommand)this.GetValue(EnterCommandProperty);
            set => this.SetValue(EnterCommandProperty, value);
        }

        public Thickness IconMargin
        {
            get => (Thickness)this.GetValue(IconMarginProperty);
            set => this.SetValue(IconMarginProperty, value);
        }

        public PackIconFontAwesomeKind IconName
        {
            get => (PackIconFontAwesomeKind)this.GetValue(IconNameProperty);
            set => this.SetValue(IconNameProperty, value);
        }

        public double IconSize
        {
            get => (double)this.GetValue(IconSizeProperty);
            set => this.SetValue(IconSizeProperty, value);
        }

        public Thickness TextboxPadding
        {
            get => (Thickness)this.GetValue(TextboxPaddingProperty);
            set => this.SetValue(TextboxPaddingProperty, value);
        }

        public string WatermarkText
        {
            get => (string)this.GetValue(WatermarkTextProperty);
            set => this.SetValue(WatermarkTextProperty, value);
        }
    }
}