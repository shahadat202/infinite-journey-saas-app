using InfiniteJourney.Application;
using InfiniteJourney.Application.Common.Interfaces;
using InfiniteJourney.Infrustructure;
using InfiniteJourney.Infrustructure.Persistence;
using InfiniteJourney.Infrustructure.Storage;
using InfiniteJourney.Web.Middleware;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize all enums as their string names (e.g. "Active" not 1).
        // This keeps the API contract human-readable and avoids frontend
        // guessing integer values when enum members are reordered.
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "InfiniteJourney API";
    config.Version = "v1";
});

builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:4200", "http://hope.localhost:4200", "http://relief.localhost:4200"])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

var storageRoot = app.Services.GetRequiredService<IFileStorageService>() is LocalFileStorageService local
    ? local.RootPath
    : Path.GetFullPath(builder.Configuration["Storage:RootPath"] ?? "UPLOADED_DATA");

Directory.CreateDirectory(storageRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storageRoot),
    RequestPath = "/uploads"
});

app.UseExceptionHandler();

await DatabaseInitializer.InitializeAsync(app.Services, app.Environment.IsDevelopment());

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseCors("Frontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
