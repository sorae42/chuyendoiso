using System.ComponentModel.DataAnnotations;

namespace chuyendoiso.Models
{
    public class Unit
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;  // Tên đơn vị (bắt buộc) – VD: "Phường X", "TP Nha Trang"
        public string? Code { get; set; }                 // Mã đơn vị (nếu cần quản lý theo hệ thống mã hóa)
        public string? Type { get; set; }                 // Loại đơn vị: "Phường", "Xã", "Thị trấn", "Quận", "TP"...
        public string? Address { get; set; }              // Địa chỉ đơn vị
        public string? Description { get; set; }          // Ghi chú bổ sung

        // Navigation properties
        public List<Auth>? Users { get; set; }            // Các tài khoản thuộc đơn vị
        public List<EvaluationUnit>? EvaluationUnits { get; set; }  // Kỳ đánh giá mà đơn vị này tham gia
    }
}
