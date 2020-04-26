using MahApps.Metro.IconPacks;
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
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckboxButton), new FrameworkPropertyMetadata(typeof(CheckboxButton)));
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(CheckboxButton));
            CheckedIconProperty = DependencyProperty.Register("CheckedIcon", typeof(Enum), typeof(CheckboxButton));
            CheckedColorProperty = DependencyProperty.Register("CheckedColor", typeof(Brush), typeof(CheckboxButton));
        }

        public string Text
        {
            get { return (string)base.GetValue(TextProperty); }
            set => base.SetValue(TextProperty, value);
        }

        public Enum CheckedIcon
        {
            get { return (Enum)base.GetValue(CheckedIconProperty); }
            set => base.SetValue(CheckedIconProperty, value);
        }

        public Brush CheckedColor
        {
            get { return (Brush)base.GetValue(CheckedColorProperty); }
            set => base.SetValue(CheckedColorProperty, value);
        }
    }
}