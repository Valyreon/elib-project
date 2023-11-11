using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.Wpf.Themes.CustomComponents.Controls
{
    public partial class SeriesTileControl : UserControl
    {
        public static DependencyProperty CommandProperty;
        public static DependencyProperty ParameterProperty;
        public static DependencyProperty SeriesBookCountProperty;
        public static DependencyProperty SeriesNameProperty;

        static SeriesTileControl()
        {
            SeriesNameProperty = DependencyProperty.Register("SeriesName", typeof(string), typeof(SeriesTileControl));
            SeriesBookCountProperty = DependencyProperty.Register("SeriesBookCount", typeof(string), typeof(SeriesTileControl));
            CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(SeriesTileControl));
            ParameterProperty = DependencyProperty.Register("Parameter", typeof(BookSeries), typeof(SeriesTileControl));
        }

        public SeriesTileControl()
        {
            InitializeComponent();
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public BookSeries Parameter
        {
            get => (BookSeries)GetValue(ParameterProperty);
            set => SetValue(ParameterProperty, value);
        }

        public string SeriesBookCount
        {
            get => (string)GetValue(SeriesBookCountProperty);
            set => SetValue(SeriesBookCountProperty, value);
        }

        public string SeriesName
        {
            get => (string)GetValue(SeriesNameProperty);
            set => SetValue(SeriesNameProperty, value);
        }

        private void TileBorder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Command?.Execute(Parameter);
        }
    }
}
