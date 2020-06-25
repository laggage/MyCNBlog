using System.Reflection;

namespace MyCNBlog.Services.ResourceShaping
{
    public class TypeService : ITypeService
    {
        public bool HasProperties<T>(string fields)
        {
            if(string.IsNullOrEmpty(fields))
                return true;

            string[] splitFields = fields.Split(',');

            foreach(string splitField in splitFields)
            {
                string proeprtyName = splitField.Trim();
                PropertyInfo propertyInfo = typeof(T).GetProperty(
                    proeprtyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if(propertyInfo == null)
                    return false;
            }

            return true;
        }
    }
}
