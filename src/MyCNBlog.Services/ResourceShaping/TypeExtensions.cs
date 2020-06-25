using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MyCNBlog.Services.ResourceShaping
{
    public static class TypeExtensions
    {
        public static IEnumerable<PropertyInfo> GetProeprties(this Type source, string fields = null)
        {
            var propertyInfoList = new List<PropertyInfo>();

            if(string.IsNullOrEmpty(fields))
            {
                propertyInfoList.AddRange(source.GetProperties(BindingFlags.Public | BindingFlags.Instance));
            }
            else
            {
                string[] properties = fields.Trim().Split(',');
                foreach(string propertyName in properties)
                {
                    propertyInfoList.Add(
                        source.GetProperty(
                        propertyName.Trim(),
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase));
                }
            }

            return propertyInfoList;
        }
    }
}
