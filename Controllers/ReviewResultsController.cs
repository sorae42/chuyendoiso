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
    public class ReviewResultsController : ControllerBase
    {
        private readonly chuyendoisoContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly LogService _log;

        public ReviewResultsController(chuyendoisoContext context, IWebHostEnvironment env, LogService logService)
        {
            _context = context;
            _env = env;
            _log = logService;
        }

        // GET: api/reviewresults/list?reviewerId=1
        // Summary: Lấy danh sách kết quả thẩm định của thành viên hội đồng
        // Params: reviewerId
        [HttpGet("list")]
        [Authorize]
        public async Task<IActionResult> GetReviewResults(int reviewerId)
        {
            var results = await _context.ReviewResult
                .Include(r => r.ReviewAssignment)
                .ThenInclude(a => a.Unit)
                .Where(r => r.ReviewAssignment.ReviewerId == reviewerId)
                .Select(r => new
                {
                    r.Id,
                    r.Score,
                    r.Comment,
                    r.AttachmentPath,
                    UnitName = r.ReviewAssignment.Unit.Name,
                    SubCriteriaName = r.ReviewAssignment.SubCriteria != null ? r.ReviewAssignment.SubCriteria.Name : null
                })
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/reviewresults/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetReviewResultDetail(int id)
        {
            var result = await _context.ReviewResult
                .Include(r => r.ReviewAssignment)
                    .ThenInclude(a => a.Unit)
                .Include(r => r.ReviewAssignment.SubCriteria)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (result == null)
                return NotFound(new { message = "Không tìm thấy kết quả thẩm định!" });

            return Ok(new
            {
                result.Id,
                result.Score,
                result.Comment,
                result.AttachmentPath,
                Unit = new
                {
                    result.ReviewAssignment.Unit.Id,
                    result.ReviewAssignment.Unit.Name,
                    result.ReviewAssignment.Unit.Code
                },
                SubCriteria = result.ReviewAssignment.SubCriteria != null
                    ? new { result.ReviewAssignment.SubCriteria.Id, result.ReviewAssignment.SubCriteria.Name }
                    : null
            });
        }

        // POST: api/reviewresults/submit
        // Summary: Thành viên hội đồng nộp kết quả thẩm định đơn vị
        // Params: ReviewAssignmentId, score, comment? or attachment?
        [HttpPost("submit")]
        [Authorize]
        public async Task<IActionResult> SubmitReview([FromBody] ReviewResultDto dto)
        {
            var assignment = await _context.ReviewAssignment.FindAsync(dto.ReviewAssignmentId);
            if (assignment == null)
            {
                return NotFound("Không tìm thấy nhiệm vụ thẩm định!");
            }

            string? filePath = null;
            if (dto.Attachment != null)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads/reviewresults");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid()}_{dto.Attachment.FileName}";
                var fullPath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.Attachment.CopyToAsync(stream);
                }
                filePath = $"/uploads/reviewresults/{fileName}";
            }

            var result = new ReviewResult
            {
                ReviewAssignmentId = dto.ReviewAssignmentId,
                Score = dto.Score,
                Comment = dto.Comment,
                AttachmentPath = filePath
            };

            _context.ReviewResult.Add(result);
            await _context.SaveChangesAsync();

            await _log.WriteLogAsync(
                "Submit Review Result",
                $"Thành viên {User.Identity?.Name} đã nộp kết quả thẩm định nhiệm vụ {dto.ReviewAssignmentId}", 
                User.FindFirst(ClaimTypes.Name)?.Value
            );

            return Ok(new { message = "Nộp kết quả thẩm định thành công!" });
        }
    }
}
