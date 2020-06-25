using Microsoft.AspNetCore.Http;
using System;

namespace MyCNBlog.Core.Models.Dtos
{
    /// <summary>
    /// 用户注册数据模型
    /// </summary>
    public class UserRegisterDto
    {
        /// <summary>
        /// 用户名; 必填, 字符上限: 32
        /// </summary>
        public string UserName { get; set; }
        
        /// <summary>
        /// RSA加密后的密码; 
        /// </summary>
        public string SecurePassword { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime Birth { get; set; }
        
        public string Email { get; set; }

        /// <summary>
        /// 用户头像; MaxSize: 2M
        /// </summary>
        public IFormFile Avatar { get; set; }
    }
}
