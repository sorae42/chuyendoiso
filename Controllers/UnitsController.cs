using chuyendoiso.Data;
using chuyendoiso.DTOs;
using chuyendoiso.Models;
using chuyendoiso.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace chuyendoiso.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class UnitsController : ControllerBase
    {
        private readonly chuyendoisoContext _context;
        private readonly LogService _logService;

        public UnitsController(chuyendoisoContext context, LogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: api/units
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var units = await _context.Unit
                .Include(u => u.Users)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Code,
                    u.Type,
                    u.Address,
                    u.Description,
                    Users = u.Users.Select(user => new {
                        user.Id,
                        user.FullName,
                        user.Username
                    }).ToList()
                })
                .ToListAsync();

            return Ok(units);
        }


        // GET: api/units/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUnit(int id)
        {
            var unit = await _context.Unit
                .Include(u => u.Users)
                .Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Code,
                    u.Type,
                    u.Address,
                    u.Description,
                    Users = u.Users.Select(user => new
                    {
                        user.Id,
                        user.FullName,
                        user.Username
                    }).ToList()
                }).FirstOrDefaultAsync();

            if (unit == null)
            {
                return NotFound(new { message = "Đơn vị không tồn tại!" });
            }
            return Ok(unit);
        }

        // POST: api/units/create
        // Params: Name, Code, Type, Address, Description
        [HttpPost("create")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([FromBody] UnitDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new { message = "Vui lòng nhập tên đơn vị!" });
            }

            if (await _context.Unit.AnyAsync(u => u.Name == dto.Name))
            {
                return BadRequest(new { message = "Tên đơn vị đã tồn tại!" });
            }

            var unit = new Unit
            {
                Name = dto.Name,
                Code = dto.Code,
                Type = dto.Type,
                Address = dto.Address,
                Description = dto.Description
            };
            _context.Unit.Add(unit);
            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync("Create", $"Tạo đơn vị: {unit.Name} (ID = {unit.Id})", User.FindFirst(ClaimTypes.Name)?.Value);

            return CreatedAtAction(nameof(Index), new { id = unit.Id }, unit);
        }

        // PUT: api/units/update/{id}
        // Params: Name, Code, Type, Address,  Description
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UnitDto dto)
        {
            var existingUnit = await _context.Unit.FindAsync(id);

            if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != existingUnit.Name)
            {
                bool nameExists = await _context.Unit.AnyAsync(u => u.Name == dto.Name && u.Id != id);
                if (nameExists)
                    return BadRequest(new { message = "Tên đơn vị đã tồn tại!" });

                existingUnit.Name = dto.Name;
            }

            if (!string.IsNullOrWhiteSpace(dto.Code))
                existingUnit.Code = dto.Code;

            if (!string.IsNullOrWhiteSpace(dto.Type))
                existingUnit.Type = dto.Type;

            if (!string.IsNullOrWhiteSpace(dto.Address))
                existingUnit.Address = dto.Address;

            if (!string.IsNullOrWhiteSpace(dto.Description))
                existingUnit.Description = dto.Description;

            // Gán người dùng vào đơn vị (nếu có)
            if (dto.UserId.HasValue)
            {
                var user = await _context.Auth.FindAsync(dto.UserId.Value);
                if (user == null)
                    return BadRequest(new { message = "Không tìm thấy người dùng để gán đơn vị!" });

                user.UnitId = existingUnit.Id;
            }

            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync("Update", $"Cập nhật đơn vị: {existingUnit.Name} (ID = {existingUnit.Id})", User.FindFirst(ClaimTypes.Name)?.Value);

            return Ok(new
            {
                message = "Cập nhật đơn vị thành công!",
                data = new
                {
                    existingUnit.Id,
                    existingUnit.Name,
                    existingUnit.Code,
                    existingUnit.Type,
                    existingUnit.Address,
                    existingUnit.Description
                }
            });
        }

        // DELETE: api/units/delete/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var unit = await _context.Unit.FindAsync(id);
            if (unit == null)
            {
                return NotFound(new { message = "Đơn vị không tồn tại!" });
            }
            _context.Unit.Remove(unit);
            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync("Delete", $"Xóa đơn vị: {unit.Name} (ID = {unit.Id})", User.FindFirst(ClaimTypes.Name)?.Value);

            return Ok(new { message = "Xóa đơn vị thành công!" });
        }
    }
}
