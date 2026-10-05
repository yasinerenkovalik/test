using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Test.API.Extensions;
using Test.API.Middleware;
using Test.API.Services;
using Test.Application;
using Test.Application.Common.Interfaces;
using Test.Application.Common.Models;
using Test.Infrastructure;
using Test.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Integration testler kendi (InMemory) DbContext'ini kaydeder.
if (!builder.Environment.IsEnvironment("Testing"))
    builder.Services.AddPersistence(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddApiAuthentication(builder.Configuration);

builder.Services
    .AddControllers(options =>
        // Zorunlu alan kontrolü FluentValidation'da; MVC'nin örtük [Required]'ı kapalı.
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(options =>
        // Model binding hataları (bozuk JSON, geçersiz enum vb.) da ApiResponse formatında dönsün.
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value is { Errors.Count: > 0 })
                .SelectMany(e => e.Value!.Errors.Select(err => new ApiError(
                    e.Key, string.IsNullOrWhiteSpace(err.ErrorMessage) ? "The value is invalid." : err.ErrorMessage)))
                .ToList();

            var body = ApiResponse<object>.Fail("Validation failed.", StatusCodes.Status400BadRequest,
                context.HttpContext.TraceIdentifier, errors);
            return new ObjectResult(body) { StatusCode = StatusCodes.Status400BadRequest };
        });

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

var migrateOnStartup = app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Database:MigrateOnStartup");
if (migrateOnStartup && !app.Environment.IsEnvironment("Testing"))
{
    // Seed sadece Development'ta çalışır.
    await DatabaseInitializer.InitializeAsync(app.Services, seed: app.Environment.IsDevelopment());
}

app.UseSerilogRequestLogging();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// WebApplicationFactory<Program> için.
public partial class Program { }
