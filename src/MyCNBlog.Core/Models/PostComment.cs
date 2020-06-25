using MyCNBlog.Core.Abstractions;

namespace MyCNBlog.Core.Models
{
    /// <summary>
    /// 博文回复
    /// </summary>
    public class PostComment : Model
    {
        public int UserId { get; set; }
        
        public string Comment { get; set; }

        
        public int ReplayedPostId { get; set; }
        
        public int ReplayedUserId { get; set; }
        
        
        /// <summary>
        /// Navigation Property to Post
        /// A comment belog to a Post whick was replayed
        /// </summary>
        public virtual Post ReplayedPost { get; set; }

        /// <summary>
        /// Navigation Property to User
        /// A comment belog to a User
        /// </summary>
        public virtual BlogUser User { get; set; }
    }
}
