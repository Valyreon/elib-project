using System.Windows;
using System.Windows.Controls;

namespace ElibWpf.CustomComponents
{
    public class TextLinkButton : Button
    {
        public static DependencyProperty TextProperty;

        static TextLinkButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextLinkButton),
                new FrameworkPropertyMetadata(typeof(TextLinkButton)));
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TextLinkButton));
        }

        public string Text
        {
            get => (string) this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }
    }
}