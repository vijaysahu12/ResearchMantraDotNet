using RM.Model.MongoDbCollection;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RM.Database.MongoDbContext
{
    public interface IMongoRepository<T> where T : class
    {
        Task<T> GetByIdAsync(string id);
        Task<List<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task ReplaceOneAsync(Expression<Func<T, bool>> predicate, T document);

        Task AddInBulkAsync(List<T> entity);
        Task UpdateAsync(string id, T entity);
        Task<UpdateResult> UpdateAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> update);
        Task<UpdateResult> UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update);
        Task InsertOneAsync(T document);

        Task DeleteAsync(string id);
        Task DeleteInBulkAsync(Expression<Func<T, bool>> filter);
        Task<List<T>> FindAsync(FilterDefinition<T> filter);
        Task<T> FindOneAsync(Expression<Func<T, bool>> predicate);
        Task<(List<T> Items, long TotalCount)> GetPaginatedAsyncWithTotalCount(FilterDefinition<T> filter, Expression<Func<T, object>> sortBy, int pageNumber, int pageSize, bool isDescending = true);
        Task<List<T>> GetPaginatedAsync(Expression<Func<T, bool>> filter, Expression<Func<T, object>> sortBy, int pageNumber, int pageSize, bool isDescending = true);

        Task<long> GetTotalCounts(FilterDefinition<T> filter);
    }

    public class MongoRepository<T> : IMongoRepository<T> where T : class
    {
        private readonly IMongoCollection<T> _collection;

        public MongoRepository(IMongoDatabase database)
        {
            string collectionName = typeof(T).Name; // Automatically get collection name from class

            _collection = database.GetCollection<T>(collectionName);
        }

        public async Task<T> GetByIdAsync(string id)
        {
            return await _collection.Find(Builders<T>.Filter.Eq("_id", ObjectId.Parse(id))).FirstOrDefaultAsync();
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }
        public async Task InsertOneAsync(T document)
        {
            await _collection.InsertOneAsync(document);
        }



        public async Task AddAsync(T entity)
        {
            await _collection.InsertOneAsync(entity);
        }

        public async Task ReplaceOneAsync(Expression<Func<T, bool>> predicate, T document)
        {
            await _collection.ReplaceOneAsync(predicate, document);
        }


        public async Task AddInBulkAsync(List<T> entity)
        {
            await _collection.InsertManyAsync(entity);
        }

        public async Task UpdateAsync(string id, T entity)
        {
            await _collection.ReplaceOneAsync(Builders<T>.Filter.Eq("_id", ObjectId.Parse(id)), entity);
        }
        public async Task<UpdateResult> UpdateAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> update)
        {
            return await _collection.UpdateOneAsync(filter, update);
        }

        public async Task<UpdateResult> UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update)
        {
            return await _collection.UpdateOneAsync(filter, update);
        }


        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(Builders<T>.Filter.Eq("_id", ObjectId.Parse(id)));
        }

        public async Task<List<T>> FindAsync(FilterDefinition<T> filter)
        {
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<T> FindOneAsync(Expression<Func<T, bool>> predicate)
        {
            return await _collection.Find(predicate).FirstOrDefaultAsync();
        }

        public async Task<List<T>> GetPaginatedAsync(Expression<Func<T, bool>> filter, Expression<Func<T, object>> sortBy, int pageNumber, int pageSize, bool isDescending = true)
        {
            var query = _collection.Find(filter);

            query = isDescending
                ? query.SortByDescending(sortBy)
                : query.SortBy(sortBy);

            return await query
                .Skip((pageNumber - 1) * pageSize) // Skip records based on page number
                .Limit(pageSize) // Limit results per page
                .ToListAsync();
        }

        public async Task<(List<T> Items, long TotalCount)> GetPaginatedAsyncWithTotalCount(FilterDefinition<T> filter, Expression<Func<T, object>> sortBy, int pageNumber, int pageSize, bool isDescending = true)
        {
            var query = _collection.Find(filter);

            // Get the total count of documents matching the filter BEFORE applying pagination
            long totalCount = await query.CountDocumentsAsync();

            // Apply sorting and pagination
            query = isDescending
                ? query.SortByDescending(sortBy)
                : query.SortBy(sortBy);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
            .ToListAsync();
            return (items, totalCount);
        }
        public async Task DeleteInBulkAsync(Expression<Func<T, bool>> filter)
        {
            await _collection.DeleteManyAsync(filter);
        }

        public async Task<long> GetTotalCounts(FilterDefinition<T> filter)
        {
            return await _collection.CountDocumentsAsync(filter);
        }
    }
}
