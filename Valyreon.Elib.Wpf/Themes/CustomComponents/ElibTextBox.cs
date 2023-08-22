using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Valyreon.Elib.Wpf.CustomComponents
{
    public class ElibTextBox : TextBox
    {
        public static DependencyProperty PlaceholderProperty;

        public ElibTextBox()
        {
            AddHandler(PreviewMouseLeftButtonDownEvent,
              new MouseButtonEventHandler(SelectivelyIgnoreMouseButton), true);
            AddHandler(GotKeyboardFocusEvent,
              new RoutedEventHandler(SelectAllText), true);
            AddHandler(MouseDoubleClickEvent,
              new RoutedEventHandler(SelectAllText), true);
        }

        static ElibTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ElibTextBox),
                new FrameworkPropertyMetadata(typeof(ElibTextBox)));
            PlaceholderProperty = DependencyProperty.Register("Placeholder", typeof(string), typeof(ElibTextBox));
        }

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
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

        private static void SelectAllText(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            textBox?.SelectAll();
        }
    }
}
