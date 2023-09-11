using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Valyreon.Elib.Domain;

namespace Valyreon.Elib.Wpf.Themes.CustomComponents.Controls
{
    /// <summary>
    /// Interaction logic for ChipControl.xaml
    /// </summary>
    public partial class ChipControl : UserControl
    {
        public static DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(ChipControl));
        public static DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(ChipControl));
        public static DependencyProperty MarkNewObjectsProperty = DependencyProperty.Register("MarkNewObjects", typeof(bool), typeof(ChipControl));
        public static DependencyProperty ObjectProperty = DependencyProperty.Register("Object", typeof(object), typeof(ChipControl));
        public static DependencyProperty RemoveCommandProperty = DependencyProperty.Register("RemoveCommand", typeof(ICommand), typeof(ChipControl));
        public static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(ChipControl));
        private bool isNewObservableEntity = false;

        public ChipControl()
        {
            InitializeComponent();
        }

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public bool MarkNewObjects
        {
            get => (bool)GetValue(MarkNewObjectsProperty);
            set => SetValue(MarkNewObjectsProperty, value);
        }

        public object Object
        {
            get => (object)GetValue(ObjectProperty);
            set => SetValue(ObjectProperty, value);
        }

        public ICommand RemoveCommand
        {
            get => (ICommand)GetValue(RemoveCommandProperty);
            set => SetValue(RemoveCommandProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private void ChipBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Command == null || isNewObservableEntity)
            {
                return;
            }

            ChipBorder.BorderBrush = System.Windows.Media.Brushes.CornflowerBlue;
            Cursor = Cursors.Hand;
        }

        private void ChipBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Command == null || isNewObservableEntity)
            {
                return;
            }

            ChipBorder.BorderBrush = System.Windows.Media.Brushes.WhiteSmoke;
        }

        private void ChipBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Command?.Execute(Object);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (MarkNewObjects && Object is ObservableEntity entity && entity.Id == 0)
            {
                isNewObservableEntity = true;
                ChipBorder.BorderBrush = System.Windows.Media.Brushes.CornflowerBlue;
            }
        }
    }
}
