using System;
using Microsoft.AspNetCore.Authorization;

namespace MyCNBlog.Services.Authorization
{
    public class ResourceOwnerAuthorizationRequirement<TKey> : IAuthorizationRequirement
         where TKey : IEquatable<TKey>
    {
        public TKey OwnerId { get; }

        public ResourceOwnerAuthorizationRequirement(TKey ownerId)
        {
            OwnerId = ownerId;
        }
    }
}
