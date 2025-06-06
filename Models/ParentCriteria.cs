﻿using System.ComponentModel.DataAnnotations;

namespace chuyendoiso.Models
{
    public class ParentCriteria
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } // Tên tiêu chí
        public float? MaxScore { get; set; } // Điểm tối đa
        public string? Description { get; set; } // Mô tả tiêu chí
        public string? EvidenceInfo { get; set; } // Thông tin minh chứng

        public int? TargetGroupId { get; set; } // Id nhóm tiêu chí
        public TargetGroup TargetGroup { get; set; } // Nhóm tiêu chí

        public int? EvaluationPeriodId { get; set; } // Id kỳ đánh giá
        public EvaluationPeriod EvaluationPeriod { get; set; } // Kỳ đánh giá

        public List<SubCriteria> SubCriterias { get; set; } // Danh sách tiêu chí con
        
    }
}
