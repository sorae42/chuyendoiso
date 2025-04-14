using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using chuyendoiso.Data;
using chuyendoiso.Models;
using Microsoft.AspNetCore.Authorization;
using chuyendoiso.DTOs;
using System.Security.Claims;
using chuyendoiso.Services;

namespace chuyendoiso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubCriteriasController : Controller
    {
        private readonly chuyendoisoContext _context;
        private readonly LogService _logService;
        List<object> results;

        public SubCriteriasController(chuyendoisoContext context, LogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: api/subcriterias
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var subCriterias = await _context.SubCriteria
                .Include(p => p.ParentCriteria)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.MaxScore,
                    p.EvidenceInfo

                })
                .ToListAsync();
            return Ok(subCriterias);
        }

        // GET: api/subcriterias/id
        // Params: Id
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            var subCriteria = await _context.SubCriteria
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.MaxScore,
                    p.Description,
                    p.EvidenceInfo,
                    Parent = new
                    {
                        p.ParentCriteria.Id,
                        p.ParentCriteria.Name
                    },
                    p.EvaluatedAt
                })
                .FirstOrDefaultAsync(m => m.Id == id);

            if (subCriteria == null)
            {
                return NotFound(new { message = "Không tìm thấy nhóm chỉ tiêu!" });
            }

            return Ok(subCriteria);
        }

        // POST: api/subcriterias/create
        // Params: Name, MaxScore, Description, ParentCriteriaId, EvidenceInfo
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] SubCriteriaDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.MaxScore == null || string.IsNullOrWhiteSpace(dto.ParentCriteriaName))
            {
                return BadRequest(new { message = "Tên tiêu chí, điểm tối đa và nhóm tiêu chí là bắt buộc!" });
            }

            if (await _context.SubCriteria.AnyAsync(t => t.Name == dto.Name))
            {
                return BadRequest(new { message = "Tên tiêu chí đã tồn tại!" });
            }

            var parent = await _context.ParentCriteria.FirstOrDefaultAsync(g => g.Name == dto.ParentCriteriaName);
            var unit = User.FindFirst("Unit")?.Value ?? "Không rõ đơn vị";

            if (parent == null)
                return BadRequest(new { message = "Không tìm thấy chỉ tiêu cha!" });

            var subCriteria = new SubCriteria
            {
                Name = dto.Name,
                MaxScore = dto.MaxScore.Value,
                Description = dto.Description,
                EvidenceInfo = dto.EvidenceInfo,
                ParentCriteriaId = parent.Id,
                UnitEvaluate = unit,
                EvaluatedAt = dto.EvaluatedAt ?? DateTime.Now
            };

            _context.SubCriteria.Add(subCriteria);
            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync("Create", $"Tạo tiêu chí con: {subCriteria.Name} (ID = {subCriteria.Id}) thuộc chỉ tiêu cha: {parent.Name} ({parent.Id})", User.FindFirst(ClaimTypes.Name)?.Value);

            return CreatedAtAction(nameof(Details), new { id = subCriteria.Id }, new
            {
                subCriteria.Id,
                subCriteria.Name,
                subCriteria.MaxScore,
                subCriteria.Description,
                subCriteria.EvidenceInfo,
                Parent = parent.Name,
            });
        }

        // PUT: api/subcriterias/id
        // Params: Id, Name, MaxScore, Description, ParentCriteriaId, EvidenceInfo
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [FromBody] SubCriteriaDto dto)
        {
            var exsiting = await _context.SubCriteria.Include(p => p.ParentCriteria).FirstOrDefaultAsync(p => p.Id == id);
            var role = User.FindFirst("Role")?.Value;
            var unit = User.FindFirst("Unit")?.Value;

            if (exsiting == null)
                return NotFound(new { message = "Không tìm thấy tiêu chí con!" });

            if (role != "admin" && exsiting.UnitEvaluate != unit)
            {
                return Forbid("Bạn không có quyền chỉnh sửa tiêu chí của đơn vị khác.");
            }

            if (exsiting == null)
                return NotFound(new { message = "Không tìm thấy tiêu chí con!" });

            if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != exsiting.Name)
            {
                bool isNameExists = await _context.SubCriteria.AnyAsync(p => p.Name == dto.Name);
                if (isNameExists)
                    return BadRequest(new { message = "Tên tiêu chí đã tồn tại!" });
                
                exsiting.Name = dto.Name;
            }

            if (dto.MaxScore.HasValue)
                exsiting.MaxScore = dto.MaxScore.Value;

            if (!string.IsNullOrWhiteSpace(dto.Description))
                exsiting.Description = dto.Description;

            if (!string.IsNullOrWhiteSpace(dto.EvidenceInfo))
                exsiting.EvidenceInfo = dto.EvidenceInfo;

            if (!string.IsNullOrWhiteSpace(dto.ParentCriteriaName))
            {
                var parent = await _context.ParentCriteria.FirstOrDefaultAsync(g => g.Name == dto.ParentCriteriaName);
                if (parent == null)
                    return BadRequest(new { message = "Không tìm thấy chỉ tiêu cha!" });
                exsiting.ParentCriteriaId = parent.Id;
            }

            if (dto.EvaluatedAt.HasValue)
                exsiting.EvaluatedAt = dto.EvaluatedAt.Value;

            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync("Update", $"Cập nhật tiêu chí con: {exsiting.Name} (ID = {exsiting.Id})", User.FindFirst(ClaimTypes.Name)?.Value);

            return Ok(new { message = "Cập nhật thành công!" });
        }

        // DELETE: api/subcriterias/id
        // Params: Id
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subCriteria = await _context.SubCriteria
                .Include(p => p.ParentCriteria)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (subCriteria == null)
                return NotFound(new { message = "Không tìm thấy tiêu chí!" });

            if (subCriteria.MaxScore > 0)
                return BadRequest(new { message = "Không thể xóa tiêu chí này vì nó đã được chấm điểm!" });

            _context.SubCriteria.Remove(subCriteria);
            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync("Delete", $"Xóa tiêu chí con: {subCriteria.Name} (ID = {subCriteria.Id})", User.FindFirst(ClaimTypes.Name)?.Value);

            return Ok(new { message = "Xóa nhóm chỉ tiêu thành công!" });
        }

        // GET: api/subcriterias/by-year?year={year}
        [HttpGet("by-year")]
        [Authorize]
        public async Task<IActionResult> GetByYear([FromQuery] int? year)
        {
            int targetYear = year ?? DateTime.Now.Year;

            var role = User.FindFirst("Role")?.Value;
            var unit = User.FindFirst("Unit")?.Value;

            var query = _context.SubCriteria
                .Include(p => p.ParentCriteria)
                .Where(p => p.EvaluatedAt != null && p.EvaluatedAt.Value.Year == targetYear);

            if (role != "admin")
            {
                query = query.Where(p => p.UnitEvaluate == unit);
            }

            if (role == "admin")
            {
                results = await query
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.MaxScore,
                        p.EvidenceInfo,
                        p.EvaluatedAt,
                        p.UnitEvaluate,
                        Parent = new { p.ParentCriteria.Id, p.ParentCriteria.Name }
                    })
                    .ToListAsync<object>();
            }
            else
            {
                results = await query
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.MaxScore,
                        p.EvidenceInfo,
                        p.EvaluatedAt,
                        Parent = new { p.ParentCriteria.Id, p.ParentCriteria.Name }
                    })
                    .ToListAsync<object>();
            }

            return Ok(results);
        }

    }
}
