using MahApps.Metro.IconPacks;
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
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SymbolButton), new FrameworkPropertyMetadata(typeof(SymbolButton)));
            IconProperty = DependencyProperty.Register("Icon", typeof(Enum), typeof(SymbolButton));
            IconSizeProperty = DependencyProperty.Register("IconSize", typeof(double), typeof(SymbolButton));
        }

        public Enum Icon
        {
            get { return (Enum)base.GetValue(IconProperty); }
            set { base.SetValue(IconProperty, value); }
        }

        public double IconSize
        {
            get { return (double)base.GetValue(IconSizeProperty); }
            set => base.SetValue(IconSizeProperty, value);
        }
    }
}