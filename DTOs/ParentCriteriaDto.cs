namespace chuyendoiso.DTOs
{
    public class ParentCriteriaDto
    {
        public string? Name { get; set; }
        public float? MaxScore { get; set; }
        public string? Description { get; set; }
        public IFormFile? EvidenceFile { get; set; }
        public int? TargetGroupId { get; set; }
        public int? EvaluationPeriodId { get; set; }
    }
}