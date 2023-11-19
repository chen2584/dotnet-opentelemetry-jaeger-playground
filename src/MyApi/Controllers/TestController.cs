using Microsoft.AspNetCore.Mvc;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    readonly ILogger<TestController> _logger;
    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    [HttpGet("log")]
    public ActionResult TestLog()
    {
        var random = new Random();
        var result = random.Next(1, 6);
        _logger.LogInformation("Anonymous player is rolling the dice: {result}", result);
        return Ok();
    }
}