using System.Windows;
using System.Windows.Controls;

namespace ElibWpf.CustomComponents
{
    public class TextBoxWithSymbol : TextBox
    {
        public static DependencyProperty IconNameProperty;
        public static DependencyProperty WatermarkTextProperty;

        static TextBoxWithSymbol()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBoxWithSymbol), new FrameworkPropertyMetadata(typeof(TextBoxWithSymbol)));
            IconNameProperty = DependencyProperty.Register("IconName", typeof(FontAwesome.WPF.FontAwesomeIcon), typeof(TextBoxWithSymbol));
            WatermarkTextProperty = DependencyProperty.Register("WatermarkText", typeof(string), typeof(TextBoxWithSymbol));
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