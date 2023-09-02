using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Valyreon.Elib.Wpf.CustomComponents
{
    public class SymbolButton : Button
    {
        public static DependencyProperty ImageProperty;
        public static DependencyProperty IconSizeProperty;

        static SymbolButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SymbolButton),
                new FrameworkPropertyMetadata(typeof(SymbolButton)));
            ImageProperty = DependencyProperty.Register("Image", typeof(ImageSource), typeof(SymbolButton));
            IconSizeProperty = DependencyProperty.Register("IconSize", typeof(double), typeof(SymbolButton));
        }

        public ImageSource Image
        {
            get => (ImageSource)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        public double IconSize
        {
            get => (double)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }
    }
}
