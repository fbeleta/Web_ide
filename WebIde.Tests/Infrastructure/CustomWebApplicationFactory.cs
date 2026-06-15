using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;
using WebIde.DAL;

namespace WebIde.Tests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Provide dummy OAuth credentials — handlers throw at startup if ClientId is empty.
        builder.ConfigureAppConfiguration(config =>
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GitHub:ClientId"]     = "test-client-id",
                ["GitHub:ClientSecret"] = "test-client-secret",
                ["Google:ClientId"]     = "test-google-id",
                ["Google:ClientSecret"] = "test-google-secret",
            }));

        builder.ConfigureServices(services =>
        {
            // ── DbContext → InMemory ──────────────────────────────────────────────
            // Remove BOTH DbContextOptions and IDbContextOptionsConfiguration<> descriptors.
            // EF Core 9 registers the Npgsql provider through the latter; having two
            // providers in the same SP causes InvalidOperationException.
            var dbDescriptors = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<WebIdeDbContext>) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GetGenericArguments().FirstOrDefault() == typeof(WebIdeDbContext)))
                .ToList();
            foreach (var d in dbDescriptors) services.Remove(d);

            // Capture the name once so all scopes (seed + request) share the same database.
            var testDbName = "TestDb_" + Guid.NewGuid();
            services.AddDbContext<WebIdeDbContext>(options =>
                options.UseInMemoryDatabase(testDbName));

            // ── Redis → Mock ──────────────────────────────────────────────────────
            var redisDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IConnectionMultiplexer));
            if (redisDescriptor is not null) services.Remove(redisDescriptor);

            var redisMock = new Mock<IConnectionMultiplexer>();
            var dbMock    = new Mock<IDatabase>();
            dbMock.Setup(d => d.ListRightPushAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(),
                It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(1);
            dbMock.Setup(d => d.ListRange(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<long>(),
                It.IsAny<CommandFlags>()))
                .Returns(Array.Empty<RedisValue>());
            dbMock.Setup(d => d.ListRightPush(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(),
                It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .Returns(1);
            redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(dbMock.Object);

            // RedisSubscriptionService (a hosted BackgroundService) calls GetSubscriber()
            // on startup; without this it returns null and the host is torn down.
            var subscriberMock = new Mock<ISubscriber>();
            subscriberMock.Setup(s => s.SubscribeAsync(
                    It.IsAny<RedisChannel>(),
                    It.IsAny<Action<RedisChannel, RedisValue>>(),
                    It.IsAny<CommandFlags>()))
                .Returns(Task.CompletedTask);
            subscriberMock.Setup(s => s.UnsubscribeAsync(
                    It.IsAny<RedisChannel>(),
                    It.IsAny<Action<RedisChannel, RedisValue>>(),
                    It.IsAny<CommandFlags>()))
                .Returns(Task.CompletedTask);
            redisMock.Setup(r => r.GetSubscriber(It.IsAny<object>()))
                .Returns(subscriberMock.Object);

            services.AddSingleton<IConnectionMultiplexer>(redisMock.Object);

            // ── Authentication → TestAuthHandler ─────────────────────────────────
            // Register the test handler scheme, then forward ALL cookie-based schemes
            // to it. This avoids "scheme already exists" errors while ensuring that
            // [Authorize(AuthenticationSchemes = "Identity.Application")] succeeds.
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });

            // Forward authenticate on all cookie schemes → TestAuthHandler
            services.ConfigureAll<CookieAuthenticationOptions>(opts =>
                opts.ForwardAuthenticate = TestAuthHandler.SchemeName);

            // Override the default + challenge scheme so bare [Authorize] works too
            services.PostConfigure<AuthenticationOptions>(opts =>
            {
                opts.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                opts.DefaultChallengeScheme    = TestAuthHandler.SchemeName;
            });
        });
    }
}
