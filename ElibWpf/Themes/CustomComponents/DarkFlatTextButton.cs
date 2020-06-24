using System.Windows;
using System.Windows.Controls;

namespace ElibWpf.CustomComponents
{
    public class DarkFlatTextButton : Button
    {
        public static DependencyProperty IconNameProperty;
        public static DependencyProperty TextProperty;

        static DarkFlatTextButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DarkFlatTextButton),
                new FrameworkPropertyMetadata(typeof(DarkFlatTextButton)));
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(DarkFlatTextButton));
        }

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }
    }
}