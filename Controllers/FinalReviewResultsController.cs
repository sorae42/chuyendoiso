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
        [Authorize]
        public async Task<IActionResult> ListFinalReviewResults()
        {
            var results = await _context.FinalReviewResult
                .Include(r => r.ReviewAssignment)
                    .ThenInclude(a => a.Unit)
                .Include(r => r.ReviewAssignment.SubCriteria)
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
        [Authorize]
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
        [Authorize(Roles = "chair,admin")]
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

            var reviewerIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(reviewerIdStr, out int reviewerId);

            await _log.WriteLogAsync(
                "Phê duyệt kết quả thẩm định (Admin)",
                $"Chủ tịch hội đồng duyệt kết quả cho nhiệm vụ thẩm định ID={assignment.Id} với điểm {dto.FinalScore}",
                User.FindFirst(ClaimTypes.Name)?.Value,
                relatedUserId: reviewerId
            );


            await _log.WriteLogAsync(
                "Phê duyệt kết quả thẩm định", 
                $"Chủ tịch hội đồng duyệt điểm thẩm định: {assignment.Id}", 
                User.FindFirst(ClaimTypes.Name)?.Value
            );

            return Ok(new { message = "Duyệt điểm thẩm định thành công!" });
        }

        // POST: api/finalreviewresults/reject
        // Summary: Chủ tịch hội đồng từ chối kết quả thẩm định
        // Params: reviewAssignmentId, rejectReason, attachment?, isFinalFail
        [HttpPost("reject")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> RejectFinalReview([FromForm] FinalReviewRejectionDto dto)
        {
            var assignment = await _context.ReviewAssignment
                .Include(a => a.Reviewer).ThenInclude(r => r.Auth)
                .Include(a => a.Unit)
                .Include(a => a.SubCriteria)
                .FirstOrDefaultAsync(a => a.Id == dto.ReviewAssignmentId);

            if (assignment == null)
                return NotFound("Không tìm thấy nhiệm vụ!");

            var result = await _context.FinalReviewResult
                .FirstOrDefaultAsync(r => r.ReviewAssignmentId == dto.ReviewAssignmentId);

            if (result == null)
                return NotFound("Chưa có kết quả để từ chối!");

            if (string.IsNullOrWhiteSpace(dto.RejectReason))
                return BadRequest("Lý do từ chối là bắt buộc.");

            string? filePath = null;
            if (dto.Attachment != null)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads/final-rejects");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid()}_{dto.Attachment.FileName}";
                var fullPath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.Attachment.CopyToAsync(stream);
                }

                filePath = $"/uploads/final-rejects/{fileName}";
            }

            result.IsRejected = true;
            result.RejectReason = dto.RejectReason;
            result.RejectAttachmentPath = filePath;
            result.IsFinalFail = dto.IsFinalFail;

            await _context.SaveChangesAsync();

            var chair = User.FindFirst(ClaimTypes.Name)?.Value;
            var reviewerUserId = assignment.Reviewer.AuthId;

            // Log cho admin
            await _log.WriteLogAsync(
                "Chủ tịch từ chối kết quả thẩm định",
                $"Tiêu chí '{assignment.SubCriteria?.Name}' của đơn vị '{assignment.Unit.Name}' bị từ chối bởi chủ tịch.",
                chair
            );

            // Log cá nhân cho reviewer
            await _log.WriteLogAsync(
                "Kết quả bị từ chối bởi chủ tịch",
                $"Kết quả đánh giá của bạn cho tiêu chí '{assignment.SubCriteria?.Name}' đã bị từ chối. Lý do: {dto.RejectReason}",
                chair,
                relatedUserId: reviewerUserId
            );

            return Ok(new
            {
                message = "Từ chối kết quả thành công. Reviewer sẽ được thông báo để gửi lại.",
                isFinalFail = dto.IsFinalFail
            });
        }

        // POST: api/finalreviewresults/resubmit
        // Summary: Reviewer nộp lại kết quả sau khi bị từ chối
        [HttpPost("resubmit")]
        [Authorize]
        public async Task<IActionResult> ResubmitReview([FromForm] ReviewResubmitDto dto)
        {
            var assignment = await _context.ReviewAssignment
                .Include(a => a.Reviewer).ThenInclude(r => r.Auth)
                .Include(a => a.Unit)
                .Include(a => a.SubCriteria)
                .FirstOrDefaultAsync(a => a.Id == dto.ReviewAssignmentId);

            if (assignment == null)
                return NotFound("Không tìm thấy nhiệm vụ!");

            var result = await _context.FinalReviewResult
                .FirstOrDefaultAsync(r => r.ReviewAssignmentId == dto.ReviewAssignmentId);

            if (result == null || !result.IsRejected)
                return BadRequest("Chưa từng bị từ chối hoặc chưa có kết quả nào để gửi lại!");

            if (result.IsFinalFail)
                return BadRequest("Chủ tịch đã đánh giá kết quả không đạt. Bạn không được phép gửi lại.");

            var username = User.Identity?.Name;
            if (assignment.Reviewer.Auth.Username != username)
                return Forbid("Bạn không có quyền nộp lại kết quả này.");

            string? filePath = null;
            if (dto.Attachment != null)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads/resubmits");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid()}_{dto.Attachment.FileName}";
                var fullPath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.Attachment.CopyToAsync(stream);
                }

                filePath = $"/uploads/resubmits/{fileName}";
            }

            // Ghi đè kết quả cũ
            result.FinalScore = dto.Score;
            result.FinalComment = dto.Comment;
            result.FinalAttachmentPath = filePath;
            result.IsRejected = false;
            result.RejectReason = null;
            result.RejectAttachmentPath = null;
            result.IsFinalFail = false;

            await _context.SaveChangesAsync();

            await _log.WriteLogAsync(
                "Nộp lại kết quả sau khi bị từ chối",
                $"Reviewer '{username}' đã gửi lại kết quả cho tiêu chí '{assignment.SubCriteria?.Name}' của đơn vị '{assignment.Unit.Name}'.",
                username,
                relatedUserId: assignment.Reviewer.AuthId
            );

            await _log.WriteLogAsync(
                "Reviewer gửi lại kết quả thẩm định",
                $"Kết quả cho tiêu chí '{assignment.SubCriteria?.Name}' của đơn vị '{assignment.Unit.Name}' đã được nộp lại.",
                username
            );

            return Ok(new { message = "Nộp lại kết quả thành công!" });
        }

        // POST: api/finalreviewresults/update
        // Summary: Chủ tịch cập nhật kết quả thẩm định
        //Params: reviewAssignmentId, finalScore, finalComment?, finalAttachment?
        [HttpPost("update")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateFinalReview([FromForm] FinalReviewResultDto dto)
        {
            var result = await _context.FinalReviewResult
                .Include(r => r.ReviewAssignment)
                    .ThenInclude(a => a.Unit)
                .Include(r => r.ReviewAssignment.SubCriteria)
                .FirstOrDefaultAsync(r => r.ReviewAssignmentId == dto.ReviewAssignmentId);

            if (result == null)
                return NotFound("Không tìm thấy kết quả thẩm định để cập nhật!");

            string? filePath = result.FinalAttachmentPath;

            if (dto.FinalAttachment != null)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads/finalreviews");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid()}_{dto.FinalAttachment.FileName}";
                var fullPath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.FinalAttachment.CopyToAsync(stream);
                }

                filePath = $"/uploads/finalreviews/{fileName}";
            }

            result.FinalScore = dto.FinalScore;
            result.FinalComment = dto.FinalComment;
            result.FinalAttachmentPath = filePath;

            await _context.SaveChangesAsync();

            var assignment = result.ReviewAssignment;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            await _log.WriteLogAsync(
                "Cập nhật kết quả thẩm định (chủ tịch)",
                $"Bạn đã chỉnh sửa kết quả tiêu chí '{assignment.SubCriteria?.Name}' của đơn vị '{assignment.Unit.Name}'.",
                username
            );

            return Ok(new { message = "Cập nhật kết quả thẩm định thành công!" });
        }
    }
}
