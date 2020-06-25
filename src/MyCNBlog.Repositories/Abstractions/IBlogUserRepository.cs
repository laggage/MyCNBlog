using System.Threading.Tasks;
using MyCNBlog.Core.Abstractions;
using MyCNBlog.Core.Models;

namespace MyCNBlog.Repositories.Abstraction
{
    public interface IBlogUserRepository : IRepository<BlogUser>
    {
        PaginationList<BlogUser> Query(BlogUserQueryParameters parameters);
        Task<PaginationList<BlogUser>> QueryAsync(BlogUserQueryParameters parameters);
    }
}
