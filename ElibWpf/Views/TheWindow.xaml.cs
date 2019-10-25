using ElibWpf.ViewModels;
using MahApps.Metro.Controls;

namespace ElibWpf.Views
{
    /// <summary>
    /// Interaction logic for TheWindow.xaml
    /// </summary>
    public partial class TheWindow : MetroWindow
    {
        public TheWindow()
        {
            //bla bla
            InitializeComponent();
            this.DataContext = new TheWindowViewModel();
        }
    }
}