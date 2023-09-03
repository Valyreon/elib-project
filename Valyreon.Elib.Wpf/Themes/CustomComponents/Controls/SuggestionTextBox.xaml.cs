using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.Wpf.Themes.CustomComponents.Controls
{
    /// <summary>
    /// Interaction logic for SuggestionTextBox.xaml
    /// </summary>
    public partial class SuggestionTextBox : UserControl
    {
        public static DependencyProperty SuggestionsProperty = DependencyProperty.Register("Suggestions", typeof(ObservableCollection<ObservableEntity>), typeof(SuggestionTextBox));
        public static DependencyProperty TextChangedCommandProperty = DependencyProperty.Register("TextChangedCommand", typeof(ICommand), typeof(SuggestionTextBox));
        public static DependencyProperty SubmitCommandProperty = DependencyProperty.Register("SubmitCommand", typeof(ICommand), typeof(SuggestionTextBox));
        public static DependencyProperty PlaceholderProperty = DependencyProperty.Register("Placeholder", typeof(string), typeof(SuggestionTextBox));
        public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SuggestionTextBox));

        public SuggestionTextBox()
        {
            InitializeComponent();
        }

        public ObservableCollection<ObservableEntity> Suggestions
        {
            get => (ObservableCollection<ObservableEntity>)GetValue(SuggestionsProperty);
            set => SetValue(SuggestionsProperty, value);
        }

        public ICommand TextChangedCommand
        {
            get => (ICommand)GetValue(TextChangedCommandProperty);
            set => SetValue(TextChangedCommandProperty, value);
        }

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public ICommand SubmitCommand
        {
            get => (ICommand)GetValue(SubmitCommandProperty);
            set => SetValue(SubmitCommandProperty, value);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextChangedCommand?.Execute(TextBox.Text);
            Popup.IsOpen = TextBox.Text != null && TextBox.Text.Length > 1 && Suggestions.Count > 0;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Down or Key.Tab)
            {
                SuggestionListBox.SelectedIndex = 0;
                SuggestionListBox.Focus();
                e.Handled = true;
            }
            else if (e.Key is Key.Enter)
            {
                SubmitCommand?.Execute(TextBox.Text);
                TextBox.Text = string.Empty;
                Popup.IsOpen = false;
                SuggestionListBox.SelectedItem = null;
                e.Handled = true;
            }
        }

        private void SuggestionListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Tab)
            {
                var currentIndex = SuggestionListBox.SelectedIndex;
                SuggestionListBox.SelectedIndex = currentIndex == Suggestions.Count - 1 ? 0 : currentIndex + 1;
            }
            else if (e.Key is Key.Enter && SuggestionListBox.SelectedItem != null)
            {
                SubmitExisting();
            }
            e.Handled = true;
        }

        private void SuggestionListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SuggestionListBox.SelectedItem != null)
            {
                SubmitExisting();
                e.Handled = true;
            }
        }

        private void SubmitExisting()
        {
            SubmitCommand?.Execute(((UserCollection)SuggestionListBox.SelectedItem).Tag);
            TextBox.Text = string.Empty;
            TextBox.Focus();
            Popup.IsOpen = false;
            SuggestionListBox.SelectedItem = null;
        }
    }
}
