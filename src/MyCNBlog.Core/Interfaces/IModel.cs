namespace MyCNBlog.Core.Interfaces
{
    public interface IEntity<TKey>
    {
        TKey Id { get; set; }
        bool IsDeleted { get; set; }
    }
}
