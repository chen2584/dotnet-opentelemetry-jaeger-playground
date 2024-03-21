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

builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName))
        .AddConsoleExporter();
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
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJHVXlFWVVBVTZ4S1RzdW5VY3BCRjlpUW5jc240UVYyUVI1M2VjUUNTeUZzIn0.eyJleHAiOjE3MTEwODMzODcsImlhdCI6MTcxMTA0NzM4NywianRpIjoiOGVlNjA1MTgtNGIyZS00MmEwLTlmYTUtMjc3NWI4MjkyMDEwIiwiaXNzIjoiaHR0cDovL2tleWNsb2FrOjgwODAvcmVhbG1zL29wZW50ZWxlbWV0cnkiLCJhdWQiOlsiY29sbGVjdG9yIiwiYWNjb3VudCJdLCJzdWIiOiI5MWY0OWNiNy1iMWMyLTRkZjEtYWNjOC0yMjYyMzFkOTI5ZjYiLCJ0eXAiOiJCZWFyZXIiLCJhenAiOiJjb2xsZWN0b3IiLCJhY3IiOiIxIiwiYWxsb3dlZC1vcmlnaW5zIjpbIi8qIl0sInJlYWxtX2FjY2VzcyI6eyJyb2xlcyI6WyJvZmZsaW5lX2FjY2VzcyIsInVtYV9hdXRob3JpemF0aW9uIiwiZGVmYXVsdC1yb2xlcy1vcGVudGVsZW1ldHJ5Il19LCJyZXNvdXJjZV9hY2Nlc3MiOnsiYWNjb3VudCI6eyJyb2xlcyI6WyJtYW5hZ2UtYWNjb3VudCIsIm1hbmFnZS1hY2NvdW50LWxpbmtzIiwidmlldy1wcm9maWxlIl19fSwic2NvcGUiOiJlbWFpbCBwcm9maWxlIiwiY2xpZW50SG9zdCI6IjE3Mi4yNy4wLjEiLCJlbWFpbF92ZXJpZmllZCI6ZmFsc2UsInByZWZlcnJlZF91c2VybmFtZSI6InNlcnZpY2UtYWNjb3VudC1jb2xsZWN0b3IiLCJjbGllbnRBZGRyZXNzIjoiMTcyLjI3LjAuMSIsImNsaWVudF9pZCI6ImNvbGxlY3RvciJ9.3vHUizpLQVbkCJ57C_J0_QaIWn4s070auWoYbeN42itl38zf0ivwSqfMveVonlQI5EOkvwp_tc6I4FqVhdKp1a0f8KKr3zlR71OcyVrsmzisCSjvZf7Y0nhPIMuuHTM9mmdzZz5tZG0jNK2Ni_8O4CdO37qoxoiXZJtIosSEpkAsE_i-mAFTnNxtPgjaf0HYJVimHdcvrFfBcwKIZaT0hl-AFXieMLg7aTnoMn5e0E0x_Wndjm1X1bue6FSpEgGyx4GO0BMwlnHIgpo9UIyM5fDMdwmm7cQ6nR69yLlkgP7U1sX1FBEm_Ra1LImpqJbclmC5LqmV-ZjTUChsoi6uTw");
                return client;
            };
        }));
    // .WithMetrics(metrics => metrics
    //     .AddAspNetCoreInstrumentation()
    //     .AddHttpClientInstrumentation()
    //     .AddOtlpExporter(o =>
    //     {
    //         o.Endpoint = new Uri("http://localhost:4317"); // Default ไม่ต้องใส่ก็ได้
    //     }));

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
