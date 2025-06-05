namespace chuyendoiso.DTOs
{
    public class FinalReviewRejectionDto
    {
        public int ReviewAssignmentId { get; set; }
        public string RejectReason { get; set; } = null!;
        public bool IsFinalFail { get; set; } = false;
        public IFormFile? Attachment { get; set; }
    }
}
