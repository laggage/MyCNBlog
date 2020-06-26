using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MyCNBlog.Core.Models.Dtos;

namespace MyCNBlog.Services.Authorization
{
    /// <summary>
    /// <see cref="BlogAddDto.IsOpened"/>参数鉴权, 当这个参数不为null且被设置为true时
    /// 验证用户是否有管理员权限
    /// </summary>
    public class BlogPartiallyUpdateAuthorizationHandler 
        : AuthorizationHandler<RolesAuthorizationRequirement, BlogAddDto>
    {
        private readonly IdentityOptions _identityOptions;

        public BlogPartiallyUpdateAuthorizationHandler(
            IOptions<IdentityOptions> identityOptions)
        {
            _identityOptions = identityOptions.Value ?? throw new System.ArgumentNullException(nameof(identityOptions));
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            RolesAuthorizationRequirement requirement, 
            BlogAddDto resource)
        {
            // 如果涉及到修改 "IsOpened" , 则必须指定的角色, 否则直接403
            if(resource.IsOpened.HasValue)
            {
                if(context.User.IsInRoles(requirement.AllowedRoles))
                    context.Succeed(requirement);
                else
                {
                    return Task.CompletedTask;
                }
            }

            if(resource.UserId == AuthHelpers.GetUserId(
                context.User, _identityOptions.ClaimsIdentity.UserIdClaimType))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
