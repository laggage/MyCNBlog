using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MyCNBlog.Core.Models.Dtos
{
    public class TagDto
    {
        int Id { get; set; }
        public string Name { get; set; }
    }

    public class TagDtoComparer:IEqualityComparer<TagDto>
    {
        public bool Equals([AllowNull] TagDto x, [AllowNull] TagDto y)
        {
            return x != null && y != null && x.Name == y.Name;
        }

        public int GetHashCode([DisallowNull] TagDto obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
