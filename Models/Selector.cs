using Domain;
using System.Collections.Generic;
using System.Data.Entity;

namespace Models
{
    public class Selector<T>
        where T : DomainEntity
    {
        private readonly DbSet<T> dbSet;
        public ICollection<T> SelectedItems { get; private set; }

        public Selector(DbSet<T> dbSet)
        {
            this.dbSet = dbSet;
            SelectedItems = new List<T>();
        }

        public void AddId(int id)
        {
            SelectedItems.Add(dbSet.Find(id));
        }

        public void Add(T item)
        {
            SelectedItems.Add(item);
        }
    }
}