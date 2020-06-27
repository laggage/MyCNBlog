using System;
using MyCNBlog.Core.Models;
using MyCNBlog.Database;
using MyCNBlog.Repositories.Abstractions;

namespace MyCNBlog.Repositories
{
    public class TagRepository : AppRepository<Tag>, ITagRepository
    {
        public TagRepository(MyCNBlogDbContext context) : base(context)
        {
        }
    }
}
