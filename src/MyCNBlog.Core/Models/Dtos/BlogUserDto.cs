using System;

namespace MyCNBlog.Core.Models.Dtos
{
    public class BlogUserDto
    {
        public int Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 用户邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public Sex Sex { get; set; }

        public DateTime Birth { get; set; }

        public DateTime RegiserDate { get; set; }

        /// <summary>
        /// Physcial path of avatar
        /// </summary>
        public string AvatarUrl { get; set; }

        public BlogDto Blog { get; set; }
    }
}
