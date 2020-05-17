using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ElibWpf.CustomComponents
{
    public class CheckboxButton : CheckBox
    {
        public static DependencyProperty TextProperty;
        public static DependencyProperty CheckedIconProperty;
        public static DependencyProperty CheckedColorProperty;

        static CheckboxButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckboxButton),
                new FrameworkPropertyMetadata(typeof(CheckboxButton)));
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(CheckboxButton));
            CheckedIconProperty = DependencyProperty.Register("CheckedIcon", typeof(Enum), typeof(CheckboxButton));
            CheckedColorProperty = DependencyProperty.Register("CheckedColor", typeof(Brush), typeof(CheckboxButton));
        }

        public Brush CheckedColor
        {
            get => (Brush) this.GetValue(CheckedColorProperty);
            set => this.SetValue(CheckedColorProperty, value);
        }

        public Enum CheckedIcon
        {
            get => (Enum) this.GetValue(CheckedIconProperty);
            set => this.SetValue(CheckedIconProperty, value);
        }

        public string Text
        {
            get => (string) this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }
    }
}