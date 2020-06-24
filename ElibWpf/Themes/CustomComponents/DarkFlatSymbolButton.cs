using System;
using System.Windows;
using System.Windows.Controls;

namespace ElibWpf.CustomComponents
{
    public class DarkFlatSymbolButton : Button
    {
        public static DependencyProperty IconProperty;
        public static DependencyProperty IconSizeProperty;

        static DarkFlatSymbolButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DarkFlatSymbolButton),
                new FrameworkPropertyMetadata(typeof(DarkFlatSymbolButton)));
            IconProperty = DependencyProperty.Register("Icon", typeof(Enum), typeof(DarkFlatSymbolButton));
            IconSizeProperty = DependencyProperty.Register("IconSize", typeof(double), typeof(DarkFlatSymbolButton));
        }

        public Enum Icon
        {
            get => (Enum)this.GetValue(IconProperty);
            set => this.SetValue(IconProperty, value);
        }

        public double IconSize
        {
            get => (double)this.GetValue(IconSizeProperty);
            set => this.SetValue(IconSizeProperty, value);
        }
    }
}