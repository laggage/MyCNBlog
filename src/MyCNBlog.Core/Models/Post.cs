using MyCNBlog.Core.Abstractions;
using System;
using System.Collections.Generic;

namespace MyCNBlog.Core.Models
{
    /// <summary>
    /// 博文
    /// </summary>
    public class Post : Model
    {
        public int BlogId { get; set; }
        public int AuthorId { get; set; }
        
        /// <summary>
        /// Max size: 512
        /// </summary>
        public string Description { get; set; }
        
        public DateTime LastModified { get; set; }
        
        public DateTime CreateDate { get; set; }
        
        public bool IsPublic { get; set; }

        public string Title { get; set; }

        /// <summary>
        /// Post Content will be save to a physical file instead of a database;
        /// This property represent a physical path where the post content will be save
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Navigation property to comments
        /// A post has many comments
        /// </summary>
        public virtual ICollection<PostComment> Comments { get; set; }

        /// <summary>
        /// Navigation property to Blog
        /// A Post belong to a Blog
        /// </summary>
        public virtual Blog Blog { get; set; }

        public virtual ICollection<PostTag> PostTags { get; set; }

        public virtual BlogUser Author { get; set; }
    }
}
