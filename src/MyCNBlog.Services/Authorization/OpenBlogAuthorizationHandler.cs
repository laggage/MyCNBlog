using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using MyCNBlog.Core.Models.Dtos;

namespace MyCNBlog.Services.Authorization
{
    /// <summary>
    /// <see cref="BlogAddDto.IsOpened"/>参数鉴权, 当这个参数不为null且被设置为true时
    /// 验证用户是否有管理员权限
    /// </summary>
    public class OpenBlogAuthorizationHandler 
        : AuthorizationHandler<RolesAuthorizationRequirement, BlogAddDto>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            RolesAuthorizationRequirement requirement, 
            BlogAddDto resource)
        {
            if(resource.IsOpened.HasValue && resource.IsOpened.Value)
            {
                if(context.User.IsInRoles(requirement.AllowedRoles))
                    context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
