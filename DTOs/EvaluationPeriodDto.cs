namespace chuyendoiso.DTOs
{
    public class EvaluationPeriodDto
    {
        public string? Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public List<int>? UnitIds { get; set; }
        public List<int>? ParentCriteriaIds { get; set; }
    }
}
