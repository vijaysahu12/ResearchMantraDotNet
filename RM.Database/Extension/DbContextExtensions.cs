using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace RM.Database.Extension
{
    public static class DbContextExtensions
    {

        /// <summary>
        /// The SqlQueryAsync.
        /// </summary>
        /// <typeparam name="T">.</typeparam>
        /// <param name="db">The db<see cref="DbContext"/>.</param>
        /// <param name="sql">The sql<see cref="string"/>.</param>
        /// <param name="parameters">The parameters<see cref="object[]"/>.</param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="Task{T[]}"/>.</returns>
        public static async Task<List<T>> SqlQueryFirstOrDefaultAsync<T>(this DbContext db, string sql, object[] parameters = null, CancellationToken cancellationToken = default) where T : class
        {

            try
            {
                parameters ??= new object[] { };

                using var db2 = new ContextForQueryType<T>(db.Database.GetDbConnection(), db.Database.CurrentTransaction);
                var rr = await db2.Set<T>().FromSqlRaw(sql, parameters).ToListAsync(cancellationToken);
                return rr;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// The SqlQueryAsync.
        /// </summary>
        /// <typeparam name="T">.</typeparam>
        /// <param name="db">The db<see cref="DbContext"/>.</param>
        /// <param name="sql">The sql<see cref="string"/>.</param>
        /// <param name="parameters">The parameters<see cref="object[]"/>.</param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="Task{T[]}"/>.</returns>
        public static async Task<List<T>> SqlQueryAsync<T>(this DbContext db, string sql, object[] parameters = null, CancellationToken cancellationToken = default) where T : class
        {

            try
            {

                parameters ??= new object[] { };

                if (typeof(T).GetProperties().Any())
                {
                    using var db2 = new ContextForQueryType<T>(db.Database.GetDbConnection(), db.Database.CurrentTransaction);
                    //db2.Database.SetCommandTimeout(2000);
                    var rr = await db2.Set<T>().FromSqlRaw(sql, parameters).ToListAsync(cancellationToken);
                    return rr;

                    //var db2 = new ContextForQueryType<T>(db.Database.GetDbConnection(), db.Database.CurrentTransaction);

                    ////db2.Database.SetCommandTimeout(2000);
                    //var rr = await db2.Set<T>().FromSqlRaw(sql, parameters).ToListAsync(cancellationToken);
                    //return rr;

                }
                else
                {
                    await db.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
                    return default;
                }
            }
            catch (DbException ex)
            {
                return default;
            }
        }

        /// <summary>
        /// The SqlQueryToListAsync.
        /// </summary>
        /// <typeparam name="T">.</typeparam>
        /// <param name="db">The db<see cref="DbContext"/>.</param>
        /// <param name="sql">The sql<see cref="string"/>.</param>
        /// <param name="parameters">The parameters<see cref="object[]"/>.</param>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="Task{List{T}}"/>.</returns>
        public static async Task<List<T>> SqlQueryToListAsync<T>(this DbContext db, string sql, object[] parameters = null, CancellationToken cancellationToken = default) where T : class
        {
            try
            {
                parameters ??= Array.Empty<object>();

                if (typeof(T).GetProperties().Any())
                {
                    using var db2 = new ContextForQueryType<T>(db.Database.GetDbConnection(), db.Database.CurrentTransaction);
                    db2.Database.SetCommandTimeout(db.Database.GetCommandTimeout());
                    return await db2.Set<T>().FromSqlRaw(sql, parameters).ToListAsync(cancellationToken);
                }
                else
                {
                    await db.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
                    return default;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public static async Task<T> SqlQueryFirstOrDefaultAsync2<T>(this DbContext db, string sql, object[] parameters = null, CancellationToken cancellationToken = default) where T : class
        {
            parameters ??= Array.Empty<object>();

            using var db2 = new ContextForQueryType<T>(db.Database.GetDbConnection(), db.Database.CurrentTransaction);
            var rr = await db2.Set<T>().FromSqlRaw(sql, parameters).ToListAsync(cancellationToken);
            return rr.FirstOrDefault();
        }
        /// <summary>
        /// Defines the <see cref="ContextForQueryType{T}" />.
        /// </summary>
        /// <typeparam name="T">.</typeparam>
        private class ContextForQueryType<T> : DbContext where T : class
        {
            /// <summary>
            /// Defines the connection.
            /// </summary>
            private readonly DbConnection connection;

            /// <summary>
            /// Defines the transaction.
            /// </summary>
            private readonly IDbContextTransaction transaction;

            /// <summary>
            /// Initializes a new instance of the <see cref="ContextForQueryType{T}"/> class.
            /// </summary>
            /// <param name="connection">The connection<see cref="DbConnection"/>.</param>
            /// <param name="tran">The tran<see cref="IDbContextTransaction"/>.</param>
            public ContextForQueryType(DbConnection connection, IDbContextTransaction tran)
            {
                this.connection = connection;
                transaction = tran;

                if (tran != null)
                {
                    Database.UseTransaction((tran as IInfrastructure<DbTransaction>).Instance);
                }
            }

            /// <summary>
            /// The OnConfiguring.
            /// </summary>
            /// <param name="optionsBuilder">The optionsBuilder<see cref="DbContextOptionsBuilder"/>.</param>
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                if (transaction != null)
                {
                    optionsBuilder.UseSqlServer(connection);
                }
                else
                {
                    optionsBuilder.UseSqlServer(connection, options => options.EnableRetryOnFailure());
                }
            }

            /// <summary>
            /// The OnModelCreating.
            /// </summary>
            /// <param name="modelBuilder">The modelBuilder<see cref="ModelBuilder"/>.</param>
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<T>().HasNoKey().ToView(null);
            }
        }
        public class OutputParameter<TValue>
        {
            /// <summary>
            /// Defines the _valueSet.
            /// </summary>
            private bool _valueSet = false;

            /// <summary>
            /// Defines the _value.
            /// </summary>
            public TValue _value;

            /// <summary>
            /// Gets the Value.
            /// </summary>
            public TValue Value
            {
                get
                {
                    if (!_valueSet)
                    {
                        throw new InvalidOperationException("Value not set.");
                    }

                    return _value;
                }
            }

            /// <summary>
            /// The SetValue.
            /// </summary>
            /// <param name="value">The value<see cref="object"/>.</param>
            public void SetValue(object value)
            {
                _valueSet = true;

                _value = null == value || Convert.IsDBNull(value) ? default : (TValue)value;
            }
        }
    }
}
