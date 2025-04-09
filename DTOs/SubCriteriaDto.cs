namespace chuyendoiso.DTOs
{
    public class SubCriteriaDto
    {
        public string? Name { get; set; }
        public float? MaxScore { get; set; }
        public string? Description { get; set; }
        public string? EvidenceInfo { get; set; }
        public string? ParentCriteriaName { get; set; }
        public DateTime? EvaluatedAt { get; set; }
    }
}
