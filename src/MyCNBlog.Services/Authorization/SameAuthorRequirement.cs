using Microsoft.AspNetCore.Authorization;

namespace MyCNBlog.Services.Authorization
{
    public class SameAuthorRequirement:IAuthorizationRequirement
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="allowedRoles">允许一些角色跳过相同作者这个条件</param>
        public SameAuthorRequirement(params string[] allowedRoles)
        {
            AllowedRoles = allowedRoles;
        }

        public string[] AllowedRoles { get; }
    }
}
