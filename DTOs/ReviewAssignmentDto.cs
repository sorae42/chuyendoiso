namespace chuyendoiso.DTOs
{
    public class ReviewAssignmentDto
    {
        public int ReviewerId { get; set; }

        public List<ReviewAssignmentUnitDto> Assignments { get; set; } = new();
    }

    public class ReviewAssignmentUnitDto
    {
        public int UnitId { get; set; }

        public List<int> SubCriteriaIds { get; set; } = new();
    }
}
