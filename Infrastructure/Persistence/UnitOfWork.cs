using DrawingMarketplace.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace DrawingMarketplace.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly DrawingMarketplaceContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(DrawingMarketplaceContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
            return;

        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction == null)
            return;

        try
        {
            await _context.SaveChangesAsync(ct);
            await _transaction.CommitAsync(ct);
        }
        catch
        {
            await RollbackTransactionAsync(ct);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction == null)
            return;

        await _transaction.RollbackAsync(ct);
        await DisposeTransactionAsync();
    }

    private async ValueTask DisposeTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeTransactionAsync();
        await _context.DisposeAsync();
    }

    public void Dispose()
    {
        DisposeTransactionAsync().AsTask().GetAwaiter().GetResult();
        _context.Dispose();
    }
}
