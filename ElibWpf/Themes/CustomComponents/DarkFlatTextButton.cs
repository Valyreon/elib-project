using ElibWpf.CustomComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ElibWpf.CustomComponents
{
    public class DarkFlatTextButton : Button
    {
        public static DependencyProperty IconNameProperty;
        public static DependencyProperty TextProperty;

        static DarkFlatTextButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DarkFlatTextButton), new FrameworkPropertyMetadata(typeof(DarkFlatTextButton)));
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(DarkFlatTextButton));
        }

        public string Text
        {
            get { return (string)base.GetValue(TextProperty); }
            set => base.SetValue(TextProperty, value);
        }
    }
}
