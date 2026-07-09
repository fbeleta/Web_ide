using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebIde.Web.Controllers.Api;

// All /api endpoints authenticate via either the Identity browser cookie or a
// Personal Access Token. Individual actions may layer role requirements on top
// with their own [Authorize(Roles = ...)]; those also specify ApiAuthSchemes.Api.
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = ApiAuthSchemes.Api)]
public abstract class BaseApiController : ControllerBase { }
