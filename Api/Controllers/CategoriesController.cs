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
        public async Task<ActionResult<List<CategoryDto>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [AllowAnonymous]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CategoryDto>> GetById(Guid id)
        {
            var category = await _service.GetByIdAsync(id);
            return category == null ? NotFound() : Ok(category);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create(CreateCategoryDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<CategoryDto>> Update(Guid id, UpdateCategoryDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return await _service.DeleteAsync(id) ? NoContent() : NotFound();
        }
    }
}
