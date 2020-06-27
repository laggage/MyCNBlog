using System;
using MyCNBlog.Core.Abstractions;

namespace MyCNBlog.Core.Models
{
    /// <summary>
    /// 博文回复
    /// </summary>
    public class PostComment : Entity
    {
        public int UserId { get; set; }
        
        public string Comment { get; set; }

        
        public int RepliedPostId { get; set; }
        
        public int? RepliedUserId { get; set; }

        public int? RepliedCommentId { get; set; }

        public DateTime PostedTime { get; set; }
        
        /// <summary>
        /// Navigation Property to Post
        /// A comment belog to a Post whick was replayed
        /// </summary>
        public virtual Post RepliedPost { get; set; }

        /// <summary>
        /// Navigation Property to User
        /// A comment belog to a User
        /// </summary>
        public virtual BlogUser User { get; set; }

        public virtual BlogUser RepliedUser { get; set; }

        public virtual PostComment RepliedComment { get; set; }

        public PostComment()
        {
        }

        public PostComment(
            string comment, int userId, 
            int repliedPostId, int repliedUserId)
        {
            Comment = comment;
            UserId = userId;
            RepliedPostId = repliedPostId;
            RepliedUserId = repliedUserId;
        }
    }
}
