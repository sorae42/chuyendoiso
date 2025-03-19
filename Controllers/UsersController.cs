using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using chuyendoiso.Data;
using chuyendoiso.Models;
using Microsoft.AspNetCore.Authorization;

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

        // GET: api/Users/Details/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Auth.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound(new { message = "User not found!" });
            }

            return Ok(user);
        }

        // POST: api/Users/Create
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] Auth auth)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            auth.Password = BCrypt.Net.BCrypt.HashPassword(auth.Password);

            _context.Auth.Add(auth);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Details), new { id = auth.Id }, auth);
        }

        // POST: api/Users/Edit/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,FullName,Password,Email,Phone")] Auth auth)
        {
            if (id != auth.Id)
            {
                return BadRequest(new { message = "Không tìm thấy ID!" });
            }

            var existingUser = await _context.Auth.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng!" });
            }

            existingUser.Username = auth.Username ?? existingUser.Username;
            existingUser.FullName = auth.FullName ?? existingUser.FullName;
            existingUser.Email = auth.Email ?? existingUser.Email;
            existingUser.Phone = auth.Phone ?? existingUser.Phone;

            // Kiểm tra nếu mật khẩu có thay đổi
            if (!string.IsNullOrEmpty(auth.Password))
            {
                existingUser.Password = BCrypt.Net.BCrypt.HashPassword(auth.Password);
            }
            try
            {
                _context.Update(existingUser);
                await _context.SaveChangesAsync();
                return Ok(existingUser);
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
