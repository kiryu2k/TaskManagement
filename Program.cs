using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TaskManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TaskManagementContext") ?? throw new InvalidOperationException("Connection string 'TaskManagementContext' not found.")));
builder.Services.AddControllersWithViews();
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();