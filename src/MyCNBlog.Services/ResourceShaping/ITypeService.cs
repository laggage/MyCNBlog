namespace MyCNBlog.Services.ResourceShaping
{
    public interface ITypeService
    {
        bool HasProperties<T>(string fields);
    }
}
