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
const string bearerToken = "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJHVXlFWVVBVTZ4S1RzdW5VY3BCRjlpUW5jc240UVYyUVI1M2VjUUNTeUZzIn0.eyJleHAiOjE3MTM1MzY5MDIsImlhdCI6MTcxMzUwMDkxNywianRpIjoiZWQzZTdkNDAtYTNkYS00YzZlLTk0NjEtMzliOTdmZDY2MGI3IiwiaXNzIjoiaHR0cDovL2tleWNsb2FrOjgwODAvcmVhbG1zL29wZW50ZWxlbWV0cnkiLCJhdWQiOlsiY29sbGVjdG9yIiwiYWNjb3VudCJdLCJzdWIiOiI5MWY0OWNiNy1iMWMyLTRkZjEtYWNjOC0yMjYyMzFkOTI5ZjYiLCJ0eXAiOiJCZWFyZXIiLCJhenAiOiJjb2xsZWN0b3IiLCJhY3IiOiIxIiwiYWxsb3dlZC1vcmlnaW5zIjpbIi8qIl0sInJlYWxtX2FjY2VzcyI6eyJyb2xlcyI6WyJvZmZsaW5lX2FjY2VzcyIsInVtYV9hdXRob3JpemF0aW9uIiwiZGVmYXVsdC1yb2xlcy1vcGVudGVsZW1ldHJ5Il19LCJyZXNvdXJjZV9hY2Nlc3MiOnsiYWNjb3VudCI6eyJyb2xlcyI6WyJtYW5hZ2UtYWNjb3VudCIsIm1hbmFnZS1hY2NvdW50LWxpbmtzIiwidmlldy1wcm9maWxlIl19fSwic2NvcGUiOiJlbWFpbCBwcm9maWxlIiwiY2xpZW50SG9zdCI6IjE3Mi4yMi4wLjEiLCJlbWFpbF92ZXJpZmllZCI6ZmFsc2UsInByZWZlcnJlZF91c2VybmFtZSI6InNlcnZpY2UtYWNjb3VudC1jb2xsZWN0b3IiLCJjbGllbnRBZGRyZXNzIjoiMTcyLjIyLjAuMSIsImNsaWVudF9pZCI6ImNvbGxlY3RvciJ9.ORoATUron2U2KPN9iO2fW0VjERHk-NsW3RYQFMtkmFZdq31Tpy22tHK5r0wQPo5hQvmJIz-KyOEkKPKkG5-0Z8B-GB457GGN3m3FyQesaXQigAHC03hCkvwmKoPuT4_Hx35HSo-EJqgzBNlWYpj3iIJ92UWAbUJXLDLoTKQOxF4X9sFpexQHHnA_VMZ-muREbwdL6AWNegL2lkje0FEluMfGv1tnwZbHnyue4w3r198bcfAipNQkU8fjob9FxVvkVv2vTb2MFRHhV1JeRuxIadtJESzj7_S5Sga6k5Vh9v0Grr5FTVYSvgJHjz-7D1qh1saUYXh4HougHeEbJkbLUA";

builder.Logging.AddOpenTelemetry(c => 
{
    c.AddOtlpExporter(o =>
    {
        o.Endpoint = new Uri("http://localhost:4318/v1/logs"); // Default ไม่ต้องใส่ก็ได้,
        o.Protocol = OtlpExportProtocol.HttpProtobuf;
        o.HttpClientFactory = () =>
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            return client;
        };
    });
});

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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
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
            metricReaderOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 10000;
            otlpExporterOptions.HttpClientFactory = () =>
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
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
