namespace chuyendoiso.DTOs
{
    public class UnitDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Type { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }

        public int? UserId { get; set; }
    }
}
