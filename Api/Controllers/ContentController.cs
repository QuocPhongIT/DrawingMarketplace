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

        // GET: api/contents
        [HttpGet]
        public async Task<ActionResult<List<ContentDto>>> GetAll()
        {
            var contents = await _service.GetAllAsync();
            return Ok(contents);
        }

        // GET: api/contents/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ContentDto>> GetById(Guid id)
        {
            var content = await _service.GetByIdAsync(id);
            if (content == null)
                return NotFound();
            return Ok(content);
        }

        // POST: api/contents
        [HttpPost]
        public async Task<ActionResult<ContentDto>> Create([FromBody] CreateContentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdContent = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdContent.Id }, createdContent);
        }

        // PUT: api/contents/{id}
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ContentDto>> Update(Guid id, [FromBody] UpdateContentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedContent = await _service.UpdateAsync(id, dto);
            if (updatedContent == null)
                return NotFound();

            return Ok(updatedContent);
        }

        // DELETE: api/contents/{id}
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
        [HttpPatch("{id:guid}/approve")]
        public async Task<ActionResult<ContentDto>> Approve(Guid id, [FromQuery] bool publish)
        {
            var result = await _service.ApproveContentAsync(id, publish);
            if (result == null) return NotFound();
            return Ok(result);
        }

    }
}
