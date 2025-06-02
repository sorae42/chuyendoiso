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
        // GET: api/subcriteriaassignments/my-assignments?periodId=1
        [HttpGet("my-assignments")]
        [Authorize]
        public async Task<IActionResult> GetMyAssignments([FromBody] int periodId)
        {
            var unitName = User.FindFirst("Unit")?.Value;
            if (string.IsNullOrEmpty(unitName))
                return Unauthorized(new { message = "Không xác định được đơn vị!" });

            var unit = await _context.Unit.FirstOrDefaultAsync(u => u.Name == unitName);
            if (unit == null)
                return NotFound(new { message = "Không tìm thấy đơn vị!" });

            var assignments = await _context.SubCriteriaAssignment
                .Include(a => a.EvaluationPeriod)
                .Include(a => a.SubCriteria)
                .Where(a => a.UnitId == unit.Id && a.EvaluationPeriodId == periodId)
                .Select(a => new
                {
                    a.Id,
                    Subcriteria = new
                    {
                        a.SubCriteria.Id,
                        a.SubCriteria.Name,
                        a.SubCriteria.Description,
                        a.SubCriteria.MaxScore,
                        a.SubCriteria.EvidenceInfo
                    },
                    a.Score,
                    a.Comment,
                    a.EvidenceInfo,
                    a.EvaluatedAt,
                    PeriodName = a.EvaluationPeriod.Name,
                    PeriodStart = a.EvaluationPeriod.StartDate,
                    PeriodEnd = a.EvaluationPeriod.EndDate
                })
                .ToListAsync();

            return Ok(assignments);
        }

        // POST: api/submitcriteria/{id}
        [HttpPost("submit/{unitAssignmentId}")]
        [Authorize]
        public async Task<IActionResult> Submit(int unitAssignmentId, [FromBody] SubmitCriteriaDto dto)
        {
            // Lấy thông tin người dùng hiện tại
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var unitName = User.FindFirst("Unit")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(unitName))
                return Unauthorized(new { message = "Không xác định được người dùng hoặc đơn vị!" });

            var assignment = await _context.SubCriteriaAssignment
                .Include(a => a.EvaluationPeriod)
                .Include(a => a.Unit)
                .Include(a => a.SubCriteria)
                .FirstOrDefaultAsync(a => a.Id == unitAssignmentId);

            if (assignment == null)
                return NotFound(new { message = "Không tìm thấy nhiệm vụ đánh giá!" });

            if (assignment.Unit.Name != unitName)
                return Forbid("Bạn không có quyền nộp đánh giá cho tiêu chí này!");

            if (assignment.EvaluationPeriod.IsLocked)
                return BadRequest(new { message = "Kỳ đánh giá đã bị khóa!" });

            // Cập nhật kết quả
            assignment.Score = dto.Score;
            assignment.Comment = dto.Comment;
            assignment.EvidenceInfo = dto.EvidenceInfo;
            assignment.EvaluatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Nộp kết quả thành công!",
                evaluatedAt = assignment.EvaluatedAt,
                score = assignment.Score,
                comment = assignment.Comment,
                evidence = assignment.EvidenceInfo,
                criteria = assignment.SubCriteria.Name,
                period = assignment.EvaluationPeriod.Name
            });
        }
    }
}