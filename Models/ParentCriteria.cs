using System.ComponentModel.DataAnnotations;

namespace chuyendoiso.Models
{
    public class ParentCriteria
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } // Tên tiêu chí
        public int MaxScore { get; set; } // Điểm tối đa
        public string? Description { get; set; } // Mô tả tiêu chí
        public TargetGroup TargetGroup { get; set; } // Nhóm tiêu chí
        public List<SubCriteria> SubCriterias { get; set; } // Danh sách tiêu chí con
        public string? EvidenceInfo { get; set; } // Thông tin minh chứng
    }
}
