using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Valyreon.Elib.Wpf.CustomComponents
{
    public class ElibTextBox : TextBox
    {
        public static DependencyProperty PlaceholderForegroundFocusedProperty;
        public static DependencyProperty PlaceholderForegroundProperty;
        public static DependencyProperty PlaceholderProperty;

        static ElibTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ElibTextBox),
                new FrameworkPropertyMetadata(typeof(ElibTextBox)));
            PlaceholderProperty = DependencyProperty.Register("Placeholder", typeof(string), typeof(ElibTextBox));
            PlaceholderForegroundProperty = DependencyProperty.Register("PlaceholderForeground", typeof(SolidColorBrush), typeof(ElibTextBox));
            PlaceholderForegroundFocusedProperty = DependencyProperty.Register("PlaceholderForegroundFocused", typeof(SolidColorBrush), typeof(ElibTextBox));
        }

        public ElibTextBox()
        {
            AddHandler(PreviewMouseLeftButtonDownEvent,
              new MouseButtonEventHandler(SelectivelyIgnoreMouseButton), true);
            AddHandler(GotKeyboardFocusEvent,
              new RoutedEventHandler(SelectAllText), true);
            AddHandler(MouseDoubleClickEvent,
              new RoutedEventHandler(SelectAllText), true);
        }

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public SolidColorBrush PlaceholderForeground
        {
            get => (SolidColorBrush)GetValue(PlaceholderForegroundProperty);
            set => SetValue(PlaceholderForegroundProperty, value);
        }

        public SolidColorBrush PlaceholderForegroundFocused
        {
            get => (SolidColorBrush)GetValue(PlaceholderForegroundFocusedProperty);
            set => SetValue(PlaceholderForegroundFocusedProperty, value);
        }

        private static void SelectAllText(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            textBox?.SelectAll();
        }

        private static void SelectivelyIgnoreMouseButton(object sender,
                                                                 MouseButtonEventArgs e)
        {
            // Find the TextBox
            DependencyObject parent = e.OriginalSource as UIElement;
            while (parent is not null and not TextBox)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent != null)
            {
                var textBox = (TextBox)parent;
                if (!textBox.IsKeyboardFocusWithin)
                {
                    // If the text box is not yet focussed, give it the focus and
                    // stop further processing of this click event.
                    textBox.Focus();
                    e.Handled = true;
                }
            }
        }
    }
}
