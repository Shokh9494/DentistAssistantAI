using CommunityToolkit.Maui;
using DentistAssistantAI.App.Services;
using DentistAssistantAI.App.Security;
using DentistAssistantAI.App.ViewModels;
using DentistAssistantAI.App.Views;
using DentistAssistantAI.Application.Services;
using DentistAssistantAI.Core.Interfaces;
using DentistAssistantAI.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace DentistAssistantAI.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // AI service
            builder.Services.AddHttpClient<IOpenAIService, OpenAIService>(httpClient =>
                new OpenAIService(httpClient, ApiKeys.OpenAIKey));

            // Media services
            builder.Services.AddSingleton<IMediaPickerService, MauiMediaPickerService>();
            builder.Services.AddSingleton<IMediaFileCache, MediaFileCache>();

            // Patient service
            builder.Services.AddSingleton<IPatientService, PatientService>();

            // Visit service
            builder.Services.AddSingleton<IVisitService, VisitService>();

            // Application layer
            builder.Services.AddSingleton<AIManager>();

            // View models
            builder.Services.AddSingleton<MainPageViewModel>();
            builder.Services.AddSingleton<PatientsPageViewModel>();
            builder.Services.AddTransient<PatientDetailViewModel>();

            // Pages
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<PatientsPage>();
            builder.Services.AddTransient<PatientDetailPage>();

            // Shell
            builder.Services.AddSingleton<AppShell>();

            return builder.Build();
        }
    }
}
