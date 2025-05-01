namespace chuyendoiso.DTOs
{
    public class FinalReviewResultDto
    {
        public int ReviewAssignmentId { get; set; }
        public float? FinalScore { get; set; }
        public string? FinalComment { get; set; }
        public IFormFile? FinalAttachment { get; set; }
    }
}