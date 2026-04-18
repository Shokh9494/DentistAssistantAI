using CommunityToolkit.Maui;
using DentistAssistantAI.App.Services;
using DentistAssistantAI.App.ViewModels;
using DentistAssistantAI.App.Views;
using DentistAssistantAI.Core.Interfaces;
using DentistAssistantAI.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

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

            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("DentistAssistantAI.App.appsettings.json");
            if (stream != null)
                builder.Configuration.AddJsonStream(stream);

#if DEBUG
            using var devStream = assembly.GetManifestResourceStream("DentistAssistantAI.App.appsettings.Development.json");
            if (devStream != null)
                builder.Configuration.AddJsonStream(devStream);
#endif

            var webApiBaseUrl = builder.Configuration["WebApi:BaseUrl"]
                ?? "http://localhost:5186";

            // AI service via WebAPI
            builder.Services.AddHttpClient<IAIManager, WebApiAIManager>(c =>
                c.BaseAddress = new Uri(webApiBaseUrl));

            // Media services
            builder.Services.AddSingleton<IMediaPickerService, MauiMediaPickerService>();
            builder.Services.AddSingleton<IMediaFileCache, MediaFileCache>();

            // Patient service
            builder.Services.AddSingleton<IPatientService, PatientService>();

            // Visit service
            builder.Services.AddSingleton<IVisitService, VisitService>();

            // View models
            builder.Services.AddSingleton<MainPageViewModel>();
            builder.Services.AddSingleton<TeacherPageViewModel>();
            builder.Services.AddSingleton<StudentPageViewModel>();
            builder.Services.AddSingleton<PatientsPageViewModel>();
            builder.Services.AddTransient<PatientDetailViewModel>();

            // Pages
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<TeacherPage>();
            builder.Services.AddSingleton<StudentPage>();
            builder.Services.AddSingleton<PatientsPage>();
            builder.Services.AddTransient<PatientDetailPage>();

            // Shell
            builder.Services.AddSingleton<AppShell>();

            return builder.Build();
        }
    }
}
