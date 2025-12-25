using DrawingMarketplace.Domain.Entities;

namespace DrawingMarketplace.Application.Interfaces
{
    public interface ICrudService<TEntity, TDto, TCreateDto, TUpdateDto>
    where TEntity : BaseEntity
    where TDto : class
    {
        Task<List<TDto>> GetAllAsync();
        Task<TDto?> GetByIdAsync(Guid id);
        Task<TDto> CreateAsync(TCreateDto dto);
        Task<TDto?> UpdateAsync(Guid id, TUpdateDto dto);
        Task<bool> DeleteAsync(Guid id);
    }

}
