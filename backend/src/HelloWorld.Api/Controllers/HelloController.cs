using Microsoft.AspNetCore.Mvc;
using HelloWorld.Api.Models;

namespace HelloWorld.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var response = new HelloWorldResponse(
            "Hello World from .NET 10 API!",
            DateTime.UtcNow
        );
        return Ok(response);
    }

    [HttpGet("{name}")]
    public IActionResult Get(string name)
    {
        var response = new HelloWorldResponse(
            $"Hello {name} from .NET 10 API!",
            DateTime.UtcNow
        );
        return Ok(response);
    }
}
