using System.ComponentModel.DataAnnotations;

namespace chuyendoiso.Models
{
    public class TargetGroup
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } // Tên nhóm tiêu chí
        public List<ParentCriteria> ParentCriterias { get; set; } // Danh sách tiêu chí
    }
}
