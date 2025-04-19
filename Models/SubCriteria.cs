using System.ComponentModel.DataAnnotations;

namespace chuyendoiso.Models
{
    public class SubCriteria
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } // Tên tiêu chí
        public float? MaxScore { get; set; } // Điểm tối đa
        public string? Description { get; set; } // Mô tả tiêu chí
        public string? EvidenceInfo { get; set; } // Thông tin minh chứng

        public int ParentCriteriaId { get; set; } // Id tiêu chí cha
        public ParentCriteria ParentCriteria { get; set; } // Tiêu chí cha

        public DateTime? EvaluatedAt { get; set; } // Thời gian đánh giá
        public string? UnitEvaluate { get; set; } // Đơn vị đánh giá
}
}
