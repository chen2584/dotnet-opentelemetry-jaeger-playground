using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;

namespace MyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    readonly ILogger<TestController> _logger;
    readonly ActivitySource _activitySource;
    readonly IMeterFactory _meterFactory;
    public TestController(ILogger<TestController> logger, ActivitySource activitySource, IMeterFactory meterFactory)
    {
        _logger = logger;
        _activitySource = activitySource;
        _meterFactory = meterFactory;
    }

    private async Task ExecuteFirstChildActivity()
    {
        using var childActivity = _activitySource.StartActivity("FirstChildActivity");
        await Task.Delay(500);
    }

    private async Task ExecuteSecondChildActivity()
    {
        using var childActivity = _activitySource.StartActivity("SecondChildActivity");
        await Task.Delay(500);
    }

    [HttpGet("log")]
    public ActionResult TestLog()
    {
        var random = new Random();
        var result = random.Next(1, 6);
        _logger.LogInformation("Anonymous player is rolling the dice: {result}", result);
        throw new Exception("This is a book");
        return Ok();
    }

    [HttpGet("execute/activity")]
    public async Task<ActionResult> ExecuteActivity()
    {
        using var parentActivity = _activitySource.StartActivity("ParentActivity"); // It maybe null if no config opentelemetry

        var random = new Random();
        var randomNumber = random.Next(1, 6);
        parentActivity?.SetTag("random.number", randomNumber); // Opentelemetry suggest to use constrant key variable instead

        var eventTags = new Dictionary<string, object?>
        {
            { "foo", 1 },
            { "bar", "Hello, World!" },
            { "baz", new int[] { 1, 2, 3 } }
        };
        parentActivity?.AddEvent(new("Gonna try event!", DateTimeOffset.Now, new(eventTags)));
        parentActivity?.SetStatus(ActivityStatusCode.Error, "Something bad happened!");

        using var childActivity = _activitySource.StartActivity("ChildActivity"); // It maybe null if no config opentelemetry
        childActivity.SetTag("child_tag", 123456789);
        // ExecuteFirstChildActivity();
        // await Task.WhenAll(ExecuteFirstChildActivity(), ExecuteSecondChildActivity());

        // var activityContext = Activity.Current!.Context; // Always get latest activity // Get a context from somewhere, perhaps it's passed in as a parameter
        // var links = new List<ActivityLink>
        // {
        //     new(activityContext)
        // };
        // using var anotherActivity = _activitySource.StartActivity(ActivityKind.Internal, name: "AnotherActivity", links: links);

        var meter = _meterFactory.Create("TestMeter");
        var counter = meter.CreateCounter<long>("execute_test_activity");
        counter.Add(1);
        return Ok();
    }
}