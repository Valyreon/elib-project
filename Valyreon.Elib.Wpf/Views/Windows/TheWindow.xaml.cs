using MahApps.Metro.Controls;
using Valyreon.Elib.Wpf.ViewModels.Windows;

namespace Valyreon.Elib.Wpf.Views.Windows
{
    /// <summary>
    ///     Interaction logic for TheWindow.xaml
    /// </summary>
    public partial class TheWindow : MetroWindow
    {
        public TheWindow()
        {
            InitializeComponent();
            DataContext = new TheWindowViewModel();
        }
    }
}
