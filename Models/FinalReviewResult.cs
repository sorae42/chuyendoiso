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
    }
}
