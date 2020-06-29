using System;
using System.Collections.Generic;

namespace MyCNBlog.Core.Models.Dtos
{
    public class PostDto
    {
        public int Id { get; set; }

        public string Title { get; set; }

        /// <summary>
        /// Max size: 512
        /// </summary>
        public string Description { get; set; }

        public DateTime LastModified { get; set; }

        public DateTime CreateDate { get; set; }

        //public BlogUserDto Author { get; set; }

        public string PostContentUrl { get; set; }

        public bool? IsDeleted { get; set; }

        public int ViewCount { get; set; }

        public int CommentsCount { get; set; }

        /// <summary>
        /// 是否置顶
        /// Default: False
        /// </summary>
        public bool IsTopMost { get; set; }

        /// <summary>
        /// 置顶顺序
        /// </summary>
        public int TopMostOrder { get; set; }

        /// <summary>
        /// 博文标签
        /// </summary>
        public TagDto[] Tags { get; set; }

        /// <summary>
        /// 一篇文章属于一个博客
        /// </summary>
        public BlogDto Blog { get; set; }

        public PostDto()
        {
            IsTopMost = false;
        }
    }

    public class PostAddDto
    {
        public string Title { get; set; }

        /// <summary>
        /// Max size: 512
        /// </summary>
        public string Description { get; set; }

        public string Content { get; set; }

        public bool IsPublic { get; set; }

        /// <summary>
        /// 博文标签
        /// JSONPath synatax: 
        ///     {"op": "add", "value":  { "name":"name" } ,"path":"/tags/-"}
        /// </summary>
        public List<TagDto> Tags { get; set; }

        public bool IsTopMost { get; set; }

        /// <summary>
        /// 置顶顺序
        /// </summary>
        public int TopMostOrder { get; set; }
    }
}
