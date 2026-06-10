using Docker.DotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using WebIde.DAL;
using WebIde.Worker.Models;
using WebIde.Worker.Services;
using WebIde.Worker.Workers;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        var config = ctx.Configuration;

        // ── Database — factory pattern required for singleton hosted services ─────
        services.AddDbContextFactory<WebIdeDbContext>(opts =>
            opts.UseNpgsql(
                config.GetConnectionString("WebIdeDb"),
                o => o.MigrationsAssembly("WebIde.DAL")));

        // ── Redis ─────────────────────────────────────────────────────────────────
        var redisConn = config["Redis:ConnectionString"] ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConn));

        // ── Docker — DooD: connects to host daemon via the mounted socket ─────────
        services.AddSingleton<DockerClient>(_ =>
            new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"))
                .CreateClient());

        // ── Config ────────────────────────────────────────────────────────────────
        services.Configure<WorkerOptions>(config.GetSection("Worker"));
        services.Configure<SandboxOptions>(config.GetSection("Sandbox"));

        // ── Core services ─────────────────────────────────────────────────────────
        services.AddSingleton<SandboxOrchestrator>();
        services.AddScoped<SubmissionEvaluator>();

        // ── Hosted services ───────────────────────────────────────────────────────
        // HeartbeatService first — frontend health check depends on the key existing
        services.AddHostedService<HeartbeatService>();
        // Reaper runs StartAsync before SubmissionWorker starts processing
        services.AddHostedService<StuckSubmissionReaper>();
        services.AddHostedService<SubmissionWorker>();
    })
    .Build();

await host.RunAsync();
