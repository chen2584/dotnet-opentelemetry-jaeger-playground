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

    [HttpGet("metric/counter")]
    public ActionResult TestMetricCounter()
    {
        var meter = _meterFactory.Create("TestMeter");
        var counter = meter.CreateCounter<long>("execute_test_counter", "Requests");
        counter.Add(1, new KeyValuePair<string, object?>("request-by", "Chenz"));
        return Ok();
    }

    [HttpGet("metric/counter/2")]
    public ActionResult TestMetricCoubterz()
    {
        var meter = _meterFactory.Create("TestMeter");
        var counter = meter.CreateCounter<long>("execute_test_counter", "Requests");
        counter.Add(1, new KeyValuePair<string, object?>("request-by", "Chen"));
        return Ok();
    }

    [HttpGet("metric/updown/counter")]
    public ActionResult TestMetricUpDown()
    {
        var meter = _meterFactory.Create("TestMeter");
        var counter = meter.CreateUpDownCounter<long>("execute_test_up_down", "Requests");
        counter.Add(-1, new KeyValuePair<string, object?>("request-by", "Chenz"));
        return Ok();
    }

    [HttpGet("metric/histogram")]
    public ActionResult TestMyHistogram()
    {
        var meter = _meterFactory.Create("TestMeter");
        var counter = meter.CreateHistogram<long>("my_histogram", "Requests", "This is my histogram");
        counter.Record(1, new KeyValuePair<string, object?>("request-by", "Chenz"));
        return Ok();
    }

    [HttpGet("log/information")]
    public ActionResult LogInformation([FromQuery] string message)
    {
        _logger.LogInformation("{message}", message);
        return Ok();
    }

    [HttpGet("log/error")]
    public ActionResult LogError([FromQuery] string message)
    {
        _logger.LogError("{message}", message);
        return Ok();
    }
}