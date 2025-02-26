using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using chuyendoiso.Data;
using chuyendoiso.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace chuyendoiso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthsController : Controller
    {
        private readonly chuyendoisoContext _context;

        public AuthsController(chuyendoisoContext context)
        {
            _context = context;
        }

        // GET: Auths
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _context.Auth.ToListAsync();
            return Ok(users);
        }

        // GET: Auths/Details/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var auth = await _context.Auth.FirstOrDefaultAsync(m => m.Id == id);
            if (auth == null)
            {
                return NotFound(new { message = "User not found!" });
            }

            return Ok(auth);
        }

        // POST: Auths/Create
        [HttpPost]
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

        // POST: Auths/Edit/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,Password,Email,Phone")] Auth auth)
        {
            if (id != auth.Id)
            {
                return BadRequest(new { message = "Không tìm thấy ID!" });
            }

            var existingAuth = await _context.Auth.FindAsync(id);
            if (existingAuth == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng!" });
            }

            existingAuth.Username = auth.Username ?? existingAuth.Username;
            existingAuth.Email = auth.Email ?? existingAuth.Email;
            existingAuth.Phone = auth.Phone ?? existingAuth.Phone;

            // Kiểm tra nếu mật khẩu có thay đổi
            if (!string.IsNullOrEmpty(auth.Password))
            {
                existingAuth.Password = BCrypt.Net.BCrypt.HashPassword(auth.Password);
            }
            try
            {
                _context.Update(existingAuth);
                await _context.SaveChangesAsync();
                return Ok(existingAuth);
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, new { message = "Lỗi cập nhật thông tin!" });
            }
        }

        // POST: Auths/Delete/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var auth = await _context.Auth.FindAsync(id);
            if (auth == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng!" });
            }

            _context.Auth.Remove(auth);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa người dùng thành công!" });
        }

        // POST: api/Auths/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string password, [FromForm] string username)
        {
            var user = _context.Auth.Where(x => x.Username == username).FirstOrDefault();

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không chính xác!" });
            }
                
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username)
        };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60),
                IsPersistent = true,
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            return Ok(new { message = "Đăng nhập thành công!" });
        }

        // POST: api/Auths/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Đăng xuất thành công!" });
        }

        private bool AuthExists(int id)
        {
            return _context.Auth.Any(e => e.Id == id);
        }
    }
}
