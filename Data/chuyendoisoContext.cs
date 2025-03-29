using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using chuyendoiso.Models;

namespace chuyendoiso.Data
{
    public class chuyendoisoContext : DbContext
    {
        public chuyendoisoContext (DbContextOptions<chuyendoisoContext> options)
            : base(options)
        {
        }

        public DbSet<chuyendoiso.Models.Auth> Auth { get; set; } = default!;
        public DbSet<chuyendoiso.Models.TargetGroup> TargetGroup { get; set; } = default!;
        public DbSet<chuyendoiso.Models.ParentCriteria> ParentCriteria { get; set; } = default!;
        public DbSet<chuyendoiso.Models.SubCriteria> SubCriteria { get; set; } = default!;
    }
}
