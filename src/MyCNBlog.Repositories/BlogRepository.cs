using System.Linq;
using Microsoft.EntityFrameworkCore;
using MyCNBlog.Core.Models;
using MyCNBlog.Database;

namespace MyCNBlog.Repositories.Abstractions
{
    public class BlogRepository : AppRepository<Blog>, IBlogRepository
    {
        public BlogRepository(MyCNBlogDbContext context) : base(context)
        {
        }

        public override IQueryable<Blog> Query()
        {
            return _context.Set<Blog>().Include(x => x.User);
        }
    }
}
