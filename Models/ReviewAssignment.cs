using System.ComponentModel.DataAnnotations;

namespace chuyendoiso.Models
{
    public class ReviewAssignment
    {
        [Key]
        public int Id { get; set; }

        public int ReviewerId { get; set; }
        public Reviewer Reviewer { get; set; }

        public int UnitId { get; set; }
        public Unit Unit { get; set; }

        public int? SubCriteriaId { get; set; }
        public SubCriteria? SubCriteria { get; set; }
    }

}
