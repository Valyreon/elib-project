using System;
using Domain;
using MVVMLibrary;

namespace Models.Observables
{
    public class ObservableUserCollection : ObservableObject
    {
        public ObservableUserCollection(UserCollection collection)
        {
            if (collection is null)
            {
                throw new ArgumentException("Constructor argument can't be null.");
            }

            this.Collection = collection;
        }

        public UserCollection Collection { get; }

        public int Id => this.Collection.Id;

        public string Tag
        {
            get => this.Collection.Tag;
            set
            {
                this.Collection.Tag = value;
                this.RaisePropertyChanged(() => this.Tag);
            }
        }
    }
}