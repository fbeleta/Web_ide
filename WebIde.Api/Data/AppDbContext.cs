using Microsoft.EntityFrameworkCore;
using WebIde.Model;
using WebIde.Model.Enums;

namespace WebIde.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Problem> Problems => Set<Problem>();
    public DbSet<ProblemSet> ProblemSets => Set<ProblemSet>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<TestCase> TestCases => Set<TestCase>();
    public DbSet<ExecutionResult> ExecutionResults => Set<ExecutionResult>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── User ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(u => u.Id);
            e.Property(u => u.Username).IsRequired();
            e.Property(u => u.Email).IsRequired();
            e.Property(u => u.DisplayName).IsRequired();
            e.Property(u => u.Role).HasConversion<string>();
            e.Property(u => u.RegisteredAt).HasColumnType("timestamptz");
        });

        // ── Organization ─────────────────────────────────────────────────────
        modelBuilder.Entity<Organization>(e =>
        {
            e.ToTable("organizations");
            e.HasKey(o => o.Id);
            e.Property(o => o.Name).IsRequired();
            e.Property(o => o.Description).IsRequired();

            // N-N: Organization <-> User (members)
            e.HasMany(o => o.Members)
             .WithMany(u => u.Organizations)
             .UsingEntity(j => j.ToTable("organization_members"));
        });

        // ── Problem ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Problem>(e =>
        {
            e.ToTable("problems");
            e.HasKey(p => p.Id);
            e.Property(p => p.Title).IsRequired();
            e.Property(p => p.Description).IsRequired();
            e.Property(p => p.Difficulty).HasConversion<string>();
            e.Property(p => p.CreatedAt).HasColumnType("timestamptz");
            e.Property(p => p.AuthorUsername).IsRequired();
        });

        // ── Tag ───────────────────────────────────────────────────────────────
        modelBuilder.Entity<Tag>(e =>
        {
            e.ToTable("tags");
            e.HasKey(t => t.Id);
            e.Property(t => t.Name).IsRequired();
            e.HasIndex(t => t.Name).IsUnique();

            // N-N: Problem <-> Tag
            e.HasMany(t => t.Problems)
             .WithMany(p => p.Tags)
             .UsingEntity(j => j.ToTable("problem_tags"));
        });

        // ── ProblemSet ────────────────────────────────────────────────────────
        modelBuilder.Entity<ProblemSet>(e =>
        {
            e.ToTable("problem_sets");
            e.HasKey(ps => ps.Id);
            e.Property(ps => ps.Title).IsRequired();
            e.Property(ps => ps.Description).IsRequired();
            e.Property(ps => ps.CreatedAt).HasColumnType("timestamptz");

            e.HasOne(ps => ps.Organization)
             .WithMany(o => o.ProblemSets)
             .HasForeignKey("organization_id")
             .OnDelete(DeleteBehavior.Cascade);

            // N-N: ProblemSet <-> Problem
            e.HasMany(ps => ps.Problems)
             .WithMany()
             .UsingEntity(j => j.ToTable("problem_set_problems"));
        });

        // ── TestCase ──────────────────────────────────────────────────────────
        modelBuilder.Entity<TestCase>(e =>
        {
            e.ToTable("test_cases");
            e.HasKey(tc => tc.Id);
            e.Property(tc => tc.InputArgs).IsRequired();
            e.Property(tc => tc.ExpectedOutput).IsRequired();

            e.HasOne(tc => tc.Problem)
             .WithMany(p => p.TestCases)
             .HasForeignKey("problem_id")
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── ExecutionResult ───────────────────────────────────────────────────
        modelBuilder.Entity<ExecutionResult>(e =>
        {
            e.ToTable("execution_results");
            e.HasKey(er => er.Id);
            e.Property(er => er.Stdout).IsRequired();
            e.Property(er => er.Stderr).IsRequired();
        });

        // ── Submission ────────────────────────────────────────────────────────
        modelBuilder.Entity<Submission>(e =>
        {
            e.ToTable("submissions");
            e.HasKey(s => s.Id);
            e.Property(s => s.SourceCode).IsRequired();
            e.Property(s => s.Language).IsRequired();
            e.Property(s => s.Status).HasConversion<string>();
            e.Property(s => s.SubmittedAt).HasColumnType("timestamptz");

            e.HasOne(s => s.User)
             .WithMany(u => u.Submissions)
             .HasForeignKey("user_id")
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.Problem)
             .WithMany(p => p.Submissions)
             .HasForeignKey("problem_id")
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.ExecutionResult)
             .WithOne()
             .HasForeignKey<Submission>("execution_result_id")
             .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
