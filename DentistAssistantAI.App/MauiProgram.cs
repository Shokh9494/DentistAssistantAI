using CommunityToolkit.Maui;
using DentistAssistantAI.App.Services;
using DentistAssistantAI.App.Security;
using DentistAssistantAI.App.ViewModels;
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

            builder.Services.AddHttpClient<IOpenAIService, OpenAIService>(httpClient =>
                new OpenAIService(httpClient, ApiKeys.OpenAIKey));

            builder.Services.AddSingleton<IMediaPickerService, MauiMediaPickerService>();
            builder.Services.AddSingleton<IMediaFileCache, MediaFileCache>();
            builder.Services.AddSingleton<AIManager>();
            builder.Services.AddSingleton<MainPageViewModel>();
            builder.Services.AddSingleton<MainPage>();

            return builder.Build();
        }
    }
}
