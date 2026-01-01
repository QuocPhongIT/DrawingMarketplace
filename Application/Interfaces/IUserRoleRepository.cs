namespace DrawingMarketplace.Application.Interfaces
{
    public interface IUserRoleRepository
    {
        Task AddRoleAsync(Guid userId, Guid roleId);
        Task<bool> HasRoleAsync(Guid userId, Guid roleId);
    }
}
