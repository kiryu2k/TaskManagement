using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Models;

public class Project
{
    [Key] public int Id { get; set; }
    [Required] public string Name { get; set; } = null!;
    [Required] public string CustomerCompany { get; set; } = null!;
    [Required] public string ExecutorCompany { get; set; } = null!;
    [Required] public DateTime StartDate { get; set; }
    [Required] public DateTime EndDate { get; set; }
    [Required] public int Priority { get; set; }
    [Required] public int LeaderId { get; set; }
    public virtual ICollection<Employee> Employees { get; set; } = new HashSet<Employee>();
}
