# Plan: Database Seed File + Schema Updates

## Context
The project has a complete domain model (C# classes) and a React frontend with 4 pages, but no database setup yet. The task is to:
1. Identify gaps between the existing C# schema and what the frontend actually renders
2. Add missing entities/fields to the C# model
3. Create a comprehensive SQL seed file with dummy data that makes all frontend pages realistic

---

## Phase 1: Schema Updates (C# Model)

### Gaps found by comparing frontend to model

| Frontend shows | Current model | Fix needed |
|---|---|---|
| "Elite Tier" badge on UserProfile | No tier field | Add `Tier` enum + field to User |
| User location ("Zagreb") | No location field | Add `Location?` to User |
| User avatar | No avatar | Add `AvatarUrl?` to User |
| Teams section (Team Zenith, Quantum Solvers) | No Team entity | Add `Team` class |
| Activity heatmap (84 data points) | No daily activity | Add `UserActivity` class |
| Problem acceptance rate (derived) | No cached field | Computable from submissions – no new field needed |
| Daily Challenge hero | No marker on Problem | Add `IsDailyChallenge` bool to Problem |

### Files to modify

**`/WebIde.Model/User.cs`** — Add:
```csharp
public string? AvatarUrl { get; set; }
public string? Location { get; set; }
public UserTier Tier { get; set; } = UserTier.Beginner;
public List<Team> Teams { get; set; } = [];
public List<UserActivity> Activities { get; set; } = [];
```

**`/WebIde.Model/Problem.cs`** — Add:
```csharp
public bool IsDailyChallenge { get; set; }
```

**New file `/WebIde.Model/Team.cs`**:
```csharp
public class Team {
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public bool IsLive { get; set; }
    public int ActiveProblemCount { get; set; }
    public List<User> Members { get; set; } = [];
}
```

**New file `/WebIde.Model/UserActivity.cs`**:
```csharp
public class UserActivity {
    public required int Id { get; set; }
    public required DateTime Date { get; set; }  // day granularity
    public required int SubmissionCount { get; set; }
    public required User User { get; set; }
}
```

**New file `/WebIde.Model/Enums/UserTier.cs`**:
```csharp
public enum UserTier { Beginner, Intermediate, Pro, Elite }
```

**Update `/docs/domain-model.md`** — add new entities to ERD section.

---

## Phase 2: SQL Seed File

**New file `/db/seed.sql`**

### Seed data plan

**Tags (13):**
Arrays, Hash Map, Sliding Window, Binary Search, Dynamic Programming, Two Pointers, Stack, Queue, Tree, Graph, String, Math, Greedy

**Users (10):** varied roles, tiers, locations, Croatian names matching the app's academic theme
- `ana_kovac` — Student, Elite, Zagreb (the "main" profile shown in UserProfile page)
- `mario_basicic` — Student, Pro, Split
- `prof_horvat` — Instructor, Pro, Zagreb
- `luka_novak` — Student, Intermediate, Rijeka
- `maja_juric` — Student, Pro, Zagreb
- `ivana_kralj` — Student, Elite, Osijek
- `petar_boban` — Student, Intermediate, Zadar
- `sara_modric` — Student, Pro, Zagreb
- `tomislav_petar` — Student, Beginner, Varaždin
- `admin` — Admin, Elite, Zagreb

**Problems (16):** spread across Easy/Medium/Hard
- Two Sum, Valid Parentheses, Palindrome Number, Maximum Subarray, Longest Substring Without Repeating Characters, Merge Intervals, Container With Most Water, 3Sum, Binary Tree Level Order Traversal, Median of Two Sorted Arrays, Trapping Rain Water, LRU Cache, Word Ladder, Climbing Stairs, Coin Change
- **Valid Sudoku Solver** — marked as `IsDailyChallenge = true`

**TestCases:** 2–3 sample + 3–5 hidden per problem (covering all 16 problems)

**Organizations (3):**
- FER Algorithms (all 10 members)
- Advanced Algorithms 101 (6 members)
- Neural Arch Systems (4 members)

**Teams (3):**
- Team Zenith (live, ana_kovac, maja_juric, ivana_kralj, sara_modric)
- Quantum Solvers (inactive, mario_basicic, luka_novak, petar_boban)
- Debug Dynasty (live, tomislav_petar + others)

**ProblemSets (4):**
- Week 1 – Fundamentals (org: FER Algorithms, public)
- Week 2 – Intermediate Structures (org: FER Algorithms, public)
- Neural Coding Sprint (org: Neural Arch Systems, private)
- Advanced Contest Prep (org: Advanced Algorithms 101, public)

**Submissions (~120):**
- Multiple submissions per user across problems
- Mix of Accepted, WrongAnswer, TimeLimitExceeded, CompileError statuses
- Spread across 90 days (to populate activity heatmap)
- Enough accepted submissions per user for rankings:
  - ana_kovac: ~50 accepted (rank #1 in seed data)
  - ivana_kralj: ~45 accepted
  - sara_modric, maja_juric: ~35 accepted each
  - etc.
- ExecutionResult rows for each completed submission (Accepted/WrongAnswer)

**UserActivity (90 rows per active user):**
- Daily activity counts for last 90 days for ana_kovac and top users
- Varying intensity (0–8 submissions per day)

---

## File locations

| File | Action |
|---|---|
| `/WebIde.Model/User.cs` | Modify — add Tier, Location, AvatarUrl, Teams, Activities |
| `/WebIde.Model/Problem.cs` | Modify — add IsDailyChallenge |
| `/WebIde.Model/Team.cs` | Create — new entity |
| `/WebIde.Model/UserActivity.cs` | Create — new entity |
| `/WebIde.Model/Enums/UserTier.cs` | Create — new enum |
| `/docs/domain-model.md` | Modify — add Team, UserActivity, UserTier to ERD |
| `/db/seed.sql` | Create — full PostgreSQL seed (CREATE TABLE + INSERT) |

---

## Verification

1. Run `npm run dev` in `WebIde.Frontend` — pages should reflect the structure of seeded data
2. Once EF Core is set up, run `psql -f db/seed.sql` to populate the database
3. Check UserProfile page: stats grid, organizations, teams, activity heatmap all populated
4. Check ProblemLibrary page: 15+ problems with correct tags, difficulty, acceptance rates
5. Check SubmissionResults page: submissions with ExecutionResult stdout/stderr
