using MyCNBlog.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCNBlog.Core.Models
{
    /// <summary>
    /// 博文
    /// </summary>
    public class Post : Entity
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

        public bool IsTopMost { get; set; }

        /// <summary>
        /// 置顶顺序
        /// </summary>
        public int TopMostOrder { get; set; }

        /// <summary>
        /// Navigation property to comments
        /// A post has many comments
        /// </summary>
        public virtual ICollection<PostComment> Comments { get; set; }

        private Blog _blog;
        /// <summary>
        /// Navigation property to Blog
        /// A Post belong to a Blog
        /// </summary>
        public virtual Blog Blog
        {
            get => _blog;
            set
            {
                _blog = value;
                BlogId = _blog.Id;
            }
        }

        public virtual ICollection<PostTag> PostTags { get; set; }

        public Tag[] Tags => PostTags?.Where(x => x.PostId == Id)
            .Select(x => x.Tag)
            .ToArray();

        public virtual BlogUser Author { get; set; }

        public Post()
        {
            CreateDate = DateTime.Now;
            LastModified = DateTime.Now;
            PostTags = new List<PostTag>();
            Comments = new List<PostComment>();
        }

        public override string ToString()
        {
            return $"{Title} - {Description.Take(128)}";
        }

        public void AddTags(IEnumerable<Tag> tags)
        {
            foreach(Tag tag in tags)
            {
                PostTags.Add(new PostTag
                {
                    Post = this,
                    Tag = tag,
                    PostId = Id,
                    TagId = tag.Id
                });
            }
        }

        public void AddTags(params Tag[] tags)
        {
            AddTags(tags.AsEnumerable());
        }
    }
}
