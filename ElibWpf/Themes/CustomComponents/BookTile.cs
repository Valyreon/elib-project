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

        static BookTile()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BookTile), new FrameworkPropertyMetadata(typeof(BookTile)));
            CoverProperty = DependencyProperty.Register("Cover", typeof(IList<byte>), typeof(BookTile));
            TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(BookTile));
            AuthorsProperty = DependencyProperty.Register("Authors", typeof(string), typeof(BookTile));
            SeriesInfoProperty = DependencyProperty.Register("SeriesInfo", typeof(string), typeof(BookTile));
            TileCommandProperty = DependencyProperty.Register("TileCommand", typeof(ICommand), typeof(BookTile));
        }

        public IList<byte> Cover
        {
            get { return (IList<byte>)base.GetValue(CoverProperty); }
            set { base.SetValue(CoverProperty, value); }
        }

        public string Title
        {
            get { return (string)base.GetValue(TitleProperty); }
            set => base.SetValue(TitleProperty, value);
        }

        public string Authors
        {
            get { return (string)base.GetValue(AuthorsProperty); }
            set => base.SetValue(AuthorsProperty, value);
        }

        public string SeriesInfo
        {
            get { return (string)base.GetValue(SeriesInfoProperty); }
            set => base.SetValue(SeriesInfoProperty, value);
        }

        public ICommand TileCommand
        {
            get { return (ICommand)base.GetValue(TileCommandProperty); }
            set => base.SetValue(TileCommandProperty, value);
        }
    }
}
