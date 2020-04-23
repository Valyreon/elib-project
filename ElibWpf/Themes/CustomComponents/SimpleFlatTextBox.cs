using System;
using System.Windows;
using System.Windows.Controls;

namespace ElibWpf.CustomComponents
{
    public class SimpleFlatSearchBox : TextBox
    {
        public static DependencyProperty IconProperty;
        public static DependencyProperty WatermarkTextProperty;

        static SimpleFlatSearchBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SimpleFlatSearchBox), new FrameworkPropertyMetadata(typeof(SimpleFlatSearchBox)));
            IconProperty = DependencyProperty.Register("IconName", typeof(Enum), typeof(SimpleFlatSearchBox));
            WatermarkTextProperty = DependencyProperty.Register("WatermarkText", typeof(string), typeof(SimpleFlatSearchBox));
        }

        public Enum IconName
        {
            get { return (Enum)base.GetValue(IconProperty); }
            set { base.SetValue(IconProperty, value); }
        }

        public string WatermarkText
        {
            get { return (string)base.GetValue(WatermarkTextProperty); }
            set { base.SetValue(WatermarkTextProperty, value); }
        }
    }
}