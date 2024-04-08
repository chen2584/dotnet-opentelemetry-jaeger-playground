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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJHVXlFWVVBVTZ4S1RzdW5VY3BCRjlpUW5jc240UVYyUVI1M2VjUUNTeUZzIn0.eyJleHAiOjE3MTI2MzAxMDgsImlhdCI6MTcxMjU5NDEwOSwianRpIjoiOGQxNTk3MjAtZjc3Yi00OTI2LThjODQtMTFkZTM4ZmZlMmI0IiwiaXNzIjoiaHR0cDovL2tleWNsb2FrOjgwODAvcmVhbG1zL29wZW50ZWxlbWV0cnkiLCJhdWQiOlsiY29sbGVjdG9yIiwiYWNjb3VudCJdLCJzdWIiOiI5MWY0OWNiNy1iMWMyLTRkZjEtYWNjOC0yMjYyMzFkOTI5ZjYiLCJ0eXAiOiJCZWFyZXIiLCJhenAiOiJjb2xsZWN0b3IiLCJhY3IiOiIxIiwiYWxsb3dlZC1vcmlnaW5zIjpbIi8qIl0sInJlYWxtX2FjY2VzcyI6eyJyb2xlcyI6WyJvZmZsaW5lX2FjY2VzcyIsInVtYV9hdXRob3JpemF0aW9uIiwiZGVmYXVsdC1yb2xlcy1vcGVudGVsZW1ldHJ5Il19LCJyZXNvdXJjZV9hY2Nlc3MiOnsiYWNjb3VudCI6eyJyb2xlcyI6WyJtYW5hZ2UtYWNjb3VudCIsIm1hbmFnZS1hY2NvdW50LWxpbmtzIiwidmlldy1wcm9maWxlIl19fSwic2NvcGUiOiJlbWFpbCBwcm9maWxlIiwiY2xpZW50SG9zdCI6IjE3Mi4yMi4wLjEiLCJlbWFpbF92ZXJpZmllZCI6ZmFsc2UsInByZWZlcnJlZF91c2VybmFtZSI6InNlcnZpY2UtYWNjb3VudC1jb2xsZWN0b3IiLCJjbGllbnRBZGRyZXNzIjoiMTcyLjIyLjAuMSIsImNsaWVudF9pZCI6ImNvbGxlY3RvciJ9.QQ8hLTkIUxQAMEFIrUE644804v5QM-rrAbZJwPZyNkLOxiaeiyFhBSdG62DYp40bEOvp2Ws9sPmKQBaf-E50JtE1Kpdyai81tPSO9--SrVwih9Znl7Sa5P4mjBNs-EwS9nBVQCGvVaCGy_q7epwZZFPLOgEDu7wwER4F1ORYqlM8zbQ3h7q5rsmdlyGTmshAUHlFSyIuUlXbrFaWj4i7WU9Ux35oip_h9u3zKVO1TZNymdOqjMi2hy7wiKhp94H3vymvYTWILtc0VaHkP4y_0Y5CB_xIfsCoAV0bvAPS_vW9lVjgSJmQZYVwNya-9PFIDdhOWf8xybGC8j-PurCW3Q");
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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJHVXlFWVVBVTZ4S1RzdW5VY3BCRjlpUW5jc240UVYyUVI1M2VjUUNTeUZzIn0.eyJleHAiOjE3MTI2MzAxMDgsImlhdCI6MTcxMjU5NDEwOSwianRpIjoiOGQxNTk3MjAtZjc3Yi00OTI2LThjODQtMTFkZTM4ZmZlMmI0IiwiaXNzIjoiaHR0cDovL2tleWNsb2FrOjgwODAvcmVhbG1zL29wZW50ZWxlbWV0cnkiLCJhdWQiOlsiY29sbGVjdG9yIiwiYWNjb3VudCJdLCJzdWIiOiI5MWY0OWNiNy1iMWMyLTRkZjEtYWNjOC0yMjYyMzFkOTI5ZjYiLCJ0eXAiOiJCZWFyZXIiLCJhenAiOiJjb2xsZWN0b3IiLCJhY3IiOiIxIiwiYWxsb3dlZC1vcmlnaW5zIjpbIi8qIl0sInJlYWxtX2FjY2VzcyI6eyJyb2xlcyI6WyJvZmZsaW5lX2FjY2VzcyIsInVtYV9hdXRob3JpemF0aW9uIiwiZGVmYXVsdC1yb2xlcy1vcGVudGVsZW1ldHJ5Il19LCJyZXNvdXJjZV9hY2Nlc3MiOnsiYWNjb3VudCI6eyJyb2xlcyI6WyJtYW5hZ2UtYWNjb3VudCIsIm1hbmFnZS1hY2NvdW50LWxpbmtzIiwidmlldy1wcm9maWxlIl19fSwic2NvcGUiOiJlbWFpbCBwcm9maWxlIiwiY2xpZW50SG9zdCI6IjE3Mi4yMi4wLjEiLCJlbWFpbF92ZXJpZmllZCI6ZmFsc2UsInByZWZlcnJlZF91c2VybmFtZSI6InNlcnZpY2UtYWNjb3VudC1jb2xsZWN0b3IiLCJjbGllbnRBZGRyZXNzIjoiMTcyLjIyLjAuMSIsImNsaWVudF9pZCI6ImNvbGxlY3RvciJ9.QQ8hLTkIUxQAMEFIrUE644804v5QM-rrAbZJwPZyNkLOxiaeiyFhBSdG62DYp40bEOvp2Ws9sPmKQBaf-E50JtE1Kpdyai81tPSO9--SrVwih9Znl7Sa5P4mjBNs-EwS9nBVQCGvVaCGy_q7epwZZFPLOgEDu7wwER4F1ORYqlM8zbQ3h7q5rsmdlyGTmshAUHlFSyIuUlXbrFaWj4i7WU9Ux35oip_h9u3zKVO1TZNymdOqjMi2hy7wiKhp94H3vymvYTWILtc0VaHkP4y_0Y5CB_xIfsCoAV0bvAPS_vW9lVjgSJmQZYVwNya-9PFIDdhOWf8xybGC8j-PurCW3Q");
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
