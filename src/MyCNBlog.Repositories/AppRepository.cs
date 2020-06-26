using MyCNBlog.Database;

namespace MyCNBlog.Repositories
{
    public class AppRepository<TEntity> : BaseRepository<TEntity>
        where TEntity : class
    {
        public AppRepository(MyCNBlogDbContext context) : base(context)
        {
        }
    }
}
