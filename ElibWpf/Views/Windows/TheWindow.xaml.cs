using ElibWpf.ViewModels;
using ElibWpf.ViewModels.Windows;
using MahApps.Metro.Controls;

namespace ElibWpf.Views.Windows
{
    /// <summary>
    /// Interaction logic for TheWindow.xaml
    /// </summary>
    public partial class TheWindow : MetroWindow
    {
        public TheWindow()
        {
            InitializeComponent();
            this.DataContext = new TheWindowViewModel();
        }
    }
}