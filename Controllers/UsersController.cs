using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using chuyendoiso.Data;
using chuyendoiso.Models;
using Microsoft.AspNetCore.Authorization;
using chuyendoiso.DTOs;
using System.Security.Claims;
using chuyendoiso.Services;
using X.PagedList;

namespace chuyendoiso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly chuyendoisoContext _context;
        private readonly LogService _logService;

        public UsersController(chuyendoisoContext context, LogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: api/users
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var query = _context.Auth.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(u => u.Username.ToLower().Contains(search) || u.FullName.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var users = await query
                .OrderBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.FullName,
                    u.Email,
                    u.Phone,
                    u.IsTwoFactorEnabled
                })
                .ToListAsync();

            var result = new
            {
                Items = users,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages
            };

            return Ok(result);
        }

        // GET: api/users/userid
        [Authorize]
        [HttpGet("userid")]
        public IActionResult GetUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok(new { userId });
        }

        // GET: api/users/id
        // Params: Id
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Auth
                .Include(u => u.Unit)
                .Where( u => u.Id == id)
                .Select( u => new
                {
                    u.Id,
                    u.Username,
                    u.FullName,
                    u.Email,
                    u.Phone,
                    u.Role,
                    Unit = u.Unit != null ? new
                    {
                        u.Unit.Id,
                        u.Unit.Name
                    } : null,
                    u.IsTwoFactorEnabled
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng!" });
            }

            return Ok(user);
        }

        // POST: api/users/create
        // Params: Username, Password, Fullname, Email, Phone
        [HttpPost("create")]
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
                Phone = dto.Phone,
                Role = dto.Role
            };
            _context.Auth.Add(user);
            await _context.SaveChangesAsync();
            await _logService.WriteLogAsync(
                "Create User", 
                $"Tạo người dùng mới: {user.Username} (ID = {user.Id})", 
                User.FindFirst(ClaimTypes.Name)?.Value
            );

            return CreatedAtAction(nameof(Details), new { id = user.Id }, new 
            {
                user.Id,    
                user.Username,
                user.FullName,
                user.Email,
                user.Phone,
                user.Role
            });
        }

        // PUT: api/users/id
        // Params: Id, Username, Password, Fullname, Email, Phone
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
            {
                if (string.IsNullOrWhiteSpace(dto.OldPassword) || !BCrypt.Net.BCrypt.Verify(dto.OldPassword, existingUser.Password))
                {
                    return BadRequest(new { message = "Mật khẩu cũ không chính xác!" });
                }

                existingUser.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            try
            {
                _context.Update(existingUser);
                await _context.SaveChangesAsync();
                await _logService.WriteLogAsync(
                    "Update User", 
                    $"Cập nhật người dùng: {existingUser.Username} (ID = {existingUser.Id})", 
                    User.FindFirst(ClaimTypes.Name)?.Value
                );

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

        // DELETE: api/users/id
        // Params: Id
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
            await _logService.WriteLogAsync(
                "Delete User", 
                $"Xóa người dùng: {user.Username} (ID = {user.Id})", 
                User.FindFirst(ClaimTypes.Name)?.Value
            );

            return Ok(new { message = "Xóa người dùng thành công!" });
        }

        // PUT: api/users/update-profile
        // Params: Fullname, Email, Phone, Password
        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UserDto dto)
        {
            var username = User.Identity?.Name;
            var user = await _context.Auth.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng!" });

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName;

            if (!string.IsNullOrWhiteSpace(dto.Phone))
                user.Phone = dto.Phone;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.OldPassword) && !string.IsNullOrWhiteSpace(dto.Password))
            {
                if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.Password))
                    return BadRequest(new { message = "Mật khẩu cũ không chính xác!" });

                if (dto.Password.Length < 6)
                    return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 6 ký tự!" });

                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _context.SaveChangesAsync();

            await _logService.WriteLogAsync(
                "Update Profile", 
                $"Cập nhật thông tin người dùng: {user.Username} (ID = {user.Id})", 
                User.FindFirst(ClaimTypes.Name)?.Value
            );

            return Ok(new { message = "Cập nhật thông tin thành công!" });
        }
    }
}
