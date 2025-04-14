using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using chuyendoiso.Data;
using chuyendoiso.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using chuyendoiso.DTOs;
using chuyendoiso.Services;

namespace chuyendoiso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParentCriteriasController : Controller
    {
        private readonly chuyendoisoContext _context;
        private readonly LogService _logService;

        public ParentCriteriasController(chuyendoisoContext context, LogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: api/parentcriterias
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var parentCriterias = await _context.ParentCriteria
                .Include(p => p.TargetGroup)
                .Include(p => p.SubCriterias)
                .Select(p => new {
                    p.Id,
                    p.Name,
                    p.MaxScore,
                    GroupId = p.TargetGroup.Id,
                    SubCriteriaId = p.SubCriterias.Select(s => s.Id)
                })
                .ToListAsync();

            return Ok(parentCriterias);
        }

        // GET: api/parentcriterias/id
        // Params: Id
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            var parentCriteria = await _context.ParentCriteria
                .Include(p => p.TargetGroup)
                .Include(p => p.SubCriterias)
                .Select(p => new {
                    p.Id,
                    p.Name,
                    p.MaxScore,
                    GroupId = p.TargetGroup.Id,
                    p.SubCriterias
                })
                .FirstOrDefaultAsync(m => m.Id == id);

            if (parentCriteria == null)
            {
                return NotFound(new { message = "Không tìm thấy nhóm chỉ tiêu!" });
            }

            return Ok(parentCriteria);
        }

        // POST: api/parentcriterias/create
        // Params: Name, MaxScore, Description, TargetGroupId, EvidenceInfo
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] ParentCriteriaDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.MaxScore == null || string.IsNullOrWhiteSpace(dto.TargetGroupName))
            {
                return BadRequest(new { message = "Tên tiêu chí, điểm tối đa và nhóm tiêu chí là bắt buộc!" });
            }

            if (await _context.ParentCriteria.AnyAsync(p => p.Name == dto.Name))
            {
                return BadRequest(new { message = "Tiêu chí đã tồn tại!" });
            }

            var group = await _context.TargetGroup.FirstOrDefaultAsync(g => g.Name == dto.TargetGroupName);
            if (group == null)
                return BadRequest(new { message = "Không tìm thấy nhóm chỉ tiêu!" });

            var parent = new ParentCriteria
            {
                Name = dto.Name,
                MaxScore = dto.MaxScore.Value,
                Description = dto.Description,
                EvidenceInfo = dto.EvidenceInfo,
                TargetGroupId = group.Id
            };

            _context.ParentCriteria.Add(parent);
            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync("Create", $"Tạo tiêu chí cha: {parent.Name} (ID = {parent.Id}) thuộc nhóm: {group.Name} ({group.Id})", User.FindFirst(ClaimTypes.Name)?.Value);

            return CreatedAtAction(nameof(Details), new { id = parent.Id}, new
            {
                parent.Id,
                parent.Name,
                parent.MaxScore,
                parent.Description,
                parent.EvidenceInfo,
                Group = group.Name
            });
        }

        // PUT: api/parentcriterias/id
        // Params: Id, Name, MaxScore, Description, TargetGroupId, EvidenceInfo
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [FromBody] ParentCriteriaDto dto)
        {
            var existing = await _context.ParentCriteria.Include(p => p.TargetGroup).FirstOrDefaultAsync(p => p.Id == id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy tiêu chí cha!" });

            if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != existing.Name)
            {
                bool isNameExists = await _context.ParentCriteria.AnyAsync(p => p.Name == dto.Name);
                if (isNameExists)
                    return BadRequest(new { message = "Tên tiêu chí đã tồn tại!" });

                existing.Name = dto.Name;
            }

            if (dto.MaxScore.HasValue)
                existing.MaxScore = dto.MaxScore.Value;

            if (!string.IsNullOrWhiteSpace(dto.Description))
                existing.Description = dto.Description;

            if (!string.IsNullOrWhiteSpace(dto.EvidenceInfo))
                existing.EvidenceInfo = dto.EvidenceInfo;

            if (!string.IsNullOrWhiteSpace(dto.TargetGroupName))
            {
                var group = await _context.TargetGroup.FirstOrDefaultAsync(g => g.Name == dto.TargetGroupName);
                if (group == null)
                    return BadRequest(new { message = "Tên nhóm không tồn tại!" });

                existing.TargetGroupId = group.Id;
            }

            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync("Update", $"Cập nhật tiêu chí cha: {existing.Name} (ID = {existing.Id})", User.FindFirst(ClaimTypes.Name)?.Value);

            return Ok(new { message = "Cập nhật thành công!" });
        }

        // DELETE: api/parentcriterias/id
        // Params: Id
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var parentCriteria = await _context.ParentCriteria
                .Include(p => p.SubCriterias)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parentCriteria == null)
                return NotFound(new { message = "Không tìm thấy tiêu chí cha!" });

            if (parentCriteria.SubCriterias != null && parentCriteria.SubCriterias.Any())
                return BadRequest(new { message = "Không thể xóa vì tiêu chí cha đang chứa tiêu chí con!" });

            if (parentCriteria.MaxScore > 0)
                return BadRequest(new { message = "Không thể xóa vì tiêu chí cha đã được chấm điểm!" });

            if (parentCriteria.SubCriterias.Any(s => s.MaxScore > 0))
                return BadRequest(new { message = "Không thể xóa vì tiêu chí con đã được chấm điểm!" });

            _context.ParentCriteria.Remove(parentCriteria);
            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync("Delete", $"Xóa tiêu chí cha: {parentCriteria.Name} (ID = {parentCriteria.Id})", User.FindFirst(ClaimTypes.Name)?.Value);

            return Ok(new { message = "Xóa tiêu chí cha thành công!" });
        }
    }
}