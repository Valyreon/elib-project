using System.Windows;
using System.Windows.Controls;

namespace ElibWpf.CustomComponents
{
    public class HeaderButton : Button
    {
        public static DependencyProperty IconNameProperty;
        public static DependencyProperty TextProperty;

        static HeaderButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HeaderButton),
                new FrameworkPropertyMetadata(typeof(HeaderButton)));
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(HeaderButton));
        }

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }
    }
}