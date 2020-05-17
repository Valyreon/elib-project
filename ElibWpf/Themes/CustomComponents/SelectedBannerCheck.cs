using System.Windows;
using System.Windows.Controls;

namespace ElibWpf.CustomComponents
{
    public class SelectedBannerCheck : CheckBox
    {
        public static DependencyProperty TextProperty;

        static SelectedBannerCheck()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SelectedBannerCheck),
                new FrameworkPropertyMetadata(typeof(SelectedBannerCheck)));
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SelectedBannerCheck));
        }

        public string Text
        {
            get => (string) this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }
    }
}