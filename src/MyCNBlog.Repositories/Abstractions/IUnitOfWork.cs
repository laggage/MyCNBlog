using System.Threading.Tasks;

namespace MyCNBlog.Repositories.Abstraction
{
    public interface IUnitOfWork
    {
        void BeginTransaction();
        bool CommitTransaction();
        Task<bool> CommitTransactionAsync();
        Task<bool> SaveChangesAsync();
    }
}
