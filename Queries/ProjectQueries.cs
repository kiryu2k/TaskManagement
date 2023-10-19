using Microsoft.AspNetCore.Mvc;

namespace TaskManagement.Queries;

public class ProjectQueries
{
    [FromQuery(Name = "sortOrder")] public string SortOrder { get; set; }
    [FromQuery(Name = "startDate")] public DateTime? StartDate { get; set; }
    [FromQuery(Name = "endDate")] public DateTime? EndDate { get; set; }
    [FromQuery(Name = "priority")] public int? Priority { get; set; }
}
