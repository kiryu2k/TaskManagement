using Microsoft.AspNetCore.Mvc;

namespace TaskManagement.Queries;

public class TaskQueries
{
    [FromQuery(Name = "sortOrder")] public string SortOrder { get; set; } = null!;
    [FromQuery(Name = "status")] public Models.TaskStatus? Status { get; set; }
    [FromQuery(Name = "priority")] public int? Priority { get; set; }
    [FromQuery(Name = "projectId")] public int? ProjectId { get; set; }
    [FromQuery(Name = "authorId")] public int? AuthorId { get; set; }
    [FromQuery(Name = "executorId")] public int? ExecutorId { get; set; }
}
