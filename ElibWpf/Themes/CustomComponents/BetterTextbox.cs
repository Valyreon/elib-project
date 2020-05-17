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
            get => (ICommand) this.GetValue(EnterCommandProperty);
            set => this.SetValue(EnterCommandProperty, value);
        }

        public Enum Icon
        {
            get => (Enum) this.GetValue(IconProperty);
            set => this.SetValue(IconProperty, value);
        }

        public Thickness IconMargin
        {
            get => (Thickness) this.GetValue(IconMarginProperty);
            set => this.SetValue(IconMarginProperty, value);
        }

        public double IconSize
        {
            get => (double) this.GetValue(IconSizeProperty);
            set => this.SetValue(IconSizeProperty, value);
        }

        public bool IconVisible
        {
            get => (bool) this.GetValue(IconVisibleProperty);
            set => this.SetValue(IconVisibleProperty, value);
        }

        public Thickness TextboxPadding
        {
            get => (Thickness) this.GetValue(TextboxPaddingProperty);
            set => this.SetValue(TextboxPaddingProperty, value);
        }

        public string WatermarkText
        {
            get => (string) this.GetValue(WatermarkTextProperty);
            set => this.SetValue(WatermarkTextProperty, value);
        }
    }
}