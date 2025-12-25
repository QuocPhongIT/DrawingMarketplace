using AutoMapper;
using DrawingMarketplace.Application.DTOs.Catogory;
using DrawingMarketplace.Application.DTOs.Content;
using DrawingMarketplace.Application.Interfaces;
using DrawingMarketplace.Domain.Entities;

namespace DrawingMarketplace.Application.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            CreateMap<Category, CategoryDto>().ReverseMap();

            CreateMap<Category, CategoryUpsertDto.CreateCategoryDto>().ReverseMap();
            CreateMap<Category, CategoryUpsertDto.UpdateCategoryDto>().ReverseMap();

            CreateMap<Content, ContentDto>();
            CreateMap<ContentUpsertDto.CreateContentDto, Content>();
            CreateMap<ContentUpsertDto.UpdateContentDto, Content>();

            // ContentStats
            //CreateMap<ContentStats, ContentStatsDto>().ReverseMap();
        }
    }
}
