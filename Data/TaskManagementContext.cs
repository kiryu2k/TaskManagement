using Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using TaskManagement.Models;

namespace TaskManagement.Data;

public class TaskManagementContext : DbContext
{
    public TaskManagementContext(DbContextOptions<TaskManagementContext> options)
        : base(options)
    {
    }

    public DbSet<TaskManagement.Models.Employee> Employee { get; set; } = default!;
    public DbSet<TaskManagement.Models.Project> Project { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>()
            .HasMany(c => c.Employees)
            .WithMany(s => s.Projects)
            .UsingEntity(
                "ProjectEmployee",
                l => l.HasOne(typeof(Employee)).WithMany().HasForeignKey("EmployeeId").HasPrincipalKey(nameof(Models.Employee.Id)),
                r => r.HasOne(typeof(Project)).WithMany().HasForeignKey("ProjectId").HasPrincipalKey(nameof(Models.Project.Id)),
                j => j.HasKey("ProjectId", "EmployeeId"));
    }

}
