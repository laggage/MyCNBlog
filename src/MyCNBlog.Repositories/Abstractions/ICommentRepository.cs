using MyCNBlog.Core.Models;
using MyCNBlog.Repositories.Abstraction;

namespace MyCNBlog.Repositories.Abstractions
{
    public interface ICommentRepository : IRepository<PostComment>
    {
    }
}
