using ACCore.Logging;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace ACCore.TestProject
{
    public class ProjectDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=xx;Database=xx;Trusted_Connection=True;");
        }
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            AuditLogInitializer.TrackDbContext<Person>(this);
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            AuditLogInitializer.TrackDbContext<Person>(this);
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public DbSet<Person> People { get; set; }
    }
}
