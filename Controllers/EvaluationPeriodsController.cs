using chuyendoiso.Data;
using chuyendoiso.DTOs;
using chuyendoiso.Models;
using chuyendoiso.Services;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace chuyendoiso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EvaluationPeriodsController : ControllerBase
    {
        private readonly chuyendoisoContext _context;
        private readonly LogService _logService;

        public EvaluationPeriodsController(chuyendoisoContext context, LogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: api/evaluationperiods
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? year = null)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var query = _context.EvaluationPeriod
                .Include(p => p.EvaluationUnits)
                .ThenInclude(eu => eu.Unit)
                .AsQueryable();

            // Filter by year if provided
            if (year.HasValue)
            {
                query = query.Where(p => p.StartDate.Year == year.Value);
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var periods = await query
                .OrderByDescending(p => p.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            foreach (var period in periods)
            {
                await CheckAndAutoUnlock(period);
            }

            var result = new
            {
                Items = periods.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.StartDate,
                    p.EndDate,
                    Units = p.EvaluationUnits.Select(eu => new
                    {
                        eu.Unit.Id,
                        eu.Unit.Name,
                        eu.Unit.Code,
                        eu.Unit.Type
                    }).ToList()
                }),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return Ok(result);
        }

        // POST: api/evaluationperiods/create
        // Params: Name, StartDate, EndDate, UnitId
        [HttpPost("create")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([FromBody] EvaluationPeriodDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.StartDate == null || dto.EndDate == null)
            {
                return BadRequest(new { message = "Vui lòng nhập tên, ngày bắt đầu và ngày kết thúc!" });
            }

            var period = new EvaluationPeriod
            {
                Name = dto.Name,
                StartDate = dto.StartDate.Value,
                EndDate = dto.EndDate.Value
            };

            _context.EvaluationPeriod.Add(period);
            await _context.SaveChangesAsync();

            // Gán đơn vị (nếu có)
            if (dto.UnitIds != null && dto.UnitIds.Any())
            {
                foreach (var unitId in dto.UnitIds)
                {
                    _context.EvaluationUnit.Add(new EvaluationUnit
                    {
                        EvaluationPeriodId = period.Id,
                        UnitId = unitId
                    });
                }
                await _context.SaveChangesAsync();
            }

            await _logService.WriteLogAsync("Create", $"Tạo kỳ đánh giá: {period.Name} (ID = {period.Id})", User.FindFirst(ClaimTypes.Name)?.Value);

            return Ok(new
            {
                message = "Tạo kỳ đánh giá thành công!",
                period.Id,
                period.Name,
                period.StartDate,
                period.EndDate
            });

        }

        // GET: api/evaluationperiods/1
        //Params: id
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var period = await _context.EvaluationPeriod
                .Include(p => p.EvaluationUnits)
                .ThenInclude(eu => eu.Unit)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (period == null)
            {
                return NotFound(new { message = "Không tìm thấy kỳ đánh giá!" });
            }

            await CheckAndAutoUnlock(period);

            var result = new
            {
                period.Id,
                period.Name,
                period.StartDate,
                period.EndDate,
                period.IsLocked,
                period.LockedUntil,
                period.LockReason,
                period.LockAttachment,
                Units = period.EvaluationUnits.Select(eu => new
                {
                    eu.Unit.Id,
                    eu.Unit.Name,
                    eu.Unit.Code,
                    eu.Unit.Type
                })
            };

            return Ok(result);
        }

        // PUT: api/evaluationperiods/1
        // Params: Name, StartDate, EndDate, UnitId
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id, [FromBody] EvaluationPeriodDto dto)
        {
            var existing = await _context.EvaluationPeriod.FindAsync(id);
            if (existing == null)
            {
                return NotFound(new { message = "Không tìm thấy kỳ đánh giá!" });
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
                existing.Name = dto.Name;

            if (dto.StartDate.HasValue)
                existing.StartDate = dto.StartDate.Value;

            if (dto.EndDate.HasValue)
                existing.EndDate = dto.EndDate.Value;

            await _context.SaveChangesAsync();

            // Cập nhật danh sách đơn vị (nếu có)
            if (dto.UnitIds != null)
            {
                var oldUnits = _context.EvaluationUnit.Where(eu => eu.EvaluationPeriodId == id);
                _context.EvaluationUnit.RemoveRange(oldUnits);
                await _context.SaveChangesAsync();

                foreach (var unitId in dto.UnitIds)
                {
                    _context.EvaluationUnit.Add(new EvaluationUnit
                    {
                        EvaluationPeriodId = id,
                        UnitId = unitId
                    });
                }
                await _context.SaveChangesAsync();
            }

            await _logService.WriteLogAsync("Update", $"Cập nhật kỳ đánh giá: {existing.Name} (ID = {existing.Id})", User.FindFirst(ClaimTypes.Name)?.Value);

            return Ok(new { message = "Cập nhật kỳ đánh giá thành công!" });
        }

        // DELETE: api/evaluationperiods/1
        // Params: id
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var period = await _context.EvaluationPeriod.FindAsync(id);
            if (period == null)
            {
                return NotFound(new { message = "Không tìm thấy kỳ đánh giá!" });
            }

            // Kiểm tra các dữ liệu phát sinh
            bool hasUnits = await _context.EvaluationUnit.AnyAsync(u => u.EvaluationPeriodId == id);
            bool hasParentCriterias = await _context.ParentCriteria.AnyAsync(c => c.EvaluationPeriodId == id);

            if (hasUnits || hasParentCriterias)
            {
                return BadRequest(new { message = "Không thể xóa kỳ đánh giá này vì có dữ liệu phát sinh!" });
            }

            _context.EvaluationPeriod.Remove(period);
            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync("Delete", $"Xóa kỳ đánh giá: {period.Name} (ID = {period.Id})", User.FindFirst(ClaimTypes.Name)?.Value);

            return Ok(new { message = "Xóa kỳ đánh giá thành công!" });
        }

        // POST: api/evaluationperiods/lock
        // Params: id, reason, until
        [HttpPost("lockperiod")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> LockPeriod(int id, [FromBody] LockPeriodDto dto)
        {
            var period = await _context.EvaluationPeriod.FindAsync(id);
            if (period == null)
            {
                return NotFound(new { message = "Không tìm thấy kỳ đánh giá!" });
            }

            // Validate unlock date
            if (dto.UnlockDate.HasValue)
            {
                if (dto.UnlockDate < DateTime.UtcNow || dto.UnlockDate > period.EndDate)
                    return BadRequest(new { message = "Ngày mở khóa không hợp lệ!" });
            }

            period.IsLocked = true;
            period.LockReason = dto.Reason;
            period.LockedUntil = dto.UnlockDate;

            // Handle file upload
            if (dto.Attachment != null && dto.Attachment.Length > 0)
            {
                var fileName = $"lock_{id}_{DateTime.UtcNow.Ticks}.pdf";
                var filePath = Path.Combine("Uploads", "LockReasons", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? string.Empty);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Attachment.CopyToAsync(stream);
                }
                period.LockAttachment = filePath;
            }

            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync("Lock", $"Khóa kỳ đánh giá: {period.Name} (ID = {period.Id})", User.FindFirst(ClaimTypes.Name)?.Value);

            return Ok(new { message = "Khóa kỳ đánh giá thành công!" });
        }

        private async Task CheckAndAutoUnlock(EvaluationPeriod period)
        {
            if (period.IsLocked && period.LockedUntil.HasValue && DateTime.UtcNow > period.LockedUntil.Value)
            {
                period.IsLocked = false;
                period.LockReason = null;
                period.LockedUntil = null;
                period.LockAttachment = null;

                _context.EvaluationPeriod.Update(period);
                await _context.SaveChangesAsync();
            }
        }
    }
}
