using System.Collections.Generic;

namespace MyCNBlog.Services.Sort
{
    public abstract class PropertyMapping<TSource, TDestination> : IPropertyMapping
    {
        public Dictionary<string, List<MappedProperty>> MappingDictionary { get; }

        public PropertyMapping(Dictionary<string, List<MappedProperty>> MappingDict)
        {
            MappingDictionary = MappingDict;
        }
    }
}
