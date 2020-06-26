namespace MyCNBlog.Core
{
    public class AuthorizationPolicies
    {
        public const string AdminOnlyPolicy = "AdminOnly";
        public const string BlogerPolicy = "Bloger";
        public const string PostAuthorPolicy = "PostAuthor";
        public const string PostsQueryPolicy = "PostsQuery";
        public const string OpenBlogPolicy = "AdminOnlyOpenBlogPolicy";
    }
}
