using System.Windows;
using System.Windows.Controls;

namespace ElibWpf.CustomComponents
{
    public class BetterTextbox : TextBox
    {
        public static DependencyProperty IconNameProperty;
        public static DependencyProperty IconSizeProperty;
        public static DependencyProperty IconMarginProperty;
        public static DependencyProperty TextboxPaddingProperty;
        public static DependencyProperty WatermarkTextProperty;
        public static DependencyProperty EnterCommandProperty;
        public static DependencyProperty IconVisibleProperty;

        static BetterTextbox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BetterTextbox), new FrameworkPropertyMetadata(typeof(BetterTextbox)));
            IconNameProperty = DependencyProperty.Register("IconName", typeof(FontAwesome.WPF.FontAwesomeIcon), typeof(BetterTextbox));
            IconSizeProperty = DependencyProperty.Register("IconSize", typeof(double), typeof(BetterTextbox));
            WatermarkTextProperty = DependencyProperty.Register("WatermarkText", typeof(string), typeof(BetterTextbox));
            IconMarginProperty = DependencyProperty.Register("IconMargin", typeof(System.Windows.Thickness), typeof(BetterTextbox));
            TextboxPaddingProperty = DependencyProperty.Register("TextboxPadding", typeof(System.Windows.Thickness), typeof(BetterTextbox));
            EnterCommandProperty = DependencyProperty.Register("EnterCommand", typeof(System.Windows.Input.ICommand), typeof(BetterTextbox));
            IconVisibleProperty = DependencyProperty.Register("IconVisible", typeof(bool), typeof(BetterTextbox));
        }

        public FontAwesome.WPF.FontAwesomeIcon IconName
        {
            get { return (FontAwesome.WPF.FontAwesomeIcon)base.GetValue(IconNameProperty); }
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

        public bool IconVisible
        {
            get => (bool)base.GetValue(IconVisibleProperty);
            set => base.SetValue(IconVisibleProperty, value);
        }
    }
}