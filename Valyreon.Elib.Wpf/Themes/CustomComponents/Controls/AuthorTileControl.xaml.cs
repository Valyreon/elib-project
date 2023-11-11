using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.Wpf.Themes.CustomComponents.Controls
{
    public partial class AuthorTileControl : UserControl
    {
        public static DependencyProperty AuthorBookCountProperty;
        public static DependencyProperty AuthorNameProperty;
        public static DependencyProperty CommandProperty;
        public static DependencyProperty ParameterProperty;

        static AuthorTileControl()
        {
            AuthorNameProperty = DependencyProperty.Register("AuthorName", typeof(string), typeof(AuthorTileControl));
            AuthorBookCountProperty = DependencyProperty.Register("AuthorBookCount", typeof(string), typeof(AuthorTileControl));
            CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(AuthorTileControl));
            ParameterProperty = DependencyProperty.Register("Parameter", typeof(Author), typeof(AuthorTileControl));
        }

        public AuthorTileControl()
        {
            InitializeComponent();
        }

        public string AuthorBookCount
        {
            get => (string)GetValue(AuthorBookCountProperty);
            set => SetValue(AuthorBookCountProperty, value);
        }

        public string AuthorName
        {
            get => (string)GetValue(AuthorNameProperty);
            set => SetValue(AuthorNameProperty, value);
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public Author Parameter
        {
            get => (Author)GetValue(ParameterProperty);
            set => SetValue(ParameterProperty, value);
        }

        private void TileBorder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Command?.Execute(Parameter);
        }
    }
}
