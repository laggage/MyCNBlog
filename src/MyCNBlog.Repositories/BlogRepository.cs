using MyCNBlog.Core.Models;
using MyCNBlog.Database;

namespace MyCNBlog.Repositories.Abstractions
{
    public class BlogRepository : AppRepository<Blog>, IBlogRepository
    {
        public BlogRepository(MyCNBlogDbContext context) : base(context)
        {
        }
    }
}
