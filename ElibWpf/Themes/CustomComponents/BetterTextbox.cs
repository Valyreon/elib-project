using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ElibWpf.CustomComponents
{
    public class BetterTextbox : TextBox
    {
        public static DependencyProperty IconProperty;
        public static DependencyProperty IconSizeProperty;
        public static DependencyProperty IconMarginProperty;
        public static DependencyProperty TextboxPaddingProperty;
        public static DependencyProperty WatermarkTextProperty;
        public static DependencyProperty EnterCommandProperty;
        public static DependencyProperty IconVisibleProperty;

        static BetterTextbox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BetterTextbox),
                new FrameworkPropertyMetadata(typeof(BetterTextbox)));
            IconProperty = DependencyProperty.Register("Icon", typeof(Enum), typeof(BetterTextbox));
            IconSizeProperty = DependencyProperty.Register("IconSize", typeof(double), typeof(BetterTextbox));
            WatermarkTextProperty = DependencyProperty.Register("WatermarkText", typeof(string), typeof(BetterTextbox));
            IconMarginProperty = DependencyProperty.Register("IconMargin", typeof(Thickness), typeof(BetterTextbox));
            TextboxPaddingProperty =
                DependencyProperty.Register("TextboxPadding", typeof(Thickness), typeof(BetterTextbox));
            EnterCommandProperty = DependencyProperty.Register("EnterCommand", typeof(ICommand), typeof(BetterTextbox));
            IconVisibleProperty = DependencyProperty.Register("IconVisible", typeof(bool), typeof(BetterTextbox));
        }

        public ICommand EnterCommand
        {
            get => (ICommand)GetValue(EnterCommandProperty);
            set => SetValue(EnterCommandProperty, value);
        }

        public Enum Icon
        {
            get => (Enum)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public Thickness IconMargin
        {
            get => (Thickness)GetValue(IconMarginProperty);
            set => SetValue(IconMarginProperty, value);
        }

        public double IconSize
        {
            get => (double)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        public bool IconVisible
        {
            get => (bool)GetValue(IconVisibleProperty);
            set => SetValue(IconVisibleProperty, value);
        }

        public Thickness TextboxPadding
        {
            get => (Thickness)GetValue(TextboxPaddingProperty);
            set => SetValue(TextboxPaddingProperty, value);
        }

        public string WatermarkText
        {
            get => (string)GetValue(WatermarkTextProperty);
            set => SetValue(WatermarkTextProperty, value);
        }
    }
}
