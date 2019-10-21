using DataLayer;
using Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Selector<T>
        where T : Entity
    {
        private readonly DbSet<T> _dbSet;
        public ICollection<T> SelectedItems { get; private set; }

        public Selector(DbSet<T> dbSet)
        {
            this._dbSet = dbSet;
            SelectedItems = new List<T>();
        }

        public void AddId(int id)
        {
            SelectedItems.Add(_dbSet.Find(id));
        }

        public void Add(T item)
        {
            SelectedItems.Add(item);
        }
        
    }
}
