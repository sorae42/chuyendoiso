using Microsoft.AspNetCore.Mvc;
using chuyendoiso.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using chuyendoiso.Interface;
using chuyendoiso.Services;
using chuyendoiso.Models;
using chuyendoiso.DTOs;
using Microsoft.EntityFrameworkCore;

namespace chuyendoiso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthsController : Controller
    {
        private readonly chuyendoisoContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        private readonly LogService _logService;

        public AuthsController(chuyendoisoContext context, IConfiguration configuration, IEmailSender emailSender, LogService logService)
        {
            _context = context;
            _configuration = configuration;
            _emailSender = emailSender;
            _logService = logService;
        }

        // POST: api/auths/login
        // Params: username, password
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync([FromForm] string password, [FromForm] string username, [FromForm] string? trustedToken = null)
        {
            var user = _context.Auth
                .Include(u => u.Unit)
                .FirstOrDefault(x => x.Username == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không chính xác!" });
            }

            if (user.IsTwoFactorEnabled)
            {
                var now = DateTime.UtcNow;

                if (trustedToken == null || user.TrustedDeviceToken != trustedToken || user.TrustedUntil < now)
                {
                    // Generate OTP code
                    var otp = new Random().Next(100000, 999999).ToString();
                    user.OtpCode = otp;
                    user.OtpExpires = now.AddMinutes(5);
                    _context.SaveChanges();

                    // Send OTP code to email
                    await _emailSender.SendEmailAsync(user.Email, "Mã OTP đăng nhập", $"Mã OTP của bạn là <b>{otp}</b>. Hiệu lực trong 5 phút.");
                    return Ok(new { message = "Cần nhập OTP", needOtp = true });
                }
            }

            var jwt = GenerateJwtToken(user);

            await _logService.WriteLogAsync(
                "Login", 
                $"Tài khoản {username} đăng nhập", 
                username
            );

            return Ok(new { message = "Đăng nhập thành công", token = jwt, trustedToken = user.TrustedDeviceToken });
        }

        // POST: api/auths/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

            await _logService.WriteLogAsync("Logout", $"Tài khoản {username} đã đăng xuất", username);

            return Ok(new { message = "Đăng xuất thành công!" });
        }

        // POST: api/auths/forgot-password
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

        // POST: api/auths/reset-password
        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] Dictionary<string, string> request)
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

        // POST: api/auths/toggle-2fa
        [HttpPost("toggle-2fa")]
        [Authorize]
        public IActionResult ToggleTwoFactor([FromBody] bool enable2FA)
        {
            var username = User.Identity?.Name;
            var user = _context.Auth.FirstOrDefault(x => x.Username == username);
            if (user == null)
            {
                return NotFound(new { message = "Người dùng không tồn tại!" });
            }

            user.IsTwoFactorEnabled = enable2FA;
            _context.SaveChanges();

            return Ok(new { message = $"Xác thực 2 bước {(enable2FA ? "đã bật" : "đã tắt")}" });
        }

        // POST: api/auths/verify-otp
        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public IActionResult VerifyOtp([FromForm] string username, [FromForm] string otp, [FromForm] bool trustDevice = true)
        {
            var user = _context.Auth.FirstOrDefault(x => x.Username == username);
            if (user == null || user.OtpCode != otp || user.OtpExpires < DateTime.UtcNow)
                return BadRequest(new { message = "OTP không hợp lệ hoặc đã hết hạn" });

            user.OtpCode = null;
            user.OtpExpires = null;

            if (trustDevice)
            {
                user.TrustedDeviceToken = Guid.NewGuid().ToString();
                user.TrustedUntil = DateTime.UtcNow.AddYears(1);
            }

            _context.SaveChanges();

            var jwt = GenerateJwtToken(user);
            return Ok(new { message = "Xác thực thành công", token = jwt, trustedToken = user.TrustedDeviceToken });
        }

        // Method generate jwt token
        private string GenerateJwtToken(Auth user)
        {
            // Lấy thông tin cấu hình JWT từ appsettings.json
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(60),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }
    }
}