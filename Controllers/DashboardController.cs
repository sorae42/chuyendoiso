using chuyendoiso.Data;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace chuyendoiso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly chuyendoisoContext _context;

        public DashboardController(chuyendoisoContext context)
        {
            _context = context;
        }

        // Sumary endpoint for the dashboard
        // GET: api/dashboard/summary
        [HttpGet("summary")]
        [Authorize]
        public async Task<IActionResult> GetSummary()
        {
            var totalUnits = await _context.Unit.CountAsync();
            var totalEvaluationPeriods = await _context.EvaluationPeriod.CountAsync();
            var totalUsers = await _context.Auth.CountAsync();
            var totalReviewCouncils = await _context.ReviewCouncil.CountAsync();
            var totalParentCriteria = await _context.ParentCriteria.CountAsync();
            var totalSubCriteria = await _context.SubCriteria.CountAsync();

            var totalCriteria = totalParentCriteria + totalSubCriteria;

            return Ok(new
            {
                totalUnits,
                totalEvaluationPeriods,
                totalUsers,
                totalReviewCouncils,
                totalCriteria
            });
        }

        /* Summary: Dashboard tiến độ kỳ đánh giá hiện tại
         - Kỳ đánh giá đang diễn ra
         - Tỷ lệ đơn vị đã nộp kết quả
        */
        // GET: api/dashboard/current-progress
        [HttpGet("current-progress")]
        [Authorize]
        public async Task<IActionResult> GetCurrentProgress()
        {
            var now = DateTime.UtcNow;

            var currentPeriod = await _context.EvaluationPeriod
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefaultAsync(p => p.StartDate <= now && p.EndDate >= now);

            if (currentPeriod == null)
                return Ok(new { message = "Không có kỳ đánh giá hiện tại" });

            var totalUnits = await _context.Unit.CountAsync();
            var completedUnits = await _context.EvaluationUnit
                .Where(e => e.EvaluationPeriodId == currentPeriod.Id)
                .Select(e => e.UnitId)
                .Distinct()
                .CountAsync();

            var progressPercent = totalUnits == 0 ? 0 : Math.Round((double)completedUnits / totalUnits * 100, 2);

            return Ok(new
            {
                currentPeriod.Id,
                currentPeriod.Name,
                totalUnits,
                completedUnits,
                progressPercent
            });
        }

        // Summary: Thống kê top đơn vị theo điểm số
        // GET: api/dashboard/top-units
        [HttpGet("top-units")]
        [Authorize]
        public async Task<IActionResult> GetTopUnits([FromQuery] int evaluationPeriodId, [FromQuery] int top = 5)
        {
            // Lấy danh sách UnitId tham gia kỳ đánh giá đó
            var unitIds = await _context.EvaluationUnit
                .Where(eu => eu.EvaluationPeriodId == evaluationPeriodId)
                .Select(eu => eu.UnitId)
                .ToListAsync();

            if (unitIds.Count == 0)
            {
                return Ok(new { message = "Không có đơn vị nào trong kỳ đánh giá này" });
            }

            // Tìm điểm số của các ReviewResult liên quan tới các Unit đó
            var topUnits = await _context.ReviewResult
                .Include(r => r.ReviewAssignment)
                    .ThenInclude(ra => ra.Unit)
                .Where(r => unitIds.Contains(r.ReviewAssignment.UnitId) && r.Score != null)
                .GroupBy(r => new { r.ReviewAssignment.Unit.Id, r.ReviewAssignment.Unit.Name })
                .Select(g => new
                {
                    g.Key.Id,
                    g.Key.Name,
                    AverageScore = g.Average(r => r.Score)
                })
                .OrderByDescending(x => x.AverageScore)
                .Take(top)
                .ToListAsync();

            return Ok(topUnits);
        }

        // Summary: API cảnh báo đơn vị chưa thẩm định tiêu chí
        // GET: api/dashboard/alerts
        [HttpGet("alerts")]
        [Authorize]
        public async Task<IActionResult> GetAlerts()
        {
            var now = DateTime.UtcNow;
            var currentPeriod = await _context.EvaluationPeriod
                .OrderByDescending(p => p.StartDate)
                .FirstOrDefaultAsync(p => p.StartDate <= now && p.EndDate >= now);

            if (currentPeriod == null)
                return Ok(new { message = "Không có kỳ đánh giá hiện tại" });

            var completedUnits = await _context.EvaluationUnit
                .Where(e => e.EvaluationPeriodId == currentPeriod.Id)
                .Select(e => e.UnitId)
                .Distinct()
                .ToListAsync();

            var pendingUnits = await _context.Unit
                .Where(u => !completedUnits.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Code
                })
                .ToListAsync();

            return Ok(new
            {
                pendingUnits.Count,
                pendingUnits
            });
        }
    }
}
