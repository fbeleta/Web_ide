# Lab 5 Handoff — WebIde

**Deadline:** June 12, 2026  
**Branch:** develop  
**Status:** Implementation complete; run `dotnet test` and `dotnet ef database update` before demo.

---

## What Lab 5 adds

| Feature | Points | Files |
|---|---|---|
| REST API (CRUD + DTOs) for all entities | 2 | `Controllers/Api/`, `DTOs/` |
| ASP.NET Core Identity (local accounts, Admin/Manager roles, OIB/JMBG fields) | 1 | `Areas/Identity/`, `DAL/AppUser.cs`, `Program.cs` |
| Dropzone.js file upload tied to Problems | 1 | `Controllers/ProblemController.cs`, `Views/Problem/Edit.cshtml` |
| Google 3rd-party OAuth via Identity's external login | 1 | `Program.cs` (Google handler), `Areas/Identity/Pages/Account/ExternalLogin` |
| Integration tests for all API controllers | 2 | `WebIde.Tests/` |

---

## Dual-auth architecture

The project runs **two independent authentication systems** side-by-side:

| System | Cookie scheme | Login path | Used by |
|---|---|---|---|
| GitHub OAuth (existing) | `Cookies` (`CookieAuthenticationDefaults`) | `/auth/login` | MVC views, legacy users |
| ASP.NET Core Identity | `Identity.Application` | `/Identity/Account/Login` | API controllers, admin panel, Google OAuth |

**Why `AddIdentityCore` not `AddDefaultIdentity`:**  
`AddDefaultIdentity` calls `AddAuthentication(IdentityConstants.ApplicationScheme)` internally, which would override the existing `DefaultScheme = "Cookies"` and break GitHub OAuth. `AddIdentityCore` only registers Identity services without touching auth schemes.

**Three Identity cookies required:**  
`SignInManager<AppUser>` expects three cookie schemes to exist at startup:
- `Identity.Application` — the main session cookie
- `Identity.External` — temp cookie used during OAuth handshake
- `Identity.TwoFactorUserId` — required even if 2FA is disabled

All three are registered in `Program.cs` via `.AddCookie(IdentityConstants.{scheme})`.

---

## AppUser vs custom User

| | `AppUser` | `User` |
|---|---|---|
| Namespace | `WebIde.DAL` | `WebIde.Model` |
| Base class | `IdentityUser` | Plain POCO |
| Table | `AspNetUsers` | `Users` |
| Purpose | Identity auth (login, roles, Google) | Domain concept (profile, submissions, points) |
| Extra fields | `OIB` (11 digits), `JMBG` (13 digits) | `GitHubId`, `AvatarUrl`, `Score`, etc. |
| DbSet on WebIdeDbContext | `Users<AppUser>` (inherited from IdentityDbContext) | `DomainUsers` (renamed to avoid CS0114 collision) |

The two models are **not linked by FK**. A GitHub-only user has a `User` row but no `AppUser`. An Identity/Google user has an `AppUser` row but may not have a `User` row. This is intentional — linking them is Phase 2+ work.

---

## API controllers

Base route pattern: `GET /api/{entity}`, `GET /api/{entity}/{id}`, `POST /api/{entity}`, `PUT /api/{entity}/{id}`, `DELETE /api/{entity}/{id}`.

All write endpoints require `[Authorize(AuthenticationSchemes = "Identity.Application", Roles = "...")]`. The scheme must be specified as a string literal — `IdentityConstants.ApplicationScheme` is a property, not a `const`, and cannot be used in attribute arguments (CS0182). Use `ApiAuthSchemes.Identity` constant defined in `Controllers/Api/ApiAuthSchemes.cs`.

---

## Roles

Two roles are seeded at startup: **Admin** and **Manager**.

| Role | Capabilities |
|---|---|
| Admin | Full CRUD on all entities, delete any submission, view all execution results |
| Manager | Create/edit problems, problem sets, organizations, tags. No delete. |

### Assigning Admin to first user (production)

After deploying and registering an account via `/Identity/Account/Register`:

```bash
# Exec into the app container
docker compose exec webide-app bash

# Option A: psql directly
psql "Host=postgres;Database=webide;Username=webide;Password=..."
INSERT INTO "AspNetUserRoles" ("UserId","RoleId")
SELECT u."Id", r."Id" FROM "AspNetUsers" u, "AspNetRoles" r
WHERE u."Email" = 'your@email.com' AND r."Name" = 'Admin';
```

Or add `AdminSeedEmail` to `appsettings.json` and extend the role-seeding block in `Program.cs` to auto-assign Admin to that email on startup (idempotent).

---

## Google OAuth setup

1. Go to [console.cloud.google.com](https://console.cloud.google.com) → APIs & Services → Credentials
2. Create OAuth 2.0 Client ID (Web application)
3. Authorized redirect URI: `https://{PUBLIC_HOSTNAME}/signin-google`
4. Copy Client ID and Secret into `.env`:
   ```
   GOOGLE_CLIENT_ID=...
   GOOGLE_CLIENT_SECRET=...
   ```
5. Restart the app container: `docker compose up -d webide-app`

For local dev, use `dotnet user-secrets`:
```bash
dotnet user-secrets set "Google:ClientId" "..."   --project WebIde.Frontend
dotnet user-secrets set "Google:ClientSecret" "..." --project WebIde.Frontend
```

On first Google login, the user is prompted to enter OIB and JMBG before their `AppUser` record is created.

---

## File uploads (Dropzone)

Attachments are stored at `wwwroot/uploads/problems/{problemId}/{guid}{ext}`.

**Critical for deployment:** this path is mounted as a Docker named volume `uploads-data` in `docker-compose.yml`. Without this mount, files are lost every time the container restarts.

Allowed file types: `.pdf .png .jpg .jpeg .zip .txt`  
Max size: 10 MB

The upload endpoint is `POST /problems/{id}/attachments`. Dropzone sends a `RequestVerificationToken` header for CSRF validation. Without it, the endpoint returns 400.

---

## Integration tests

```bash
dotnet test WebIde.Tests
```

### How it works

`CustomWebApplicationFactory`:
- Replaces the real PostgreSQL connection with an in-memory EF database (fresh `Guid`-named DB per test run — no cross-test contamination)
- Replaces `IConnectionMultiplexer` (Redis) with a `Moq` mock — prevents startup crash when Redis is not running
- Registers `TestAuthHandler` for all auth schemes so `[Authorize]` is always satisfied (returns Admin + Manager roles)

`TestAuthHandler`: returns a hardcoded `ClaimsPrincipal` with `ClaimTypes.Name = "testuser"`, role claims `Admin` and `Manager`. No real credentials are checked.

Each test class seeds data directly via `factory.Services.CreateScope()` → `WebIdeDbContext` rather than going through the HTTP API, which keeps tests fast and deterministic.

---

## Migration

The migration `AddIdentityAndAttachments` (in `WebIde.DAL/Migrations/`) creates:
- All 7 `AspNet*` Identity tables (with `OIB` and `JMBG` columns on `AspNetUsers`)
- `Attachments` table (FK to `Problems`)

Run on deploy:
```bash
dotnet ef database update --project WebIde.DAL --startup-project WebIde.Frontend
```

This is already part of the Phase 0 deploy flow in `docs/deployment-handoff.md` — no changes needed.

---

## Breaking changes

None. All existing behavior is preserved:
- GitHub OAuth login still works (`/auth/login` → `/auth/github/callback`)
- Existing `User`, `Problem`, `Submission`, `Organization`, `ProblemSet`, `Tag`, `TestCase` tables are unchanged
- `_db.Users` renamed to `_db.DomainUsers` in `UserRepository.cs` — DB table name `"Users"` is unchanged
