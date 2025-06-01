namespace chuyendoiso.Models
{
    public class SubCriteriaAssignment
    {
        public int Id { get; set; }

        public int SubCriteriaId { get; set; }
        public SubCriteria SubCriteria { get; set; }

        public int EvaluationPeriodId { get; set; }
        public EvaluationPeriod EvaluationPeriod { get; set; }

        public int UnitId { get; set; }
        public Unit Unit { get; set; }

        public float? Score { get; set; }
        public DateTime? EvaluatedAt { get; set; }

        public string? Comment { get; set; }
        public string? EvidenceInfo { get; set; }
    }
}
