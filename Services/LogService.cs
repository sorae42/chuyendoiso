using chuyendoiso.Data;
using chuyendoiso.Models;
using System.Security.Claims;

namespace chuyendoiso.Services
{
    public class LogService
    {
        private readonly chuyendoisoContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogService(chuyendoisoContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task WriteLogAsync(string action, string description, string? username = null, int? relatedUserId = null)
        {
            var actualUsername = username ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";
            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";

            var log = new ActionLog
            {
                Username = actualUsername,
                RelatedUserId = relatedUserId,
                IPAddress = ipAddress,
                Timestamp = DateTime.UtcNow,
                Action = action,
                Description = description
            };

            _context.ActionLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
