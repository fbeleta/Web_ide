# Lab 4 — Implementation Session Log

## Overview

Full CRUD implementation for all entities in the WebIde competitive coding platform (ASP.NET MVC, EF Core 9, PostgreSQL, Tailwind CSS brutalist design).

---

## What Was Implemented

### 1. Soft Delete — All Entities

Added `public DateTime? DeletedAt { get; set; }` to:
- `WebIde.Model/Problem.cs`
- `WebIde.Model/User.cs`
- `WebIde.Model/Tag.cs`
- `WebIde.Model/Organization.cs`
- `WebIde.Model/ProblemSet.cs`
- `WebIde.Model/Submission.cs`
- `WebIde.Model/TestCase.cs`

EF Core migration created: `AddSoftDelete`

```bash
# Apply migration (requires correct DB password in appsettings.json):
cd WebIde.Frontend
dotnet tool run dotnet-ef database update --project ../WebIde.DAL --startup-project .
```

Note: A local tool manifest (`dotnet-tools.json`) was created at the repo root to pin `dotnet-ef` to v9 (resolves tool/package version mismatch with .NET 10 SDK + EF Core 9 packages).

---

### 2. ViewModels — `WebIde.Frontend/Models/`

One file per entity with `XCreateModel` and `XEditModel` classes, all with data annotations:

| File | Models |
|---|---|
| `TagModels.cs` | TagCreateModel, TagEditModel |
| `OrganizationModels.cs` | OrganizationCreateModel, OrganizationEditModel |
| `UserModels.cs` | UserCreateModel, UserEditModel |
| `ProblemModels.cs` | ProblemCreateModel, ProblemEditModel |
| `ProblemSetModels.cs` | ProblemSetCreateModel, ProblemSetEditModel |
| `SubmissionModels.cs` | SubmissionCreateModel, SubmissionEditModel |
| `TestCaseModels.cs` | TestCaseCreateModel, TestCaseEditModel |
| `ExecutionResultModels.cs` | ExecutionResultCreateModel |

Validation annotations used: `[Required]`, `[StringLength(max, MinimumLength=min)]`, `[Range]`, `[EmailAddress]`, `[RegularExpression]`

---

### 3. Repositories — Write Methods + Soft Delete Filters

All existing repositories updated:
- `GetAll()` and `GetById()` now filter `WHERE DeletedAt IS NULL`
- Added: `Add(entity)`, `Update()`, `SoftDelete(id)`, `Search(q)`

Two new repositories created:
- `WebIde.Frontend/Repositories/TestCaseRepository.cs` — scoped to parent Problem
- `WebIde.Frontend/Repositories/ExecutionResultRepository.cs` — GetById, GetBySubmissionId, Add

Both registered as `AddScoped` in `Program.cs`.

---

### 4. Localization + Flatpickr Setup

**`Program.cs`** — added `UseRequestLocalization` middleware:
```csharp
var supportedCultures = new[] { new CultureInfo("hr"), new CultureInfo("en-US") };
app.UseRequestLocalization(new RequestLocalizationOptions {
    DefaultRequestCulture = new RequestCulture("hr"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});
```

**`_Layout.cshtml`** — added CDN links:
- Flatpickr CSS (brutalist style overrides: no border-radius, black borders)
- jQuery 3.7.1
- jquery-validation + jquery-validation-unobtrusive
- Flatpickr JS + Croatian locale (`l10n/hr.js`)
- `wwwroot/js/site.js`

---

### 5. Shared Partial Views

#### `Views/Shared/_DatePicker.cshtml`
Reusable Flatpickr datepicker partial. Parameters via `ViewData`:
- `FieldName` — model property name (used for hidden input name/id)
- `Label` — display label text
- `Value` — current `DateTime?` value
- `Required` — whether field is required

Renders: text input (locale-formatted display) + hidden input (ISO UTC value for model binding). No native browser datepicker — uses Flatpickr with `enableTime: true`.

#### `Views/Shared/_AutocompleteDropdown.cshtml`
Reusable AJAX autocomplete partial. Parameters via `ViewData`:
- `FieldName` — hidden input name
- `Label` — display label
- `SearchUrl` — JSON endpoint (`/entity/search?q=`)
- `InitialText` — pre-filled display text (for Edit forms)
- `InitialIds` — pre-filled IDs (comma-separated for multi)
- `IsMulti` — `true` for tag chips (multi-select), `false` for single FK
- `Required` — validation attribute

Multi-select: renders removable chips, multiple hidden inputs with same name.
Single-select: one hidden input + text display.
Dropdown animates: `scaleY(0) → scaleY(1)` with `transform-origin: top`.
Debounced 280ms on keyup.

#### `Views/Shared/_DeleteModal.cshtml`
Animated confirmation modal (no native `confirm()`).
- `openDeleteModal(formAction, entityName)` — shows modal with entity name
- `closeDeleteModal()` — hides modal
- Backdrop fades in, box slides from `translateY(-40px)` to `0`
- DELETE button flashes red before form submit

---

### 6. CRUD Controllers

All controllers updated with full CRUD:

| Controller | Route Prefix | Special Notes |
|---|---|---|
| TagController | `/tag` | Simple name-only form |
| OrganizationController | `/organizations` | Name + description |
| UserController | `/users` | UserRole enum, RegisteredAt datepicker |
| ProblemController | `/problems` | Tag autocomplete (multi), CreatedAt datepicker |
| ProblemSetController | `/problemsets` | Organization autocomplete (single) |
| SubmissionController | `/submissions` | User + Problem autocompletes, SubmittedAt datepicker |
| TestCaseController | `/problems/{problemId}/testcases` | Scoped to parent problem |
| ExecutionResultController | `/submissions/{submissionId}/results` | Create + Read only (no Edit/Delete) |

Each controller has:
- `GET Create` + `POST Create` (with `ModelState.IsValid` check)
- `GET Edit` (ActionName="Edit") + `POST Edit` (ActionName="Edit")
- `POST Delete` (soft deletes, sets `DeletedAt = DateTime.UtcNow`)
- `GET Search` (returns JSON `[{id, label}]` for AJAX)
- `TempData["Flash"]` set on successful Create/Edit/Delete

---

### 7. Views — Create + Edit

Created for all entities:

| Entity | Create | Edit |
|---|---|---|
| Tag | `Tag/Create.cshtml` | `Tag/Edit.cshtml` |
| Organization | `Organization/Create.cshtml` | `Organization/Edit.cshtml` |
| User | `User/Create.cshtml` | `User/Edit.cshtml` |
| Problem | `Problem/Create.cshtml` | `Problem/Edit.cshtml` |
| ProblemSet | `ProblemSet/Create.cshtml` | `ProblemSet/Edit.cshtml` |
| Submission | `Submission/Create.cshtml` | `Submission/Edit.cshtml` |
| TestCase | `TestCase/Create.cshtml` | `TestCase/Edit.cshtml` |
| ExecutionResult | `ExecutionResult/Create.cshtml` | — (no edit) |

Autocomplete usage:
- Problem Create/Edit: `_AutocompleteDropdown` with `IsMulti=true` for Tags
- ProblemSet Create/Edit: `_AutocompleteDropdown` with `IsMulti=false` for Organization
- Submission Create/Edit: two `_AutocompleteDropdown` partials for User and Problem

Datepicker usage: Problem (`CreatedAt`), User (`RegisteredAt`), ProblemSet (`CreatedAt`), Submission (`SubmittedAt`)

---

### 8. Index Views — CRUD Buttons + AJAX Search

All Index views updated with:
- `@await Html.PartialAsync("_DeleteModal")` at top
- Flash message display block (`TempData["Flash"]`)
- Create button next to page heading
- AJAX search `<input>` wired to `initAjaxSearch()`
- `id="X-tbody"` on `<tbody>` for JS targeting
- Edit + Delete buttons per row
- `@section Scripts` with `initAjaxSearch(inputId, tbodyId, endpoint, rowRenderer)`

---

### 9. JavaScript — `wwwroot/js/site.js`

Four features added:

**Blur validation** — triggers jQuery Unobtrusive Validation on field blur (not only on submit):
```js
$(document).on('blur', 'input[data-val], select[data-val], textarea[data-val]', function() {
    var form = $(this).closest('form');
    if (form.data('validator')) form.validate().element(this);
});
```

**Page-load row stagger** — all `tbody tr` fade in with 35ms stagger on DOM ready.

**`initAjaxSearch(inputId, tbodyId, endpoint, rowRenderer)`** — shared AJAX search helper:
- Debounced 300ms keyup
- Shows 3-row skeleton loader while fetching
- Replaces tbody with rendered rows
- Stagger-animates new rows in

**Form focus animation** — `.brutalist-input` border widens from 2px to 3px on focus. Flash notification auto-dismisses after 3500ms.

---

## Known Issue: DB Migration Pending

The EF Core migration `AddSoftDelete` was generated but `database update` failed with `28P01: password authentication failed for user "postgres"`.

**Fix:** Update `WebIde.Frontend/appsettings.json` connection string with the correct password, then run:

```bash
cd WebIde.Frontend
dotnet tool run dotnet-ef database update --project ../WebIde.DAL --startup-project .
```

The app will not start correctly until the migration is applied (new `DeletedAt` columns are missing from DB tables).

---

## File Summary

### New Files
```
WebIde.Frontend/Models/TagModels.cs
WebIde.Frontend/Models/OrganizationModels.cs
WebIde.Frontend/Models/UserModels.cs
WebIde.Frontend/Models/ProblemModels.cs
WebIde.Frontend/Models/ProblemSetModels.cs
WebIde.Frontend/Models/SubmissionModels.cs
WebIde.Frontend/Models/TestCaseModels.cs
WebIde.Frontend/Models/ExecutionResultModels.cs
WebIde.Frontend/Repositories/TestCaseRepository.cs
WebIde.Frontend/Repositories/ExecutionResultRepository.cs
WebIde.Frontend/Controllers/TestCaseController.cs
WebIde.Frontend/Controllers/ExecutionResultController.cs
WebIde.Frontend/Views/Shared/_DatePicker.cshtml
WebIde.Frontend/Views/Shared/_AutocompleteDropdown.cshtml
WebIde.Frontend/Views/Shared/_DeleteModal.cshtml
WebIde.Frontend/Views/Tag/Create.cshtml
WebIde.Frontend/Views/Tag/Edit.cshtml
WebIde.Frontend/Views/Organization/Create.cshtml
WebIde.Frontend/Views/Organization/Edit.cshtml
WebIde.Frontend/Views/User/Create.cshtml
WebIde.Frontend/Views/User/Edit.cshtml
WebIde.Frontend/Views/Problem/Create.cshtml
WebIde.Frontend/Views/Problem/Edit.cshtml
WebIde.Frontend/Views/ProblemSet/Create.cshtml
WebIde.Frontend/Views/ProblemSet/Edit.cshtml
WebIde.Frontend/Views/Submission/Create.cshtml
WebIde.Frontend/Views/Submission/Edit.cshtml
WebIde.Frontend/Views/TestCase/Index.cshtml
WebIde.Frontend/Views/TestCase/Create.cshtml
WebIde.Frontend/Views/TestCase/Edit.cshtml
WebIde.Frontend/Views/ExecutionResult/Create.cshtml
WebIde.Frontend/Views/ExecutionResult/Details.cshtml
dotnet-tools.json (repo root — pins dotnet-ef to v9)
```

### Modified Files
```
WebIde.Model/Problem.cs
WebIde.Model/User.cs
WebIde.Model/Tag.cs
WebIde.Model/Organization.cs
WebIde.Model/ProblemSet.cs
WebIde.Model/Submission.cs
WebIde.Model/TestCase.cs
WebIde.Frontend/Program.cs
WebIde.Frontend/Views/Shared/_Layout.cshtml
WebIde.Frontend/Repositories/TagRepository.cs
WebIde.Frontend/Repositories/OrganizationRepository.cs
WebIde.Frontend/Repositories/UserRepository.cs
WebIde.Frontend/Repositories/ProblemRepository.cs
WebIde.Frontend/Repositories/ProblemSetRepository.cs
WebIde.Frontend/Repositories/SubmissionRepository.cs
WebIde.Frontend/Controllers/TagController.cs
WebIde.Frontend/Controllers/OrganizationController.cs
WebIde.Frontend/Controllers/UserController.cs
WebIde.Frontend/Controllers/ProblemController.cs
WebIde.Frontend/Controllers/ProblemSetController.cs
WebIde.Frontend/Controllers/SubmissionController.cs
WebIde.Frontend/Views/Tag/Index.cshtml
WebIde.Frontend/Views/Organization/Index.cshtml
WebIde.Frontend/Views/User/Index.cshtml
WebIde.Frontend/Views/Problem/Index.cshtml
WebIde.Frontend/Views/ProblemSet/Index.cshtml
WebIde.Frontend/Views/Submission/Index.cshtml
WebIde.Frontend/wwwroot/js/site.js
WebIde.DAL/Migrations/ (AddSoftDelete migration added)
```
