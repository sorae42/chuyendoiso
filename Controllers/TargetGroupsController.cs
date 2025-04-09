using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using chuyendoiso.Data;
using chuyendoiso.Models;
using Microsoft.AspNetCore.Authorization;
using chuyendoiso.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Security.Claims;
using chuyendoiso.DTOs;

namespace chuyendoiso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TargetGroupsController : Controller
    {
        private readonly chuyendoisoContext _context;
        private readonly LogService _logService;

        public TargetGroupsController(chuyendoisoContext context, LogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: api/targetgroups
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var targetGroups = await _context.TargetGroup
                .Include(t => t.ParentCriterias)
                    .ThenInclude(p => p.SubCriterias)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    ParentCriterias = t.ParentCriterias.Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.MaxScore,
                        p.Description,
                        p.EvidenceInfo,
                        SubCriterias = p.SubCriterias.Select(s => new
                        {
                            s.Id,
                            s.Name,
                            s.MaxScore,
                            s.EvidenceInfo
                        }).ToList()
                    }).ToList()
                })
                .ToListAsync();

            return Ok(targetGroups);
        }

        // GET: api/targetgroups/id
        // Params: Id
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            var targetGroup = await _context.TargetGroup
                .Include(t => t.ParentCriterias)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.ParentCriterias
                })
                .FirstOrDefaultAsync(m => m.Id == id);

            if (targetGroup == null)
            {
                return NotFound(new { message = "Không tìm thấy nhóm!" });
            }

            return Ok(targetGroup);
        }

        // POST: api/targetgroups/create
        // Params: Name
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] TargetGroupDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { message = "Tên nhóm không được để trống!" });
            }

            var targetGroup = new TargetGroup
            {
                Name = dto.Name
            };

            _context.TargetGroup.Add(targetGroup);
            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync("Create", $"Tạo nhóm chỉ tiêu mới: {targetGroup.Name} (ID = {targetGroup.Id})", User.FindFirst(ClaimTypes.Name)?.Value);

            return CreatedAtAction(nameof(Details), new { id = targetGroup.Id }, new
            {
                targetGroup.Id,
                targetGroup.Name
            });
        }

        // PUT: api/targetgroups/id
        // Params: Id, Name
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [FromBody] TargetGroupDto dto)
        {
            var existingTargetGroup = await _context.TargetGroup.FindAsync(id);
            if (existingTargetGroup == null)
            {
                return NotFound(new { message = "Không tìm thấy nhóm!" });
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                existingTargetGroup.Name = dto.Name;
            }

            try
            {
                await _context.SaveChangesAsync();

                await _logService.WriteLogAsync("Update", $"Cập nhật nhóm chỉ tiêu: {existingTargetGroup.Name} (ID = {existingTargetGroup.Id})", User.FindFirst(ClaimTypes.Name)?.Value);

                return Ok(new
                {
                    message = "Cập nhật thành công!",
                    data = new
                    {
                        existingTargetGroup.Id,
                        existingTargetGroup.Name
                    }
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(new { message = "Cập nhật thất bại!" });
            }
        }

        // DELETE: api/targetgroups/id
        // Params: Id
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var targetGroup = await _context.TargetGroup.FindAsync(id);
            if (targetGroup == null)
            {
                return NotFound(new { message = "Không tìm thấy nhóm!" });
            }

            _context.TargetGroup.Remove(targetGroup);
            await _context.SaveChangesAsync();
            await _logService.WriteLogAsync("Delete", $"Xóa nhóm chỉ tiêu: {targetGroup.Name} (ID = {targetGroup.Id})", User.FindFirst(ClaimTypes.Name)?.Value);

            return Ok(new { message = "Xóa nhóm thành công!" });
        }
    }
}
