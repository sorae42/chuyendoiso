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
using Microsoft.DotNet.Scaffolding.Shared.Messaging;

namespace chuyendoiso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubCriteriasController : Controller
    {
        private readonly chuyendoisoContext _context;

        public SubCriteriasController(chuyendoisoContext context)
        {
            _context = context;
        }

        // GET: /api/subcriterias
        public async Task<IActionResult> Index()
        {
            var subCriterias = await _context.SubCriteria.ToListAsync();
            return Ok(subCriterias);
        }

        // GET: /api/subcriterias/5
        // Params: Id
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            var subCriteria = await _context.SubCriteria
                .FirstOrDefaultAsync(m => m.Id == id);

            if (subCriteria == null)
            {
                return NotFound(new { message = "Không tìm thấy nhóm chỉ tiêu!" });
            }

            return Ok(subCriteria);
        }

        // POST: api/subcriterias/create
        // Params: Name, MaxScore, Description, EvidenceInfo
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([Bind("Id,Name,MaxScore,Description,EvidenceInfo")] SubCriteria subCriteria)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.SubCriteria.Add(subCriteria);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Details), new { id = subCriteria.Id}, new
            {
                subCriteria.Id,
                subCriteria.Name,
                subCriteria.MaxScore,
                subCriteria.Description,
                subCriteria.EvidenceInfo
            });
        }

        // POST: .api/subcriterias/edit/5
        // Params: Id, Name, MaxScore, Description, EvidenceInfo
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,MaxScore,Description,EvidenceInfo")] SubCriteria subCriteria)
        {
            if (id != subCriteria.Id)
            {
                return BadRequest(new { message = "ID không khớp!" });
            }

            var exsitingSubCriteria = await _context.SubCriteria.FindAsync(id);
            if (exsitingSubCriteria == null)
            {
                return NotFound(new { message = "Không tìm thấy tiêu chí con!" });
            }

            exsitingSubCriteria.Name = subCriteria.Name;

            try
            {
                _context.SubCriteria.Update(exsitingSubCriteria);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Cập nhật tiêu chí con thành công!",
                    data = new
                    {
                        exsitingSubCriteria.Id,
                        exsitingSubCriteria.Name,
                        exsitingSubCriteria.MaxScore,
                        exsitingSubCriteria.Description,
                        exsitingSubCriteria.EvidenceInfo
                    }
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(new { message = "Lỗi cập nhật thông tin!" });
            }
        }

        // POST: api/subcriterias/delete/5
        // Params: Id
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subCriteria = await _context.SubCriteria.FindAsync(id);
            if (subCriteria == null)
            {
                return NotFound(new { message = "Không tìm thấy nhóm!" });
            }

            _context.SubCriteria.Remove(subCriteria);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa nhóm chỉ tiêu thành công!" });
        }
    }
}
