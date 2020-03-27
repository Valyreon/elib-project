using FontAwesome.WPF;

using System.Windows;
using System.Windows.Controls;

namespace ElibWpf.CustomComponents
{
    public class DarkFlatSymbolButton : Button
    {
        public static DependencyProperty IconNameProperty;
        public static DependencyProperty IconSizeProperty;

        static DarkFlatSymbolButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DarkFlatSymbolButton), new FrameworkPropertyMetadata(typeof(DarkFlatSymbolButton)));
            IconNameProperty = DependencyProperty.Register("IconName", typeof(FontAwesomeIcon), typeof(DarkFlatSymbolButton));
            IconSizeProperty = DependencyProperty.Register("IconSize", typeof(double), typeof(DarkFlatSymbolButton));
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