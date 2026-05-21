using Microsoft.AspNetCore.Mvc;
using WebIde.Frontend.Models;
using WebIde.Model;
using WebIde.Web.Repositories;

namespace WebIde.Web.Controllers;

[Route("problems/{problemId:int}/testcases")]
public class TestCaseController : Controller
{
    private readonly TestCaseRepository _repo;
    private readonly ProblemRepository _problems;

    public TestCaseController(TestCaseRepository repo, ProblemRepository problems)
    {
        _repo = repo;
        _problems = problems;
    }

    [Route("")]
    public IActionResult Index(int problemId)
    {
        var problem = _problems.GetById(problemId);
        if (problem is null) return NotFound();
        ViewData["Title"] = $"TEST CASES — {problem.Title.ToUpper()}";
        ViewData["Problem"] = problem;
        return View(_repo.GetByProblemId(problemId));
    }

    [Route("create")]
    [HttpGet]
    public IActionResult Create(int problemId)
    {
        var problem = _problems.GetById(problemId);
        if (problem is null) return NotFound();
        ViewData["Title"] = "CREATE TEST CASE";
        ViewData["Problem"] = problem;
        return View(new TestCaseCreateModel { ProblemId = problemId });
    }

    [Route("create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(int problemId, TestCaseCreateModel model)
    {
        model.ProblemId = problemId;
        if (!ModelState.IsValid)
        {
            var problem = _problems.GetById(problemId);
            ViewData["Title"] = "CREATE TEST CASE";
            ViewData["Problem"] = problem;
            return View(model);
        }
        _repo.Add(new TestCase
        {
            ProblemId      = problemId,
            InputArgs      = model.InputArgs,
            ExpectedOutput = model.ExpectedOutput,
            IsSample       = model.IsSample,
            OrderIndex     = model.OrderIndex,
            Points         = model.Points,
        });
        TempData["Flash"] = "Test case created.";
        return RedirectToAction(nameof(Index), new { problemId });
    }

    [Route("{id:int}/edit")]
    [HttpGet, ActionName("Edit")]
    public IActionResult EditGet(int problemId, int id)
    {
        var tc = _repo.GetById(id);
        if (tc is null) return NotFound();
        var problem = _problems.GetById(problemId);
        ViewData["Title"] = "EDIT TEST CASE";
        ViewData["Problem"] = problem;
        return View(new TestCaseEditModel
        {
            Id             = tc.Id,
            ProblemId      = problemId,
            InputArgs      = tc.InputArgs,
            ExpectedOutput = tc.ExpectedOutput,
            IsSample       = tc.IsSample,
            OrderIndex     = tc.OrderIndex,
            Points         = tc.Points,
        });
    }

    [Route("{id:int}/edit")]
    [HttpPost, ActionName("Edit")]
    [ValidateAntiForgeryToken]
    public IActionResult EditPost(int problemId, int id, TestCaseEditModel model)
    {
        model.ProblemId = problemId;
        if (!ModelState.IsValid)
        {
            var problem = _problems.GetById(problemId);
            ViewData["Title"] = "EDIT TEST CASE";
            ViewData["Problem"] = problem;
            return View(model);
        }
        var tc = _repo.GetById(id);
        if (tc is null) return NotFound();

        tc.InputArgs      = model.InputArgs;
        tc.ExpectedOutput = model.ExpectedOutput;
        tc.IsSample       = model.IsSample;
        tc.OrderIndex     = model.OrderIndex;
        tc.Points         = model.Points;
        _repo.Update();
        TempData["Flash"] = "Test case updated.";
        return RedirectToAction(nameof(Index), new { problemId });
    }

    [Route("{id:int}/delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int problemId, int id)
    {
        _repo.SoftDelete(id);
        TempData["Flash"] = "Test case deleted.";
        return RedirectToAction(nameof(Index), new { problemId });
    }

    [Route("search")]
    [HttpGet]
    public IActionResult Search(int problemId, string q)
    {
        var results = _repo.Search(problemId, q ?? "");
        return Json(results.Select(tc => new { id = tc.Id, label = $"TC#{tc.OrderIndex}: {tc.InputArgs.Substring(0, Math.Min(40, tc.InputArgs.Length))}" }));
    }
}
