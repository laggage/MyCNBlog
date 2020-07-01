using System.Threading.Tasks;
using MyCNBlog.Core.Models;
using MyCNBlog.Repositories.Abstraction;

namespace MyCNBlog.Repositories.Abstractions
{
    public interface IBlogRepository : IRepository<Blog>
    {
        /// <summary>
        /// 查询用户发表的所有评论总数
        /// </summary>
        /// <param name="blogUserId">用户Id</param>
        /// <returns></returns>
        Task<int> QueryTotalPostedCommentCountAsync(int blogUserId);
    }
}
