using chuyendoiso.Data;
using chuyendoiso.DTOs;
using chuyendoiso.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Versioning;

namespace chuyendoiso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewCouncilController : ControllerBase
    {
        private readonly chuyendoisoContext _context;
        private readonly IHttpContextAccessor _http;

        public ReviewCouncilController(chuyendoisoContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
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

            var currentUsername = _http.HttpContext?.User?.Identity?.Name;
            var creator = await _context.Auth.FirstOrDefaultAsync(u => u.Username == currentUsername);
            if (creator == null)
                return NotFound(new { message = "Không tìm thấy người dùng!" });

            var council = new ReviewCouncil
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedById = creator.Id
            };

            _context.ReviewCouncil.Add(council);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Tạo hội đồng thành công!", data = council });
        }

        // POST: api/reviewcouncil/add-reviewer
        // Params: reviewCouncilId, authId
        // Thêm đơn vị đánh giá vào hội đồng
        [HttpPost("add-reviewer")]
        [Authorize(Roles = "admin")]
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
        [Authorize(Roles = "admin")]
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

        // PUT: api/reviewcouncil/update/{id}
        // Params: id, name, description
        public async Task<IActionResult> Update(int id, [FromBody] ReviewCouncilDto dto)
        {
            var existing = await _context.ReviewCouncil.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Không tìm thấy hội đồng!" });

            if (!string.IsNullOrWhiteSpace(dto.Name))
                existing.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.Description))
                existing.Description = dto.Description;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật hội đồng thành công!", data = new { existing.Id, existing.Name, existing.Description } });
        }

        // Delete: api/reviewcouncil/delete/{id}
        // Params: id
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

            return Ok(new { message = "Xóa hội đồng thành công!" });
        }
    }
}
