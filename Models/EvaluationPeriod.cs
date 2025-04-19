using System.ComponentModel.DataAnnotations;

namespace chuyendoiso.Models
{
    public class EvaluationPeriod
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }

        public List<EvaluationUnit> EvaluationUnits { get; set; }

        public List<ParentCriteria> ParentCriterias { get; set; }
    }

}
