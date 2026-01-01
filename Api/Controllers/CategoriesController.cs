using DrawingMarketplace.Api.Extensions;
using DrawingMarketplace.Application.DTOs.Catogory;
using DrawingMarketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static DrawingMarketplace.Application.DTOs.Catogory.CategoryUpsertDto;

namespace DrawingMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoriesController(ICategoryService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _service.GetAllAsync();
            return this.Success(categories, "Lấy danh sách category thành công", "Get categories successfully");
        }

        [AllowAnonymous]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var category = await _service.GetByIdAsync(id);
            if (category == null)
                return this.NotFound("Category", "Category not found");
            
            return this.Success(category, "Lấy chi tiết category thành công", "Get category detail successfully");
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return this.Success(created, "Tạo category thành công", "Create category successfully", 201);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateCategoryDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (updated == null)
                return this.NotFound("Category", "Category not found");
            
            return this.Success(updated, "Cập nhật category thành công", "Update category successfully");
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return this.NotFound("Category", "Category not found");
            
            return this.Success<object>(null, "Xóa category thành công", "Delete category successfully");
        }
    }
}
