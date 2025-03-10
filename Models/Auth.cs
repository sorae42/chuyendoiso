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

        // Token reset password
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }
    }
}
