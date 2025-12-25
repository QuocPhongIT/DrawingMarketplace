using DrawingMarketplace.Domain.Entities;

namespace DrawingMarketplace.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> ExistsByEmailAsync(string email);
        Task AddAsync(User user);
        Task<User?> GetByEmailAsync(string email);
        Task UpdateAsync(User user);
        Task AddRoleAsync(Guid userId, Guid roleId);
        Task<bool> HasRoleAsync(Guid userId, Guid roleId);
    }
}
