using System.Threading.Tasks;
using MyCNBlog.Core.Abstractions;
using MyCNBlog.Core.Models;
using MyCNBlog.Repositories.Abstraction;

namespace MyCNBlog.Repositories.Abstractions
{
    public interface IPostRepository : IRepository<Post>
    {
        Task<PaginationList<Post>> QueryAsync(
            PostQueryParameters parameters);

        Task<int> QueryCommentsCountAsync(int postId);
    }
}
