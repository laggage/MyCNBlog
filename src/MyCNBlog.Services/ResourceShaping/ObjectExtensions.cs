using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace MyCNBlog.Services.ResourceShaping
{
    public static class ObjectExtensions
    {
        public static ExpandoObject ToDynamicObject(
            this object source, 
            string fields = null)
        {
            var propertyInfoList = source.GetType().GetProeprties(fields).ToList();

            var expandoObject = new ExpandoObject();

            foreach(PropertyInfo propertyInfo in propertyInfoList)
            {
                try
                {
                    (expandoObject as IDictionary<string, object>).Add(
                        propertyInfo.Name, propertyInfo.GetValue(source));
                }
                catch { continue; }
            }
            return expandoObject;
        }

        internal static ExpandoObject ToDynamicObject(
            this object source, 
            in IEnumerable<PropertyInfo> propertyInfos)
        {
            var expandoObject = new ExpandoObject();

            foreach(PropertyInfo propertyInfo in propertyInfos)
            {
                try
                {
                    (expandoObject as IDictionary<string, object>).Add(
                        propertyInfo.Name, propertyInfo.GetValue(source));
                }
                catch { continue; }
            }
            return expandoObject;
        }
    }
}
