using chuyendoiso.Data;
using chuyendoiso.DTOs;
using chuyendoiso.Models;
using chuyendoiso.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace chuyendoiso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinalReviewResultsController : ControllerBase
    {
        private readonly chuyendoisoContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly LogService _log;

        public FinalReviewResultsController(chuyendoisoContext context, IWebHostEnvironment env, LogService logService)
        {
            _context = context;
            _env = env;
            _log = logService;
        }

        // GET: api/finalreviewresults/list
        // Summary: Lấy danh sách điểm thẩm định của hội đồng
        [HttpGet("list")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ListFinalReviewResults()
        {
            var results = await _context.FinalReviewResult
                .Include(r => r.ReviewAssignment)
                .ThenInclude(a => a.Unit)
                .Select(r => new
                {
                    r.Id,
                    r.FinalScore,
                    r.FinalComment,
                    r.FinalAttachmentPath,
                    UnitName = r.ReviewAssignment.Unit.Name,
                    SubCriteriaName = r.ReviewAssignment.SubCriteria != null ? r.ReviewAssignment.SubCriteria.Name : null
                })
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/finalreviewresults/dashboard
        // Summary: Dashboard tổng hợp kết quả thẩm định (nhóm theo đơn vị)
        [HttpGet("dashboard")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Dashboard()
        {
            var data = await _context.FinalReviewResult
                .Include(r => r.ReviewAssignment)
                .ThenInclude(a => a.Unit)
                .GroupBy(r => r.ReviewAssignment.Unit.Name)
                .Select(g => new
                {
                    UnitName = g.Key,
                    TotalCriteria = g.Count(),
                    AverageFinalScore = g.Average(r => r.FinalScore ?? 0)
                })
                .ToListAsync();

            return Ok(data);
        }

        // POST: api/finalreviewresults/submit
        // Summary: Chủ tịch hội đồng duyệt điểm thẩm định
        // Params: id， score, comment?, attachment?
        [HttpPost("submit")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> SubmitFinalReview([FromBody] FinalReviewResultDto dto)
        {
            var assignment = await _context.ReviewAssignment.FindAsync(dto.ReviewAssignmentId);
            if (assignment == null)
            {
                return NotFound("Không tìm thấy nhiệm vụ thẩm định!");
            }

            string? filePath = null;
            if (dto.FinalAttachment != null)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads/reviewresults");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid()}_{dto.FinalAttachment.FileName}";
                var fullPath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.FinalAttachment.CopyToAsync(stream);
                }
                filePath = $"/uploads/finalreviews/{fileName}";
            }

            var result = new FinalReviewResult
            {
                ReviewAssignmentId = dto.ReviewAssignmentId,
                FinalScore = dto.FinalScore,
                FinalComment = dto.FinalComment,
                FinalAttachmentPath = filePath
            };

            _context.FinalReviewResult.Add(result);
            await _context.SaveChangesAsync();

            await _log.WriteLogAsync(
                "Submit Final Review", 
                $"Chủ tịch hội đồng duyệt điểm thẩm định: {assignment.Id}", 
                User.FindFirst(ClaimTypes.Name)?.Value
            );

            return Ok(new { message = "Duyệt điểm thẩm định thành công!" });
        }
    }
}
