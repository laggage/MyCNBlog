using System.Linq;
using Microsoft.EntityFrameworkCore;
using MyCNBlog.Core.Models;
using MyCNBlog.Database;
using MyCNBlog.Repositories.Abstractions;

namespace MyCNBlog.Repositories
{
    public class PostCommentRepository : AppRepository<PostComment>, ICommentRepository
    {
        public PostCommentRepository(MyCNBlogDbContext context) : base(context)
        {
        }

        public override IQueryable<PostComment> Query()
        {
            return base.Query()
                .Include(x => x.User)
                .Include(x => x.RepliedPost);
        }
    }
}
