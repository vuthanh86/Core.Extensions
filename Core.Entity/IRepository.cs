using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Core.Data;
using Microsoft.EntityFrameworkCore;


namespace Core.Entity
{
    public interface IRepository<T, in TKey> where T : class
    {
        #region Open / Use Connection

        //IRepositoryConnection<T> OpenConnection();

        //IRepositoryConnection<T> UseConnection<TOther>(IRepositoryConnection<TOther> connection) where TOther : class;
        DbContext OpenConnection();

        #endregion Open / Use Connection

        #region Count

        int Count(Expression<Func<T, bool>> predicate = null);

        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);

        long LongCount(Expression<Func<T, bool>> predicate = null);

        Task<long> LongCountAsync(Expression<Func<T, bool>> predicate = null);

        #endregion Count

        #region Delete

        void Delete(T entity);

        Task DeleteAsync(T entity);

        void DeleteMany(Expression<Func<T, bool>> filterExpression);

        void DeleteMany(IEnumerable<T> items);

        Task DeleteManyAsync(Expression<Func<T, bool>> filterExpression);

        Task DeleteManyAsync(IEnumerable<T> items);

        #endregion Delete

        #region Find

        IEnumerable<T> Find(Expression<Func<T, bool>> filterExpression, FindOptions<T> findOptions = null, params Expression<Func<T, dynamic>>[] includes);

        IEnumerable<TProjection> Find<TProjection>(Expression<Func<T, bool>> filterExpression, Expression<Func<T, TProjection>> projection, FindOptions<T> findOptions = null);

        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filterExpression, FindOptions<T> findOptions = null);

        Task<IEnumerable<TProjection>> FindAsync<TProjection>(Expression<Func<T, bool>> filterExpression, Expression<Func<T, TProjection>> projection, FindOptions<T> findOptions = null);

        T FindOne(Expression<Func<T, bool>> filterExpression, FindOptions<T> findOptions = null, params Expression<Func<T, dynamic>>[] includes);

        Task<T> FindOneAsync(Expression<Func<T, bool>> filterExpression, FindOptions<T> findOptions = null);

        Task<TProjection> FindOneAsync<TProjection>(Expression<Func<T, bool>> filterExpression, Expression<Func<T, TProjection>> projection);

        T GetById(TKey id);

        Task<T> GetByIdAsync(TKey id);

        #endregion Fine

        #region Insert

        void Insert(T entity);

        Task InsertAsync(T entity);

        void InsertMany(IEnumerable<T> items);

        Task InsertManyAsync(IEnumerable<T> items);

        #endregion Insert

        #region Update

        void Update(T entity);

        Task UpdateAsync(T entity);

        void UpdateMany(IEnumerable<T> items);

        void UpdateMany(Expression<Func<T, bool>> filterExpression, Expression<Func<T, T>> updateExpression);

        Task UpdateManyAsync(IEnumerable<T> items);

        Task UpdateManyAsync(Expression<Func<T, bool>> filterExpression, Expression<Func<T, T>> updateExpression);

        #endregion Update
    }
}
