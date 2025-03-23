using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using chuyendoiso.Data;
using chuyendoiso.Models;
using Microsoft.AspNetCore.Authorization;
using chuyendoiso.DTOs;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Security.Claims;

namespace chuyendoiso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly chuyendoisoContext _context;

        public UsersController(chuyendoisoContext context)
        {
            _context = context;
        }

        // GET: /api/Users
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var users = await _context.Auth.Select( u => new
            {
                u.Id,
                u.Username,
                u.FullName,
                u.Email,
                u.Phone
            })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/Users/userid
        [Authorize]
        [HttpGet("userid")]
        public IActionResult GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok(new { userId });
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Auth
                .Where( u => u.Id == id)
                .Select( u => new
                {
                    u.Id,
                    u.Username,
                    u.FullName,
                    u.Email,
                    u.Phone
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "User not found!" });
            }

            return Ok(user);
        }

        // POST: api/Users/Create
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] UserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) ||
                string.IsNullOrWhiteSpace(dto.Password) ||
                string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest(new { message = "Username, Password và Email là bắt buộc!" });
            }

            var user = new Auth
            {
                Username = dto.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone
            };

            _context.Auth.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Details), new { id = user.Id }, new 
            {
                user.Id,
                user.Username,
                user.FullName,
                user.Email,
                user.Phone
            });
        }

        // POST: api/Users/Edit/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [FromBody] UserDto dto)
        {
            var existingUser = await _context.Auth.FindAsync(id);

            if (existingUser == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng!" });
            }

            if (!string.IsNullOrWhiteSpace(dto.Username))
                existingUser.Username = dto.Username;

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                existingUser.FullName = dto.FullName;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                existingUser.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.Phone))
                existingUser.Phone = dto.Phone;

            if (!string.IsNullOrWhiteSpace(dto.Password))
                existingUser.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            try
            {
                _context.Update(existingUser);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    existingUser.Id,
                    existingUser.Username,
                    existingUser.FullName,
                    existingUser.Email,
                    existingUser.Phone
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, new { message = "Lỗi cập nhật thông tin!" });
            }
        }

        // POST: api/Users/Delete/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Auth.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng!" });
            }

            _context.Auth.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa người dùng thành công!" });
        }
    }
}
