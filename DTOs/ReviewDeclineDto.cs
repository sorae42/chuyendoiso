namespace chuyendoiso.DTOs
{
    public class ReviewDeclineDto
    {
        public int ReviewAssignmentId { get; set; }
        public string Reason { get; set; } = null!;
        public IFormFile? Attachment { get; set; }
    }
}
