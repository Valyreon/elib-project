using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Valyreon.Elib.Wpf.CustomComponents
{
    public class CheckboxButton : CheckBox
    {
        public static DependencyProperty TextProperty;
        public static DependencyProperty IconSizeProperty;
        public static DependencyProperty ImageCheckedProperty;
        public static DependencyProperty ImageUncheckedProperty;

        static CheckboxButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckboxButton),
                new FrameworkPropertyMetadata(typeof(CheckboxButton)));
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(CheckboxButton));
            ImageCheckedProperty = DependencyProperty.Register("ImageChecked", typeof(ImageSource), typeof(CheckboxButton));
            ImageUncheckedProperty = DependencyProperty.Register("ImageUnchecked", typeof(ImageSource), typeof(CheckboxButton));
            IconSizeProperty = DependencyProperty.Register("IconSize", typeof(double), typeof(CheckboxButton));
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public ImageSource ImageChecked
        {
            get => (ImageSource)GetValue(ImageCheckedProperty);
            set => SetValue(ImageCheckedProperty, value);
        }

        public ImageSource ImageUnchecked
        {
            get => (ImageSource)GetValue(ImageUncheckedProperty);
            set => SetValue(ImageUncheckedProperty, value);
        }

        public double IconSize
        {
            get => (double)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }
    }
}
