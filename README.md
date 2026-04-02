# 🦷 DentistAssistantAI

A cross-platform clinical AI assistant for dental professionals, built with **.NET 10** and **.NET MAUI**.  
It lets dentists send text questions and clinical images (X-rays, intraoral photos) to **OpenAI GPT-4o / GPT-4o-mini** and receive structured diagnostic support in real time — in Uzbek, Russian, or English.

---

## ✨ Features

| Feature | Description |
|---|---|
| 💬 AI Chat | Conversational interface backed by GPT-4o-mini for text queries |
| 📷 Vision Analysis | Send X-ray or clinical photos to GPT-4o for structured radiology/clinical findings |
| 📸 Camera Capture | Take a photo directly from the device camera |
| 🌐 Multilingual | Auto-detects the clinician's language (Uzbek / Russian / English) |
| 🦷 Clinical Knowledge | Covers diagnostics, endodontics, periodontics, orthodontics, oral surgery, pharmacology |
| 🕐 Timestamps | Each chat bubble carries an `HH:mm` timestamp |
| 📱 Cross-Platform | Targets Android, iOS, macOS Catalyst, and Windows |

---

## 🏗️ Architecture

The solution follows **Clean Architecture** with four clearly separated projects:

```
DentistAssistantAI/
├── DentistAssistantAI.Core/              # Stable contracts & shared models
│   ├── Interfaces/IOpenAIService.cs      # HTTP abstraction boundary
│   ├── Models/AIResult.cs                # Service return type
│   └── Configuration/DentalAIConfig.cs  # Model names + system/image prompts
│
├── DentistAssistantAI.Application/       # Use-case orchestration
│   └── Services/AIManager.cs            # Thin facade over IOpenAIService
│
├── DentistAssistantAI.Infrastructure/    # External integrations
│   └── Services/OpenAIService.cs        # HTTP calls to OpenAI Chat Completions API
│
└── DentistAssistantAI.App/               # MAUI UI + DI root
    ├── MauiProgram.cs                    # App startup & DI registrations
    ├── MainPage.xaml / .xaml.cs          # Chat page (view-only scroll logic)
    ├── ViewModels/MainPageViewModel.cs   # CommunityToolkit.Mvvm ObservableObject
    ├── Models/ChatMessageItem.cs         # Immutable chat bubble model
    ├── Templates/ChatMessageTemplateSelector.cs
    ├── Services/IMediaPickerService.cs
    ├── Services/MauiMediaPickerService.cs
    ├── Services/IMediaFileCache.cs
    ├── Services/MediaFileCache.cs
    └── Security/ApiKeys.cs              # OpenAI key (not committed)
```

### Dependency graph

```
MainPage → MainPageViewModel → AIManager → IOpenAIService → OpenAIService → OpenAI API
                              ↘ IMediaPickerService → MauiMediaPickerService
                              ↘ IMediaFileCache    → MediaFileCache
```

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| UI Framework | .NET MAUI (`net10.0-android` / `ios` / `maccatalyst` / `windows`) |
| MVVM | [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) — source generators |
| MAUI Extensions | [CommunityToolkit.Maui](https://github.com/CommunityToolkit/Maui) |
| AI Backend | OpenAI Chat Completions API (`gpt-4o`, `gpt-4o-mini`) |
| HTTP | `HttpClient` via `AddHttpClient<TInterface, TImpl>` |
| Serialization | `System.Text.Json` / `System.Text.Json.Nodes` |
| Unit Testing | xUnit + hand-written fakes (no mocking library) |
| Target Runtime | .NET 10 |

---

## 🚀 Getting Started

### Prerequisites

- Visual Studio 2022 17.8+ (or VS 2026) with the **.NET MAUI** workload installed
- .NET 10 SDK
- A valid **OpenAI API key**

### 1. Clone

```bash
git clone https://github.com/Shokh9494/DentistAssistantAI.git
cd DentistAssistantAI
```

### 2. Add your API key

Open `DentistAssistantAI.App/Security/ApiKeys.cs` and set your key:

```csharp
public static class ApiKeys
{
    public const string OpenAIKey = "sk-...your-key-here...";
}
```

> ⚠️ **Never commit a real API key.** Add `ApiKeys.cs` to `.gitignore` or use User Secrets / environment variables instead.

### 3. Build & Run

Select the target platform in Visual Studio (e.g., **Windows Machine**) and press **F5**.

For a CLI build targeting Windows:

```powershell
dotnet build DentistAssistantAI.App/DentistAssistantAI.App.csproj `
  -f net10.0-windows10.0.19041.0
```

---

## 🧪 Testing

Three xUnit test projects live under `tests/`:

| Project | Covers |
|---|---|
| `DentistAssistantAI.App.Tests` | `MainPageViewModel`, `ChatMessageItem`, `ChatMessageTemplateSelector` |
| `DentistAssistantAI.Application.Tests` | `AIManager` |
| `DentistAssistantAI.Infrastructure.Tests` | `OpenAIService` (uses `StubHttpMessageHandler`) |

Run all tests:

```powershell
dotnet test
```

---

## 📁 Full Solution Layout

```
DentistAssistantAI/
├── DentistAssistantAI.Core/
├── DentistAssistantAI.Application/
├── DentistAssistantAI.Infrastructure/
├── DentistAssistantAI.App/
│   ├── Platforms/
│   │   ├── Android/
│   │   ├── iOS/
│   │   ├── MacCatalyst/
│   │   └── Windows/
│   └── Resources/
└── tests/
    ├── DentistAssistantAI.App.Tests/
    ├── DentistAssistantAI.Application.Tests/
    └── DentistAssistantAI.Infrastructure.Tests/
```

---

## ⚙️ Key Configuration Points

| Item | Location |
|---|---|
| AI model names | `DentalAIConfig.TextModel` / `DentalAIConfig.VisionModel` |
| System prompt | `DentalAIConfig.SystemPrompt` |
| Image analysis prefix | `DentalAIConfig.ImageAnalysisInstruction` |
| Default image prompt | `DentalAIConfig.DefaultImagePrompt` |
| API key wiring | `MauiProgram.cs` → `ApiKeys.OpenAIKey` |
| DI registrations | `MauiProgram.cs` — `AddHttpClient`, `AddSingleton` |

---

## 🤝 Contributing

1. Fork → branch → commit → pull request.
2. Keep changes layered — do **not** collapse the Core / Application / Infrastructure / App split.
3. Add or update xUnit tests for any changed service or view model logic.
4. Build with `net10.0-windows10.0.19041.0` before opening a PR and verify the chat-send, image-pick, camera-capture, and AI-response flows manually.

---

## 📄 License

MIT
