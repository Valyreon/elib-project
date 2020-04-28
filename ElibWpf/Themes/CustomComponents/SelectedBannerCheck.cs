using System.Windows;
using System.Windows.Controls;

namespace ElibWpf.CustomComponents
{
    public class SelectedBannerCheck : CheckBox
    {
        public static DependencyProperty TextProperty;

        static SelectedBannerCheck()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SelectedBannerCheck), new FrameworkPropertyMetadata(typeof(SelectedBannerCheck)));
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SelectedBannerCheck));
        }

        public string Text
        {
            get { return (string)base.GetValue(TextProperty); }
            set => base.SetValue(TextProperty, value);
        }
    }
}
