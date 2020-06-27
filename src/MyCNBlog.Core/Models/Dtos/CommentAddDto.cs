using System;

namespace MyCNBlog.Core.Models.Dtos
{
    public class CommentUserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public Sex Sex { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsDeleted { get; set; }
    }

    /// <summary>
    /// 用户评论
    /// </summary>
    public class CommentDto
    {
        public int Id { get; set; }

        /// <summary>
        /// 回复的博文Id
        /// </summary>
        public int RepliedPostId { get; set; }

        /// <summary>
        /// 回复的用户
        /// </summary>
        public CommentUserDto RepliedUser { get; set; }

        /// <summary>
        /// 评论内容
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// 发表评论的用户
        /// </summary>
        public CommentUserDto User { get; set; }

        /// <summary>
        /// 评论发表时间
        /// </summary>
        public DateTime PostedTime { get; set; }

        /// <summary>
        /// 评论回复的评论
        /// </summary>
        public int? RepliedCommentId { get; set; }

        public int RepliedCount { get; set; }
    }

    public class CommentAddDto
    {
        /// <summary>
        /// 博文内容
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// 回复的博文, 不可以为 null
        /// </summary>
        public int? RepliedPostId { get; set; }

        /// <summary>
        /// 回复的用户, 可以为null
        /// </summary>
        public int? RepliededUserId { get; set; }

        /// <summary>
        /// 回复某条评论, 设置此参数时必须同时设置<see cref="RepliededUserId"/>
        /// 否则返回404
        /// </summary>
        public int? RepliedCommentId { get; set; }
    }
}
