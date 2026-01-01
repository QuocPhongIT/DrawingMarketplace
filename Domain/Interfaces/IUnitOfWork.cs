namespace DrawingMarketplace.Domain.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable, IDisposable
    {
        Task BeginTransactionAsync(CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task CommitTransactionAsync(CancellationToken ct = default);
        Task RollbackTransactionAsync(CancellationToken ct = default);
    }
}
