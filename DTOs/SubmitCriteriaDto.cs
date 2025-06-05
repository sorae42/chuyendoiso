namespace chuyendoiso.DTOs
{
    public class SubmitCriteriaDto
    {
        public int SubCriteriaId { get; set; }
        public int PeriodId { get; set; }
        public float? Score { get; set; }
        public string? Comment { get; set; }
        public IFormFile? EvidenceFile { get; set; }
    }
}
