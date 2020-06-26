using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyCNBlog.Core.Abstractions;
using MyCNBlog.Core.Models;
using MyCNBlog.Database;
using MyCNBlog.Repositories.Abstractions;

namespace MyCNBlog.Repositories
{
    public class PostRepository : BaseRepository<Post>, IPostRepository
    {
        public PostRepository(MyCNBlogDbContext context) : base(context)
        {
        }

        public override void Delete(Post entity, bool softDelete)
        {
            HardOrSoftDelete(entity, softDelete);
        }

        public override IQueryable<Post> Query()
        {
            return base.Query().AsQueryable()
                .Include(x => x.PostTags)
                .ThenInclude(x => x.Tag);
        }

        public override Post QueryById(int id)
        {
            return Query()
                .FirstOrDefault(x => x.Id == id);
        }

        public async Task<PaginationList<Post>> QueryAsync(
            PostQueryParameters parameters)
        {
            IQueryable<Post> query = _context.Set<Post>().AsQueryable()
                .Include(x => x.PostTags)
                .ThenInclude(x => x.Tag)
                .Include(x => x.Author)
                .ThenInclude(x => x.Blog);
            IQueryable<Post> topMost = query.Where(x => x.IsTopMost);
            if(parameters.BlogId != null)
            {
                query = query.Include(x => x.Blog)
                    .Where(x => x.Blog.Id == parameters.BlogId);
            }
            if(parameters.IsDeleted != null)
            {
                query = query.Where(
                    p => p.IsDeleted == parameters.IsDeleted);
            }
            if(parameters.IsPublic != null)
            {
                query = query.Where(
                    p => p.IsPublic == parameters.IsPublic);
            }
            if(parameters.UserId != null)
            {
                query = query.Where(p => p.AuthorId == parameters.UserId);
            }
            if(!string.IsNullOrEmpty(parameters.Title))
            {
                query = query.Where(p => p.Title.Contains(parameters.Title));
            }
            PaginationList<Post> result = await query.PagingAsync(parameters);
            if(topMost.Any())
                result.InsertRange(0, topMost);
            return result;
        }
    }
}
