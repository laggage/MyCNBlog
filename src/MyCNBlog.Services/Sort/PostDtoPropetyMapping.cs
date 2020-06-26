using System.Collections.Generic;
using MyCNBlog.Core.Models;
using MyCNBlog.Core.Models.Dtos;

namespace MyCNBlog.Services.Sort
{
    class PostDtoPropetyMapping : PropertyMapping<PostDto, Post>
    {
        public PostDtoPropetyMapping()
            : base(new Dictionary<string, List<MappedProperty>>
            {
                { 
                    nameof(PostDto.CreateDate), 
                    new List<MappedProperty>
                    {
                        new MappedProperty(nameof(Post.CreateDate))
                    }
                },
                {
                    nameof(PostDto.LastModified),
                    new List<MappedProperty>
                    {
                        new MappedProperty(nameof(Post.LastModified))
                    }
                },
                {
                    nameof(PostDto.Id),
                    new List<MappedProperty>
                    {
                        new MappedProperty(nameof(Post.Id))
                    }
                },
                {
                    nameof(PostDto.Title),
                    new List<MappedProperty>
                    {
                        new MappedProperty(nameof(Post.Title))
                    }
                },
            })
        {
        }
    }
}
