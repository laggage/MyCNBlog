namespace MyCNBlog.Services.Sort
{
    public interface IPropertyMappingContainer
    {
        void Register<T>() where T : IPropertyMapping, new();
        IPropertyMapping Resolve<TSource, TDestination>();
        bool ValidateMappingExistsFor<TSource, TDestination>(string fields);
    }
}
