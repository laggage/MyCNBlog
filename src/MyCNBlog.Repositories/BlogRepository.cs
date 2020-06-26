using System;
using System.Collections.Generic;
using System.Text;
using MyCNBlog.Core.Models;
using MyCNBlog.Database;

namespace MyCNBlog.Repositories.Abstractions
{
    public class BlogRepository : BaseRepository<Blog>, IBlogRepository
    {
        public BlogRepository(MyCNBlogDbContext context) : base(context)
        {
        }

        public override void Delete(Blog entity, bool softDelete)
        {
            HardOrSoftDelete(entity, softDelete);
        }
    }
}
