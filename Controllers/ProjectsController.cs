using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;
using TaskManagement.Queries;

namespace TaskManagement.Controllers;

[Route("/Projects")]
public class ProjectsController : Controller
{
    private readonly TaskManagementContext _ctx;

    public ProjectsController(TaskManagementContext context)
    {
        _ctx = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(ProjectQueries queries)
    {
        var projects = await _ctx.Project.ToListAsync();
        projects = queries.Priority != null ? projects
            .Where(e => e.Priority == queries.Priority).ToList() : projects;
        projects = queries.StartDate != null ? projects
            .Where(e => e.StartDate >= queries.StartDate).ToList() : projects;
        projects = queries.EndDate != null ? projects
            .Where(e => e.EndDate <= queries.EndDate).ToList() : projects;
        return View(SortProjectsBy(projects, queries.SortOrder));
    }

    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var project = await _ctx.Project.Include(e => e.Employees).FirstOrDefaultAsync(e => e.Id == id);
        if (project == null)
        {
            return NotFound();
        }
        ViewBag.Employees = _ctx.Employee.ToHashSet().Except(project.Employees);
        return View(project);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Employees = await _ctx.Employee.ToListAsync();
        return View();
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([Bind("Id,Name,CustomerCompany,ExecutorCompany,StartDate,EndDate,Priority,LeaderId")] Project project)
    {
        if (project.StartDate >= project.EndDate)
        {
            ViewBag.Employees = await _ctx.Employee.ToListAsync();
            return View(project);
        }
        var leader = await _ctx.Employee.FindAsync(project.LeaderId);
        if (leader == null)
        {
            ViewBag.Employees = await _ctx.Employee.ToListAsync();
            return View(project);
        }
        project.Employees.Add(leader);
        _ctx.Project.Add(project);
        await _ctx.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("AddEmployees")]
    public async Task<IActionResult> AddEmployees(int projectId, int[] employeesIds)
    {
        var project = await _ctx.Project.FindAsync(projectId);
        if (project == null)
        {
            return NotFound();
        }
        if (employeesIds == null)
        {
            return BadRequest();
        }
        foreach (var employee in _ctx.Employee.Where(e => employeesIds.Contains(e.Id)))
        {
            project.Employees.Add(employee);
        }
        await _ctx.SaveChangesAsync();
        return RedirectToRoute(new
        {
            controller = "Projects",
            action = "Details",
            id = projectId
        });
    }

    [HttpPost("DeleteEmployee")]
    public async Task<IActionResult> DeleteEmployee(int projectId, int employeeId)
    {
        var project = await _ctx.Project.Include(e => e.Employees)
            .FirstOrDefaultAsync(e => e.Id == projectId);
        if (project == null)
        {
            return NotFound();
        }
        if (employeeId != project.LeaderId)
        {
            var employee = await _ctx.Employee.FindAsync(employeeId);
            project.Employees.Remove(employee);
            await _ctx.SaveChangesAsync();
        }
        return RedirectToRoute(new
        {
            controller = "Projects",
            action = "Details",
            id = projectId
        });
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var project = await _ctx.Project.FindAsync(id);
        if (project == null)
        {
            return NotFound();
        }
        ViewBag.Employees = await _ctx.Employee.ToListAsync();
        return View(project);
    }

    [HttpPost("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CustomerCompany,ExecutorCompany,StartDate,EndDate,Priority,LeaderId")] Project model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }
        if (model.StartDate >= model.EndDate)
        {
            ViewBag.Employees = await _ctx.Employee.ToListAsync();
            return View(model);
        }
        var project = await _ctx.Project.Include(e => e.Employees).FirstOrDefaultAsync(e => e.Id == id);
        if (project == null)
        {
            return NotFound();
        }
        if (project.LeaderId != model.LeaderId)
        {
            var leader = await _ctx.Employee.FindAsync(model.LeaderId);
            if (leader == null)
            {
                ViewBag.Employees = await _ctx.Employee.ToListAsync();
                return View(project);
            }
            var oldLeader = project.Employees.FirstOrDefault(e => e.Id == project.LeaderId);
            project.Employees.Remove(oldLeader);
            project.Employees.Add(leader);
            project.LeaderId = model.LeaderId;
        }
        project.Name = model.Name;
        project.CustomerCompany = model.CustomerCompany;
        project.ExecutorCompany = model.ExecutorCompany;
        project.StartDate = model.StartDate;
        project.EndDate = model.EndDate;
        project.Priority = model.Priority;
        try
        {
            await _ctx.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ProjectExists(project.Id))
            {
                return NotFound();
            }
            throw;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _ctx.Project.FindAsync(id);
        if (project == null)
        {
            return NotFound();
        }
        return View(project);
    }

    [HttpPost("Delete/{id:int}")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var project = await _ctx.Project.FindAsync(id);
        if (project != null)
        {
            _ctx.Project.Remove(project);
        }
        await _ctx.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ProjectExists(int id)
    {
        return (_ctx.Project?.Any(e => e.Id == id)).GetValueOrDefault();
    }

    private IOrderedEnumerable<Project>? SortProjectsBy(IEnumerable<Project> projects, string sortOrder)
    {
        ViewBag.NameSort = String.IsNullOrEmpty(sortOrder) ? "nameDesc" : "";
        ViewBag.CustomerSort = sortOrder == "customer" ? "customerDesc" : "customer";
        ViewBag.ExecutorSort = sortOrder == "executor" ? "executorDesc" : "executor";
        ViewBag.StartDateSort = sortOrder == "startDate" ? "startDateDesc" : "startDate";
        ViewBag.EndDateSort = sortOrder == "endDate" ? "endDateDesc" : "endDate";
        ViewBag.PrioritySort = sortOrder == "priority" ? "priorityDesc" : "priority";
        return sortOrder switch
        {
            "nameDesc" => projects.OrderByDescending(e => e.Name),
            "customer" => projects.OrderBy(e => e.CustomerCompany),
            "customerDesc" => projects.OrderByDescending(e => e.CustomerCompany),
            "executor" => projects.OrderBy(e => e.ExecutorCompany),
            "executorDesc" => projects.OrderByDescending(e => e.ExecutorCompany),
            "startDate" => projects.OrderBy(e => e.StartDate),
            "startDateDesc" => projects.OrderByDescending(e => e.StartDate),
            "endDate" => projects.OrderBy(e => e.EndDate),
            "endDateDesc" => projects.OrderByDescending(e => e.EndDate),
            "priority" => projects.OrderBy(e => e.Priority),
            "priorityDesc" => projects.OrderByDescending(e => e.Priority),
            _ => projects.OrderBy(e => e.Name),
        };
    }
}
