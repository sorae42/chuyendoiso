using chuyendoiso.Data;
using chuyendoiso.DTOs;
using chuyendoiso.Models;
using chuyendoiso.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
            var assignment = await _context.ReviewAssignment
                .Include(a => a.Reviewer)
                    .ThenInclude(r => r.Auth)
                .FirstOrDefaultAsync(a => a.Id == dto.ReviewAssignmentId);

            if (assignment == null)
            {
                return NotFound("Không tìm thấy nhiệm vụ thẩm định!");
            }

            // Kiểm tra quyền của người dùng
            if (assignment.Reviewer.Auth.Username != User.Identity?.Name)
            {
                return Forbid("Bạn không có quyền nộp kết quả thẩm định cho nhiệm vụ này!");
            }

            if (assignment.IsDeclined)
                return BadRequest("Bạn đã từ chối nhiệm vụ này, không thể nộp kết quả.");

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
                "Nộp kết quả thẩm định",
                $"Thành viên {User.Identity?.Name} đã nộp kết quả thẩm định nhiệm vụ {dto.ReviewAssignmentId}",
                User.FindFirst(ClaimTypes.Name)?.Value
            );

            return Ok(new { message = "Nộp kết quả thẩm định thành công!" });
        }

        // POST: /api/reviewresult/decline
        // Params: ReviewAssignmentId, Reason, Attachment
        [HttpPost("decline")]
        [Authorize]
        public async Task<IActionResult> DeclineAssignment([FromForm] ReviewDeclineDto dto)
        {
            var assignment = await _context.ReviewAssignment
                .Include(a => a.Reviewer)
                    .ThenInclude(r => r.Auth)
                .Include(a => a.Unit)
                .Include(a => a.SubCriteria)
                .FirstOrDefaultAsync(a => a.Id == dto.ReviewAssignmentId);

            if (assignment == null)
                return NotFound("Không tìm thấy nhiệm vụ thẩm định!");

            if (assignment.Reviewer.Auth.Username != User.Identity?.Name)
                return Forbid("Bạn không có quyền từ chối nhiệm vụ này!");

            if (assignment.IsDeclined)
                return BadRequest("Nhiệm vụ này đã bị từ chối trước đó.");

            if (string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest("Vui lòng nhập lý do từ chối!");

            string? filePath = null;
            if (dto.Attachment != null)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads/review-declines");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid()}_{dto.Attachment.FileName}";
                var fullPath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.Attachment.CopyToAsync(stream);
                }

                filePath = $"/uploads/review-declines/{fileName}";
            }

            assignment.IsDeclined = true;
            assignment.DeclineReason = dto.Reason;
            assignment.DeclinedAt = DateTime.UtcNow;
            assignment.DeclineAttachmentPath = filePath;
            assignment.IsUpdatedByUnit = false;

            await _context.SaveChangesAsync();

            var reviewerName = assignment.Reviewer.Auth.Username;
            var unitName = assignment.Unit.Name;
            var criteriaName = assignment.SubCriteria?.Name;

            // 1. Log chung cho admin
            await _log.WriteLogAsync(
                "Từ chối kết quả tự đánh giá",
                $"Reviewer '{reviewerName}' đã từ chối tiêu chí '{criteriaName}' của đơn vị '{unitName}'.",
                User.FindFirst(ClaimTypes.Name)?.Value
            );

            // 2. Log cho reviewer
            await _log.WriteLogAsync(
                "Bạn đã từ chối nhiệm vụ",
                $"Bạn đã từ chối đánh giá tiêu chí '{criteriaName}' của đơn vị '{unitName}'.",
                reviewerName,
                relatedUserId: assignment.Reviewer.AuthId
            );

            // 3. Log cho đơn vị
            var unitUser = await _context.Auth.FirstOrDefaultAsync(a => a.UnitId == assignment.UnitId);
            if (unitUser != null)
            {
                await _log.WriteLogAsync(
                    "Tiêu chí bị từ chối",
                    $"Tiêu chí '{criteriaName}' của đơn vị bạn đã bị từ chối. Vui lòng cập nhật lại để được đánh giá lại.",
                    reviewerName,
                    relatedUserId: unitUser.Id
                );
            }

            return Ok(new { message = "Từ chối đánh giá thành công. Đơn vị sẽ được thông báo để cập nhật lại." });
        }

        [HttpPost("undo-decline")]
        [Authorize]
        public async Task<IActionResult> UndoDecline([FromBody] int reviewAssignmentId)
        {
            var assignment = await _context.ReviewAssignment
                .Include(a => a.Reviewer).ThenInclude(r => r.Auth)
                .Include(a => a.Unit)
                .Include(a => a.SubCriteria)
                .FirstOrDefaultAsync(a => a.Id == reviewAssignmentId);

            if (assignment == null)
                return NotFound(new { message = "Không tìm thấy nhiệm vụ!" });

            var username = User.Identity?.Name;
            if (assignment.Reviewer.Auth.Username != username)
                return Forbid("Bạn không có quyền gỡ từ chối cho nhiệm vụ này!");

            if (!assignment.IsDeclined)
                return BadRequest(new { message = "Nhiệm vụ này chưa từng bị từ chối!" });

            if (!assignment.IsUpdatedByUnit)
                return BadRequest(new { message = "Đơn vị chưa cập nhật lại tiêu chí này!" });

            assignment.IsDeclined = false;
            assignment.DeclineReason = null;
            assignment.DeclineAttachmentPath = null;
            assignment.IsUpdatedByUnit = false;

            await _context.SaveChangesAsync();

            await _log.WriteLogAsync(
                "Gỡ từ chối nhiệm vụ",
                $"Bạn đã gỡ trạng thái từ chối cho tiêu chí '{assignment.SubCriteria?.Name}' của đơn vị '{assignment.Unit.Name}'.",
                username,
                relatedUserId: assignment.Reviewer.AuthId
            );

            await _log.WriteLogAsync(
                "Reviewer tiếp tục thẩm định",
                $"Reviewer '{username}' đã đồng ý đánh giá lại tiêu chí '{assignment.SubCriteria?.Name}' của đơn vị '{assignment.Unit.Name}'.",
                username
            );

            return Ok(new { message = "Đã gỡ trạng thái từ chối. Bạn có thể tiếp tục đánh giá tiêu chí này." });
        }

        // POST: /api/reviewresults/update
        // Summary: Cập nhật kết quả thẩm định của thành viên hội đồng
        // Params: ReviewAssignmentId, score, comment? or attachment?
        [HttpPost("update")]
        [Authorize]
        public async Task<IActionResult> UpdateReview([FromForm] ReviewResultDto dto)
        {
            var assignment = await _context.ReviewAssignment
                .Include(a => a.Reviewer)
                    .ThenInclude(r => r.Auth)
                .Include(a => a.Unit)
                .Include(a => a.SubCriteria)
                .FirstOrDefaultAsync(a => a.Id == dto.ReviewAssignmentId);

            if (assignment == null)
                return NotFound("Không tìm thấy nhiệm vụ thẩm định!");

            if (assignment.Reviewer.Auth.Username != User.Identity?.Name)
                return Forbid("Bạn không có quyền cập nhật kết quả thẩm định cho nhiệm vụ này!");

            var result = await _context.ReviewResult
                .FirstOrDefaultAsync(r => r.ReviewAssignmentId == dto.ReviewAssignmentId);

            if (result == null)
                return NotFound("Không tìm thấy kết quả để cập nhật!");

            string? filePath = result.AttachmentPath;

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

            result.Score = dto.Score;
            result.Comment = dto.Comment;
            result.AttachmentPath = filePath;

            await _context.SaveChangesAsync();

            var reviewerName = assignment.Reviewer.Auth.Username;
            var unitName = assignment.Unit.Name;
            var criteriaName = assignment.SubCriteria?.Name;

            await _log.WriteLogAsync(
                "Cập nhật kết quả thẩm định",
                $"Bạn đã cập nhật lại kết quả tiêu chí '{criteriaName}' cho đơn vị '{unitName}'.",
                reviewerName,
                relatedUserId: assignment.Reviewer.AuthId
            );

            await _log.WriteLogAsync(
                "Kết quả thẩm định được cập nhật",
                $"Thành viên '{reviewerName}' đã chỉnh sửa lại kết quả tiêu chí '{criteriaName}' của đơn vị '{unitName}'.",
                reviewerName
            );

            return Ok(new { message = "Cập nhật kết quả thẩm định thành công!" });
        }

    }
}
