using System;
using System.Threading.Tasks;
using MyCNBlog.Core.Abstractions;
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



        //public override Task<PaginationList<PostComment>> QueryAsync(
        //    QueryParameters parameters)
        //{
        //    return base.Query(parameters).;
        //}
    }
}
