using System.ComponentModel.DataAnnotations;

namespace chuyendoiso.Models
{
    public class Reviewer
    {
        [Key]
        public int Id { get; set; }

        public int AuthId { get; set; }
        public Auth Auth { get; set; }
        public bool IsChair { get; set; } = false;

        public int ReviewCouncilId { get; set; }
        public ReviewCouncil ReviewCouncil { get; set; }

        public List<ReviewAssignment> ReviewAssignments { get; set; }
    }

}
