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

        public DbSet<Auth> Auth { get; set; } = default!;
        public DbSet<TargetGroup> TargetGroup { get; set; } = default!;
        public DbSet<ParentCriteria> ParentCriteria { get; set; } = default!;
        public DbSet<SubCriteria> SubCriteria { get; set; } = default!;
        public DbSet<ActionLog> ActionLogs { get; set; } = default!;
        public DbSet<Unit> Unit { get; set; }
        public DbSet<EvaluationPeriod> EvaluationPeriod { get; set; } = default!;
        public DbSet<EvaluationUnit> EvaluationUnit { get; set; } = default!;
        public DbSet<ReviewCouncil> ReviewCouncil { get; set; } = default!;
        public DbSet<Reviewer> Reviewer { get; set; } = default!;
        public DbSet<ReviewAssignment> ReviewAssignment { get; set; } = default!;
        public DbSet<ReviewResult> ReviewResult { get; set; } = default!;
        public DbSet<FinalReviewResult> FinalReviewResult { get; set; } = default!;
    }
}
