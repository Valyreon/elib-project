using System.Windows.Input;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.ViewModels.Controls;

namespace Valyreon.Elib.Wpf.ViewModels.Flyouts
{
    public class EditBookViewModel : ViewModelWithValidation
    {
        private readonly IUnitOfWorkFactory uowFactory;
        private EditBookFormViewModel editBookForm;

        public EditBookViewModel(Book book, IUnitOfWorkFactory uowFactory)
        {
            Book = book;
            this.uowFactory = uowFactory;
            EditBookForm = new EditBookFormViewModel(book, uowFactory);
            HandleRevert();
        }

        public Book Book { get; }

        public ICommand CancelButtonCommand => new RelayCommand(HandleCancel);

        public EditBookFormViewModel EditBookForm
        {
            get => editBookForm;
            set => Set(() => EditBookForm, ref editBookForm, value);
        }

        public ICommand RevertButtonCommand => new RelayCommand(HandleRevert);

        public ICommand SaveButtonCommand => new RelayCommand(HandleSave);

        private void HandleCancel()
        {
            MessengerInstance.Send(new OpenBookDetailsFlyoutMessage(Book));
        }

        private void HandleRevert()
        {
            EditBookForm = new EditBookFormViewModel(Book, uowFactory);
        }

        private void HandleSave()
        {
            if (EditBookForm.UpdateBook())
            {
                MessengerInstance.Send(new OpenBookDetailsFlyoutMessage(Book));
            }
        }
    }
}
