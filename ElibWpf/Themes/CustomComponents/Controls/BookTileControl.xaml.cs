using Domain;
using ElibWpf.Animations;
using ElibWpf.CustomComponents;
using Models.Observables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ElibWpf.Themes.CustomComponents.Controls
{
    /// <summary>
    /// Interaction logic for BookTileControl.xaml
    /// </summary>
    public partial class BookTileControl : UserControl
    {
        public static DependencyProperty AuthorCommandProperty;
        public static DependencyProperty AuthorParameterProperty;
        public static DependencyProperty AuthorsProperty;
        public static DependencyProperty CoverProperty;
        public static DependencyProperty SeriesCommandProperty;
        public static DependencyProperty SeriesInfoProperty;
        public static DependencyProperty SeriesParameterProperty;
        public static DependencyProperty TileCommandProperty;
        public static DependencyProperty TileParameterProperty;
        public static DependencyProperty SelectCommandProperty;
        public static DependencyProperty TitleProperty;
        public static DependencyProperty IsSelectedProperty;

        static BookTileControl()
        {
            CoverProperty = DependencyProperty.Register("Cover", typeof(IList<byte>), typeof(BookTileControl));
            TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(BookTileControl));
            AuthorsProperty = DependencyProperty.Register("Authors", typeof(string), typeof(BookTileControl));
            SeriesInfoProperty = DependencyProperty.Register("SeriesInfo", typeof(string), typeof(BookTileControl));
            IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(BookTileControl));

            TileCommandProperty = DependencyProperty.Register("TileCommand", typeof(ICommand), typeof(BookTileControl));
            AuthorCommandProperty = DependencyProperty.Register("AuthorCommand", typeof(ICommand), typeof(BookTileControl));
            SeriesCommandProperty = DependencyProperty.Register("SeriesCommand", typeof(ICommand), typeof(BookTileControl));
            SelectCommandProperty = DependencyProperty.Register("SelectCommand", typeof(ICommand), typeof(BookTileControl));

            TileParameterProperty = DependencyProperty.Register("TileParameter", typeof(ObservableBook), typeof(BookTileControl));
            AuthorParameterProperty = DependencyProperty.Register("AuthorParameter", typeof(ICollection<ObservableAuthor>), typeof(BookTileControl));
            SeriesParameterProperty = DependencyProperty.Register("SeriesParameter", typeof(ObservableSeries), typeof(BookTileControl));
        }

        public BookTileControl()
        {
            InitializeComponent();
            duration = new Duration(new TimeSpan(0, 0, 0, 0, 300));
        }

        public ICommand AuthorCommand
        {
            get => (ICommand)base.GetValue(AuthorCommandProperty);
            set => base.SetValue(AuthorCommandProperty, value);
        }

        public ICollection<ObservableAuthor> AuthorParameter
        {
            get => (ICollection<ObservableAuthor>)base.GetValue(AuthorParameterProperty);
            set => base.SetValue(AuthorParameterProperty, value);
        }

        public string Authors
        {
            get => (string)base.GetValue(AuthorsProperty);
            set => base.SetValue(AuthorsProperty, value);
        }

        public IList<byte> Cover
        {
            get => (IList<byte>)base.GetValue(CoverProperty);
            set => base.SetValue(CoverProperty, value);
        }

        public ICommand SeriesCommand
        {
            get => (ICommand)base.GetValue(SeriesCommandProperty);
            set => base.SetValue(SeriesCommandProperty, value);
        }

        public string SeriesInfo
        {
            get => (string)base.GetValue(SeriesInfoProperty);
            set => base.SetValue(SeriesInfoProperty, value);
        }

        public ObservableSeries SeriesParameter
        {
            get => (ObservableSeries)base.GetValue(SeriesParameterProperty);
            set => base.SetValue(SeriesParameterProperty, value);
        }

        public ICommand TileCommand
        {
            get => (ICommand)base.GetValue(TileCommandProperty);
            set => base.SetValue(TileCommandProperty, value);
        }

        public ObservableBook TileParameter
        {
            get => (ObservableBook)base.GetValue(TileParameterProperty);
            set => base.SetValue(TileParameterProperty, value);
        }

        public ICommand SelectCommand
        {
            get => (ICommand)base.GetValue(SelectCommandProperty);
            set => base.SetValue(SelectCommandProperty, value);
        }

        public string Title
        {
            get => (string)base.GetValue(TitleProperty);
            set => base.SetValue(TitleProperty, value);
        }

        public bool IsSelected
        {
            get => (bool)base.GetValue(IsSelectedProperty);
            set => base.SetValue(IsSelectedProperty, value);
        }

        Border tileBorder;
        TextLinkButton theBookTitle;
        SelectedBannerCheck selectedCheckbox;
        Duration duration;

        private void BookContainer_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!selectedCheckbox.IsChecked.Value)
            {
                BrushAnimation borderAnim = new BrushAnimation(Brushes.CornflowerBlue,duration);
                borderAnim.Completed += (e, s) => tileBorder.BorderBrush = Brushes.CornflowerBlue;
                borderAnim.Completed += (e, s) => theBookTitle.Foreground = Brushes.CornflowerBlue;

                DoubleAnimation opacAnim = new DoubleAnimation(1, duration, FillBehavior.Stop);
                opacAnim.Completed += (e, s) => selectedCheckbox.Opacity = 1;

                tileBorder.BeginAnimation(Border.BorderBrushProperty, borderAnim);
                theBookTitle.BeginAnimation(TextLinkButton.ForegroundProperty, borderAnim);
                selectedCheckbox.BeginAnimation(CheckBox.OpacityProperty, opacAnim);
            }
            else
            {
                BrushAnimation borderAnim = new BrushAnimation(Brushes.CornflowerBlue, duration);
                borderAnim.Completed += (e, s) => tileBorder.BorderBrush = Brushes.CornflowerBlue;
                borderAnim.Completed += (e, s) => theBookTitle.Foreground = Brushes.CornflowerBlue;
                tileBorder.BeginAnimation(Border.BorderBrushProperty, borderAnim);
                theBookTitle.BeginAnimation(TextLinkButton.ForegroundProperty, borderAnim);
            }
        }

        private void BookContainer_MouseLeave(object sender, MouseEventArgs e)
        {
            BrushAnimation borderAnim = new BrushAnimation(Brushes.LightGray, duration);
            borderAnim.Completed += (e, s) => tileBorder.BorderBrush = Brushes.LightGray;

            BrushAnimation titleAnim = new BrushAnimation(Brushes.Black, duration);
            titleAnim.Completed += (e, s) => theBookTitle.Foreground = Brushes.Black;

            if (!selectedCheckbox.IsChecked.Value)
            {
                DoubleAnimation opacAnim = new DoubleAnimation(0, duration, FillBehavior.Stop);
                opacAnim.Completed += (e, s) => selectedCheckbox.Opacity = 0;

                tileBorder.BeginAnimation(Border.BorderBrushProperty, borderAnim);
                theBookTitle.BeginAnimation(TextLinkButton.ForegroundProperty, titleAnim);
                selectedCheckbox.BeginAnimation(CheckBox.OpacityProperty, opacAnim);
            }
            else
            {
                tileBorder.BeginAnimation(Border.BorderBrushProperty, borderAnim);
                theBookTitle.BeginAnimation(TextLinkButton.ForegroundProperty, titleAnim);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            tileBorder = Template.FindName("TileBorder", this) as Border;
            theBookTitle = Template.FindName("TheBookTitle", this) as TextLinkButton;
            selectedCheckbox = Template.FindName("SelectedCheckbox", this) as SelectedBannerCheck;

            if(selectedCheckbox.IsChecked.HasValue && selectedCheckbox.IsChecked.Value)
            {
                selectedCheckbox.Opacity = 1;
            }
        }

        private void SelectedCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            selectedCheckbox.Opacity = 1;
        }

        private void SelectedCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {

        }
    }
}
