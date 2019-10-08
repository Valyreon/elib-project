using ElibWpf.ViewModels;
using System.Windows;

namespace ElibWpf.Views
{
    /// <summary>
    /// Interaction logic for TheWindow.xaml
    /// </summary>
    public partial class TheWindow : Window
    {
        public TheWindow()
        {
            InitializeComponent();
            this.DataContext = new TheWindowViewModel();
        }
    }
}
