using FontAwesome.WPF;

using System.Windows;
using System.Windows.Controls;

namespace ElibWpf.CustomComponents
{
    public class SymbolButton : Button
    {
        public static DependencyProperty IconNameProperty;
        public static DependencyProperty IconSizeProperty;

        static SymbolButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SymbolButton), new FrameworkPropertyMetadata(typeof(SymbolButton)));
            IconNameProperty = DependencyProperty.Register("IconName", typeof(FontAwesomeIcon), typeof(SymbolButton));
            IconSizeProperty = DependencyProperty.Register("IconSize", typeof(double), typeof(SymbolButton));
        }

        public FontAwesomeIcon IconName
        {
            get { return (FontAwesomeIcon)base.GetValue(IconNameProperty); }
            set { base.SetValue(IconNameProperty, value); }
        }

        public double IconSize
        {
            get { return (double)base.GetValue(IconSizeProperty); }
            set => base.SetValue(IconSizeProperty, value);
        }
    }
}