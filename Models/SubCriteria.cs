using System.ComponentModel.DataAnnotations;

namespace chuyendoiso.Models
{
    public class SubCriteria
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } // Tên tiêu chí
        public int MaxScore { get; set; } // Điểm tối đa
        public string? Description { get; set; } // Mô tả tiêu chí
        public ParentCriteria ParentCriteria { get; set; } // Tiêu chí cha
        public string? EvidenceInfo { get; set; } // Thông tin minh chứng
    }
}
