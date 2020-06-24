using System;
using System.Windows;
using System.Windows.Controls;

namespace ElibWpf.CustomComponents
{
    public class SymbolButton : Button
    {
        public static DependencyProperty IconProperty;
        public static DependencyProperty IconSizeProperty;

        static SymbolButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SymbolButton),
                new FrameworkPropertyMetadata(typeof(SymbolButton)));
            IconProperty = DependencyProperty.Register("Icon", typeof(Enum), typeof(SymbolButton));
            IconSizeProperty = DependencyProperty.Register("IconSize", typeof(double), typeof(SymbolButton));
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