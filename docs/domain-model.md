# Domain Model

All classes live in `WebIde.Model`. The model references `Microsoft.EntityFrameworkCore` for EF annotations and is used by both `WebIde.DAL` (DbContext) and `WebIde.Frontend` (repositories).

## EF Core Annotations

The model is annotated for Entity Framework Core:

| Pattern | Usage |
|---|---|
| `[Key]` | On every `Id` property — marks it as the primary key |
| `virtual ICollection<T>` | On 1-N and N-N collection navigation properties — enables EF lazy loading proxy |
| `int XxxId` + `[ForeignKey("XxxId")]` | On 1-N owned side (Submission, TestCase, ProblemSet) — explicit FK column + navigation |
| `= null!` | On required navigation properties to suppress nullable warnings while letting EF set the value |

N-N relationships (Problem↔Tag, Problem↔ProblemSet, User↔Organization) are configured with explicit join table names in `WebIdeDbContext.OnModelCreating`.

## Enums

### `DifficultyLevel`
Classifies problem difficulty.
```
Easy | Medium | Hard
```

### `SubmissionStatus`
Tracks the lifecycle of a code submission.
```
Pending → Running → Accepted
                  → WrongAnswer
                  → TimeLimitExceeded
                  → MemoryLimitExceeded
                  → CompileError
```

### `UserRole`
Controls what a user can do on the platform.
```
Admin | Instructor | Student
```

---

## Classes

### `User`
A registered platform user.

| Property | Type | Notes |
|---|---|---|
| Id | int | |
| Username | string | required |
| Email | string | required |
| DisplayName | string | required |
| Role | UserRole | |
| RegisteredAt | DateTime | |
| Submissions | List\<Submission\> | 1-N: user has many submissions |
| Organizations | List\<Organization\> | N-N: user belongs to many orgs |

---

### `Organization`
A group of users — represents a course, team, or institution.

| Property | Type | Notes |
|---|---|---|
| Id | int | |
| Name | string | required |
| Description | string | required |
| Members | List\<User\> | N-N with User |
| ProblemSets | List\<ProblemSet\> | 1-N: org owns many problem sets |

---

### `Problem`
A coding problem with a description and constraints.

| Property | Type | Notes |
|---|---|---|
| Id | int | |
| Title | string | required |
| Description | string | required |
| Difficulty | DifficultyLevel | |
| TimeLimitMs | int | execution time cap per submission |
| MemoryLimitKb | int | memory cap per submission |
| CreatedAt | DateTime | |
| AuthorUsername | string | required |
| TestCases | List\<TestCase\> | 1-N: problem has many test cases |
| Tags | List\<Tag\> | N-N: problem has many tags |
| Submissions | List\<Submission\> | 1-N: problem has many submissions |

---

### `Tag`
A label for categorizing problems (e.g. "Arrays", "Binary Search").

| Property | Type | Notes |
|---|---|---|
| Id | int | |
| Name | string | required |
| Problems | List\<Problem\> | N-N with Problem |

---

### `TestCase`
A single input/output pair used to judge a submission.

| Property | Type | Notes |
|---|---|---|
| Id | int | |
| InputArgs | string | required — serialized input (e.g. JSON array) |
| ExpectedOutput | string | required |
| IsSample | bool | true = shown to user; false = hidden judge-only |
| OrderIndex | int | display/execution order |
| Points | int | partial scoring weight |
| Problem | Problem | required — owning problem |

---

### `Submission`
One attempt by a user to solve a problem.

| Property | Type | Notes |
|---|---|---|
| Id | int | |
| SourceCode | string | required |
| Language | string | required — e.g. "cpp", "c" |
| Status | SubmissionStatus | |
| SubmittedAt | DateTime | |
| Score | int | 0–100, based on passing test cases |
| WallTimeMs | int | measured execution time |
| PeakMemoryKb | int | measured peak memory |
| User | User | required |
| Problem | Problem | required |
| ExecutionResult | ExecutionResult? | null while status is Pending/Running |

---

### `ExecutionResult`
Raw output from the sandbox for a submission.

| Property | Type | Notes |
|---|---|---|
| Id | int | |
| Stdout | string | required |
| Stderr | string | required |
| ExitCode | int | 0 = clean exit |
| TimedOut | bool | true if wall time exceeded limit |
| MemoryExceeded | bool | true if peak memory exceeded limit |

---

### `ProblemSet`
An ordered collection of problems — used for courses or curated playlists.

| Property | Type | Notes |
|---|---|---|
| Id | int | |
| Title | string | required |
| Description | string | required |
| CreatedAt | DateTime | |
| IsPublic | bool | controls visibility to non-members |
| OrderIndex | int | ordering within the organization |
| Organization | Organization | required — owning org |
| Problems | List\<Problem\> | N-N with Problem |

---

## Relationships

```mermaid
erDiagram
    User {
        int Id
        string Username
        string Email
        string DisplayName
        UserRole Role
        DateTime RegisteredAt
    }
    Organization {
        int Id
        string Name
        string Description
    }
    Problem {
        int Id
        string Title
        string Description
        DifficultyLevel Difficulty
        int TimeLimitMs
        int MemoryLimitKb
        DateTime CreatedAt
        string AuthorUsername
    }
    ProblemSet {
        int Id
        string Title
        string Description
        DateTime CreatedAt
        bool IsPublic
        int OrderIndex
    }
    Tag {
        int Id
        string Name
    }
    TestCase {
        int Id
        string InputArgs
        string ExpectedOutput
        bool IsSample
        int OrderIndex
        int Points
    }
    Submission {
        int Id
        string SourceCode
        string Language
        SubmissionStatus Status
        DateTime SubmittedAt
        int Score
        int WallTimeMs
        int PeakMemoryKb
    }
    ExecutionResult {
        int Id
        string Stdout
        string Stderr
        int ExitCode
        bool TimedOut
        bool MemoryExceeded
    }

    User             ||--o{ Submission    : "makes"
    User             }o--o{ Organization  : "belongs to"
    Organization     ||--o{ ProblemSet    : "owns"
    ProblemSet       }o--o{ Problem       : "contains"
    Problem          ||--o{ TestCase      : "has"
    Problem          ||--o{ Submission    : "receives"
    Problem          }o--o{ Tag           : "tagged with"
    Submission       ||--o| ExecutionResult : "produces"
```

Key relationships:
- **1-N**: Problem → TestCases, Problem → Submissions, User → Submissions, Organization → ProblemSets
- **N-N**: Problem ↔ Tag, Problem ↔ ProblemSet, User ↔ Organization
- `ExecutionResult` is nullable on `Submission` — it only exists after the sandbox has finished running
