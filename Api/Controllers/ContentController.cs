using DrawingMarketplace.Application.DTOs.Content;
using DrawingMarketplace.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static DrawingMarketplace.Application.DTOs.Content.ContentUpsertDto;

namespace DrawingMarketplace.Api.Controllers
{
    [ApiController]
    [Route("api/contents")]
    public class ContentsController : ControllerBase
    {
        private readonly IContentService _service;

        public ContentsController(IContentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<ContentDto>>> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ContentDto>> GetById(Guid id)
        {
            var content = await _service.GetByIdAsync(id);
            return content == null ? NotFound() : Ok(content);
        }

        [HttpPost]
        public async Task<ActionResult<ContentDto>> Create(CreateContentDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ContentDto>> Update(Guid id, UpdateContentDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            return await _service.DeleteAsync(id) ? NoContent() : NotFound();
        }

        [HttpPatch("{id:guid}")]
        public async Task<ActionResult<ContentDto>> UpdateStatus(Guid id, [FromBody] bool publish)
        {
            var result = await _service.ApproveContentAsync(id, publish);
            return result == null ? NotFound() : Ok(result);
        }
    }
}
