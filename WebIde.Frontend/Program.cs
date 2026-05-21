using System.Globalization;
using Microsoft.AspNetCore.Localization;
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
builder.Services.AddScoped<TestCaseRepository>();
builder.Services.AddScoped<ExecutionResultRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

var supportedCultures = new[] { new CultureInfo("hr"), new CultureInfo("en-US") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("hr"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
