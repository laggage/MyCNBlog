using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using MyCNBlog.Database;
using MyCNBlog.Repositories.Abstraction;

namespace MyCNBlog.Repositories
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private IDbContextTransaction _transaction;
        private IDbContextTransaction _Transaction
        {
            get => _transaction;
            set
            {
                if(_transaction != null)
                {
                    _transaction.Rollback();
                    _transaction.Dispose();
                }
                _transaction = value;
            }
        }

        private readonly DbContext _context;
        private readonly ILogger<UnitOfWork> _logger;

        public UnitOfWork(MyCNBlogDbContext context, 
            ILogger<UnitOfWork> logger)
        {
            _context = context;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void BeginTransaction()
        {
            _Transaction = _context.Database.BeginTransaction();
        }

        public bool CommitTransaction()
        {
            try
            {
                if(_Transaction == null)
                    throw new InvalidOperationException("Please invoke BeginTransaction first");
                _Transaction.Commit();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                _Transaction.Dispose();
                _Transaction = null;
            }
        }

        public Task<bool> CommitTransactionAsync()
        {
            return Task.Run(() => CommitTransaction());
        }

        /// <summary>
        /// TODO: 复习一下C#中关于资源清理的内容
        /// </summary>
        ~UnitOfWork()
        {
            _Transaction?.Dispose();
        }

        public async Task<bool> SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Savechanges encounter error. ");
                return false;
            }
        }
    }
}
