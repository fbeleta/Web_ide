using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebIde.DAL;
using WebIde.Model;
using WebIde.Web.DTOs;

namespace WebIde.Web.Controllers.Api;

[Route("api/testcase")]
public class TestCaseApiController : BaseApiController
{
    private readonly WebIdeDbContext _db;
    public TestCaseApiController(WebIdeDbContext db) => _db = db;

    // Public: returns input and metadata only — no expected output.
    [HttpGet]
    public ActionResult<IEnumerable<TestCasePublicDto>> GetAll([FromQuery] int? problemId)
    {
        var query = _db.TestCases.AsQueryable();
        if (problemId.HasValue) query = query.Where(tc => tc.ProblemId == problemId.Value);
        return Ok(query.OrderBy(tc => tc.OrderIndex).ToList().Select(ToPublicDto));
    }

    // Public: single test case without expected output.
    [HttpGet("{id:int}")]
    public ActionResult<TestCasePublicDto> GetById(int id)
    {
        var tc = _db.TestCases.FirstOrDefault(t => t.Id == id);
        return tc is null ? NotFound() : Ok(ToPublicDto(tc));
    }

    // Admin-only: includes expected output for judging configuration.
    [HttpGet("{id:int}/full")]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity, Roles = "Admin")]
    public ActionResult<TestCaseDto> GetByIdFull(int id)
    {
        var tc = _db.TestCases.FirstOrDefault(t => t.Id == id);
        return tc is null ? NotFound() : Ok(ToDto(tc));
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity, Roles = "Admin")]
    public ActionResult<TestCaseDto> Create([FromBody] CreateTestCaseDto dto)
    {
        var tc = new TestCase
        {
            ProblemId      = dto.ProblemId,
            InputArgs      = dto.InputArgs,
            ExpectedOutput = dto.ExpectedOutput,
            IsSample       = dto.IsSample,
            OrderIndex     = dto.OrderIndex,
            Points         = dto.Points,
        };
        _db.TestCases.Add(tc);
        _db.SaveChanges();
        return CreatedAtAction(nameof(GetById), new { id = tc.Id }, ToDto(tc));
    }

    [HttpPut("{id:int}")]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity, Roles = "Admin")]
    public ActionResult<TestCaseDto> Update(int id, [FromBody] UpdateTestCaseDto dto)
    {
        var tc = _db.TestCases.FirstOrDefault(t => t.Id == id);
        if (tc is null) return NotFound();
        if (dto.InputArgs is not null)      tc.InputArgs      = dto.InputArgs;
        if (dto.ExpectedOutput is not null) tc.ExpectedOutput = dto.ExpectedOutput;
        if (dto.IsSample.HasValue)          tc.IsSample       = dto.IsSample.Value;
        if (dto.OrderIndex.HasValue)        tc.OrderIndex     = dto.OrderIndex.Value;
        if (dto.Points.HasValue)            tc.Points         = dto.Points.Value;
        _db.SaveChanges();
        return Ok(ToDto(tc));
    }

    [HttpDelete("{id:int}")]
    [Authorize(AuthenticationSchemes = ApiAuthSchemes.Identity, Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        var tc = _db.TestCases.FirstOrDefault(t => t.Id == id);
        if (tc is null) return NotFound();
        _db.TestCases.Remove(tc);
        _db.SaveChanges();
        return NoContent();
    }

    private static TestCasePublicDto ToPublicDto(TestCase tc) => new()
    {
        Id         = tc.Id,
        InputArgs  = tc.InputArgs,
        IsSample   = tc.IsSample,
        OrderIndex = tc.OrderIndex,
        Points     = tc.Points,
        ProblemId  = tc.ProblemId,
    };

    private static TestCaseDto ToDto(TestCase tc) => new()
    {
        Id             = tc.Id,
        InputArgs      = tc.InputArgs,
        ExpectedOutput = tc.ExpectedOutput,
        IsSample       = tc.IsSample,
        OrderIndex     = tc.OrderIndex,
        Points         = tc.Points,
        ProblemId      = tc.ProblemId,
    };
}
