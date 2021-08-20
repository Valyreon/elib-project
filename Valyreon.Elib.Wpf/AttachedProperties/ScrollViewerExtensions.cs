using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Valyreon.Elib.Wpf.AttachedProperties
{
    public static class ScrollViewerExtensions
    {
        public static readonly DependencyProperty ScrollToBottomProperty =
            DependencyProperty.RegisterAttached("ScrollToBottom", typeof(ICommand), typeof(ScrollViewerExtensions),
                new FrameworkPropertyMetadata(null, OnScrollToBottomPropertyChanged));

        /// <summary>
        ///     VerticalOffset attached property
        /// </summary>
        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached("VerticalOffset", typeof(double),
                typeof(ScrollViewerExtensions), new FrameworkPropertyMetadata(double.NaN,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnVerticalOffsetPropertyChanged));

        /// <summary>
        ///     Just a flag that the binding has been applied.
        /// </summary>
        private static readonly DependencyProperty VerticalScrollBindingProperty =
            DependencyProperty.RegisterAttached("VerticalScrollBinding", typeof(bool?), typeof(ScrollViewerExtensions));

        public static void BindVerticalOffset(this ScrollViewer scrollViewer)
        {
            if (scrollViewer.GetValue(VerticalScrollBindingProperty) != null)
            {
                return;
            }

            scrollViewer.SetValue(VerticalScrollBindingProperty, true);
            scrollViewer.ScrollChanged += (s, se) =>
            {
                if (se.VerticalChange == 0)
                {
                    return;
                }

                SetVerticalOffset(scrollViewer, se.VerticalOffset);
            };
        }

        public static ICommand GetScrollToBottom(this DependencyObject ob)
        {
            return (ICommand)ob.GetValue(ScrollToBottomProperty);
        }

        public static double GetVerticalOffset(this DependencyObject depObj)
        {
            return (double)depObj.GetValue(VerticalOffsetProperty);
        }

        public static void SetScrollToBottom(this DependencyObject ob, ICommand value)
        {
            ob.SetValue(ScrollToBottomProperty, value);
        }

        public static void SetVerticalOffset(this DependencyObject depObj, double value)
        {
            depObj.SetValue(VerticalOffsetProperty, value);
        }

        private static void OnScrollToBottomPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is ScrollViewer scrollViewer)
            {
                scrollViewer.Loaded += OnScrollViewerLoaded;
            }
        }

        private static void OnScrollViewerLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is ScrollViewer theSender)
            {
                theSender.Loaded -= OnScrollViewerLoaded;
                theSender.Unloaded += OnScrollViewerUnloaded;
                theSender.ScrollChanged += OnScrollViewerScrollChanged;
            }
        }

        private static void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            if (scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight)
            {
                var command = GetScrollToBottom(sender as ScrollViewer);
                if (command?.CanExecute(null) != true)
                {
                    return;
                }

                command.Execute(null);
            }
        }

        private static void OnScrollViewerUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is ScrollViewer theSender)
            {
                theSender.Unloaded -= OnScrollViewerUnloaded;
                theSender.ScrollChanged -= OnScrollViewerScrollChanged;
            }
        }

        private static void OnVerticalOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScrollViewer scrollViewer || double.IsNaN((double)e.NewValue))
            {
                return;
            }

            BindVerticalOffset(scrollViewer);
            scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
        }
    }
}
