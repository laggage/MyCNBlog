using System;
using System.Collections.Generic;
using System.Linq;

namespace MyCNBlog.Services.Sort
{
    public class PropertyMappingContainer : IPropertyMappingContainer
    {
        protected internal readonly IList<IPropertyMapping> PropertyMappings = new List<IPropertyMapping>();

        public void Register<T>() where T : IPropertyMapping, new()
        {
            if(PropertyMappings.Any(x => x.GetType() == typeof(T)))
                return;
            PropertyMappings.Add(new T());
        }

        public IPropertyMapping Resolve<TSource, TDestination>()
        {
            IEnumerable<PropertyMapping<TSource, TDestination>> result = PropertyMappings.OfType<PropertyMapping<TSource, TDestination>>();
            if(result.Count() > 0)
                return result.First();
            throw new InvalidCastException(
               string.Format("Cannot find property mapping instance for {0}, {1}", typeof(TSource), typeof(TDestination)));
        }

        public bool ValidateMappingExistsFor<TSource, TDestination>(string fields)
        {
            if(string.IsNullOrEmpty(fields))
                return true;

            IPropertyMapping propertyMapping = Resolve<TSource, TDestination>();

            string[] splitFields = fields.Split(',');

            foreach(string property in splitFields)
            {
                string trimmedProperty = property.Trim();
                int indexOfFirstWhiteSpace = trimmedProperty.IndexOf(' ');
                string propertyName = indexOfFirstWhiteSpace <= 0 ? trimmedProperty : trimmedProperty.Remove(indexOfFirstWhiteSpace);

                if(!propertyMapping.MappingDictionary.Keys.Any(x => string.Equals(propertyName, x, StringComparison.OrdinalIgnoreCase)))
                    return false;
            }
            return true;
        }
    }
}
