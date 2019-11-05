using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace ElibWpf.CustomComponents
{
    public class TextLinkButton : Button
    {
        public static DependencyProperty TextProperty;

        static TextLinkButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextLinkButton), new FrameworkPropertyMetadata(typeof(TextLinkButton)));
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TextLinkButton));
        }

        public string Text
        {
            get { return (string)base.GetValue(TextProperty); }
            set => base.SetValue(TextProperty, value);
        }
    }
}
