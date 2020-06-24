using Domain;
using ElibWpf.Animations;
using ElibWpf.CustomComponents;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ElibWpf.Themes.CustomComponents.Controls
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
        private static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
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
            this.InitializeComponent();
            this.duration = new Duration(new TimeSpan(0, 0, 0, 0, 300));
        }

        public ICommand AuthorCommand
        {
            get => (ICommand)this.GetValue(AuthorCommandProperty);
            set => this.SetValue(AuthorCommandProperty, value);
        }

        public ICollection<Author> AuthorParameter
        {
            get => (ICollection<Author>)this.GetValue(AuthorParameterProperty);
            set => this.SetValue(AuthorParameterProperty, value);
        }

        public string Authors
        {
            get => (string)this.GetValue(AuthorsProperty);
            set => this.SetValue(AuthorsProperty, value);
        }

        public Cover Cover
        {
            get => (Cover)this.GetValue(CoverProperty);
            set => this.SetValue(CoverProperty, value);
        }

        public bool IsSelected
        {
            get => (bool)this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value);
        }

        public ICommand SelectCommand
        {
            get => (ICommand)this.GetValue(SelectCommandProperty);
            set => this.SetValue(SelectCommandProperty, value);
        }

        public ICommand SeriesCommand
        {
            get => (ICommand)this.GetValue(SeriesCommandProperty);
            set => this.SetValue(SeriesCommandProperty, value);
        }

        public string SeriesInfo
        {
            get => (string)this.GetValue(SeriesInfoProperty);
            set => this.SetValue(SeriesInfoProperty, value);
        }

        public BookSeries SeriesParameter
        {
            get => (BookSeries)this.GetValue(SeriesParameterProperty);
            set => this.SetValue(SeriesParameterProperty, value);
        }

        public ICommand TileCommand
        {
            get => (ICommand)this.GetValue(TileCommandProperty);
            set => this.SetValue(TileCommandProperty, value);
        }

        public Book TileParameter
        {
            get => (Book)this.GetValue(TileParameterProperty);
            set => this.SetValue(TileParameterProperty, value);
        }

        public string Title
        {
            get => (string)this.GetValue(TitleProperty);
            set => this.SetValue(TitleProperty, value);
        }

        private void BookContainer_MouseEnter(object sender, MouseEventArgs ev)
        {
            if (this.selectedCheckbox.IsChecked != null && !this.selectedCheckbox.IsChecked.Value)
            {
                BrushAnimation borderAnim = new BrushAnimation(Brushes.CornflowerBlue, this.duration);
                borderAnim.Completed += (e, s) => this.tileBorder.BorderBrush = Brushes.CornflowerBlue;
                borderAnim.Completed += (e, s) => this.theBookTitle.Foreground = Brushes.CornflowerBlue;

                DoubleAnimation opacAnim = new DoubleAnimation(1, this.duration, FillBehavior.Stop);
                opacAnim.Completed += (e, s) => this.selectedCheckbox.Opacity = 1;

                this.tileBorder.BeginAnimation(Border.BorderBrushProperty, borderAnim);
                this.theBookTitle.BeginAnimation(ForegroundProperty, borderAnim);
                this.selectedCheckbox.BeginAnimation(OpacityProperty, opacAnim);
            }
            else
            {
                BrushAnimation borderAnim = new BrushAnimation(Brushes.CornflowerBlue, this.duration);
                borderAnim.Completed += (e, s) => this.tileBorder.BorderBrush = Brushes.CornflowerBlue;
                borderAnim.Completed += (e, s) => this.theBookTitle.Foreground = Brushes.CornflowerBlue;
                this.tileBorder.BeginAnimation(Border.BorderBrushProperty, borderAnim);
                this.theBookTitle.BeginAnimation(ForegroundProperty, borderAnim);
            }
        }

        private void BookContainer_MouseLeave(object sender, MouseEventArgs ev)
        {
            BrushAnimation borderAnim = new BrushAnimation(Brushes.LightGray, this.duration);
            borderAnim.Completed += (e, s) => this.tileBorder.BorderBrush = Brushes.LightGray;

            BrushAnimation titleAnim = new BrushAnimation(Brushes.Black, this.duration);
            titleAnim.Completed += (e, s) => this.theBookTitle.Foreground = Brushes.Black;

            if (this.selectedCheckbox.IsChecked != null && !this.selectedCheckbox.IsChecked.Value)
            {
                DoubleAnimation opacAnim = new DoubleAnimation(0, this.duration, FillBehavior.Stop);
                opacAnim.Completed += async (e, s) =>
                {
                    await semaphoreSlim.WaitAsync();

                    if (this.selectedCheckbox.IsChecked.Value == false)
                    {
                        this.selectedCheckbox.Opacity = 0;
                    }

                    semaphoreSlim.Release();
                };

                this.tileBorder.BeginAnimation(Border.BorderBrushProperty, borderAnim);
                this.theBookTitle.BeginAnimation(ForegroundProperty, titleAnim);
                this.selectedCheckbox.BeginAnimation(OpacityProperty, opacAnim);
            }
            else
            {
                this.tileBorder.BeginAnimation(Border.BorderBrushProperty, borderAnim);
                this.theBookTitle.BeginAnimation(ForegroundProperty, titleAnim);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.tileBorder = this.Template.FindName("TileBorder", this) as Border;
            this.theBookTitle = this.Template.FindName("TheBookTitle", this) as TextLinkButton;
            this.selectedCheckbox = this.Template.FindName("SelectedCheckbox", this) as SelectedBannerCheck;

            if (this.selectedCheckbox?.IsChecked != null && this.selectedCheckbox.IsChecked.Value)
            {
                this.selectedCheckbox.Opacity = 1;
            }
        }

        private void SelectedCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            this.selectedCheckbox.Opacity = 1;
        }

        private void SelectedCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
        }
    }
}