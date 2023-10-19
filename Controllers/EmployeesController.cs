using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;

namespace TaskManagement.Controllers;

[Route("/Employees")]
public partial class EmployeesController : Controller
{
    private readonly TaskManagementContext _ctx;

    public EmployeesController(TaskManagementContext context)
    {
        _ctx = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(await _ctx.Employee.ToListAsync());
    }

    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var employee = await _ctx.Employee.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }
        return View(employee);
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,MiddleName,Email")] Models.Employee employee)
    {
        if (!IsValid(employee) || await IsEmailUsed(employee.Email))
        {
            return View(employee);
        }
        _ctx.Employee.Add(employee);
        await _ctx.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _ctx.Employee.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }
        return View(employee);
    }

    [HttpPost("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,MiddleName,Email")] Models.Employee model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }
        if (!IsValid(model))
        {
            return View(model);
        }
        var employee = await _ctx.Employee.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }
        if (employee.Email != model.Email && await IsEmailUsed(model.Email))
        {
            return View(model);
        }
        employee.FirstName = model.FirstName;
        employee.LastName = model.LastName;
        employee.MiddleName = model.MiddleName;
        employee.Email = model.Email;
        try
        {
            await _ctx.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EmployeeExists(employee.Id))
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
        var employee = await _ctx.Employee.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }
        return View(employee);
    }

    [HttpPost("Delete/{id:int}")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var employee = await _ctx.Employee.FindAsync(id);
        if (employee != null)
        {
            _ctx.Employee.Remove(employee);
        }
        await _ctx.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool EmployeeExists(int id)
    {
        return (_ctx.Employee?.Any(e => e.Id == id)).GetValueOrDefault();
    }

    private async Task<bool> IsEmailUsed(string email)
    {
        var result = await _ctx.Employee.Where(e => e.Email == email).ToListAsync();
        return !(result == null || result.Count == 0);
    }

    private bool IsValid(Models.Employee employee)
    {
        var isValid = IsEmailValid(employee.Email) && IsNameValid(employee.FirstName) && IsNameValid(employee.LastName);
        if (employee.MiddleName == null || employee.MiddleName.Length == 0)
        {
            return isValid;
        }
        return isValid && IsNameValid(employee.MiddleName);
    }

    private bool IsNameValid(string name)
    {
        var regex = NameRegex();
        return regex.IsMatch(name);
    }

    private bool IsEmailValid(string email)
    {
        var regex = EmailRegex();
        return regex.IsMatch(email);
    }

    [GeneratedRegex("^([\\w\\.\\-]+)@([\\w\\-]+)((\\.(\\w){2,3})+)$")]
    private static partial Regex EmailRegex();
    [GeneratedRegex("^[A-Z][a-z]+$")]
    private static partial Regex NameRegex();
}
