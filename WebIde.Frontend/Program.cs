
using Microsoft.EntityFrameworkCore;
using WebIde.DAL;
using WebIde.Web.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// EF Core with PostgreSQL
builder.Services.AddDbContext<WebIdeDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("WebIdeDbContext"),
        opt => opt.MigrationsAssembly("WebIde.DAL")));

// EF-backed repositories — scoped to match DbContext lifetime
builder.Services.AddScoped<ProblemRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<OrganizationRepository>();
builder.Services.AddScoped<ProblemSetRepository>();
builder.Services.AddScoped<SubmissionRepository>();
builder.Services.AddScoped<TagRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
