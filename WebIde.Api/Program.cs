using Microsoft.EntityFrameworkCore;
using WebIde.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.MigrationsAssembly("WebIde.Api")
    )
);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
    options.AddPolicy("ViteDev", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()));

builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors("ViteDev");
app.MapControllers();
app.Run();
