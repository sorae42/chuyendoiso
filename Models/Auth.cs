using System.ComponentModel.DataAnnotations;

namespace chuyendoiso.Models
{
    public class Auth
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string? FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }

        public int? UnitId { get; set; }
        public Unit? Unit { get; set; }

        // Token reset password
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }

        // 2FA
        public bool IsTwoFactorEnabled { get; set; } = false;
        public string? OtpCode { get; set; }
        public DateTime? OtpExpires { get; set; }
        public string? TrustedDeviceToken { get; set; }
        public DateTime? TrustedUntil { get; set; }

    }
}
