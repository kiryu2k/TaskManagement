using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Models;

public class Task
{
    [Key] public int Id { get; set; }
    [Required] public string Name { get; set; } = null!;
    [Required] public string Description { get; set; } = null!;
    [Required] public int ProjectId { get; set; }
    [Required] public int AuthorId { get; set; }
    [Required] public int ExecutorId { get; set; }
    [Required] public TaskStatus Status { get; set; }
    [Required] public int Priority { get; set; }
    public Project Project { get; set; } = null!;
    public Employee Author { get; set; } = null!;
    public Employee Executor { get; set; } = null!;
}
