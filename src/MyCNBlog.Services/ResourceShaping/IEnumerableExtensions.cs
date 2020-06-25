using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace MyCNBlog.Services.ResourceShaping
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<ExpandoObject> ToDynamicObject<T>(
            this IEnumerable<T> source, in string fields = null)
        {
            if(source == null)
                throw new ArgumentNullException(nameof(source));

            var expandoObejctList = new List<ExpandoObject>();
            var propertyInfoList = typeof(T).GetProeprties(fields).ToList();

            foreach(T x in source)
            {
                expandoObejctList.Add(x.ToDynamicObject(propertyInfoList));
            }
            return expandoObejctList;
        }
    }
}
