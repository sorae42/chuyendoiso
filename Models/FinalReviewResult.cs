using System.ComponentModel.DataAnnotations;

namespace chuyendoiso.Models
{
    public class FinalReviewResult
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReviewAssignmentId { get; set; }
        public ReviewAssignment ReviewAssignment { get; set; }

        public float? FinalScore { get; set; }
        public string? FinalComment { get; set; }
        public string? FinalAttachmentPath { get; set; }

        public bool IsRejected { get; set; } = false;
        public string? RejectReason { get; set; }
        public string? RejectAttachmentPath { get; set; }
        public DateTime? RejectedAt { get; set; }

        public bool IsFinalFail { get; set; } = false; // nếu = true thì cấm cấp dưới gửi nữa
    }
}
