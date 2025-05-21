using DocumentFormat.OpenXml.Presentation;
using System.ComponentModel.DataAnnotations;

namespace chuyendoiso.Models
{
    public class ActionLog
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string IPAddress { get; set; }
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public string Description { get; set; }
        public int? RelatedUserId { get; set; }
        public Auth? RelatedUser { get; set; }
    }
}