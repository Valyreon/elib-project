using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.Interfaces;

namespace Valyreon.Elib.Wpf.ViewModels.Dialogs
{
    public class ChooseAuthorDialogViewModel : DialogViewModel
    {
        private readonly Action<Author> onConfirm;
        private string filterText;
        private Author selectedItem;
        private readonly IEnumerable<Author> allAuthors;

        public ChooseAuthorDialogViewModel(IEnumerable<Author> authors, Action<Author> onConfirm)
        {
            allAuthors = authors;
            ShownAuthors = new ObservableCollection<Author>(authors);
            this.onConfirm = onConfirm;
        }

        public ICommand CancelCommand => new RelayCommand(Close);

        public ICommand DoneCommand => new RelayCommand(Done);

        public ICommand FilterChangedCommand => new RelayCommand(FilterAuthors);

        public string FilterText
        {
            get => filterText;
            set => Set(() => FilterText, ref filterText, value);
        }

        public Author SelectedItem
        {
            get => selectedItem;
            set => Set(() => SelectedItem, ref selectedItem, value);
        }

        public ObservableCollection<Author> ShownAuthors { get; set; }

        private void FilterAuthors()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShownAuthors.Clear();
                foreach (var a in allAuthors.Where(a => string.IsNullOrWhiteSpace(FilterText) || a.Name.IndexOf(FilterText, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    ShownAuthors.Add(a);
                }
            });
        }

        private void Done()
        {
            onConfirm(SelectedItem);
            Close();
        }
    }
}
