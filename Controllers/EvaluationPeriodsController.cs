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
        public async Task<IActionResult> Index()
        {
            var periods = await _context.EvaluationPeriod
                .Include(p => p.EvaluationUnits)
                    .ThenInclude(eu => eu.Unit)
                .OrderByDescending(p => p.StartDate)
                .Select(p => new
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
                })
                .ToListAsync();

            return Ok(periods);
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
            return Ok(period);
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

            await _logService.WriteLogAsync("Delete", $"Xóa kỳ đánh giá: {period.Name} (ID = {period.Id})",
        User.FindFirst(ClaimTypes.Name)?.Value);

            return Ok(new { message = "Xóa kỳ đánh giá thành công!" });
        }
    }
}
