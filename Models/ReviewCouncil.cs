using DocumentFormat.OpenXml.Office2010.PowerPoint;
using System.ComponentModel.DataAnnotations;

namespace chuyendoiso.Models
{
    public class ReviewCouncil
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CreatedById { get; set; }
        public Auth CreatedBy { get; set; }

        public List<Reviewer> Reviewers { get; set; }
    }

}
