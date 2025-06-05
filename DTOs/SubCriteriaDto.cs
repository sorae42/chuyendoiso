namespace chuyendoiso.DTOs
{
    public class SubCriteriaDto
    {
        public string? Name { get; set; }
        public float? MaxScore { get; set; }
        public string? Description { get; set; }
        public IFormFile? EvidenceInfo { get; set; }
        public int? ParentCriteriaId { get; set; }
        public DateTime? EvaluatedAt { get; set; }
    }
}
