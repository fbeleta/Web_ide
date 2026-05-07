# EF Core Skill

Use this guide when adding a new entity, modifying the model, or running migrations in the WebIde project.

## Project layout

| Project | Purpose |
|---|---|
| `WebIde.Model` | Entity classes with EF annotations |
| `WebIde.DAL` | `WebIdeDbContext`, migrations |
| `WebIde.Frontend` | Repositories using `WebIdeDbContext` via DI |

---

## Adding a new entity

### 1. Create the model class in `WebIde.Model/`

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebIde.Model;

public class MyEntity
{
    [Key]
    public int Id { get; set; }

    public required string Name { get; set; }

    // 1-N owned side — add FK int + [ForeignKey] + virtual nav
    public int ParentId { get; set; }
    [ForeignKey("ParentId")]
    public virtual ParentEntity Parent { get; set; } = null!;

    // 1-N owning side — virtual ICollection
    public virtual ICollection<ChildEntity> Children { get; set; } = new List<ChildEntity>();
}
```

**Rules:**
- `[Key]` on every `Id`
- Collection nav properties → `virtual ICollection<T>`, never `List<T>`
- FK nav properties → `virtual T Xxx { get; set; } = null!;` with matching `int XxxId` + `[ForeignKey]`
- For N-N: add `virtual ICollection<T>` on **both** sides; configure join table name in `OnModelCreating`

### 2. Add `DbSet<T>` to `WebIdeDbContext`

```csharp
// WebIde.DAL/WebIdeDbContext.cs
public DbSet<MyEntity> MyEntities { get; set; }
```

If N-N, add `.UsingEntity(j => j.ToTable("MyJoinTable"))` in `OnModelCreating`.

### 3. Add a repository in `WebIde.Frontend/Repositories/`

```csharp
public class MyEntityRepository
{
    private readonly WebIdeDbContext _db;
    public MyEntityRepository(WebIdeDbContext db) => _db = db;

    public List<MyEntity> GetAll() =>
        _db.MyEntities.Include(e => e.Parent).OrderBy(e => e.Id).ToList();

    public MyEntity? GetById(int id) =>
        _db.MyEntities.Include(e => e.Parent).FirstOrDefault(e => e.Id == id);
}
```

### 4. Register the repository in `Program.cs`

```csharp
builder.Services.AddScoped<MyEntityRepository>();
```

---

## Running migrations

Always run from the `WebIde.DAL` directory with `--startup-project` pointing to `WebIde.Frontend` (which holds the connection string).

```bash
# Generate migration
cd WebIde.DAL
dotnet ef migrations add <DescriptiveName> --startup-project ../WebIde.Frontend --context WebIdeDbContext

# Apply to database
dotnet ef database update --startup-project ../WebIde.Frontend --context WebIdeDbContext

# Undo last migration (before applying to DB)
dotnet ef migrations remove --startup-project ../WebIde.Frontend --context WebIdeDbContext
```

Connection string is in `WebIde.Frontend/appsettings.json` under `ConnectionStrings:WebIdeDb`.

---

## Querying with Include

Always `.Include()` navigation properties you plan to access — EF won't auto-load them without lazy loading proxies enabled.

```csharp
// Good — explicit eager loading
_db.Submissions
    .Include(s => s.User)
    .Include(s => s.Problem).ThenInclude(p => p.Tags)
    .ToList();

// Bad — User will be null at runtime
_db.Submissions.ToList();
```
