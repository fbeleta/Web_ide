# Sitemap — URL → Controller → Action → View

Generated for Lab 3 — WebIde project.

## Default / Home

| URL | Controller | Action | View |
|---|---|---|---|
| `/` | `HomeController` | `Index` | `Views/Home/Index.cshtml` |
| `/Home/Privacy` | `HomeController` | `Privacy` | `Views/Home/Privacy.cshtml` |
| `/Home/Error` | `HomeController` | `Error` | `Views/Shared/Error.cshtml` |

## Problems

Custom route prefix: `/problems`

| URL | Controller | Action | View |
|---|---|---|---|
| `/problems` | `ProblemController` | `Index` | `Views/Problem/Index.cshtml` |
| `/problems?sort=difficulty-asc` | `ProblemController` | `Index` | `Views/Problem/Index.cshtml` |
| `/problems?sort=difficulty-desc` | `ProblemController` | `Index` | `Views/Problem/Index.cshtml` |
| `/problems?sort=title` | `ProblemController` | `Index` | `Views/Problem/Index.cshtml` |
| `/problems?sort=acceptance-asc` | `ProblemController` | `Index` | `Views/Problem/Index.cshtml` |
| `/problems?sort=acceptance-desc` | `ProblemController` | `Index` | `Views/Problem/Index.cshtml` |
| `/problems/{id}` | `ProblemController` | `Details` | `Views/Problem/Details.cshtml` |

## Submissions

Custom route prefix: `/submissions`

| URL | Controller | Action | View |
|---|---|---|---|
| `/submissions` | `SubmissionController` | `Index` | `Views/Submission/Index.cshtml` |
| `/submissions?sort=date-asc` | `SubmissionController` | `Index` | `Views/Submission/Index.cshtml` |
| `/submissions?sort=score-desc` | `SubmissionController` | `Index` | `Views/Submission/Index.cshtml` |
| `/submissions?sort=score-asc` | `SubmissionController` | `Index` | `Views/Submission/Index.cshtml` |
| `/submissions?sort=status` | `SubmissionController` | `Index` | `Views/Submission/Index.cshtml` |
| `/submissions/{id}` | `SubmissionController` | `Details` | `Views/Submission/Details.cshtml` |

## Leaderboard

Custom route prefix: `/leaderboard`

| URL | Controller | Action | View |
|---|---|---|---|
| `/leaderboard` | `LeaderboardController` | `Index` | `Views/Leaderboard/Index.cshtml` |
| `/leaderboard?sort=solved` | `LeaderboardController` | `Index` | `Views/Leaderboard/Index.cshtml` |
| `/leaderboard?sort=score` | `LeaderboardController` | `Index` | `Views/Leaderboard/Index.cshtml` |

## Users

Custom route prefix: `/users`

| URL | Controller | Action | View |
|---|---|---|---|
| `/users` | `UserController` | `Index` | `Views/User/Index.cshtml` |
| `/users/{id}` | `UserController` | `Details` | `Views/User/Details.cshtml` |

## Organizations

Custom route prefix: `/orgs`

| URL | Controller | Action | View |
|---|---|---|---|
| `/orgs` | `OrganizationController` | `Index` | `Views/Organization/Index.cshtml` |
| `/orgs/{id}` | `OrganizationController` | `Details` | `Views/Organization/Details.cshtml` |

## Tags

Default route: `/Tag/...`

| URL | Controller | Action | View |
|---|---|---|---|
| `/Tag` | `TagController` | `Index` | `Views/Tag/Index.cshtml` |
| `/Tag/Details/{id}` | `TagController` | `Details` | `Views/Tag/Details.cshtml` |

## Problem Sets

Default route: `/ProblemSet/...`

| URL | Controller | Action | View |
|---|---|---|---|
| `/ProblemSet` | `ProblemSetController` | `Index` | `Views/ProblemSet/Index.cshtml` |
| `/ProblemSet/Details/{id}` | `ProblemSetController` | `Details` | `Views/ProblemSet/Details.cshtml` |

## Notes

- Controllers with `[Route("...")]` class-level attribute use custom URL prefixes (problems, submissions, leaderboard, users, orgs).
- All other controllers use the default route pattern `{controller}/{action}/{id?}` defined in `Program.cs`.
- Shared partial views: `Views/Shared/_Layout.cshtml`, `Views/Shared/_Sidebar.cshtml`.
