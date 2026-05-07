using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Web.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<WebIdeDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("WebIdeDb"),
        o => o.MigrationsAssembly("WebIde.DAL")));

builder.Services.AddScoped<ProblemRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<OrganizationRepository>();
builder.Services.AddScoped<ProblemSetRepository>();
builder.Services.AddScoped<SubmissionRepository>();
builder.Services.AddScoped<TagRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
