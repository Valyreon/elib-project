using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Valyreon.Elib.DataLayer.Extensions;
using Valyreon.Elib.DataLayer.Interfaces;
using Valyreon.Elib.Domain;
using static Dapper.SqlMapper;

namespace Valyreon.Elib.DataLayer.Repositories
{
    public abstract class RepositoryBase<T>: IRepository<T>, ICache<T> where T : ObservableEntity, new()
    {
        protected static IDictionary<int, T> Cache { get; } = new Dictionary<int, T>();
        protected IDbTransaction Transaction { get; }
        protected IDbConnection Connection => Transaction.Connection;

        public string Table { get; }
        public IEnumerable<string> ColumnNames { get; }

        protected string BaseQuery => $"SELECT * FROM {Table}";

        protected RepositoryBase(IDbTransaction transaction)
        {
            Transaction = transaction;
            ColumnNames = typeof(T).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ColumnAttribute)))
                .Select(p => p.Name);
            var tableAttribute = typeof(T).GetCustomAttributes(typeof(TableAttribute), true).Single() as TableAttribute;
            Table = tableAttribute.Name;
        }

        protected virtual Task<IEnumerable<T>> GetAllByPropertyAsync(string propertyName, object propertyValue, QueryParameters parameters = null)
        {
            var query = BaseQuery.Where($"{propertyName} = @propValue").Apply(parameters);

            var dbArgs = new DynamicParameters();
            dbArgs.Add("@propValue", propertyValue);

            return Connection.QueryAsync<T>(query, dbArgs, Transaction);
        }

        protected virtual Task<T> GetFirstByPropertyAsync(string propertyName, object propertyValue)
        {
            var query = BaseQuery.Where($"{propertyName} = @propValue");

            var dbArgs = new DynamicParameters();
            dbArgs.Add("@propValue", propertyValue);

            return Connection.QueryFirstAsync<T>(query, dbArgs, Transaction);
        }

        protected static string SelectFrom(string name)
        {
            return $"SELECT * FROM {name}";
        }

        public async Task CreateAsync(T entity)
        {
            var query = @$"INSERT INTO {Table}({string.Join(", ", ColumnNames)})
                VALUES ({string.Join(", ", ColumnNames.Select(n => "@" + n))}); SELECT last_insert_rowid() ";

            entity.Id = await Connection.ExecuteScalarAsync<int>(query, entity, Transaction);

            Cache.Add(entity.Id, entity);
        }

        public async Task CreateAsync(IEnumerable<T> entities)
        {
            foreach(var entity in entities)
            {
                await CreateAsync(entity);
            } 
        }

        public async Task<T> FindAsync(int id)
        {
            if (Cache.ContainsKey(id))
            {
                return Cache[id];
            } 

            var result = await Connection.QueryFirstAsync<T>(BaseQuery.Where("Id = @Id"),
                new { Id = id },
                Transaction);

            if(result != null)
            {
                Cache.Add(result.Id, result);
            }

            return result;
        }

        public async Task<IEnumerable<T>> FindAsync(IEnumerable<int> ids, QueryParameters parameters = null)
        {
            var query = BaseQuery.Where($"Id IN ({string.Join(',', ids)})").Apply(parameters);

            var result = await Connection.QueryAsync<T>(query, null, Transaction);

            return Cache.FilterAndUpdateCache(result);
        }

        public async Task<IEnumerable<T>> GetAllAsync(QueryParameters parameters = null)
        {
            var result = await Connection.QueryAsync<T>(BaseQuery.Apply(parameters),
                null,
                Transaction);

            return Cache.FilterAndUpdateCache(result);
        }

        public Task UpdateAsync(T entity)
        {
            var query = $"UPDATE {Table} SET {string.Join(", ", ColumnNames.Select(n => n + " = " + "@" + n))} WHERE Id = @Id";

            return Connection.ExecuteAsync(query, entity, Transaction);
        }

        public Task UpdateAsync(IEnumerable<T> entities)
        {
            return Task.WhenAll(entities.Select(UpdateAsync));
        }

        public async Task DeleteAsync(T entity)
        {
            await DeleteAsync(entity.Id);
            entity.Id = 0;
        }

        public async Task DeleteAsync(IEnumerable<T> entities)
        {
            await DeleteAsync(entities.Select(e => e.Id));
            entities.ToList().ForEach(e => e.Id = 0);
        }

        public Task DeleteAsync(int id)
        {
            Cache.Remove(id);
            return Connection.ExecuteAsync($"DELETE FROM {Table} WHERE Id = @RemoveId", new { RemoveId = id }, Transaction);
        }

        public Task DeleteAsync(IEnumerable<int> ids)
        {
            _ = ids.Select(i => Cache.Remove(i));
            return Connection.ExecuteAsync($"DELETE FROM {Table} WHERE Id IN ({string.Join(',', ids)})", null, Transaction);
        }

        public void ClearCache()
        {
            Cache.Clear();
        }

        public IEnumerable<T> GetCachedObjects()
        {
            return Cache.Values;
        }

        public bool IsCached(Func<T, bool> entity)
        {
            return Cache.Values.Any(entity);
        }
    }
}
