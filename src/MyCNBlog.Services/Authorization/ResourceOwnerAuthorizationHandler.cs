using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MyCNBlog.Services.Authorization
{
    public class ResourceOwnerAuthorizationHandler<TKey> : AuthorizationHandler<ResourceOwnerAuthorizationRequirement<TKey>>
        where TKey: IEquatable<TKey>
    {
        private readonly IdentityOptions _identityOptions;

        public ResourceOwnerAuthorizationHandler(IOptions<IdentityOptions> identityOptions)
        {
            _identityOptions = identityOptions.Value ?? throw new ArgumentNullException(nameof(identityOptions));
        }
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            ResourceOwnerAuthorizationRequirement<TKey> requirement)
        {
            int userId = AuthHelpers.GetUserId(context.User, _identityOptions.ClaimsIdentity.UserIdClaimType).Value;
            
            if(requirement.OwnerId.Equals(userId))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
