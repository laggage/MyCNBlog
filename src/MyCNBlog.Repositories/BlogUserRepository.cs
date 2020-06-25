using System.Linq;
using System.Threading.Tasks;
using MyCNBlog.Core.Abstractions;
using MyCNBlog.Core.Models;
using MyCNBlog.Database;
using MyCNBlog.Repositories.Abstraction;

namespace MyCNBlog.Repositories
{
    public class BlogUserRepository : BaseRepository<BlogUser>, IBlogUserRepository
    {
        public BlogUserRepository(MyCNBlogDbContext context):base(context)
        {
        }

        public PaginationList<BlogUser> Query(BlogUserQueryParameters parameters)
        {
            IQueryable<BlogUser> query = _context.Set<BlogUser>().AsQueryable();
            if(!string.IsNullOrEmpty(parameters.UserName))
                query=  query.Where(x => x.NormalizedUserName.Contains(parameters.UserName));

            return query.Paging(parameters);
        }

        public Task<PaginationList<BlogUser>> QueryAsync(BlogUserQueryParameters parameters)
        {
            return Task.Run(() => Query(parameters));
        }

        /// <summary>
        /// 实现软删除
        /// </summary>
        /// <param name="entity"></param>
        private void SoftDelete(BlogUser entity)
        {
            BlogUser user = _context.Set<BlogUser>().Find(entity.Id);
            user.IsDeleted = true;
            _context.Set<BlogUser>().Update(user);
        }

        public override void Delete(BlogUser entity, bool softDelete)
        {
            if(softDelete)
                SoftDelete(entity);
            else
                Delete(entity);
        }
    }
}
