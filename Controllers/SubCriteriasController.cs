using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using chuyendoiso.Data;
using chuyendoiso.Models;
using Microsoft.AspNetCore.Authorization;
using chuyendoiso.DTOs;
using System.Security.Claims;
using chuyendoiso.Services;
using DocumentFormat.OpenXml.Spreadsheet;

namespace chuyendoiso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubCriteriasController : Controller
    {
        private readonly chuyendoisoContext _context;
        private readonly LogService _logService;
        private readonly IWebHostEnvironment _env;

        public SubCriteriasController(chuyendoisoContext context, LogService logService, IWebHostEnvironment env)
        {
            _context = context;
            _logService = logService;
            _env = env;
        }

        // GET: api/subcriterias
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return BadRequest(new { message = "Không xác định được người dùng!" });

            var query = _context.SubCriteria
                .Include(sc => sc.ParentCriteria)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(sc => sc.Name.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var subCriterias = await query
                .OrderBy(sc => sc.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.MaxScore,
                    p.EvidenceInfo,
                    Parent = p.ParentCriteria != null ? p.ParentCriteria.Name : null
                })
                .ToListAsync();

            var result = new
            {
                Items = subCriterias,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages
            };

            return Ok(result);
        }

        // GET: api/subcriterias/id
        // Params: Id
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            var subCriteria = await _context.SubCriteria
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.MaxScore,
                    p.Description,
                    p.EvidenceInfo,
                    Parent = new
                    {
                        p.ParentCriteria.Id,
                        p.ParentCriteria.Name
                    },
                    p.EvaluatedAt
                })
                .FirstOrDefaultAsync(m => m.Id == id);

            if (subCriteria == null)
            {
                return NotFound(new { message = "Không tìm thấy nhóm chỉ tiêu!" });
            }

            return Ok(subCriteria);
        }

        // GET: api/subcriterias/by-unit?unitId=5
        [HttpGet("by-unit")]
        [Authorize]
        public async Task<IActionResult> GetByUnit([FromQuery] int unitId)
        {
            var unit = await _context.Unit.FindAsync(unitId);
            if (unit == null)
                return BadRequest(new { message = "Không tìm thấy đơn vị!" });

            var subCriterias = await _context.SubCriteria
                .Where(sc => sc.UnitEvaluate == unit.Name)
                .Select(sc => new
                {
                    sc.Id,
                    sc.Name,
                    sc.Description,
                    sc.MaxScore,
                    sc.UnitEvaluate,
                    sc.ParentCriteriaId
                })
                .ToListAsync();

            return Ok(subCriterias);
        }

        // POST: api/subcriterias/create
        // Params: Name, MaxScore, Description, ParentCriteriaId, EvidenceInfo
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] SubCriteriaDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.ParentCriteriaId == null)
            {
                return BadRequest(new { message = "Tên tiêu chí và ID tiêu chí cha là bắt buộc!" });
            }

            var exists = await _context.SubCriteria.AnyAsync(t => t.Name == dto.Name && t.ParentCriteriaId == dto.ParentCriteriaId);
            if (exists)
            {
                return BadRequest(new { message = "Tên tiêu chí đã tồn tại!" });
            }

            var parent = await _context.ParentCriteria.FindAsync(dto.ParentCriteriaId.Value);
            if (parent == null)
                return BadRequest(new { message = "Không tìm thấy chỉ tiêu cha!" });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return BadRequest(new { message = "Không xác định được người dùng!" });

            var user = await _context.Auth.Include(u => u.Unit).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || user.Unit == null)
                return BadRequest(new { message = "Không xác định được đơn vị của người dùng!" });

            string? filePath = null;
            if (dto.EvidenceFile != null)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads/subcriteria-evidence");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid()}_{dto.EvidenceFile.FileName}";
                var fullPath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.EvidenceFile.CopyToAsync(stream);
                }

                filePath = $"/uploads/subcriteria-evidence/{fileName}";
            }

            var subCriteria = new SubCriteria
            {
                Name = dto.Name,
                MaxScore = dto.MaxScore,
                Description = dto.Description,
                EvidenceInfo = filePath,
                ParentCriteriaId = parent.Id,
                UnitEvaluate = user.Unit.Name,
                EvaluatedAt = dto.EvaluatedAt.HasValue
                    ? DateTime.SpecifyKind(dto.EvaluatedAt.Value, DateTimeKind.Utc)
                    : DateTime.UtcNow
            };

            _context.SubCriteria.Add(subCriteria);
            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync(
                "Tạo tiêu chí con", 
                $"Tạo tiêu chí con: {subCriteria.Name} (ID = {subCriteria.Id}) thuộc chỉ tiêu cha: {parent.Name} ({parent.Id})", 
                User.FindFirst(ClaimTypes.Name)?.Value
            );

            return CreatedAtAction(nameof(Details), new { id = subCriteria.Id }, new
            {
                subCriteria.Id,
                subCriteria.Name,
                subCriteria.MaxScore,
                subCriteria.Description,
                subCriteria.EvidenceInfo,
                Parent = parent.Name,
            });
        }

        // PUT: api/subcriterias/id
        // Params: Id, Name, MaxScore, Description, ParentCriteriaId, EvidenceInfo
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(int id, [FromForm] SubCriteriaDto dto)
        {
            var existing = await _context.SubCriteria
                .Include(sc => sc.ParentCriteria)
                .FirstOrDefaultAsync(sc => sc.Id == id);

            if (existing == null)
                return NotFound(new { message = "Không tìm thấy tiêu chí con!" });

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var unit = User.FindFirst("Unit")?.Value;

            // Chỉ tiêu con sẽ bị khóa nếu kỳ đánh giá của tiêu chí cha đã bị khóa
            if (existing.ParentCriteria?.EvaluationPeriodId != null)
            {
                var period = await _context.EvaluationPeriod.FindAsync(existing.ParentCriteria.EvaluationPeriodId);
                if (period != null && period.IsLocked)
                {
                    return BadRequest(new { message = "Không thể chỉnh sửa vì kỳ đánh giá đã bị khóa!" });
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != existing.Name)
            {
                bool isNameExists = await _context.SubCriteria.AnyAsync(p =>
                    p.Name == dto.Name &&
                    p.ParentCriteriaId == (dto.ParentCriteriaId ?? existing.ParentCriteriaId) &&
                    p.Id != id);

                if (isNameExists)
                    return BadRequest(new { message = "Tên tiêu chí đã tồn tại!" });

                existing.Name = dto.Name;
            }

            if (dto.MaxScore.HasValue)
                existing.MaxScore = dto.MaxScore.Value;

            if (!string.IsNullOrWhiteSpace(dto.Description))
                existing.Description = dto.Description;

            if (dto.EvidenceFile != null)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads/subcriteria-evidence");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid()}_{dto.EvidenceFile.FileName}";
                var fullPath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.EvidenceFile.CopyToAsync(stream);
                }

                existing.EvidenceInfo = $"/uploads/subcriteria-evidence/{fileName}";
            }

            if (dto.ParentCriteriaId.HasValue && dto.ParentCriteriaId.Value != existing.ParentCriteriaId)
            {
                var parent = await _context.ParentCriteria.FindAsync(dto.ParentCriteriaId.Value);
                if (parent == null)
                    return BadRequest(new { message = "Không tìm thấy chỉ tiêu cha!" });

                existing.ParentCriteriaId = parent.Id;
            }

            if (dto.EvaluatedAt.HasValue)
                existing.EvaluatedAt = dto.EvaluatedAt.Value;

            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync(
                "Cập nhật tiêu chí con", 
                $"Cập nhật tiêu chí con: {existing.Name} (ID = {existing.Id})", 
                User.FindFirst(ClaimTypes.Name)?.Value);

            return Ok(new { message = "Cập nhật thành công!" });
        }

        // DELETE: api/subcriterias/id
        // Params: Id
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subCriteria = await _context.SubCriteria
                .Include(p => p.ParentCriteria)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (subCriteria == null)
                return NotFound(new { message = "Không tìm thấy tiêu chí!" });

            if (subCriteria.MaxScore > 0)
                return BadRequest(new { message = "Không thể xóa tiêu chí này vì nó đã được chấm điểm!" });

            _context.SubCriteria.Remove(subCriteria);
            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync(
                "Xóa tiêu chí con", 
                $"Xóa tiêu chí con: {subCriteria.Name} (ID = {subCriteria.Id})", 
                User.FindFirst(ClaimTypes.Name)?.Value
            );

            return Ok(new { message = "Xóa nhóm chỉ tiêu thành công!" });
        }
    }
}
