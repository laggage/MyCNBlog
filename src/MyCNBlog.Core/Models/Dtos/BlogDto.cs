namespace MyCNBlog.Core.Models.Dtos
{
    /// <summary>
    /// 代表一个博客对象, 每个用户都可以有且之多有一个博客
    /// </summary>
    public class BlogDto
    {
        public int Id { get; set; }

        /// <summary>
        /// 博客功能是否开通, default: false
        /// </summary>
        public bool IsOpened { get; set; }

        /// <summary>
        /// 博客签名
        /// </summary>
        public string Sign { get; set; }

        public int UserId { get; set; }

        /// <summary>
        /// 博主
        /// </summary>
        public BlogUserDto Bloger { get; set; }

        public int TotalPostCount { get; set; }
    }
}
