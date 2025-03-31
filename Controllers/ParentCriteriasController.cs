using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using chuyendoiso.Data;
using chuyendoiso.Models;
using Microsoft.AspNetCore.Authorization;

namespace chuyendoiso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParentCriteriasController : Controller
    {
        private readonly chuyendoisoContext _context;

        public ParentCriteriasController(chuyendoisoContext context)
        {
            _context = context;
        }

        // GET: /api/parentcriterias
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var parentCriterias = await _context.ParentCriteria.ToListAsync();
            return Ok(parentCriterias);
        }

        // GET: /api/parentcriterias/5
        // Params: Id
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            var parentCriteria = await _context.ParentCriteria
                .FirstOrDefaultAsync(m => m.Id == id);

            if (parentCriteria == null)
            {
                return NotFound(new { message = "Không tìm thấy nhóm chỉ tiêu!" });
            }

            return Ok(parentCriteria);
        }

        // POST: /api/parentcriterias/create
        // Params: Name, MaxScore, Description, EvidenceInfo
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] ParentCriteria parentCriteria)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.ParentCriteria.Add(parentCriteria);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Details), new { id = parentCriteria.Id}, new
            {
                parentCriteria.Id,
                parentCriteria.Name,
                parentCriteria.MaxScore,
                parentCriteria.Description,
                parentCriteria.EvidenceInfo
            });
        }

        // POST: /api/parentcriterias/edit/5
        // Params: Id, Name, MaxScore, Description, EvidenceInfo
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [FromBody] ParentCriteria parentCriteria)
        {
            if (id != parentCriteria.Id)
            {
                return BadRequest(new { message = "ID không khớp!" });
            }

            var existingParentCriteria = await _context.ParentCriteria.FindAsync(id);
            if (existingParentCriteria == null)
            {
                return NotFound(new { message = "Không tìm thấy nhóm chỉ tiêu!" });
            }

            existingParentCriteria.Name = parentCriteria.Name;

            try
            {
                _context.ParentCriteria.Update(existingParentCriteria);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Cập nhật thành công!",
                    data = new
                    {
                        existingParentCriteria.Id,
                        existingParentCriteria.Name,
                        existingParentCriteria.MaxScore,
                        existingParentCriteria.Description,
                        existingParentCriteria.EvidenceInfo
                    }
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(new { message = "Cập nhật thất bại!" });
            }
        }

        // POST: /api/parentcriterias/delete/5
        // Params: Id
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var parentCriteria = await _context.ParentCriteria.FindAsync(id);
            if (parentCriteria == null)
            {
                return NotFound(new { message = "Không tìm thấy nhóm!" });
            }

            _context.ParentCriteria.Remove(parentCriteria);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa nhóm chỉ tiêu thành công!" });
        }
    }
}
