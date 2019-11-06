using Domain;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ElibWpf.CustomComponents
{
    public class BookTile : Control
    {
        public static DependencyProperty CoverProperty;
        public static DependencyProperty TitleProperty;
        public static DependencyProperty AuthorsProperty;
        public static DependencyProperty SeriesInfoProperty;
        public static DependencyProperty TileCommandProperty;
        public static DependencyProperty AuthorCommandProperty;
        public static DependencyProperty SeriesCommandProperty;
        public static DependencyProperty AuthorParameterProperty;
        public static DependencyProperty SeriesParameterProperty;



        static BookTile()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BookTile), new FrameworkPropertyMetadata(typeof(BookTile)));
            CoverProperty = DependencyProperty.Register("Cover", typeof(IList<byte>), typeof(BookTile));
            TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(BookTile));
            AuthorsProperty = DependencyProperty.Register("Authors", typeof(string), typeof(BookTile));
            SeriesInfoProperty = DependencyProperty.Register("SeriesInfo", typeof(string), typeof(BookTile));
            TileCommandProperty = DependencyProperty.Register("TileCommand", typeof(ICommand), typeof(BookTile));

            AuthorCommandProperty = DependencyProperty.Register("AuthorCommand", typeof(ICommand), typeof(BookTile));
            SeriesCommandProperty = DependencyProperty.Register("SeriesCommand", typeof(ICommand), typeof(BookTile));

            AuthorParameterProperty = DependencyProperty.Register("AuthorParameter", typeof(ICollection<Author>), typeof(BookTile));
            SeriesParameterProperty = DependencyProperty.Register("SeriesParameter", typeof(BookSeries), typeof(BookTile));
        }

        public IList<byte> Cover
        {
            get => (IList<byte>)base.GetValue(CoverProperty);
            set => base.SetValue(CoverProperty, value);
        }

        public string Title
        {
            get => (string)base.GetValue(TitleProperty);
            set => base.SetValue(TitleProperty, value);
        }

        public string Authors
        {
            get => (string)base.GetValue(AuthorsProperty);
            set => base.SetValue(AuthorsProperty, value);
        }

        public string SeriesInfo
        {
            get => (string)base.GetValue(SeriesInfoProperty);
            set => base.SetValue(SeriesInfoProperty, value);
        }

        public ICommand TileCommand
        {
            get => (ICommand)base.GetValue(TileCommandProperty);
            set => base.SetValue(TileCommandProperty, value);
        }

        public ICommand AuthorCommand
        {
            get => (ICommand)base.GetValue(AuthorCommandProperty);
            set => base.SetValue(AuthorCommandProperty, value);
        }

        public ICommand SeriesCommand
        {
            get => (ICommand)base.GetValue(SeriesCommandProperty);
            set => base.SetValue(SeriesCommandProperty, value);
        }

        public ICollection<Author> AuthorParameter
        {
            get => (ICollection<Author>)base.GetValue(AuthorParameterProperty);
            set => base.SetValue(AuthorParameterProperty, value);
        }

        public BookSeries SeriesParameter
        {
            get => (BookSeries)base.GetValue(SeriesParameterProperty);
            set => base.SetValue(SeriesParameterProperty, value);
        }
    }
}
