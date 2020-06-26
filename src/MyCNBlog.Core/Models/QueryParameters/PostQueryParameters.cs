using MyCNBlog.Core.Abstractions;

namespace MyCNBlog.Core.Models
{
    public class PostQueryParameters: QueryParameters
    {
        public string Title { get; set; }
        /// <summary>
        /// 此参数, 只对博文作者和管理员有效
        /// </summary>
        public bool? IsDeleted { get; set; }
        public int? UserId { get; set; }
        public int? BlogId { get; set; }

        /// <summary>
        /// 此参数, 只对博文作者和管理员有效
        /// </summary>
        public bool? IsPublic { get; set; }

        public PostQueryParameters()
        {
            IsPublic = true;
            IsDeleted = false;
        }
    }
}
