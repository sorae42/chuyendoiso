﻿using chuyendoiso.Data;
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
    public class SubCriteriaAssignmentsController : ControllerBase
    {
        private readonly chuyendoisoContext _context;
        private readonly LogService _logService;
        private readonly IWebHostEnvironment _env;

        public SubCriteriaAssignmentsController(chuyendoisoContext context, LogService logService, IWebHostEnvironment env)
        {
            _context = context;
            _logService = logService;
            _env = env;
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
                unitId = await _context.Auth
                    .Where(u => u.Id == userId)
                    .Select(u => u.UnitId)
                    .FirstOrDefaultAsync();

                if (unitId == null || unitId == 0)
                    return NotFound(new { message = "Không tìm thấy đơn vị!" });
            }

            var assignmentsQuery = _context.SubCriteriaAssignment
                .Include(a => a.SubCriteria)
                    .ThenInclude(sc => sc.ParentCriteria)
                .Include(a => a.Unit)
                .Include(a => a.EvaluationPeriod)
                .AsQueryable();

            if (role != "admin")
                assignmentsQuery = assignmentsQuery.Where(a => a.UnitId == unitId);

            var assignments = await assignmentsQuery.ToListAsync();

            var grouped = assignments
                .GroupBy(a => a.EvaluationPeriod)
                .Select(g => new
                {
                    Id = g.Key.Id,
                    Name = g.Key.Name,
                    StartDate = g.Key.StartDate,
                    EndDate = g.Key.EndDate,
                    SubCriterias = g.Select(a => new
                    {
                        Id = a.SubCriteria.Id,
                        Name = a.SubCriteria.Name,
                        Description = a.SubCriteria.Description,
                        MaxScore = a.SubCriteria.MaxScore,
                        EvidenceInfo = a.SubCriteria.EvidenceInfo,
                        Assignment = new
                        {
                            a.Id,
                            a.Score,
                            a.Comment,
                            a.EvidenceInfo,
                            a.EvaluatedAt,
                            UnitName = a.Unit.Name
                        }
                    }).ToList()
                })
                .ToList();

            return Ok(grouped);
        }

        // GET: api/subcriteriaassignments/scored?unitId=5&periodId=2
        // Summary: Lấy danh sách các assignment đã được đánh giá
        [HttpGet("scored")]
        [Authorize]
        public async Task<IActionResult> GetScoredAssignments([FromQuery] int? unitId, [FromQuery] int? periodId)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Không xác định được người dùng!" });

            if (role != "admin")
            {
                unitId = await _context.Auth
                    .Where(u => u.Id == userId)
                    .Select(u => u.UnitId)
                    .FirstOrDefaultAsync();

                if (unitId == null || unitId == 0)
                    return NotFound(new { message = "Không tìm thấy đơn vị!" });
            }

            var query = _context.SubCriteriaAssignment
                .Include(a => a.Unit)
                .Include(a => a.EvaluationPeriod)
                .Include(a => a.SubCriteria)
                .AsQueryable();

            if (unitId.HasValue)
                query = query.Where(a => a.UnitId == unitId.Value);

            if (periodId.HasValue)
                query = query.Where(a => a.EvaluationPeriodId == periodId.Value);

            query = query.Where(a => a.EvaluatedAt != null);

            var results = await query
                .Select(a => new
                {
                    a.Id,
                    PeriodId = a.EvaluationPeriod.Id,
                    PeriodName = a.EvaluationPeriod.Name,
                    UnitId = a.Unit.Id,
                    UnitName = a.Unit.Name,
                    SubCriteriaId = a.SubCriteria.Id,
                    SubCriteriaName = a.SubCriteria.Name,
                    a.Score,
                    a.Comment,
                    a.EvidenceInfo,
                    a.EvaluatedAt
                })
                .OrderByDescending(a => a.EvaluatedAt)
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/subcriteriaassignments/{id}
        // Lấy chi tiết điểm đánh giá theo ID assignment
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetAssignmentDetail(int id)
        {
            var assignment = await _context.SubCriteriaAssignment
                .Include(a => a.Unit)
                .Include(a => a.SubCriteria)
                .Include(a => a.EvaluationPeriod)
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    a.Id,
                    SubCriteria = new
                    {
                        a.SubCriteria.Id,
                        a.SubCriteria.Name,
                        a.SubCriteria.Description,
                        a.SubCriteria.MaxScore,
                        a.SubCriteria.EvidenceInfo
                    },
                    EvaluationPeriod = new
                    {
                        a.EvaluationPeriod.Id,
                        a.EvaluationPeriod.Name,
                        a.EvaluationPeriod.StartDate,
                        a.EvaluationPeriod.EndDate,
                        a.EvaluationPeriod.IsLocked
                    },
                    Unit = new
                    {
                        a.Unit.Id,
                        a.Unit.Name
                    },
                    a.Score,
                    a.Comment,
                    a.EvidenceInfo,
                    a.EvaluatedAt
                })
                .FirstOrDefaultAsync();

            if (assignment == null)
                return NotFound(new { message = "Không tìm thấy kết quả đánh giá!" });

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Không xác định được người dùng!" });

            if (role != "admin")
            {
                var unitId = await _context.Auth
                    .Where(u => u.Id == userId)
                    .Select(u => u.UnitId)
                    .FirstOrDefaultAsync();

                if (unitId == 0 || assignment.Unit.Id != unitId)
                    return Forbid("Bạn không có quyền truy cập đánh giá này!");
            }

            return Ok(assignment);
        }

        // Summary: Tạo endpoint gán tiêu chí cho đơn vị trong kỳ
        // POST: api/subcriteriaassignments/assign-unit
        [HttpPost("assign-unit")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AssignUnit([FromBody] SeedAssignmentDto dto)
        {
            // Kiểm tra tồn tại các đối tượng liên quan
            var subCriteria = await _context.SubCriteria.FindAsync(dto.SubCriteriaId);
            var period = await _context.EvaluationPeriod.FindAsync(dto.PeriodId);
            var unit = await _context.Unit.FindAsync(dto.UnitId);

            if (subCriteria == null)
                return BadRequest(new { message = "Không tìm thấy tiêu chí con!" });

            if (period == null)
                return BadRequest(new { message = "Không tìm thấy kỳ đánh giá!" });

            if (unit == null)
                return BadRequest(new { message = "Không tìm thấy đơn vị!" });

            // Kiểm tra đã gán chưa
            var exists = await _context.SubCriteriaAssignment.AnyAsync(a =>
                a.SubCriteriaId == dto.SubCriteriaId &&
                a.EvaluationPeriodId == dto.PeriodId &&
                a.UnitId == dto.UnitId);

            if (exists)
                return BadRequest(new { message = "Tiêu chí đã được gán trước đó!" });

            var assignment = new SubCriteriaAssignment
            {
                SubCriteriaId = dto.SubCriteriaId,
                EvaluationPeriodId = dto.PeriodId,
                UnitId = dto.UnitId
            };

            _context.SubCriteriaAssignment.Add(assignment);
            await _context.SaveChangesAsync();

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out int userId);

            await _logService.WriteLogAsync(
                "Gán tiêu chí cho đơn vị",
                $"Bạn được phân công tiêu chí '{subCriteria.Name}' trong kỳ '{period.Name}'",
                User.FindFirst(ClaimTypes.Name)?.Value,
                relatedUserId: userId
            );

            await _logService.WriteLogAsync(
                "Gán tiêu chí cho đơn vị",
                $"Phân công tiêu chí '{subCriteria.Name}' cho đơn vị '{unit.Name}' trong kỳ '{period.Name}'",
                User.FindFirst(ClaimTypes.Name)?.Value
            );

            return Ok(new { message = "Gán tiêu chí thành công!", assignmentId = assignment.Id });
        }

        // GET: api/subcriteriaassignments/assigned?unitId=5&periodId=2
        // Summary: Lấy tất cả tiêu chí đã giao cho đơn vị theo kỳ (bao gồm cả chưa đánh giá)
        [HttpGet("assigned")]
        [Authorize]
        public async Task<IActionResult> GetAssignedCriteria([FromQuery] int? unitId, [FromQuery] int? periodId)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { message = "Không xác định được người dùng!" });

            if (role != "admin")
            {
                unitId = await _context.Auth
                    .Where(u => u.Id == userId)
                    .Select(u => u.UnitId)
                    .FirstOrDefaultAsync();

                if (unitId == null || unitId == 0)
                    return NotFound(new { message = "Không tìm thấy đơn vị!" });
            }

            var query = _context.SubCriteriaAssignment
                .Include(a => a.SubCriteria)
                .Include(a => a.EvaluationPeriod)
                .Where(a => a.UnitId == unitId);

            if (periodId.HasValue)
                query = query.Where(a => a.EvaluationPeriodId == periodId.Value);

            var results = await query
                .Select(a => new
                {
                    SubCriteriaId = a.SubCriteria.Id,
                    SubCriteriaName = a.SubCriteria.Name,
                    a.SubCriteria.Description,
                    a.SubCriteria.MaxScore,
                    AssignmentId = a.Id,
                    a.Score,
                    a.Comment,
                    a.EvidenceInfo,
                    a.EvaluatedAt
                })
                .OrderBy(a => a.SubCriteriaId)
                .ToListAsync();

            return Ok(results);
        }

        // POST: api/subcriteriaassignments/submit
        // Submit mới điểm đánh giá cho tiêu chí trong kỳ của đơn vị
        [HttpPost("submit")]
        [Authorize]
        public async Task<IActionResult> Submit([FromForm] SubmitCriteriaDto dto)
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

            string? filePath = null;
            if (dto.EvidenceFile != null)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads/evidence-files");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid()}_{dto.EvidenceFile.FileName}";
                var fullPath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.EvidenceFile.CopyToAsync(stream);
                }

                filePath = $"/uploads/evidence-files/{fileName}";
            }

            assignment.Score = dto.Score;
            assignment.Comment = dto.Comment;
            assignment.EvidenceInfo = filePath;
            assignment.EvaluatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync(
                "Nộp kết quả đánh giá",
                $"Bạn đã nộp kết quả cho tiêu chí '{assignment.SubCriteria.Name}' trong kỳ '{assignment.EvaluationPeriod.Name}' với điểm {dto.Score}",
                User.FindFirst(ClaimTypes.Name)?.Value,
                relatedUserId: userId
            );

            await _logService.WriteLogAsync(
                "Hệ thống ghi nhận kết quả",
                $"Tiêu chí '{assignment.SubCriteria.Name}' được đánh giá bởi đơn vị '{assignment.Unit.Name}' với điểm {dto.Score}",
                User.FindFirst(ClaimTypes.Name)?.Value
            );

            return Ok(new
            {
                message = "Nộp kết quả thành công!",
                assignmentId = assignment.Id,
                evaluatedAt = assignment.EvaluatedAt
            });
        }

        // PUT: api/subcriteriaassignments/submit/{assignmentId}
        // Chỉnh sửa kết quả đánh giá đã nộp
        [HttpPut("submit/{assignmentId}")]
        [Authorize]
        public async Task<IActionResult> Edit(int assignmentId, [FromForm] SubmitCriteriaDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

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
                .Include(a => a.Unit)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null)
                return NotFound(new { message = "Không tìm thấy nhiệm vụ đánh giá!" });

            if (assignment.EvaluationPeriod.IsLocked)
                return BadRequest(new { message = "Kỳ đánh giá đã bị khóa!" });

            if (role != "admin" && assignment.UnitId != unitId)
                return Forbid("Bạn không có quyền chỉnh sửa đánh giá này!");

            string? filePath = null;
            if (dto.EvidenceFile != null)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads/evidence-files");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid()}_{dto.EvidenceFile.FileName}";
                var fullPath = Path.Combine(uploads, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.EvidenceFile.CopyToAsync(stream);
                }
                filePath = $"/uploads/evidence-files/{fileName}";
            }

            assignment.Score = dto.Score;
            assignment.Comment = dto.Comment;
            assignment.EvidenceInfo = filePath;
            assignment.EvaluatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync(
                "Chỉnh sửa kết quả đánh giá",
                $"Bạn đã cập nhật kết quả cho tiêu chí '{assignment.SubCriteria.Name}' trong kỳ '{assignment.EvaluationPeriod.Name}' với điểm mới {dto.Score}",
                User.FindFirst(ClaimTypes.Name)?.Value,
                relatedUserId: userId
            );

            await _logService.WriteLogAsync(
                "Hệ thống ghi nhận chỉnh sửa",
                $"Tiêu chí '{assignment.SubCriteria.Name}' của đơn vị '{assignment.Unit.Name}' được cập nhật kết quả với điểm mới {dto.Score}",
                User.FindFirst(ClaimTypes.Name)?.Value
            );

            return Ok(new 
            { 
                message = "Cập nhật thành công!", 
                evaluatedAt = assignment.EvaluatedAt 
            });
        }

        [HttpPost("unit/update-criteria")]
        [Authorize]
        public async Task<IActionResult> MarkUpdatedByUnit([FromBody] UnitUpdateCriteriaDto dto)
        {
            var assignment = await _context.ReviewAssignment
                .Include(a => a.Unit)
                .Include(a => a.Reviewer).ThenInclude(r => r.Auth)
                .Include(a => a.SubCriteria)
                .FirstOrDefaultAsync(a => a.Id == dto.ReviewAssignmentId);

            if (assignment == null)
                return NotFound(new { message = "Không tìm thấy nhiệm vụ!" });

            var currentUsername = User.Identity?.Name;
            var currentUser = await _context.Auth.FirstOrDefaultAsync(a => a.Username == currentUsername);

            if (currentUser?.UnitId != assignment.UnitId)
                return Forbid("Bạn không thuộc đơn vị được phép cập nhật tiêu chí này.");

            assignment.IsUpdatedByUnit = true;
            await _context.SaveChangesAsync();

            // Ghi log cho reviewer
            await _logService.WriteLogAsync(
                "Đơn vị cập nhật lại tiêu chí",
                $"Đơn vị '{assignment.Unit.Name}' đã cập nhật lại tiêu chí '{assignment.SubCriteria?.Name}'.",
                currentUsername,
                relatedUserId: assignment.Reviewer.AuthId
            );
            
            await _logService.WriteLogAsync(
                "Đơn vị cập nhật lại sau khi bị từ chối",
                $"Tiêu chí '{assignment.SubCriteria?.Name}' đã được đơn vị '{assignment.Unit.Name}' cập nhật lại sau khi bị từ chối.",
                currentUsername
            );

            return Ok(new { message = "Đã cập nhật lại thông tin tiêu chí. Reviewer sẽ được thông báo để đánh giá lại." });
        }
    }
}