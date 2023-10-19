using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Models;

[Index(nameof(Email), IsUnique = true)]
public partial class Employee
{
    [Key] public int Id { get; set; }
    [Required] public string FirstName { get; set; } = null!;
    [Required] public string LastName { get; set; } = null!;
    public string? MiddleName { get; set; }
    [Required] public string Email { get; set; } = null!;
    public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
}
