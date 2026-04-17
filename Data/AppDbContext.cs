
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Models;
 
namespace BlindMatchPAS.Data;
 
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }
 
    public DbSet<User> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ResearchArea> ResearchAreas { get; set; }
    public DbSet<SupervisorInterest> SupervisorInterests { get; set; }
    public DbSet<SupervisorExpertise> SupervisorExpertises { get; set; }
 
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SupervisorInterest>()
            .HasIndex(si => new { si.SupervisorId, si.ProjectId })
            .IsUnique();
 
        modelBuilder.Entity<SupervisorInterest>()
            .HasOne(si => si.Supervisor)
            .WithMany()
            .HasForeignKey(si => si.SupervisorId)
            .OnDelete(DeleteBehavior.Restrict);
 
        modelBuilder.Entity<SupervisorInterest>()
            .HasOne(si => si.Project)
            .WithMany()
            .HasForeignKey(si => si.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
 
        modelBuilder.Entity<SupervisorExpertise>()
            .HasIndex(se => new { se.SupervisorId, se.ResearchAreaId })
            .IsUnique();
 
        modelBuilder.Entity<SupervisorExpertise>()
            .HasOne(se => se.Supervisor)
            .WithMany()
            .HasForeignKey(se => se.SupervisorId)
            .OnDelete(DeleteBehavior.Cascade);
 
        modelBuilder.Entity<SupervisorExpertise>()
            .HasOne(se => se.ResearchArea)
            .WithMany()
            .HasForeignKey(se => se.ResearchAreaId)
            .OnDelete(DeleteBehavior.Cascade);
 
        modelBuilder.Entity<Project>()
            .HasOne(p => p.Student)
            .WithMany()
            .HasForeignKey(p => p.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
 
        modelBuilder.Entity<Project>()
            .HasOne(p => p.MatchedSupervisor)
            .WithMany()
            .HasForeignKey(p => p.MatchedSupervisorId)
            .OnDelete(DeleteBehavior.Restrict);
 
        modelBuilder.Entity<ResearchArea>().HasData(
            new ResearchArea { Id = 1, Name = "Artificial Intelligence" },
            new ResearchArea { Id = 2, Name = "Web Development" },
            new ResearchArea { Id = 3, Name = "Cybersecurity" },
            new ResearchArea { Id = 4, Name = "Cloud Computing" },
            new ResearchArea { Id = 5, Name = "Machine Learning" },
            new ResearchArea { Id = 6, Name = "Mobile Development" },
            new ResearchArea { Id = 7, Name = "Data Science" }
        );
    }
}