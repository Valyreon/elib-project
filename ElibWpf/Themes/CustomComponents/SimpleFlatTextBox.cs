using System.Windows;
using System.Windows.Controls;

namespace ElibWpf.CustomComponents
{
    public class SimpleFlatTextBox : TextBox
    {
        public static DependencyProperty IconNameProperty;
        public static DependencyProperty WatermarkTextProperty;

        static SimpleFlatTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SimpleFlatTextBox), new FrameworkPropertyMetadata(typeof(SimpleFlatTextBox)));
            IconNameProperty = DependencyProperty.Register("IconName", typeof(FontAwesome.WPF.FontAwesomeIcon), typeof(SimpleFlatTextBox));
            WatermarkTextProperty = DependencyProperty.Register("WatermarkText", typeof(string), typeof(SimpleFlatTextBox));
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