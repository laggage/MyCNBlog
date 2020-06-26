using MyCNBlog.Core.Models;
using MyCNBlog.Database;

namespace MyCNBlog.Repositories.Abstractions
{
    public class BlogRepository : BaseRepository<Blog>, IBlogRepository
    {
        public BlogRepository(MyCNBlogDbContext context) : base(context)
        {
        }
    }
}
