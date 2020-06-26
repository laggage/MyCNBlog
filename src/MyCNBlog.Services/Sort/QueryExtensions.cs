using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace MyCNBlog.Services.Sort
{
    public static class QueryExtensions
    {
        public static IQueryable<T> ApplySort<T>(
           this IQueryable<T> data, 
           in string orderBy, 
           in IPropertyMapping propertyMapping)
        {
            if(data == null)
                throw new ArgumentNullException(nameof(data));
            if(string.IsNullOrEmpty(orderBy))
                return data;

            string[] splitOrderBy = orderBy.Split(',');
            foreach(string property in splitOrderBy)
            {
                string trimmedProperty = property.Trim();
                int indexOfFirstSpace = trimmedProperty.IndexOf(' ');
                bool desc = trimmedProperty.EndsWith(" desc");
                string propertyName = indexOfFirstSpace > 0 ? trimmedProperty.Remove(indexOfFirstSpace) : trimmedProperty;
                propertyName = propertyMapping.MappingDictionary.Keys.FirstOrDefault(
                    x => string.Equals(x, propertyName, StringComparison.OrdinalIgnoreCase)); //ignore case of sort property

                if(!propertyMapping.MappingDictionary.TryGetValue(
                    propertyName, out List<MappedProperty> mappedProperties))
                    throw new InvalidCastException($"key mapping for {propertyName} is missing");

                mappedProperties.Reverse();
                foreach(MappedProperty mappedProperty in mappedProperties)
                {
                    if(mappedProperty.Revert)
                        desc = !desc;
                    data = data.OrderBy($"{mappedProperty.Name} {(desc ? "descending" : "ascending")} ");
                }
            }

            return data;
        }

        public static Task<IQueryable<T>> ApplySortAsync<T>(
           this IQueryable<T> data,
           string orderBy,
           IPropertyMapping propertyMapping)
        {
            return Task.Run(() => data.ApplySort(orderBy, propertyMapping));
        }
    }
}
