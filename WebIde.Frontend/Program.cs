using WebIde.Web.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Mock repositories — singleton (static in-memory data, no real DB for Lab 2)
builder.Services.AddSingleton<ProblemRepository>();
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<OrganizationRepository>();
builder.Services.AddSingleton<ProblemSetRepository>();
builder.Services.AddSingleton<SubmissionRepository>();
builder.Services.AddSingleton<TagRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
