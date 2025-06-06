using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using chuyendoiso.Data;
using chuyendoiso.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using chuyendoiso.DTOs;
using chuyendoiso.Services;

namespace chuyendoiso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParentCriteriasController : Controller
    {
        private readonly chuyendoisoContext _context;
        private readonly LogService _logService;
        private readonly IWebHostEnvironment _env;

        public ParentCriteriasController(chuyendoisoContext context, LogService logService, IWebHostEnvironment env)
        {
            _context = context;
            _logService = logService;
            _env = env;
        }

        // GET: api/parentcriterias
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var parentCriterias = await _context.ParentCriteria
                .Include(p => p.TargetGroup)
                .Include(p => p.EvaluationPeriod)
                .Include(p => p.SubCriterias)
                .Select(p => new {
                    p.Id,
                    p.Name,
                    p.MaxScore,
                    Group = p.TargetGroup == null ? null : new
                    {
                        p.TargetGroup.Id,
                        p.TargetGroup.Name
                    },
                    EvaluationPeriod = p.EvaluationPeriod == null ? null : new
                    {
                        p.EvaluationPeriod.Id,
                        p.EvaluationPeriod.Name
                    },
                    SubCriteriaIds = p.SubCriterias.Select(s => s.Id).ToList()
                })
                .ToListAsync();

            return Ok(parentCriterias);
        }

        // GET: api/parentcriterias/id
        // Params: Id
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            var parentCriteria = await _context.ParentCriteria
                .Include(p => p.TargetGroup)
                .Include(p => p.SubCriterias)
                .Select(p => new {
                    p.Id,
                    p.Name,
                    p.MaxScore,
                    GroupId = p.TargetGroup == null ? null : new
                    {
                        p.TargetGroup.Id,
                        p.TargetGroup.Name
                    },
                    EvaluationPeriod = p.EvaluationPeriod == null ? null : new
                    {
                        p.EvaluationPeriod.Id,
                        p.EvaluationPeriod.Name
                    },
                    SubCriterias = p.SubCriterias.Select(s => new
                    {
                        s.Id,
                        s.Name
                    }).ToList()
                })
                .FirstOrDefaultAsync(m => m.Id == id);

            if (parentCriteria == null)
            {
                return NotFound(new { message = "Không tìm thấy nhóm chỉ tiêu!" });
            }

            return Ok(parentCriteria);
        }

        // POST: api/parentcriterias/create
        // Params: Name, MaxScore, Description, TargetGroupId, EvidenceInfo
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] ParentCriteriaDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { message = "Tên tiêu chí là bắt buộc!" });
            }

            bool isDuplicate = await _context.ParentCriteria.AnyAsync(p => p.Name == dto.Name);
            if (isDuplicate)
            {
                return BadRequest(new { message = "Tiêu chí đã tồn tại!" });
            }

            int? groupId = null;
            if (dto.TargetGroupId.HasValue)
            {
                var group = await _context.TargetGroup.FindAsync(dto.TargetGroupId.Value);
                if (group == null)
                {
                    return BadRequest(new { message = "Không tìm thấy nhóm chỉ tiêu!" });
                }
                groupId = group.Id;
            }

            string? filePath = null;
            if (dto.EvidenceFile != null)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads/parentcriteria-evidence");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid()}_{dto.EvidenceFile.FileName}";
                var fullPath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.EvidenceFile.CopyToAsync(stream);
                }

                filePath = $"/uploads/parentcriteria-evidence/{fileName}";
            }

            var parent = new ParentCriteria
            {
                Name = dto.Name,
                MaxScore = dto.MaxScore,
                Description = dto.Description,
                EvidenceInfo = filePath,
                TargetGroupId = groupId,
                EvaluationPeriodId = dto.EvaluationPeriodId
            };

            try
            {
                _context.ParentCriteria.Add(parent);
                await _context.SaveChangesAsync();

                await _logService.WriteLogAsync(
                    "Tạo tiêu chí cha",
                    $"Tạo tiêu chí cha: {parent.Name} (ID = {parent.Id})",
                    User.FindFirst(ClaimTypes.Name)?.Value
                );

                return CreatedAtAction(nameof(Details), new { id = parent.Id }, new
                {
                    parent.Id,
                    parent.Name,
                    parent.MaxScore,
                    parent.Description,
                    parent.EvidenceInfo,
                    parent.TargetGroupId,
                    parent.EvaluationPeriodId
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new 
                { 
                    message = "Lỗi khi lưu vào cơ sở dữ liệu.", 
                    detail = ex.InnerException?.Message ?? ex.Message 
                });
            }
        }

        // PUT: api/parentcriterias/id
        // Params: Id, Name, MaxScore, Description, TargetGroupId, EvidenceInfo
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [FromBody] ParentCriteriaDto dto)
        {
            var existing = await _context.ParentCriteria.Include(p => p.TargetGroup).FirstOrDefaultAsync(p => p.Id == id);

            if (existing == null)
                return NotFound(new { message = "Không tìm thấy tiêu chí cha!" });

            if (existing.EvaluationPeriodId.HasValue)
            {
                var currentPeriod = await _context.EvaluationPeriod.FindAsync(existing.EvaluationPeriodId.Value);
                if (currentPeriod != null && currentPeriod.IsLocked)
                    return BadRequest(new { message = "Không thể chỉnh sửa vì kỳ đánh giá hiện tại đã bị khóa!" });
            }

            if (dto.EvaluationPeriodId.HasValue && dto.EvaluationPeriodId != existing.EvaluationPeriodId)
            {
                var period = await _context.EvaluationPeriod.FindAsync(dto.EvaluationPeriodId.Value);
                if (period == null)
                    return BadRequest(new { message = "Không tìm thấy kỳ đánh giá mới!" });

                if (period.IsLocked)
                    return BadRequest(new { message = "Không thể thay đổi vì kỳ đánh giá đã bị khóa!" });

                existing.EvaluationPeriodId = dto.EvaluationPeriodId.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != existing.Name)
            {
                bool isNameExists = await _context.ParentCriteria.AnyAsync(p =>
                    p.Name == dto.Name &&
                    p.Id != id &&
                    p.TargetGroupId == dto.TargetGroupId);

                if (isNameExists)
                    return BadRequest(new { message = "Tên tiêu chí đã tồn tại trong nhóm chỉ tiêu này!" });

                existing.Name = dto.Name;
            }

            if (dto.MaxScore.HasValue)
                existing.MaxScore = dto.MaxScore.Value;

            if (!string.IsNullOrWhiteSpace(dto.Description))
                existing.Description = dto.Description;

            string? filePath = null;
            if (dto.EvidenceFile != null)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads/parentcriteria-evidence");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid()}_{dto.EvidenceFile.FileName}";
                var fullPath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.EvidenceFile.CopyToAsync(stream);
                }
                filePath = $"/uploads/parentcriteria-evidence/{fileName}";
            }

            if (dto.TargetGroupId.HasValue && dto.TargetGroupId != existing.TargetGroupId)
            {
                var newGroup = await _context.TargetGroup.FindAsync(dto.TargetGroupId.Value);
                if (newGroup == null)
                    return BadRequest(new { message = "Không tìm thấy nhóm chỉ tiêu mới!" });

                existing.TargetGroupId = newGroup.Id;
            }

            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync(
                "Cập nhật tiêu chí cha", 
                $"Cập nhật tiêu chí cha: {existing.Name} (ID = {existing.Id})", 
                User.FindFirst(ClaimTypes.Name)?.Value
            );

            return Ok(new { message = "Cập nhật thành công!" });
        }

        // DELETE: api/parentcriterias/id
        // Params: Id
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var parentCriteria = await _context.ParentCriteria
                .Include(p => p.SubCriterias)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (parentCriteria == null)
                return NotFound(new { message = "Không tìm thấy tiêu chí cha!" });

            if (parentCriteria.SubCriterias != null && parentCriteria.SubCriterias.Any())
            {
                if (parentCriteria.SubCriterias.Any(s => s.MaxScore > 0))
                    return BadRequest(new { message = "Không thể xóa vì tiêu chí con đã được chấm điểm!" });

                return BadRequest(new { message = "Không thể xóa vì tiêu chí cha đang chứa tiêu chí con!" });
            }

            _context.ParentCriteria.Remove(parentCriteria);
            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync(
                "Xóa tiêu chí cha", 
                $"Xóa tiêu chí cha: {parentCriteria.Name} (ID = {parentCriteria.Id})", 
                User.FindFirst(ClaimTypes.Name)?.Value
            );

            return Ok(new { message = "Xóa tiêu chí cha thành công!" });
        }
    }
}