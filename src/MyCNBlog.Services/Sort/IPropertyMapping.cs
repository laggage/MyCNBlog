using System.Collections.Generic;

namespace MyCNBlog.Services.Sort
{
    public interface IPropertyMapping
    {
        Dictionary<string, List<MappedProperty>> MappingDictionary { get; }
    }
}
