using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace MyCNBlog.Services
{
    public static class AuthHelpers
    {
        public static string[] GetRoles(ClaimsPrincipal user, string roleClaimType)
        {
            return user?.FindFirst(roleClaimType)?
                .Value.Split(',');
        }

        public static bool IsInRoles(this ClaimsPrincipal user, IEnumerable<string> roles)
        {
            if(roles == null)
                return false;
            foreach(string role in roles)
                if(!user.IsInRole(role))
                    return false;
            return true;
        }

        public static int? GetUserId(this ClaimsPrincipal user, string userIdClaimType)
        {
            string id = user.FindFirst(userIdClaimType)?.Value;
            return id == null ? null : new int?(int.Parse(id));
        }
    }
}
