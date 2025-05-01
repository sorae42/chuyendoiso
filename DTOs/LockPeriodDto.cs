namespace chuyendoiso.DTOs
{
    public class LockPeriodDto
    {
        public DateTime? UnlockDate { get; set; }
        public string? Reason { get; set; }
        public IFormFile? Attachment { get; set; }
    }

}
