namespace MyCNBlog.Core.Interfaces
{
    public interface IModel<TKey>
    {
        TKey Id { get; set; }
        bool IsDeleted { get; set; }
    }
}
