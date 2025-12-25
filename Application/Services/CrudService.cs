using AutoMapper;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DrawingMarketplace.Application.Services
{
    public class CrudService<TEntity, TDto, TCreateDto, TUpdateDto>
      : ICrudService<TEntity, TDto, TCreateDto, TUpdateDto>
      where TEntity : BaseEntity
      where TDto : class
    {
        protected readonly DrawingMarketplaceContext _context;
        protected readonly IMapper _mapper;
        protected readonly DbSet<TEntity> _dbSet;

        public CrudService(DrawingMarketplaceContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _dbSet = context.Set<TEntity>();
        }

        public async Task<List<TDto>> GetAllAsync()
        {
            var entities = await _dbSet.AsNoTracking().ToListAsync();
            return _mapper.Map<List<TDto>>(entities);
        }

        public async Task<TDto?> GetByIdAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            return entity == null ? null : _mapper.Map<TDto>(entity);
        }

        public async Task<TDto> CreateAsync(TCreateDto dto) 
        {
            var entity = _mapper.Map<TEntity>(dto);

            _dbSet.Add(entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<TDto>(entity);
        }

        public async Task<TDto?> UpdateAsync(Guid id, TUpdateDto dto)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null) return null;

            _mapper.Map(dto, entity);

            await _context.SaveChangesAsync();
            return _mapper.Map<TDto>(entity);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null) return false;

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
