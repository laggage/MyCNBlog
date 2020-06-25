using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyCNBlog.Core.Abstractions;
using MyCNBlog.Repositories.Abstraction;

namespace MyCNBlog.Repositories
{
    public static class QueryExtensions
    {
        public static PaginationList<TEntity> Paging<TEntity>(
            this IQueryable<TEntity> source, 
            QueryParameters parameters)
        {
            if(source is null)
                throw new ArgumentNullException(nameof(source));
            if(parameters is null)
                throw new ArgumentNullException(nameof(parameters));

            IQueryable<TEntity> query = source.Skip(
                (parameters.PageIndex-1) * parameters.PageSize)
                .Take(parameters.PageSize);
            return new PaginationList<TEntity>(
                parameters.PageIndex, parameters.PageSize, source.Count(),
                query.ToList());
        }

        public static Task<PaginationList<TEntity>> PagingAsync<TEntity>(
            this IQueryable<TEntity> source,
            QueryParameters parameters)
        {
            return Task.Run(() => source.Paging(parameters));
        }
    }

    public abstract class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly DbContext _context;

        public BaseRepository(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public PaginationList<TEntity> Query(QueryParameters parameters)
        {
            return _context.Set<TEntity>().AsQueryable().Paging(parameters);
        }

        public TEntity QueryById(int id, string fields = null)
        {
            return _context.Set<TEntity>().Find(id);
        }

        public Task<PaginationList<TEntity>> QueryAsync(QueryParameters parameters)
        {
            return Task.Run(() => Query(parameters));
        }

        public Task<TEntity> QueryByIdAsync(int id, string fields = null)
        {
            return Task.Run(() => QueryById(id, fields));
        }

        #region Delete

        /// <summary>
        /// 其他Delete方法, 最终都回调用到这里, 所以让这个方法可以被派生类重写
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Delete(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
        }

        public void DeleteById(int id)
        {
            Delete(QueryById(id));
        }

        public void Delete(IEnumerable<TEntity> entities)
        {
            foreach(TEntity entity in entities)
                Delete(entity);
        }

        public void DeleteById(IEnumerable<int> ids)
        {
            foreach(int id in ids)
            {
                TEntity entity = QueryById(id);
                if(entity != null)
                    Delete(entity);
            }
        }

        #endregion


        #region Soft Delete supported

        /// <summary>
        /// 软删除的实现留到派生类
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="softDelete"></param>
        public abstract void Delete(TEntity entity, bool softDelete);

        public void DeleteById(int id, bool softDelete)
        {
            Delete(QueryById(id), softDelete);
        }

        public void Delete(IEnumerable<TEntity> entities, bool softDelete)
        {
            foreach(TEntity entity in entities)
                Delete(entity, softDelete);
        }

        public void DeleteById(IEnumerable<int> ids, bool softDelete)
        {
            foreach(int id in ids)
            {
                DeleteById(id);
            }
        }

        #endregion

        public void Add(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
        }

        public void Add(IEnumerable<TEntity> entities)
        {
            foreach(TEntity entity in entities)
                Add(entity);
        }

        public void Update(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
        }

        public void Update(IEnumerable<TEntity> entities)
        {
            foreach(TEntity entity in entities)
                Update(entity);
        }

        
    }
}
