namespace MyCNBlog.Core.Models.Dtos
{
    public class UserLoginDto
    {
        /// <summary>
        /// 用户名/邮箱
        /// </summary>
        public string UserName { get; set; }
        
        /// <summary>
        /// 加密后的用户密码
        /// </summary>
        public string SecurePassword { get; set; }

        /// <summary>
        /// 使用的客户端
        /// </summary>
        public string Client { get; set; }

        public bool RememberMe { get; set; }
    }
}
