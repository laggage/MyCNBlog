using System.Linq;
using System.Threading.Tasks;
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

        public Task<int> QueryTotalPostedCommentCountAsync(int blogUserId)
        {
            return _context.Set<PostComment>().CountAsync(x => x.UserId == blogUserId);
        }
    }
}
