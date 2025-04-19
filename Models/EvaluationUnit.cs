using System.ComponentModel.DataAnnotations;

namespace chuyendoiso.Models
{
    public class EvaluationUnit
    {
        [Key]
        public int Id { get; set; }
        public int EvaluationPeriodId { get; set; }
        public EvaluationPeriod EvaluationPeriod { get; set; }
        public int UnitId { get; set; }
        public Unit Unit { get; set; }
    }

}
