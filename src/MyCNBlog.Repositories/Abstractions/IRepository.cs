using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MyCNBlog.Core.Abstractions;

namespace MyCNBlog.Repositories.Abstraction
{
    /// <summary>
    /// 仓储模式, 参考 Blog.Core, 添加翻页功能
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TKey">实体主键类型</typeparam>
    public interface IRepository<TEntity, TKey>
    {
        IQueryable<TEntity> Query();
        PaginationList<TEntity> Query(QueryParameters parameters);
        IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predict);
        TEntity QueryById(TKey id);
        Task<PaginationList<TEntity>> QueryAsync(QueryParameters parameters);
        Task<IQueryable<TEntity>> QueryAsync(Expression<Func<TEntity, bool>> predict);
        Task<TEntity> QueryByIdAsync(TKey id);


        void Delete(TEntity entity);
        void DeleteById(TKey id);
        void Delete(IEnumerable<TEntity> entities);
        void DeleteById(IEnumerable<TKey> ids);

        void Add(TEntity entity);
        void Add(IEnumerable<TEntity> entities);

        void Update(TEntity entity);
        void Update(IEnumerable<TEntity> entities);
        //void Update(TKey id, TEntity entity);
    }

    public interface IRepository<TEntity> : IRepository<TEntity, int>
    {
        //TEntity QueryNotDeletedAsync(int id);
        //PaginationList<TEntity> QueryNotDeletedAsync(QueryParameters parameters);

        void Delete(TEntity entity, bool softDelete);
        void DeleteById(int id, bool softDelete);
        void Delete(IEnumerable<TEntity> entities, bool softDelete);
        void DeleteById(IEnumerable<int> ids, bool softDelete);
    }
}
