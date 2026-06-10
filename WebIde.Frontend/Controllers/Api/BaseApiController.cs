using Microsoft.AspNetCore.Mvc;

namespace WebIde.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase { }
