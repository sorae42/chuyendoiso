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
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.ParentCriteriaId == null)
            {
                return BadRequest(new { message = "Tên tiêu chí và ID tiêu chí cha là bắt buộc!" });
            }

            var exists = await _context.SubCriteria.AnyAsync(t => t.Name == dto.Name && t.ParentCriteriaId == dto.ParentCriteriaId);
            if (exists)
            {
                return BadRequest(new { message = "Tên tiêu chí đã tồn tại!" });
            }

            var parent = await _context.ParentCriteria.FindAsync(dto.ParentCriteriaId.Value);
            if (parent == null)
                return BadRequest(new { message = "Không tìm thấy chỉ tiêu cha!" });

            var unit = User.FindFirst("Unit")?.Value ?? "Không rõ đơn vị";

            var subCriteria = new SubCriteria
            {
                Name = dto.Name,
                MaxScore = dto.MaxScore,
                Description = dto.Description,
                EvidenceInfo = dto.EvidenceInfo,
                ParentCriteriaId = parent.Id,
                UnitEvaluate = unit,
                EvaluatedAt = dto.EvaluatedAt.HasValue
                    ? DateTime.SpecifyKind(dto.EvaluatedAt.Value, DateTimeKind.Utc)
                    : DateTime.UtcNow
            };

            _context.SubCriteria.Add(subCriteria);
            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync(
                "Create Sub Criteria", 
                $"Tạo tiêu chí con: {subCriteria.Name} (ID = {subCriteria.Id}) thuộc chỉ tiêu cha: {parent.Name} ({parent.Id})", 
                User.FindFirst(ClaimTypes.Name)?.Value
            );

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
            var existing = await _context.SubCriteria
                .Include(sc => sc.ParentCriteria)
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (existing == null)
                return NotFound(new { message = "Không tìm thấy tiêu chí con!" });

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var unit = User.FindFirst("Unit")?.Value;

            // Chỉ tiêu con sẽ bị khóa nếu kỳ đánh giá của tiêu chí cha đã bị khóa
            if (existing.ParentCriteria?.EvaluationPeriodId != null)
            {
                var period = await _context.EvaluationPeriod.FindAsync(existing.ParentCriteria.EvaluationPeriodId);
                if (period != null && period.IsLocked)
                {
                    return BadRequest(new { message = "Không thể chỉnh sửa vì kỳ đánh giá đã bị khóa!" });
                }
            }

            if (role != "admin" && existing.UnitEvaluate != unit)
            {
                return StatusCode(403, new { message = "Bạn không có quyền chỉnh sửa tiêu chí của đơn vị khác." });
            }

            if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != existing.Name)
            {
                bool isNameExists = await _context.SubCriteria.AnyAsync(p =>
                    p.Name == dto.Name &&
                    p.ParentCriteriaId == (dto.ParentCriteriaId ?? existing.ParentCriteriaId) &&
                    p.Id != id);

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

            if (dto.ParentCriteriaId.HasValue && dto.ParentCriteriaId.Value != existing.ParentCriteriaId)
            {
                var parent = await _context.ParentCriteria.FindAsync(dto.ParentCriteriaId.Value);
                if (parent == null)
                    return BadRequest(new { message = "Không tìm thấy chỉ tiêu cha!" });

                existing.ParentCriteriaId = parent.Id;
            }

            if (dto.EvaluatedAt.HasValue)
                existing.EvaluatedAt = dto.EvaluatedAt.Value;

            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync(
                "Update Sub Criteria", 
                $"Cập nhật tiêu chí con: {existing.Name} (ID = {existing.Id})", 
                User.FindFirst(ClaimTypes.Name)?.Value);

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

            await _logService.WriteLogAsync(
                "Delete Sub Criteria", 
                $"Xóa tiêu chí con: {subCriteria.Name} (ID = {subCriteria.Id})", 
                User.FindFirst(ClaimTypes.Name)?.Value
            );

            return Ok(new { message = "Xóa nhóm chỉ tiêu thành công!" });
        }

        //// GET: api/subcriterias/by-year?year={year}
        //[HttpGet("by-year")]
        //[Authorize]
        //public async Task<IActionResult> GetByYear([FromQuery] int? year)
        //{
        //    int targetYear = year ?? DateTime.Now.Year;

        //    var role = User.FindFirst("Role")?.Value;
        //    var unit = User.FindFirst("Unit")?.Value;

        //    var query = _context.SubCriteria
        //        .Include(p => p.ParentCriteria)
        //        .Where(p => p.EvaluatedAt != null && p.EvaluatedAt.Value.Year == targetYear);

        //    if (role != "admin")
        //    {
        //        query = query.Where(p => p.UnitEvaluate == unit);
        //    }

        //    if (role == "admin")
        //    {
        //        results = await query
        //            .Select(p => new
        //            {
        //                p.Id,
        //                p.Name,
        //                p.MaxScore,
        //                p.EvidenceInfo,
        //                p.EvaluatedAt,
        //                p.UnitEvaluate,
        //                Parent = new { p.ParentCriteria.Id, p.ParentCriteria.Name }
        //            })
        //            .ToListAsync<object>();
        //    }
        //    else
        //    {
        //        results = await query
        //            .Select(p => new
        //            {
        //                p.Id,
        //                p.Name,
        //                p.MaxScore,
        //                p.EvidenceInfo,
        //                p.EvaluatedAt,
        //                Parent = new { p.ParentCriteria.Id, p.ParentCriteria.Name }
        //            })
        //            .ToListAsync<object>();
        //    }

        //    return Ok(results);
        //}
    }
}
