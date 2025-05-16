using chuyendoiso.Data;
using chuyendoiso.DTOs;
using chuyendoiso.Models;
using chuyendoiso.Services;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;
using System.Security.Claims;

namespace chuyendoiso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewCouncilController : ControllerBase
    {
        private readonly chuyendoisoContext _context;
        private readonly IHttpContextAccessor _http;
        private readonly LogService _logService;

        public ReviewCouncilController(chuyendoisoContext context, IHttpContextAccessor http, LogService log)
        {
            _context = context;
            _http = http;
            _logService = log;
        }

        // GET: api/reviewcouncil
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var councils = await _context.ReviewCouncil
                .Include(c => c.Reviewers)
                .ThenInclude(r => r.Auth)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Description,
                    c.CreatedAt,
                    Chair = c.Reviewers
                        .Where(r => r.IsChair)
                        .Select(r => new { r.Auth.FullName, r.Auth.Username })
                        .FirstOrDefault(),
                    MemberCount = c.Reviewers.Count
                })
                .ToListAsync();

            return Ok(councils);
        }

        // GET: api/reviewcouncil/{id}
        // Params: id
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var council = await _context.ReviewCouncil
                .Include(c => c.Reviewers)
                .ThenInclude(r => r.Auth)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (council == null)
                return NotFound(new { message = "Không tìm thấy hội đồng!" });

            var result = new
            {
                council.Id,
                council.Name,
                council.Description,
                council.CreatedAt,
                Chair = council.Reviewers
                    .Where(r => r.IsChair)
                    .Select(r => new { r.Auth.FullName, r.Auth.Username })
                    .FirstOrDefault(),
                Members = council.Reviewers
                    .Where(r => !r.IsChair)
                    .Select(r => new { r.Id, r.Auth.FullName, r.Auth.Username })
                    .ToList()
            };

            return Ok(result);
        }

        // GET: api/reviewcouncil/assignments/{id}
        // Params: id
        [HttpGet("assignments/{id}")]
        [Authorize]
        public async Task<IActionResult> GetAssignments(int id)
        {
            var council = await _context.ReviewCouncil
                .Include(c => c.Reviewers)
                    .ThenInclude(r => r.Auth)
                .Include(c => c.Reviewers)
                    .ThenInclude(r => r.ReviewAssignments)
                        .ThenInclude(a => a.Unit)
                .Include(c => c.Reviewers)
                    .ThenInclude(r => r.ReviewAssignments)
                        .ThenInclude(a => a.SubCriteria)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (council == null)
                return NotFound(new { message = "Không tìm thấy hội đồng!" });

            var result = council.Reviewers.Select(r => new
            {
                ReviewerId = r.Id,
                FullName = r.Auth.FullName,
                Username = r.Auth.Username,
                IsChair = r.IsChair,
                Assignments = r.ReviewAssignments.Select(a => new
                {
                    a.Id,
                    UnitId = a.UnitId,
                    UnitName = a.Unit?.Name,
                    SubCriteriaId = a.SubCriteriaId,
                    SubCriteriaName = a.SubCriteria?.Name
                }).ToList()
            });

            return Ok(result);
        }

        // POST: api/reviewcouncil/create
        // Params: Name, Description, Id creator
        // Tạo hội đồng đánh giá 
        [HttpPost("create")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([FromBody] ReviewCouncilDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { message = "Vui lòng nhập tên Hội đồng!" });

            var chairUser = await _context.Auth.FindAsync(dto.ChairAuthId);
            if (chairUser == null)
                return BadRequest(new { message = "Không tìm thấy chủ tịch hội đồng!" });

            var council = new ReviewCouncil
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedById = chairUser.Id
            };

            _context.ReviewCouncil.Add(council);
            await _context.SaveChangesAsync();

            var chair = new Reviewer
            {
                ReviewCouncilId = council.Id,
                AuthId = chairUser.Id,
                IsChair = true
            };

            _context.Reviewer.Add(chair);
            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync("Create Council", $"Tạo hội đồng '{council.Name}', chủ tịch {chairUser.Username}", User.Identity?.Name);

            return Ok(new
            {
                message = "Tạo hội đồng và phân công chủ tịch thành công!",
                data = new
                {
                    council.Id,
                    council.Name,
                    Chair = chairUser.Username
                }
            });
        }
        
        // POST: api/reviewcouncil/add-reviewer
        // Params: reviewCouncilId, authId
        // Thêm thành viên thẩm định vào hội đồng
        [HttpPost("add-reviewer")]
        [Authorize(Roles = "chair,admin")]
        public async Task<IActionResult> AddReviewer([FromBody] ReviewerDto dto)
        {
            var council = await _context.ReviewCouncil.FindAsync(dto.ReviewCouncilId);
            var user = await _context.Auth.FindAsync(dto.AuthId);

            if (council == null || user == null)
                return BadRequest(new { message = "Không tìm thấy hội đồng hoặc người dùng!" });

            var reviewer = new Reviewer
            {
                ReviewCouncilId = dto.ReviewCouncilId,
                AuthId = dto.AuthId
            };

            _context.Reviewer.Add(reviewer);
            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync("Add Reviewer", $"Thêm đơn vị đánh giá: {user.Username} vào hội đồng {council.Name}", User.FindFirst(ClaimTypes.Name)?.Value);

            return Ok(new 
            { 
                message = "Thêm đơn vị đánh giá thành công!",
                data = new
                {
                    reviewer.Id,
                    user.Username,
                    CouncilName = council.Name
                }
            });
        }

        // POST: api/reviewcouncil/assign
        // Params: reviewerId, unitId, subCriteriaId
        // Phân công thẩm định chỉ tiêu
        [HttpPost("assign")]
        [Authorize(Roles = "admin,chair")]
        public async Task<IActionResult> Assign([FromBody] ReviewAssignmentDto dto)
        {
            var reviewer = await _context.Reviewer
                .Include(r => r.Auth)
                .FirstOrDefaultAsync(r => r.Id == dto.ReviewerId);

            var unit = await _context.Unit.FindAsync(dto.UnitId);
            var sub = dto.SubCriteriaId.HasValue
                    ? await _context.SubCriteria.FindAsync(dto.SubCriteriaId.Value)
                    : null;

            if (reviewer == null || unit == null)
                return BadRequest(new { message = "Không tìm thấy đơn vị!" });

            var assignment = new ReviewAssignment
            {
                ReviewerId = dto.ReviewerId,
                UnitId = dto.UnitId,
                SubCriteriaId = dto.SubCriteriaId
            };

            _context.ReviewAssignment.Add(assignment);
            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync("Assign", $"Phân công thẩm định: {reviewer.Auth.Username} cho đơn vị {unit.Name}", User.FindFirst(ClaimTypes.Name)?.Value);

            return Ok(new 
            { 
                message = "Phân công thẩm định thành công!", 
                data = new
                {
                    assignment.Id,
                    dto.ReviewerId,
                    Name = reviewer.Auth.Username,
                    dto.UnitId,
                    UnitName = unit.Name,
                    dto.SubCriteriaId,
                    SubCriteriaName = sub != null ? sub.Name : null
                }
            });
        }

        // PUT: api/reviewcouncil/{id}
        // Params: id, name, description
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id, [FromBody] ReviewCouncilDto dto)
        {
            var council = await _context.ReviewCouncil
                .Include(c => c.Reviewers)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (council == null)
                return NotFound(new { message = "Không tìm thấy hội đồng!" });

            if (!string.IsNullOrWhiteSpace(dto.Name))
                council.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.Description))
                council.Description = dto.Description;

            // Cập nhật chủ tịch nếu có
            if (dto.ChairAuthId.HasValue)
            {
                var newChair = await _context.Auth.FindAsync(dto.ChairAuthId.Value);
                if (newChair == null)
                    return BadRequest(new { message = "Không tìm thấy người dùng!" });

                // Xóa chủ tịch cũ
                var oldChair = council.Reviewers.FirstOrDefault(r => r.IsChair);
                if (oldChair != null)
                {
                    oldChair.IsChair = false;
                }
                else
                {
                    _context.Reviewer.Add(new Reviewer
                    {
                        ReviewCouncilId = council.Id,
                        AuthId = newChair.Id,
                        IsChair = true
                    });
                }

                council.CreatedById = dto.ChairAuthId.Value;
            }

            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync("Update", $"Cập nhật hội đồng: {council.Name} (ID = {council.Id})", User.FindFirst(ClaimTypes.Name)?.Value);

            return Ok(new 
            { 
                message = "Cập nhật hội đồng thành công!", 
                data = new 
                { 
                    council.Id, 
                    council.Name, 
                    council.Description,
                    NewChairId = dto.ChairAuthId
                } 
            });
        }

        // Delete: api/reviewcouncil/delete-reviewer/{id}
        // Params: id
        [HttpDelete("delete-reviewer/{id}")]
        [Authorize(Roles = "admin,chair")]
        public async Task<IActionResult> DeleteReviewer(int id)
        {
            var existing = await _context.Reviewer
                .Include(r => r.ReviewAssignments)
                .FirstOrDefaultAsync(r => r.Id == id );

            if (existing == null)
                return NotFound(new { message = "Không tìm thấy thành viên hội đồng!" });

            if (existing.ReviewAssignments != null && existing.ReviewAssignments.Any())
            {
                return BadRequest(new { message = "Không thể xóa thành viên đã được phân công thẩm định!" });
            }

            _context.Reviewer.Remove(existing);
            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync("Delete Reviewer", $"Xóa thành viên hội đồng: {existing.Auth.Username}", User.FindFirst(ClaimTypes.Name)?.Value);

            return Ok(new { message = "Xóa thành viên hội đồng thành công!" });
        }

        // DELETE: api/reviewcouncil/delete-council/{id}
        // Params: id
        [HttpDelete("delete-council/{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteCouncil(int id)
        {
            var council = await _context.ReviewCouncil
                .Include(c => c.Reviewers)
                    .ThenInclude(r => r.ReviewAssignments)
                .Include(c => c.Reviewers)
                    .ThenInclude(r => r.Auth)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (council == null)
                return NotFound(new { message = "Không tìm thấy hội đồng!" });

            // Kiểm tra thành viên đã được phân công thẩm định
            bool anyAssignments = council.Reviewers.Any(r => r.ReviewAssignments.Any());
            if (anyAssignments)
                return BadRequest(new { message = "Không thể xóa hội đồng đã có thành viên được phân công thẩm định!" });

            // Xóa tất cả thành viên hội đồng
            _context.Reviewer.RemoveRange(council.Reviewers);

            // Xóa hội đồng
            _context.ReviewCouncil.Remove(council);
            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync("Delete Council", $"Xóa hội đồng '{council.Name}'", User.Identity?.Name);

            return Ok(new { message = "Xóa hội đồng thành công!" });
        }
    }
}
