using DentistAssistantAI.Application.Services;
using DentistAssistantAI.Core.Interfaces;
using DentistAssistantAI.Infrastructure.Services;
using DentistAssistantAI.WebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

var apiKey = builder.Configuration["OpenAI:ApiKey"]
    ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured.");

builder.Services.AddHttpClient<IOpenAIService, OpenAIService>(httpClient =>
    new OpenAIService(httpClient, apiKey));

builder.Services.AddSingleton<IAIManager, AIManager>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors();
app.UseStaticFiles();

app.MapChatEndpoints();
app.MapTeacherEndpoints();
app.MapStudentEndpoints();
app.MapClinicalCaseEndpoints();

app.MapFallbackToFile("index.html");

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
