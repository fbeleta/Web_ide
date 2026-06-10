---
name: webide-lab5
description: >
  Non-obvious implementation decisions from Lab 5 — ASP.NET Core Identity
  alongside existing GitHub OAuth, API auth, Dropzone, Google OAuth, and
  integration test wiring for the WebIde project.
triggers:
  - "identity"
  - "addidentity"
  - "oauth"
  - "google"
  - "dropzone"
  - "api controller"
  - "integration test"
  - "webapplicationfactory"
---

# WebIde Lab 5 — Key decisions

Read `docs/lab5-handoff.md` for full context. This skill encodes the 8 non-obvious
decisions that are easy to get wrong.

---

## 1. Use `AddIdentityCore`, NOT `AddDefaultIdentity`

`AddDefaultIdentity` internally calls `AddAuthentication(IdentityConstants.ApplicationScheme)`.
This overrides the existing `DefaultScheme = "Cookies"` (used by GitHub OAuth) and breaks GitHub login.

`AddIdentityCore` registers Identity services only, without touching auth schemes.

```csharp
// WRONG — breaks GitHub OAuth
builder.Services.AddDefaultIdentity<AppUser>()...

// CORRECT
builder.Services.AddIdentityCore<AppUser>(opts => { ... })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<WebIdeDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();
```

Then register the three Identity cookies **manually** in the existing `.AddAuthentication(...)` chain.

---

## 2. Three Identity cookie schemes are required

`SignInManager<AppUser>` expects all three to exist at startup, or it throws:

```csharp
.AddCookie(IdentityConstants.ApplicationScheme, opts => { opts.LoginPath = "/Identity/Account/Login"; ... })
.AddCookie(IdentityConstants.ExternalScheme)          // temp cookie for OAuth handshake
.AddCookie(IdentityConstants.TwoFactorUserIdScheme)   // required even if 2FA is disabled
```

Register these in `Program.cs` chained after `.AddGitHub(...)` in the existing
`.AddAuthentication(...).AddCookie(...).AddGitHub(...)` block.

---

## 3. API `[Authorize]` must specify the Identity scheme

Without `AuthenticationSchemes`, the GitHub cookie (`"Cookies"`) can satisfy `[Authorize]`
and allow GitHub-only users (no `AppUser`) to call admin write endpoints.

```csharp
// WRONG — GitHub cookie can satisfy this
[Authorize(Roles = "Admin")]

// CORRECT — only Identity.Application cookie accepted
[Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity, Roles = "Admin")]
```

`ApiAuthSchemes.Identity = "Identity.Application"` is a `const string` in
`Controllers/Api/ApiAuthSchemes.cs`.

---

## 4. `IdentityConstants.ApplicationScheme` cannot be used in `[Authorize]`

It is a **property**, not a `const`. C# attribute arguments must be compile-time constants (CS0182).

Solution: create `ApiAuthSchemes.cs` with:
```csharp
internal static class ApiAuthSchemes {
    public const string Identity = "Identity.Application";
}
```

---

## 5. `AppUser` must live in `WebIde.DAL`, not `WebIde.Model`

`IdentityUser` is in `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, which depends on EF Core.
`WebIde.Model` is a pure POCO project — adding EF there would be a layering violation.

---

## 6. Dropzone upload needs AntiForgery header

Without the token, the multipart POST to `/problems/{id}/attachments` returns 400 (CSRF rejected).

```js
new Dropzone("#attachment-dz", {
    url: "/problems/@Model.Id/attachments",
    headers: {
        "RequestVerificationToken": document.querySelector('input[name="__RequestVerificationToken"]').value
    },
    ...
});
```

The page must have `@Html.AntiForgeryToken()` (or a form tag helper) somewhere to emit the token.

---

## 7. `public partial class Program {}` is required for `WebApplicationFactory<Program>`

`WebApplicationFactory<Program>` needs to access the `Program` class. Because `Program.cs` uses
top-level statements, the class is implicitly `internal`. Add at the very end of `Program.cs`:

```csharp
public partial class Program { }
```

---

## 8. Test factory must mock `IConnectionMultiplexer`

`ConnectionMultiplexer.Connect()` runs at `app.Build()` time in `Program.cs`.
In the test environment there is no Redis server, so the call throws.

In `CustomWebApplicationFactory.ConfigureWebHost`:
```csharp
// Remove the real Redis registration
var redis = services.SingleOrDefault(d => d.ServiceType == typeof(IConnectionMultiplexer));
if (redis != null) services.Remove(redis);

// Replace with a Moq mock
var mockMux = new Mock<IConnectionMultiplexer>();
services.AddSingleton(mockMux.Object);
```

Also replace the DbContext with an InMemory database using a unique name per factory instance
so tests don't share state:
```csharp
services.AddDbContext<WebIdeDbContext>(o => o.UseInMemoryDatabase("Test_" + Guid.NewGuid()));
```

---

## 9. `DomainUsers` vs `Users` DbSet

`IdentityDbContext<AppUser>` inherits a `DbSet<AppUser> Users` property.
The existing domain `DbSet<User> Users` in `WebIdeDbContext` would shadow it (CS0114).

Fix: rename the domain DbSet to `DomainUsers`. The DB table name stays `"Users"` because
EF derives table names from the entity CLR type name, not the DbSet property name.

Update every `_db.Users` → `_db.DomainUsers` in `UserRepository.cs` (5 occurrences).

---

## 10. Design-time factory prevents migration failures

`dotnet ef migrations add` invokes app startup, which calls `ConnectionMultiplexer.Connect()`
and crashes when Redis is unavailable (e.g. on developer machines without Docker).

Fix: add `WebIdeDbContextFactory : IDesignTimeDbContextFactory<WebIdeDbContext>` in `WebIde.DAL`.
EF tools use this instead of starting the full app:

```csharp
public class WebIdeDbContextFactory : IDesignTimeDbContextFactory<WebIdeDbContext> {
    public WebIdeDbContext CreateDbContext(string[] args) {
        var options = new DbContextOptionsBuilder<WebIdeDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=webide;Username=postgres;Password=postgres",
                o => o.MigrationsAssembly("WebIde.DAL"))
            .Options;
        return new WebIdeDbContext(options);
    }
}
```

---

## 11. Attachment files need a Docker volume

Files are written to `wwwroot/uploads/problems/{id}/{guid}{ext}` inside the container.
Without a named volume mount, they are lost on every `docker compose up`.

`docker-compose.yml`:
```yaml
services:
  webide-app:
    volumes:
      - uploads-data:/app/wwwroot/uploads
volumes:
  uploads-data:
```
