using Microsoft.AspNetCore.Mvc;
using chuyendoiso.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using chuyendoiso.Interface;

namespace chuyendoiso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthsController : Controller
    {
        private readonly chuyendoisoContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;

        public AuthsController(chuyendoisoContext context, IConfiguration configuration, IEmailSender emailSender)
        {
            _context = context;
            _configuration = configuration;
            _emailSender = emailSender;
        }

        // POST: api/Auths/login
        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromForm] string password, [FromForm] string username)
        {
            var user = _context.Auth.Where(x => x.Username == username).FirstOrDefault();

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không chính xác!" });
            }

            // Lấy thông tin cấu hình JWT từ appsettings.json
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(60),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { message = "Đăng nhập thành công!", token = tokenString });
        }

        // POST: api/Auths/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Đăng xuất thành công!" });
        }

        // POST: api/Auths/forgot-password
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] Dictionary<string, string> request)
        {
            if (!request.ContainsKey("email"))
            {
                return BadRequest(new { message = "Email không được để trống!" });
            }

            string email = request["email"];
            var user = _context.Auth.FirstOrDefault(x => x.Email == email);

            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng!" });
            }

            // Create token reset password
            string resetToken = Guid.NewGuid().ToString();
            user.ResetToken = resetToken;
            user.ResetTokenExpires = DateTime.Now.AddHours(1);

            _context.SaveChanges();

            // Link reset password
            string resetLink = $"{_configuration["AppUrl"]}/resetpassword?token={resetToken}";

            // Send email
            await _emailSender.SendEmailAsync(email, "Đặt lại mật khẩu",
                $"Nhấn vào link sau để đặt lại mật khẩu: <a href='{resetLink}'>Đặt lại mật khẩu</a>");

            return Ok(new { message = "Email đặt lại mật khẩu đã được gửi!" });
        }

        // POST: api/Auths/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] Dictionary<string, string> request)
        {
            if (!request.ContainsKey("token") || !request.ContainsKey("newPassword"))
            {
                return BadRequest(new { message = "Thiếu token hoặc mật khẩu mới!" });
            }

            string token = request["token"];
            string newPassword = request["newPassword"];

            var user = _context.Auth.FirstOrDefault(x => x.ResetToken == token && x.ResetTokenExpires > DateTime.Now);

            if (user == null)
            {
                return NotFound(new { message = "Token không hợp lệ hoặc đã hết hạn!" });
            }

            // Hash new password
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);

            // Delete token after used
            user.ResetToken = null;
            user.ResetTokenExpires = null;

            _context.SaveChanges();

            return Ok(new { message = "Đặt lại mật khẩu thành công!" });
        }
    }
}
