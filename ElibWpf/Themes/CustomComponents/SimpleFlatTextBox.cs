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
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SimpleFlatSearchBox),
                new FrameworkPropertyMetadata(typeof(SimpleFlatSearchBox)));
            IconProperty = DependencyProperty.Register("IconName", typeof(Enum), typeof(SimpleFlatSearchBox));
            WatermarkTextProperty =
                DependencyProperty.Register("WatermarkText", typeof(string), typeof(SimpleFlatSearchBox));
        }

        public Enum IconName
        {
            get => (Enum) this.GetValue(IconProperty);
            set => this.SetValue(IconProperty, value);
        }

        public string WatermarkText
        {
            get => (string) this.GetValue(WatermarkTextProperty);
            set => this.SetValue(WatermarkTextProperty, value);
        }
    }
}