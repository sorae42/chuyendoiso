using System.ComponentModel.DataAnnotations;

namespace chuyendoiso.Models
{
    public class Unit
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Type { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }

        // Navigation properties
        public List<Auth>? Users { get; set; }
        public List<EvaluationUnit>? EvaluationUnits { get; set; }
    }
}