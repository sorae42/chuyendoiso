namespace chuyendoiso.DTOs
{
    public class ReviewResultDto
    {
        public int ReviewAssignmentId { get; set; }
        public float? Score { get; set; }
        public string? Comment { get; set; }
        public IFormFile? Attachment { get; set; }
    }
}
