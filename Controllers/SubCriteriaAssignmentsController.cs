using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using chuyendoiso.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using chuyendoiso.DTOs;

namespace chuyendoiso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubCriteriaAssignmentsController : ControllerBase
    {
        private readonly chuyendoisoContext _context;

        public SubCriteriaAssignmentsController(chuyendoisoContext context)
        {
            _context = context;
        }

        // Summary: Đơn vị xem tiêu chí được giao trong kỳ đánh giá
        // GET: api/subcriteriaassignments/my-evaluation-periods
        [HttpGet("my-evaluation-periods")]
        [Authorize]
        public async Task<IActionResult> GetMyEvaluationPeriods()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if(!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Không xác định được người dùng!" });

            // Xác định đơn vị hiện tại
            int? unitId = null;
            if (role != "admin")
            {
                var unitIdFromDb = await _context.Auth
                    .Where(u => u.Id == userId)
                    .Select(u => u.UnitId)
                    .FirstOrDefaultAsync();

                if (unitIdFromDb == 0)
                    return NotFound(new { message = "Không tìm thấy đơn vị!" });

                unitId = unitIdFromDb;
            }

            var periods = await _context.EvaluationPeriod
                .Include(p => p.ParentCriterias)
                .ThenInclude(pc => pc.SubCriterias)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.StartDate,
                    p.EndDate,
                    SubCriterias = p.ParentCriterias
                .SelectMany(pc => pc.SubCriterias)
                .Where(sc => role == "admin" || _context.SubCriteriaAssignment.Any(a => a.SubCriteriaId == sc.Id && a.UnitId == unitId))
                .Select(sc => new
                {
                    sc.Id,
                    sc.Name,
                    sc.Description,
                    sc.MaxScore,
                    sc.EvidenceInfo,
                    Assignment = _context.SubCriteriaAssignment
                        .Where(a => a.SubCriteriaId == sc.Id && (role == "admin" || a.UnitId == unitId))
                        .Select(a => new
                        {
                            a.Id,
                            a.Score,
                            a.Comment,
                            a.EvidenceInfo,
                            a.EvaluatedAt,
                            UnitName = a.Unit.Name
                        })
                        .FirstOrDefault()
                    })
                })
                .Where(p => p.SubCriterias.Any())
                .ToListAsync();

            return Ok(periods);
        }

        // POST: api/unitassignments/submit
        // Submit mới điểm đánh giá cho tiêu chí trong kỳ của đơn vị
        [HttpPost("submit")]
        [Authorize]
        public async Task<IActionResult> Submit([FromBody] SubmitCriteriaDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Không xác định được người dùng!" });

            int? unitId = null;
            if (role != "admin")
            {
                unitId = await _context.Auth
                    .Where(u => u.Id == userId)
                    .Select(u => u.UnitId)
                    .FirstOrDefaultAsync();

                if (unitId == null || unitId == 0)
                    return NotFound(new { message = "Không tìm thấy đơn vị!" });
            }

            var assignment = await _context.SubCriteriaAssignment
                .Include(a => a.EvaluationPeriod)
                .Include(a => a.Unit)
                .Include(a => a.SubCriteria)
                .FirstOrDefaultAsync(a =>
                    a.SubCriteriaId == dto.SubCriteriaId &&
                    a.EvaluationPeriodId == dto.PeriodId &&
                    (role == "admin" || a.Unit.Id == unitId));

            if (assignment == null)
                return NotFound(new { message = "Không tìm thấy nhiệm vụ đánh giá phù hợp!" });

            if (assignment.EvaluationPeriod.IsLocked)
                return BadRequest(new { message = "Kỳ đánh giá đã bị khóa!" });

            assignment.Score = dto.Score;
            assignment.Comment = dto.Comment;
            assignment.EvidenceInfo = dto.EvidenceInfo;
            assignment.EvaluatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Nộp kết quả thành công!",
                assignmentId = assignment.Id,
                evaluatedAt = assignment.EvaluatedAt
            });
        }


        // PUT: api/unitassignments/submit/{assignmentId}
        // Chỉnh sửa kết quả đánh giá đã nộp
        [HttpPut("submit/{assignmentId}")]
        [Authorize]
        public async Task<IActionResult> Edit(int assignmentId, [FromBody] SubmitCriteriaDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Không xác định được người dùng!" });

            int? unitId = null;
            if (role != "admin")
            {
                unitId = await _context.Auth
                    .Where(u => u.Id == userId)
                    .Select(u => u.UnitId)
                    .FirstOrDefaultAsync();

                if (unitId == null || unitId == 0)
                    return NotFound(new { message = "Không tìm thấy đơn vị!" });
            }

            var assignment = await _context.SubCriteriaAssignment
                .Include(a => a.EvaluationPeriod)
                .Include(a => a.SubCriteria)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null)
                return NotFound(new { message = "Không tìm thấy nhiệm vụ đánh giá!" });

            if (assignment.EvaluationPeriod.IsLocked)
                return BadRequest(new { message = "Kỳ đánh giá đã bị khóa!" });

            if (role != "admin" && assignment.UnitId != unitId)
                return Forbid("Bạn không có quyền chỉnh sửa đánh giá này!");

            assignment.Score = dto.Score;
            assignment.Comment = dto.Comment;
            assignment.EvidenceInfo = dto.EvidenceInfo;
            assignment.EvaluatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new 
            { 
                message = "Cập nhật thành công!", 
                evaluatedAt = assignment.EvaluatedAt 
            });
        }
    }
}