using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MyCNBlog.Core.Models;

namespace MyCNBlog.Services.Authorization
{
    public class PostsQueryAuthorizationHandler : AuthorizationHandler<PostsQueryAuthorizationRequirment, PostQueryParameters>
    {
        private readonly IdentityOptions _identityOptions;

        public PostsQueryAuthorizationHandler(IOptions<IdentityOptions> identityOptions)
        {
            _identityOptions = identityOptions.Value ?? throw new ArgumentNullException(nameof(identityOptions));
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PostsQueryAuthorizationRequirment requirement,
            PostQueryParameters resource)
        {
            if(resource.IsDeleted == true || resource.IsPublic == false)
            {
                // 管理员鉴权
                if(context.User.IsInRoles(requirement.AllowRoles))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }

                // 如果用户已经登录则强制将限制查询参数而不是返回403
                int? userId = context.User.GetUserId(_identityOptions.ClaimsIdentity.UserIdClaimType);
                if(resource.UserId is null && context.User.Identity.IsAuthenticated && !(userId is null))
                    resource.UserId = userId;

                if(userId.HasValue && resource.UserId == userId)
                {
                    context.Succeed(requirement);
                }
            }
            else
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
