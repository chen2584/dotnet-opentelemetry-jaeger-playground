using System.Diagnostics;
using System.Net.Http.Headers;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
const string serviceName = "my-api";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing => tracing
        // .AddSource(serviceName)
        .SetErrorStatusOnException(false)
        .AddAspNetCoreInstrumentation()
        // .AddHttpClientInstrumentation()
        .AddOtlpExporter(o =>
        {
            o.Endpoint = new Uri("http://localhost:4318/v1/traces"); // Default ไม่ต้องใส่ก็ได้,
            o.Protocol = OtlpExportProtocol.HttpProtobuf;
            o.HttpClientFactory = () =>
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJHVXlFWVVBVTZ4S1RzdW5VY3BCRjlpUW5jc240UVYyUVI1M2VjUUNTeUZzIn0.eyJleHAiOjE3MTI4MTA1MDgsImlhdCI6MTcxMjc3NDUwOSwianRpIjoiMjExY2QzZTYtNmQ0NS00ZmFlLTk4MjUtMjQxNjA1MDRiY2M1IiwiaXNzIjoiaHR0cDovL2tleWNsb2FrOjgwODAvcmVhbG1zL29wZW50ZWxlbWV0cnkiLCJhdWQiOlsiY29sbGVjdG9yIiwiYWNjb3VudCJdLCJzdWIiOiI5MWY0OWNiNy1iMWMyLTRkZjEtYWNjOC0yMjYyMzFkOTI5ZjYiLCJ0eXAiOiJCZWFyZXIiLCJhenAiOiJjb2xsZWN0b3IiLCJhY3IiOiIxIiwiYWxsb3dlZC1vcmlnaW5zIjpbIi8qIl0sInJlYWxtX2FjY2VzcyI6eyJyb2xlcyI6WyJvZmZsaW5lX2FjY2VzcyIsInVtYV9hdXRob3JpemF0aW9uIiwiZGVmYXVsdC1yb2xlcy1vcGVudGVsZW1ldHJ5Il19LCJyZXNvdXJjZV9hY2Nlc3MiOnsiYWNjb3VudCI6eyJyb2xlcyI6WyJtYW5hZ2UtYWNjb3VudCIsIm1hbmFnZS1hY2NvdW50LWxpbmtzIiwidmlldy1wcm9maWxlIl19fSwic2NvcGUiOiJlbWFpbCBwcm9maWxlIiwiY2xpZW50SG9zdCI6IjE3Mi4yOC4wLjEiLCJlbWFpbF92ZXJpZmllZCI6ZmFsc2UsInByZWZlcnJlZF91c2VybmFtZSI6InNlcnZpY2UtYWNjb3VudC1jb2xsZWN0b3IiLCJjbGllbnRBZGRyZXNzIjoiMTcyLjI4LjAuMSIsImNsaWVudF9pZCI6ImNvbGxlY3RvciJ9.VBK1iDVHQvlACYEwk8SCseQhUzwUGUgR4EXULbt-u7JNd3tKr74sivFIO_j4p2Zolxjuicip56itTbyHe3K8a5QiWpBKCvOIkrtypWidelab1vTPZ8XnRGBz3_iytihu1Ohlejxt9htST8hx-sok7ccCwlKOQpfyZ2daABc9gruFkxYHJjAcZKySf_JpI4AJvniQC8op3CNQnrOqYe9FyQZIFxJ4feLe286B7QCMJCinNfA6mLKPihClE0y76XHUyHCTIqWm5CHk0EtLsBiAZEKXSRnegD2k42F4f3IGr5osDdx7ySlcFb8UrTDKDRgQsK_gi2bAbnbqNC2wrG8vzA");
                return client;
            };
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddProcessInstrumentation()
        .AddMeter("TestMeter")
        .AddView(instrumentName: "my_histogram", new ExplicitBucketHistogramConfiguration { Boundaries = new double[] { 10, 20 } })
        .AddOtlpExporter((otlpExporterOptions, metricReaderOptions) =>
        {
            otlpExporterOptions.Endpoint = new Uri("http://localhost:4318/v1/metrics"); // Default ไม่ต้องใส่ก็ได้,
            otlpExporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
            metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 1000;
            otlpExporterOptions.HttpClientFactory = () =>
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJHVXlFWVVBVTZ4S1RzdW5VY3BCRjlpUW5jc240UVYyUVI1M2VjUUNTeUZzIn0.eyJleHAiOjE3MTI4MTA1MDgsImlhdCI6MTcxMjc3NDUwOSwianRpIjoiMjExY2QzZTYtNmQ0NS00ZmFlLTk4MjUtMjQxNjA1MDRiY2M1IiwiaXNzIjoiaHR0cDovL2tleWNsb2FrOjgwODAvcmVhbG1zL29wZW50ZWxlbWV0cnkiLCJhdWQiOlsiY29sbGVjdG9yIiwiYWNjb3VudCJdLCJzdWIiOiI5MWY0OWNiNy1iMWMyLTRkZjEtYWNjOC0yMjYyMzFkOTI5ZjYiLCJ0eXAiOiJCZWFyZXIiLCJhenAiOiJjb2xsZWN0b3IiLCJhY3IiOiIxIiwiYWxsb3dlZC1vcmlnaW5zIjpbIi8qIl0sInJlYWxtX2FjY2VzcyI6eyJyb2xlcyI6WyJvZmZsaW5lX2FjY2VzcyIsInVtYV9hdXRob3JpemF0aW9uIiwiZGVmYXVsdC1yb2xlcy1vcGVudGVsZW1ldHJ5Il19LCJyZXNvdXJjZV9hY2Nlc3MiOnsiYWNjb3VudCI6eyJyb2xlcyI6WyJtYW5hZ2UtYWNjb3VudCIsIm1hbmFnZS1hY2NvdW50LWxpbmtzIiwidmlldy1wcm9maWxlIl19fSwic2NvcGUiOiJlbWFpbCBwcm9maWxlIiwiY2xpZW50SG9zdCI6IjE3Mi4yOC4wLjEiLCJlbWFpbF92ZXJpZmllZCI6ZmFsc2UsInByZWZlcnJlZF91c2VybmFtZSI6InNlcnZpY2UtYWNjb3VudC1jb2xsZWN0b3IiLCJjbGllbnRBZGRyZXNzIjoiMTcyLjI4LjAuMSIsImNsaWVudF9pZCI6ImNvbGxlY3RvciJ9.VBK1iDVHQvlACYEwk8SCseQhUzwUGUgR4EXULbt-u7JNd3tKr74sivFIO_j4p2Zolxjuicip56itTbyHe3K8a5QiWpBKCvOIkrtypWidelab1vTPZ8XnRGBz3_iytihu1Ohlejxt9htST8hx-sok7ccCwlKOQpfyZ2daABc9gruFkxYHJjAcZKySf_JpI4AJvniQC8op3CNQnrOqYe9FyQZIFxJ4feLe286B7QCMJCinNfA6mLKPihClE0y76XHUyHCTIqWm5CHk0EtLsBiAZEKXSRnegD2k42F4f3IGr5osDdx7ySlcFb8UrTDKDRgQsK_gi2bAbnbqNC2wrG8vzA");
                return client;
            };
        }));

builder.Services.AddSingleton(_ => new ActivitySource(serviceName));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
