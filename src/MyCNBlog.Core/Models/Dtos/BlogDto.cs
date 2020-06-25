namespace MyCNBlog.Core.Models.Dtos
{
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
    }
}
