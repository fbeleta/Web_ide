using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebIde.Model;

namespace WebIde.DAL;

public class WebIdeDbContext : IdentityDbContext<AppUser>
{
    public WebIdeDbContext(DbContextOptions<WebIdeDbContext> options) : base(options) { }

    public DbSet<Problem> Problems { get; set; }
    public DbSet<User> DomainUsers { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<TestCase> TestCases { get; set; }
    public DbSet<Submission> Submissions { get; set; }
    public DbSet<ExecutionResult> ExecutionResults { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<ProblemSet> ProblemSets { get; set; }
    public DbSet<Attachment> Attachments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // sets up Identity tables

        // N-N: Problem <-> Tag
        modelBuilder.Entity<Problem>()
            .HasMany(p => p.Tags)
            .WithMany(t => t.Problems)
            .UsingEntity(j => j.ToTable("ProblemTags"));

        // N-N: Problem <-> ProblemSet
        modelBuilder.Entity<ProblemSet>()
            .HasMany(ps => ps.Problems)
            .WithMany()
            .UsingEntity(j => j.ToTable("ProblemSetProblems"));

        // N-N: User <-> Organization
        modelBuilder.Entity<Organization>()
            .HasMany(o => o.Members)
            .WithMany(u => u.Organizations)
            .UsingEntity(j => j.ToTable("OrganizationMembers"));
    }
}
