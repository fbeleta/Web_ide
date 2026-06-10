using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebIde.DAL;
using WebIde.Model;
using WebIde.Web.DTOs;

namespace WebIde.Web.Controllers.Api;

[Route("api/executionresult")]
public class ExecutionResultApiController : BaseApiController
{
    private readonly WebIdeDbContext _db;
    public ExecutionResultApiController(WebIdeDbContext db) => _db = db;

    [HttpGet]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity, Roles = "Admin")]
    public ActionResult<IEnumerable<ExecutionResultDto>> GetAll() =>
        Ok(_db.ExecutionResults.OrderByDescending(e => e.Id).ToList().Select(ToDto));

    [HttpGet("{id:int}")]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity)]
    public ActionResult<ExecutionResultDto> GetById(int id)
    {
        var er = _db.ExecutionResults.FirstOrDefault(e => e.Id == id);
        return er is null ? NotFound() : Ok(ToDto(er));
    }

    private static ExecutionResultDto ToDto(ExecutionResult er) => new()
    {
        Id             = er.Id,
        SubmissionId   = er.SubmissionId,
        TestCaseId     = er.TestCaseId,
        Stdout         = er.Stdout,
        Stderr         = er.Stderr,
        ExitCode       = er.ExitCode,
        WallTimeMs     = er.WallTimeMs,
        PeakMemoryKb   = er.PeakMemoryKb,
        Verdict        = er.Verdict.ToString(),
        TimedOut       = er.TimedOut,
        MemoryExceeded = er.MemoryExceeded,
    };
}
