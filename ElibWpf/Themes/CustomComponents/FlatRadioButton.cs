using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ElibWpf.CustomComponents
{
    public class FlatRadioButton : RadioButton
    {
        static FlatRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FlatRadioButton), new FrameworkPropertyMetadata(typeof(FlatRadioButton)));
        }
    }
}
