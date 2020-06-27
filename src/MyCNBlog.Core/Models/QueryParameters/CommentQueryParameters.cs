using MyCNBlog.Core.Abstractions;

namespace MyCNBlog.Core.Models
{
    public class CommentQueryParameters : QueryParameters
    {
        /// <summary>
        /// "RepliedCommentId" "RepliedUserId" "RepliedPostId" 这三个参数
        /// 普通用户至少填写一个, 否则返回403;
        /// </summary>
        public int? RepliedPostId { get; set; }

        /// <summary>
        /// 某个用户发表的评论
        /// 此参数只对管理员权限或者资源拥有者有效
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// 回复某个用户的评论;
        /// "RepliedCommentId" "RepliedUserId" "RepliedPostId" 这三个参数
        /// 普通用户至少填写一个, 否则返回403;
        /// </summary>
        public int? RepliedUserId { get; set; }

        /// <summary>
        /// "RepliedCommentId" "RepliedUserId" "RepliedPostId" 这三个参数
        /// 普通用户至少填写一个, 否则返回403;
        /// </summary>
        public int? RepliedCommentId { get; set; }
    }
}
