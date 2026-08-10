using Atmos.Api.ExceptionHandling;
using Atmos.Application;
using Atmos.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";
    options.UseUtcTimestamp = true;
    options.IncludeScopes = true;
});
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHealthChecks();
builder.Services.AddResponseCompression(options => options.EnableForHttps = true);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseResponseCompression();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapOpenApi();

app.Run();

public partial class Program;
