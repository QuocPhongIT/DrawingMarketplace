using AutoMapper;
using DrawingMarketplace.Application.DTOs.Catogory;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Entities;
using DrawingMarketplace.Infrastructure.Persistence;
using System.Threading.Tasks;
using static DrawingMarketplace.Application.DTOs.Catogory.CategoryUpsertDto;

namespace DrawingMarketplace.Application.Services
{
    public class CategoryService
        : CrudService<Category, CategoryDto, CreateCategoryDto, UpdateCategoryDto>,
          ICategoryService
    {
        public CategoryService(DrawingMarketplaceContext context, IMapper mapper)
            : base(context, mapper)
        {
        }

    }
}
