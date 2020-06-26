using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MyCNBlog.Core.Models;

namespace MyCNBlog.Services.Authorization
{
    public class PostAuthorizationHandler : AuthorizationHandler<SameAuthorRequirement, Post>
    {
        private readonly IdentityOptions _identityOPtions;

        public PostAuthorizationHandler(IOptions<IdentityOptions> identityOptions)
        {
            _identityOPtions = identityOptions.Value ?? throw new System.ArgumentNullException(nameof(identityOptions));
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            SameAuthorRequirement requirement,
            Post resource)
        {
            string[] roles = AuthHelpers.GetRoles(context.User, _identityOPtions.ClaimsIdentity.RoleClaimType);

            if(roles != null)
                foreach(string role in requirement.AllowedRoles)
                {
                    if(roles.Contains(role))
                    {
                        context.Succeed(requirement);
                        return Task.CompletedTask;
                    }
                }

            string sId = context.User.FindFirstValue(
                    _identityOPtions.ClaimsIdentity.UserIdClaimType);

            if(sId != null && int.Parse(sId) == resource.AuthorId)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
