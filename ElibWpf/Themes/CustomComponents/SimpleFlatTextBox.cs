using System.Windows;
using System.Windows.Controls;

namespace ElibWpf.CustomComponents
{
    public class SimpleFlatSearchBox : TextBox
    {
        public static DependencyProperty IconNameProperty;
        public static DependencyProperty WatermarkTextProperty;

        static SimpleFlatSearchBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SimpleFlatSearchBox), new FrameworkPropertyMetadata(typeof(SimpleFlatSearchBox)));
            IconNameProperty = DependencyProperty.Register("IconName", typeof(FontAwesome.WPF.FontAwesomeIcon), typeof(SimpleFlatSearchBox));
            WatermarkTextProperty = DependencyProperty.Register("WatermarkText", typeof(string), typeof(SimpleFlatSearchBox));
        }

        public FontAwesome.WPF.FontAwesomeIcon IconName
        {
            get { return (FontAwesome.WPF.FontAwesomeIcon)base.GetValue(IconNameProperty); }
            set { base.SetValue(IconNameProperty, value); }
        }

        public string WatermarkText
        {
            get { return (string)base.GetValue(WatermarkTextProperty); }
            set { base.SetValue(WatermarkTextProperty, value); }
        }
    }
}