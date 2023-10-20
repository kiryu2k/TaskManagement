using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaskManagement.Data;
using TaskManagement.Models;
using TaskManagement.Queries;

namespace TaskManagement.Controllers;

[Route("/Tasks")]
public class TasksController : Controller
{
    private readonly TaskManagementContext _ctx;

    public TasksController(TaskManagementContext context)
    {
        _ctx = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(TaskQueries queries)
    {
        var tasks = await _ctx.Task.Include(e => e.Project)
            .Include(e => e.Author).Include(e => e.Executor).ToListAsync();
        tasks = queries.Status != null ? tasks
            .Where(e => e.Status == queries.Status).ToList() : tasks;
        tasks = queries.Priority != null ? tasks
            .Where(e => e.Priority == queries.Priority).ToList() : tasks;
        tasks = queries.ProjectId != null ? tasks
            .Where(e => e.ProjectId == queries.ProjectId).ToList() : tasks;
        tasks = queries.AuthorId != null ? tasks
            .Where(e => e.AuthorId == queries.AuthorId).ToList() : tasks;
        tasks = queries.ExecutorId != null ? tasks
            .Where(e => e.ExecutorId == queries.ExecutorId).ToList() : tasks;
        ViewBag.Projects = new SelectList(await _ctx.Project
            .Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.Name
            }).ToListAsync(), "Value", "Text");
        ViewBag.Employees = new SelectList(await _ctx.Employee
            .Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.FirstName + e.LastName
            }).ToListAsync(), "Value", "Text");
        ViewBag.Status = new SelectList(TaskStatusConverter.StatusToString, "Key", "Value");
        return View(SortTasksBy(tasks, queries.SortOrder));
    }

    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var task = await _ctx.Task.Include(e => e.Project).Include(e => e.Author)
            .Include(e => e.Executor).FirstOrDefaultAsync(e => e.Id == id);
        if (task == null)
        {
            return NotFound();
        }
        ViewBag.Status = TaskStatusConverter.StatusToString[task.Status];
        return View(task);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Projects = await _ctx.Project.ToListAsync();
        ViewBag.Employees = await _ctx.Employee.ToListAsync();
        ViewBag.Status = TaskStatusConverter.StatusToString;
        return View();
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([Bind("Id,Name,Description,ProjectId,AuthorId,ExecutorId,Status,Priority")] Models.Task task)
    {
        if (!TaskStatusConverter.StatusToString.ContainsKey(task.Status))
        {
            return BadRequest();
        }
        var project = await _ctx.Project.Include(e => e.Employees)
            .FirstOrDefaultAsync(e => e.Id == task.ProjectId);
        if (project == null)
        {
            return BadRequest();
        }
        task.Project = project;
        var author = project.Employees.FirstOrDefault(e => e.Id == task.AuthorId);
        if (author == null)
        {
            ViewBag.Projects = await _ctx.Project.ToListAsync();
            ViewBag.Employees = await _ctx.Employee.ToListAsync();
            ViewBag.Status = TaskStatusConverter.StatusToString;
            ModelState.AddModelError(nameof(Models.Task.AuthorId),
                "This author doesn't work on the selected project");
            return View(task);
        }
        task.Author = author;
        if (task.AuthorId == task.ExecutorId)
        {
            task.Executor = author;
        }
        else
        {
            var executor = project.Employees.FirstOrDefault(e => e.Id == task.ExecutorId);
            if (executor == null)
            {
                ViewBag.Projects = await _ctx.Project.ToListAsync();
                ViewBag.Employees = await _ctx.Employee.ToListAsync();
                ViewBag.Status = TaskStatusConverter.StatusToString;
                ModelState.AddModelError(nameof(Models.Task.ExecutorId),
                    "This executor doesn't work on the selected project");
                return View(task);
            }
            task.Executor = executor;
        }
        _ctx.Task.Add(task);
        await _ctx.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var task = await _ctx.Task.Include(e => e.Project).FirstOrDefaultAsync(e => e.Id == id);
        if (task == null)
        {
            return NotFound();
        }
        ViewBag.Employees = await _ctx.Employee.Include(e => e.Projects)
            .Where(e => e.Projects.Contains(task.Project)).ToListAsync();
        ViewBag.Status = TaskStatusConverter.StatusToString;
        return View(task);
    }

    [HttpPost("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ExecutorId,Status,Priority")] Models.Task model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }
        if (!TaskStatusConverter.StatusToString.ContainsKey(model.Status))
        {
            return BadRequest();
        }
        var task = await _ctx.Task.Include(e => e.Project).Include(e => e.Executor)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (task == null)
        {
            return NotFound();
        }
        if (task.ExecutorId != model.ExecutorId)
        {
            var executor = await _ctx.Employee.Include(e => e.Projects)
                .Where(e => e.Projects.Contains(task.Project))
                .FirstOrDefaultAsync(e => e.Id == model.ExecutorId);
            if (executor == null)
            {
                return BadRequest();
            }
            task.ExecutorId = model.ExecutorId;
            task.Executor = executor;
        }
        task.Name = model.Name;
        task.Description = model.Description;
        task.ExecutorId = model.ExecutorId;
        task.Status = model.Status;
        task.Priority = model.Priority;
        try
        {
            await _ctx.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TaskExists(task.Id))
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
        var task = await _ctx.Task.Include(e => e.Project).Include(e => e.Author)
            .Include(e => e.Executor).FirstOrDefaultAsync(e => e.Id == id);
        if (task == null)
        {
            return NotFound();
        }
        ViewBag.Status = TaskStatusConverter.StatusToString[task.Status];
        return View(task);
    }

    [HttpPost("Delete/{id:int}")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var task = await _ctx.Task.FindAsync(id);
        if (task != null)
        {
            _ctx.Task.Remove(task);
        }
        await _ctx.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool TaskExists(int id)
    {
        return (_ctx.Task?.Any(e => e.Id == id)).GetValueOrDefault();
    }

    private IOrderedEnumerable<Models.Task>? SortTasksBy(IEnumerable<Models.Task> tasks, string sortOrder)
    {
        ViewBag.NameSort = String.IsNullOrEmpty(sortOrder) ? "nameDesc" : "";
        ViewBag.DescriptionSort = sortOrder == "description" ? "descriptionDesc" : "description";
        ViewBag.ProjectSort = sortOrder == "project" ? "projectDesc" : "project";
        ViewBag.AuthorSort = sortOrder == "author" ? "authorDesc" : "author";
        ViewBag.ExecutorSort = sortOrder == "executor" ? "executorDesc" : "executor";
        ViewBag.StatusSort = sortOrder == "status" ? "statusDesc" : "status";
        ViewBag.PrioritySort = sortOrder == "priority" ? "priorityDesc" : "priority";
        return sortOrder switch
        {
            "nameDesc" => tasks.OrderByDescending(e => e.Name),
            "description" => tasks.OrderBy(e => e.Description),
            "descriptionDesc" => tasks.OrderByDescending(e => e.Description),
            "project" => tasks.OrderBy(e => e.Project.Name),
            "projectDesc" => tasks.OrderByDescending(e => e.Project.Name),
            "author" => tasks.OrderBy(e => e.Author.FirstName),
            "authorDesc" => tasks.OrderByDescending(e => e.Author.FirstName),
            "executor" => tasks.OrderBy(e => e.Executor.FirstName),
            "executorDesc" => tasks.OrderByDescending(e => e.Executor.FirstName),
            "status" => tasks.OrderBy(e => e.Status),
            "statusDesc" => tasks.OrderByDescending(e => e.Status),
            "priority" => tasks.OrderBy(e => e.Priority),
            "priorityDesc" => tasks.OrderByDescending(e => e.Priority),
            _ => tasks.OrderBy(e => e.Name),
        };
    }
}
