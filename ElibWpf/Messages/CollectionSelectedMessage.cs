using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElibWpf.Messages
{
    public class CollectionSelectedMessage
    {
        public UserCollection Collection { get; }

        public CollectionSelectedMessage(UserCollection collection)
        {
            this.Collection = collection;
        }
    }
}
