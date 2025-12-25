using DrawingMarketplace.Application.DTOs.Catogory;
using DrawingMarketplace.Domain.Entities;
using static DrawingMarketplace.Application.DTOs.Catogory.CategoryUpsertDto;

namespace DrawingMarketplace.Application.Interfaces
{
    public interface ICategoryService
    : ICrudService<Category, CategoryDto, CreateCategoryDto, UpdateCategoryDto>
    {
    }

}
