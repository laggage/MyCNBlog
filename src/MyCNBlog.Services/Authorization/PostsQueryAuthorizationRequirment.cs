using Microsoft.AspNetCore.Authorization;

namespace MyCNBlog.Services.Authorization
{
    public class PostsQueryAuthorizationRequirment : IAuthorizationRequirement
    {
        public string[] AllowRoles { get; set; }
    }
}
