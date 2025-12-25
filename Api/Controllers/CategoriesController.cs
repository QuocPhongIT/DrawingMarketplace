using DrawingMarketplace.Application.DTOs.Catogory;
using DrawingMarketplace.Application.Interfaces;
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

        [HttpGet]
        public async Task<ActionResult<List<CategoryDto>>> GetAll()
        {
            var categories = await _service.GetAllAsync();
            return Ok(categories);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CategoryDto>> GetById(Guid id)
        {
            var category = await _service.GetByIdAsync(id);
            if (category == null)
                return NotFound();
            return Ok(category);
        }
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdCategory = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdCategory.Id }, createdCategory);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<CategoryDto>> Update(Guid id, [FromBody] UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedCategory = await _service.UpdateAsync(id, dto);
            if (updatedCategory == null)
                return NotFound();

            return Ok(updatedCategory);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
