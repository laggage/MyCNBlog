using MyCNBlog.Core.Abstractions;
using System.Collections.Generic;

namespace MyCNBlog.Core.Models
{
    /// <summary>
    /// A Tag can belong to many posts, a post can have many tags
    /// </summary>
    public class Tag : Model
    {
        public string Name { get; set; }


        public virtual ICollection<PostTag> PostTags { get; set; }


        public Tag(string name)
        {
            Name=name;
        }

        public Tag()
        {
        }
    }

    public class PostTag : Model
    {
        public int TagId { get; set; }

        public int PostId { get; set; }

        public virtual Tag Tag { get; set; }
        
        public virtual Post Post { get; set; }
    }
}
