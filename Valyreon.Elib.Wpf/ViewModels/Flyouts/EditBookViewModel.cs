using System.Collections.Generic;
using System.Windows.Input;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;
using Valyreon.Elib.Mvvm;
using Valyreon.Elib.Wpf.Messages;
using Valyreon.Elib.Wpf.Models;
using Valyreon.Elib.Wpf.ViewModels.Controls;

namespace Valyreon.Elib.Wpf.ViewModels.Flyouts
{
    public class EditBookViewModel : ViewModelWithValidation
    {
        private readonly ApplicationProperties applicationProperties;
        private readonly LinkedListNode<Book> node;
        private readonly IUnitOfWorkFactory uowFactory;
        private EditBookFormViewModel editBookForm;

        public EditBookViewModel(LinkedListNode<Book> node, IUnitOfWorkFactory uowFactory, ApplicationProperties applicationProperties)
        {
            this.node = node;
            Book = node.Value;
            this.uowFactory = uowFactory;
            this.applicationProperties = applicationProperties;
            EditBookForm = new EditBookFormViewModel(Book, uowFactory);
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
            var details = new BookDetailsViewModel(node, applicationProperties, uowFactory);
            MessengerInstance.Send(new OpenFlyoutMessage(details));
        }

        private void HandleRevert()
        {
            EditBookForm = new EditBookFormViewModel(Book, uowFactory);
        }

        private void HandleSave()
        {
            if (EditBookForm.UpdateBook())
            {
                var details = new BookDetailsViewModel(node, applicationProperties, uowFactory);
                MessengerInstance.Send(new OpenFlyoutMessage(details));
            }
        }
    }
}
