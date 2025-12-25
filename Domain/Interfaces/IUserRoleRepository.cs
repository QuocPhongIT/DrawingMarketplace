namespace DrawingMarketplace.Domain.Interfaces
{
    public interface IUserRoleRepository
    {
        Task AddRoleAsync(Guid userId, Guid roleId);
        Task<bool> HasRoleAsync(Guid userId, Guid roleId);
    }
}
