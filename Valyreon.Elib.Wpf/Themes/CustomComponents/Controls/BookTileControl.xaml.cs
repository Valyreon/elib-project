using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Wpf.Animations;
using Valyreon.Elib.Wpf.CustomComponents;

namespace Valyreon.Elib.Wpf.Themes.CustomComponents.Controls
{
    /// <summary>
    ///     Interaction logic for BookTileControl.xaml
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
        private readonly Duration duration;
        private SelectedBannerCheck selectedCheckbox;
        private TextLinkButton theBookTitle;

        private Border tileBorder;

        static BookTileControl()
        {
            CoverProperty = DependencyProperty.Register("Cover", typeof(Cover), typeof(BookTileControl));
            TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(BookTileControl));
            AuthorsProperty = DependencyProperty.Register("Authors", typeof(string), typeof(BookTileControl));
            SeriesInfoProperty = DependencyProperty.Register("SeriesInfo", typeof(string), typeof(BookTileControl));
            IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(BookTileControl));

            TileCommandProperty = DependencyProperty.Register("TileCommand", typeof(ICommand), typeof(BookTileControl));
            AuthorCommandProperty =
                DependencyProperty.Register("AuthorCommand", typeof(ICommand), typeof(BookTileControl));
            SeriesCommandProperty =
                DependencyProperty.Register("SeriesCommand", typeof(ICommand), typeof(BookTileControl));
            SelectCommandProperty =
                DependencyProperty.Register("SelectCommand", typeof(ICommand), typeof(BookTileControl));

            TileParameterProperty =
                DependencyProperty.Register("TileParameter", typeof(Book), typeof(BookTileControl));
            AuthorParameterProperty = DependencyProperty.Register("AuthorParameter",
                typeof(ICollection<Author>), typeof(BookTileControl));
            SeriesParameterProperty =
                DependencyProperty.Register("SeriesParameter", typeof(BookSeries), typeof(BookTileControl));
        }

        public BookTileControl()
        {
            InitializeComponent();
            duration = new Duration(new TimeSpan(0, 0, 0, 0, 300));
        }

        public ICommand AuthorCommand
        {
            get => (ICommand)GetValue(AuthorCommandProperty);
            set => SetValue(AuthorCommandProperty, value);
        }

        public ICollection<Author> AuthorParameter
        {
            get => (ICollection<Author>)GetValue(AuthorParameterProperty);
            set => SetValue(AuthorParameterProperty, value);
        }

        public string Authors
        {
            get => (string)GetValue(AuthorsProperty);
            set => SetValue(AuthorsProperty, value);
        }

        public Cover Cover
        {
            get => (Cover)GetValue(CoverProperty);
            set => SetValue(CoverProperty, value);
        }

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public ICommand SelectCommand
        {
            get => (ICommand)GetValue(SelectCommandProperty);
            set => SetValue(SelectCommandProperty, value);
        }

        public ICommand SeriesCommand
        {
            get => (ICommand)GetValue(SeriesCommandProperty);
            set => SetValue(SeriesCommandProperty, value);
        }

        public string SeriesInfo
        {
            get => (string)GetValue(SeriesInfoProperty);
            set => SetValue(SeriesInfoProperty, value);
        }

        public BookSeries SeriesParameter
        {
            get => (BookSeries)GetValue(SeriesParameterProperty);
            set => SetValue(SeriesParameterProperty, value);
        }

        public ICommand TileCommand
        {
            get => (ICommand)GetValue(TileCommandProperty);
            set => SetValue(TileCommandProperty, value);
        }

        public Book TileParameter
        {
            get => (Book)GetValue(TileParameterProperty);
            set => SetValue(TileParameterProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        private void BookContainer_MouseEnter(object sender, MouseEventArgs ev)
        {
            if (selectedCheckbox.IsChecked == false)
            {
                var borderAnim = new BrushAnimation(Brushes.CornflowerBlue, duration);
                borderAnim.Completed += (e, s) => tileBorder.BorderBrush = Brushes.CornflowerBlue;
                borderAnim.Completed += (e, s) => theBookTitle.Foreground = Brushes.CornflowerBlue;

                var opacAnim = new DoubleAnimation(1, duration, FillBehavior.Stop);
                opacAnim.Completed += (e, s) => selectedCheckbox.Opacity = 1;

                tileBorder.BeginAnimation(Border.BorderBrushProperty, borderAnim);
                theBookTitle.BeginAnimation(ForegroundProperty, borderAnim);
                selectedCheckbox.BeginAnimation(OpacityProperty, opacAnim);
            }
            else
            {
                var borderAnim = new BrushAnimation(Brushes.CornflowerBlue, duration);
                borderAnim.Completed += (e, s) => tileBorder.BorderBrush = Brushes.CornflowerBlue;
                borderAnim.Completed += (e, s) => theBookTitle.Foreground = Brushes.CornflowerBlue;
                tileBorder.BeginAnimation(Border.BorderBrushProperty, borderAnim);
                theBookTitle.BeginAnimation(ForegroundProperty, borderAnim);
            }
        }

        private void BookContainer_MouseLeave(object sender, MouseEventArgs ev)
        {
            var borderAnim = new BrushAnimation(Brushes.LightGray, duration);
            borderAnim.Completed += (e, s) => tileBorder.BorderBrush = Brushes.LightGray;

            var titleAnim = new BrushAnimation(Brushes.Black, duration);
            titleAnim.Completed += (e, s) => theBookTitle.Foreground = Brushes.Black;

            if (selectedCheckbox.IsChecked == false)
            {
                var opacAnim = new DoubleAnimation(0, duration, FillBehavior.Stop);
                opacAnim.Completed += (e, s) =>
                {
                    if (!selectedCheckbox.IsChecked.Value)
                    {
                        selectedCheckbox.Opacity = 0;
                    }
                };

                tileBorder.BeginAnimation(Border.BorderBrushProperty, borderAnim);
                theBookTitle.BeginAnimation(ForegroundProperty, titleAnim);
                selectedCheckbox.BeginAnimation(OpacityProperty, opacAnim);
            }
            else
            {
                tileBorder.BeginAnimation(Border.BorderBrushProperty, borderAnim);
                theBookTitle.BeginAnimation(ForegroundProperty, titleAnim);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            tileBorder = Template.FindName("TileBorder", this) as Border;
            theBookTitle = Template.FindName("TheBookTitle", this) as TextLinkButton;
            selectedCheckbox = Template.FindName("SelectedCheckbox", this) as SelectedBannerCheck;

            if (selectedCheckbox?.IsChecked != null && selectedCheckbox.IsChecked.Value)
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

        private void OpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(TileParameter.Path))
            {
                Process.Start("explorer.exe", "/select, " + $@"""{TileParameter.Path}""");
            }
        }
    }
}
