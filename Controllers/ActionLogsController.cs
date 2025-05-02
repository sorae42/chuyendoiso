using chuyendoiso.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace chuyendoiso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActionLogsController : ControllerBase
    {
        private readonly chuyendoisoContext _context;

        public ActionLogsController(chuyendoisoContext context)
        {
            _context = context;
        }

        // GET: api/actionlogs
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var query = _context.ActionLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower().Trim();
                query = query.Where(log => log.Username.ToLower().Contains(search) || log.Action.ToLower().Contains(search));
            }

            var total = await query.CountAsync();
            var logs = await query
                .OrderByDescending(log => log.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                Items = logs,
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }

        // GET: api/actionlogs/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Details(int id)
        {
            var log = await _context.ActionLogs.FirstOrDefaultAsync(l => l.Id == id);
            if (log == null)
            {
                return NotFound(new { message = "Không tìm thấy nhật ký hoạt động!" });
            }

            return Ok(new
            {
                log.Id,
                log.Username,
                log.IPAddress,
                log.Timestamp,
                log.Action,
                log.Description
            });
        }
    }
}
