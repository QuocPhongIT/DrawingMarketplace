using DrawingMarketplace.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DrawingMarketplace.Infrastructure.Persistence
{
    public sealed class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly DrawingMarketplaceContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(DrawingMarketplaceContext context)
        {
            _context = context;
        }

        public async Task BeginAsync()
        {
            if (_transaction != null)
                return;

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();

                if (_transaction != null)
                    await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
                await _transaction.RollbackAsync();

            await DisposeTransactionAsync();
        }

        private async Task DisposeTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
        }
    }
}
