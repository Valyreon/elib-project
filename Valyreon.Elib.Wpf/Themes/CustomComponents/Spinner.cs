using System.Windows;
using System.Windows.Controls;

namespace Valyreon.Elib.Wpf.CustomComponents
{
    public class Spinner : UserControl
    {
        static Spinner()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Spinner),
                new FrameworkPropertyMetadata(typeof(Spinner)));
        }
    }
}
