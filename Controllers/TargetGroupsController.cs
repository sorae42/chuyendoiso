using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using chuyendoiso.Data;
using chuyendoiso.Models;
using Microsoft.AspNetCore.Authorization;

namespace chuyendoiso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TargetGroupsController : Controller
    {
        private readonly chuyendoisoContext _context;

        public TargetGroupsController(chuyendoisoContext context)
        {
            _context = context;
        }

        // GET: /api/targetgroups
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var targetGroups = await _context.TargetGroup.ToListAsync();
            return Ok(targetGroups);
        }

        // GET: /api/targetgroups/5
        // Params: Id
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            var targetGroup = await _context.TargetGroup
                .FirstOrDefaultAsync(m => m.Id == id);

            if (targetGroup == null)
            {
                return NotFound(new { message = "Không tìm thấy nhóm!" });
            }

            return Ok(targetGroup);
        }

        // POST: /api/targetgroups/create
        // Params: Name
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] TargetGroup targetGroup)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.TargetGroup.Add(targetGroup);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Details), new { id = targetGroup.Id}, new
            {
                targetGroup.Id,
                targetGroup.Name
            });
        }

        // POST: /api/targetgroups/edit/5
        // Params: Id, Name
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [FromBody] TargetGroup targetGroup)
        {
            if (id != targetGroup.Id)
            {
                return BadRequest(new { message = "ID không khớp!" });
            }

            var existingTargetGroup = await _context.TargetGroup.FindAsync(id);
            if (existingTargetGroup == null)
            {
                return NotFound(new { message = "Không tìm thấy nhóm!" });
            }

            existingTargetGroup.Name = targetGroup.Name;

            try
            {
                _context.TargetGroup.Update(existingTargetGroup);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Cập nhật thành công!",
                    data = new
                    {
                        existingTargetGroup.Id,
                        existingTargetGroup.Name
                    }
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(new { message = "Cập nhật thất bại!" });
            }
        }

        // POST: /api/targetgroups/delete/5
        // Params: Id
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var targetGroup = await _context.TargetGroup.FindAsync(id);
            if (targetGroup == null)
            {
                return NotFound(new { message = "Không tìm thấy nhóm!" });
            }

            _context.TargetGroup.Remove(targetGroup);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Xóa nhóm thành công!" });
        }
    }
}
