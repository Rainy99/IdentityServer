using Devzhou.Data.Entity;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Devzhou.Data.Repository
{
    public class RepositoryBase<T> where T:IEntityBase
    {
        private readonly IMongoCollection<T> _collection;

        public RepositoryBase(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<T>(typeof(T).Name);
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter)
        {
            var data = await _collection.FindAsync(filter);
            return data.ToEnumerable();
        }

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> filter)
        {
            var data = await FindAsync(filter);
            return data.FirstOrDefault();
        }

        public IEnumerable<T> FindAll()
        {
            return _collection.AsQueryable();
        }

        public Task InsertAsync(T entity)
        {
            return _collection.InsertOneAsync(entity);
        }

        public Task InsertAsync(IEnumerable<T> entities)
        {
            return _collection.InsertManyAsync(entities);
        }
    }
}
