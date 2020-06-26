using System;
using MyCNBlog.Core.Models;
using MyCNBlog.Database;
using MyCNBlog.Repositories.Abstractions;

namespace MyCNBlog.Repositories
{
    public class TagRepository : BaseRepository<Tag>, ITagRepository
    {
        public TagRepository(MyCNBlogDbContext context) : base(context)
        {
        }

        public override void Delete(Tag entity, bool softDelete)
        {
            throw new NotImplementedException();
        }
    }
}
