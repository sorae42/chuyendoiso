using System.ComponentModel.DataAnnotations;

namespace chuyendoiso.Models
{
    public class ReviewResult
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReviewAssignmentId { get; set; }
        public ReviewAssignment ReviewAssignment { get; set; }

        public float? Score { get; set; }
        public string? Comment { get; set; }
        public string? AttachmentPath { get; set; }
    }
}
