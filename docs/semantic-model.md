# Semantic DB Model

Generated for Lab 3 — WebIde project.

## Tables / Entities

| Table | Key columns | Notes |
|---|---|---|
| `Users` | `Id`, `Username`, `Email`, `DisplayName`, `Role`, `RegisteredAt` | Role: Admin / Instructor / Student |
| `Organizations` | `Id`, `Name`, `Description` | Groups (courses, clubs) |
| `Problems` | `Id`, `Title`, `Description`, `Difficulty`, `TimeLimitMs`, `MemoryLimitKb`, `CreatedAt`, `AuthorUsername` | Difficulty: Easy / Medium / Hard |
| `Tags` | `Id`, `Name` | Labels for problems (e.g. "Arrays") |
| `TestCases` | `Id`, `InputArgs`, `ExpectedOutput`, `IsSample`, `OrderIndex`, `Points`, `ProblemId` | FK → Problems |
| `Submissions` | `Id`, `SourceCode`, `Language`, `Status`, `SubmittedAt`, `Score`, `WallTimeMs`, `PeakMemoryKb`, `UserId`, `ProblemId`, `ExecutionResultId` | FK → Users, Problems, ExecutionResults |
| `ExecutionResults` | `Id`, `Stdout`, `Stderr`, `ExitCode`, `TimedOut`, `MemoryExceeded` | Raw sandbox output |
| `ProblemSets` | `Id`, `Title`, `Description`, `CreatedAt`, `IsPublic`, `OrderIndex`, `OrganizationId` | FK → Organizations |

## Join Tables (N-N)

| Table | Columns | Purpose |
|---|---|---|
| `ProblemTags` | `ProblemsId`, `TagsId` | Problem ↔ Tag |
| `ProblemSetProblems` | `ProblemSetsId`, `ProblemsId` | ProblemSet ↔ Problem |
| `OrganizationMembers` | `MembersId`, `OrganizationsId` | User ↔ Organization |

## Relationships

```
Users          1──N  Submissions
Problems       1──N  Submissions
Problems       1──N  TestCases
Organizations  1──N  ProblemSets
Submissions    1──0/1 ExecutionResults

Users          N──N  Organizations   (via OrganizationMembers)
Problems       N──N  Tags            (via ProblemTags)
Problems       N──N  ProblemSets     (via ProblemSetProblems)
```
