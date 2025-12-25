namespace DrawingMarketplace.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        Task BeginAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
