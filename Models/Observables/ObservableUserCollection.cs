using Domain;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Observables
{
    public class ObservableUserCollection : ObservableObject
    {
        private readonly UserCollection collection;

        public UserCollection Collection { get => collection; }

        public ObservableUserCollection(UserCollection collection)
        {
            if (collection is null)
                throw new ArgumentException("Constructor argument can't be null.");
            this.collection = collection;
        }

        public string Tag
        {
            get => collection.Tag;
            set
            {
                this.collection.Tag = value;
                RaisePropertyChanged(() => Tag);
            }
        }

        public int Id { get => collection.Id; }
    }
}
