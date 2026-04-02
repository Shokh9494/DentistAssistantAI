# DentistAssistantAI — Agent Context

> This document is the authoritative reference for any AI coding agent working on this repository.
> Read it fully before making any changes. You are expected to act as a **senior .NET / MAUI developer**
> who cares deeply about Clean Architecture, SOLID principles, testability, and idiomatic C#.

---

## 1. Project Identity

**DentistAssistantAI** is a cross-platform clinical AI chat application for dental professionals.
It wraps the OpenAI Chat Completions API in a clean, layered .NET 10 / .NET MAUI application.
The primary user flow is: the clinician types a question or attaches an X-ray/photo → the app sends it to OpenAI → a structured clinical response is rendered in a chat bubble.

---

## 2. Solution Layout

```
DentistAssistantAI/
├── DentistAssistantAI.Core/              ← stable contracts — no MAUI, no HTTP
│   ├── Interfaces/IOpenAIService.cs
│   ├── Models/AIResult.cs
│   └── Configuration/DentalAIConfig.cs
│
├── DentistAssistantAI.Application/       ← use-case orchestration; depends only on Core
│   └── Services/AIManager.cs
│
├── DentistAssistantAI.Infrastructure/    ← concrete HTTP integration; depends on Core
│   └── Services/OpenAIService.cs
│
└── DentistAssistantAI.App/               ← MAUI UI, DI root, view models, services
    ├── MauiProgram.cs
    ├── App.xaml / App.xaml.cs
    ├── MainPage.xaml / MainPage.xaml.cs
    ├── ViewModels/MainPageViewModel.cs
    ├── Models/ChatMessageItem.cs
    ├── Templates/ChatMessageTemplateSelector.cs
    ├── Services/
    │   ├── IMediaPickerService.cs
    │   ├── MauiMediaPickerService.cs
    │   ├── IMediaFileCache.cs
    │   └── MediaFileCache.cs
    └── Security/ApiKeys.cs

tests/
├── DentistAssistantAI.App.Tests/
│   ├── ViewModels/MainPageViewModelTests.cs
│   ├── Models/ChatMessageItemTests.cs
│   └── Templates/ChatMessageTemplateSelectorTests.cs
├── DentistAssistantAI.Application.Tests/
│   └── Services/AIManagerTests.cs
└── DentistAssistantAI.Infrastructure.Tests/
    ├── Services/OpenAIServiceTests.cs
    ├── Configuration/DentalAIConfigTests.cs
    └── TestDoubles/StubHttpMessageHandler.cs
```

---

## 3. Layer Rules (must never be violated)

| Layer | May depend on | Must NOT depend on |
|---|---|---|
| `Core` | nothing external | Application, Infrastructure, App, MAUI |
| `Application` | `Core` | Infrastructure, App, MAUI |
| `Infrastructure` | `Core` | Application, App, MAUI |
| `App` | `Core`, `Application`, `Infrastructure`, MAUI | — |

- Never move a contract (`IOpenAIService`, `AIResult`, `DentalAIConfig`) out of `Core`.
- Never put HTTP calls in `Application`.
- Never put business logic in `App` view models beyond UI coordination.

---

## 4. Runtime Wiring (DI)

All registrations live in `DentistAssistantAI.App/MauiProgram.cs`:

```csharp
builder.Services.AddHttpClient<IOpenAIService, OpenAIService>(httpClient =>
    new OpenAIService(httpClient, ApiKeys.OpenAIKey));

builder.Services.AddSingleton<IMediaPickerService, MauiMediaPickerService>();
builder.Services.AddSingleton<IMediaFileCache, MediaFileCache>();
builder.Services.AddSingleton<AIManager>();
builder.Services.AddSingleton<MainPageViewModel>();
builder.Services.AddSingleton<MainPage>();
```

- `App.xaml.cs` resolves `MainPage` from DI via constructor injection — do not `new` pages manually.
- When adding a new service, register it here. Prefer `AddSingleton` for stateless services and
  `AddTransient` only when the service holds per-request state.

---

## 5. Call Flow

```
User taps Send
  → MainPageViewModel.SendMessageAsync()
      → Messages.Add(userBubble)
      → AIManager.AskDentistAI(prompt, imagePath?)
          → IOpenAIService.SendAsync(prompt, imagePath?)
              → OpenAIService: encode image to base64 if present
              → POST https://api.openai.com/v1/chat/completions
              → parse response → AIResult
          → AIManager: unwrap AIResult, map errors
      → Messages.Add(aiBubble)
  → MainPage.xaml.cs: CollectionChanged → ScrollTo last item
```

Image path flow:
1. `IMediaPickerService.PickPhotoAsync()` or `CapturePhotoAsync()` returns a `FileResult`.
2. `IMediaFileCache.CopyToLocalCacheAsync(FileResult)` copies the file into `FileSystem.CacheDirectory` and returns the local path.
3. The local path is stored as `MainPageViewModel.PendingImagePath`.
4. On send, the path is passed all the way to `OpenAIService`, which reads and base64-encodes the bytes.

---

## 6. MVVM Conventions

Use **CommunityToolkit.Mvvm** source generators — never write `INotifyPropertyChanged` boilerplate by hand.

```csharp
// Observable property with change notifications
[ObservableProperty]
[NotifyCanExecuteChangedFor(nameof(SendMessageCommand))]
private string _userInput = string.Empty;

// Computed property raised via [NotifyPropertyChangedFor]
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(HasPendingImage))]
[NotifyPropertyChangedFor(nameof(PendingImageSource))]
private string? _pendingImagePath;

// Async command with conditional execution
[RelayCommand(CanExecute = nameof(CanSendMessage))]
private async Task SendMessageAsync() { ... }

private bool CanSendMessage() =>
    !IsBusy && (!string.IsNullOrWhiteSpace(UserInput) || HasPendingImage);
```

Rules:
- View models inherit `ObservableObject`.
- Commands are `[RelayCommand]` — async tasks are wrapped automatically.
- `IsBusy` guards all async commands; set it in `try/finally`.
- No UI code (navigation, dialogs) inside view models — use services or events instead.

---

## 7. XAML / View Conventions

- All bindings use **compiled bindings** (`x:DataType`) — always set `x:DataType` on each `DataTemplate`.
- `MainPage.xaml` uses a `CollectionView` bound to `ObservableCollection<ChatMessageItem>`.
- `ChatMessageTemplateSelector` picks between `AiMessageTemplate` and `UserMessageTemplate` based on `ChatMessageItem.IsFromUser`.
- `MainPage.xaml.cs` is intentionally minimal: only the `CollectionChanged` auto-scroll handler belongs there.
- Use `CommunityToolkit.Maui` behaviors and converters before writing custom ones.
- Do not use `Shell` navigation — the app has a single-page window model.

---

## 8. Key Types Reference

### `DentalAIConfig` (`Core/Configuration/DentalAIConfig.cs`)

Central home for all AI behaviour configuration. **Never duplicate prompt text.**

| Member | Purpose |
|---|---|
| `TextModel` | `"gpt-4o-mini"` — fast model for text-only queries |
| `VisionModel` | `"gpt-4o"` — vision model for image analysis |
| `SystemPrompt` | Full system instruction injected into every request |
| `ImageAnalysisInstruction` | Prefix injected into user message when an image is present |
| `DefaultImagePrompt` | Fallback prompt when the user sends an image with no text |

### `AIResult` (`Core/Models/AIResult.cs`)

```csharp
public class AIResult
{
    public bool IsSuccess { get; set; }
    public string? Content { get; set; }
    public string? Error { get; set; }
}
```

### `ChatMessageItem` (`App/Models/ChatMessageItem.cs`)

Immutable. Constructed once, never mutated.
Properties: `Text`, `IsFromUser`, `ImagePath`, `Timestamp`, `TimestampText`, `HasImage`, `HasText`, `ImageSource`.

### `IOpenAIService` (`Core/Interfaces/IOpenAIService.cs`)

```csharp
Task<AIResult> SendAsync(string prompt, string? imagePath = null);
```

The only contract between Application/Infrastructure and the AI backend.

---

## 9. Testing Strategy

### Principles

- Use **hand-written fakes** (`FakeOpenAIService`, `StubHttpMessageHandler`) — no Moq or NSubstitute.
- Tests are in `xUnit`. Every public method/behaviour that can be unit-tested should have a test.
- MAUI-specific types (`ImageSource`, `FileResult`) are avoided in test projects — abstract them behind interfaces.

### Test project targets

- `DentistAssistantAI.App.Tests` — targets `net10.0` (not MAUI TFM) to avoid MAUI initialisation in tests.
- `DentistAssistantAI.Application.Tests` — targets `net10.0`.
- `DentistAssistantAI.Infrastructure.Tests` — targets `net10.0`; uses `StubHttpMessageHandler` to intercept HTTP.

### Naming convention

```
MethodName_Condition_ExpectedOutcome
```

Example: `AskDentistAI_ReturnsContent_WhenServiceSucceeds`

### What to test per change

| What changed | What to test |
|---|---|
| `MainPageViewModel` | Command `CanExecute`, message list mutations, error handling |
| `AIManager` | Delegates to service, maps errors, handles null content |
| `OpenAIService` | Request payload shape, response parsing, HTTP error codes |
| `ChatMessageItem` | Property values, computed properties |
| `ChatMessageTemplateSelector` | Returns correct template for user vs AI items |

---

## 10. Adding a New Feature — Checklist

When given a task requiring a new feature, follow this order:

1. **Define or reuse the contract in `Core`** — add to `IOpenAIService` or create a new interface if needed.
2. **Implement in `Infrastructure` or `App/Services`** — depending on whether it involves external I/O or MAUI APIs.
3. **Orchestrate in `Application`** if the logic is multi-step; otherwise call the service directly from the view model.
4. **Wire in `MauiProgram.cs`** — register the new service with the correct lifetime.
5. **Expose via view model** — add `[ObservableProperty]` fields and `[RelayCommand]` methods.
6. **Bind in XAML** — keep `x:DataType` set; use compiled bindings.
7. **Write xUnit tests** — add a fake/stub for any new interface; test the new behaviour.
8. **Build** with `net10.0-windows10.0.19041.0` and smoke-test the new flow.

---

## 11. Common Mistakes to Avoid

| Wrong | Correct |
|---|---|
| Hardcoding prompt text in `OpenAIService` or `AIManager` | Use `DentalAIConfig` constants |
| `new MauiMediaPickerService()` inside a view model | Inject `IMediaPickerService` via constructor |
| Calling `File.ReadAllBytes` in the view model | Keep file I/O in `OpenAIService` or `IMediaFileCache` |
| Using `Xamarin.Forms` APIs | Use `.NET MAUI` equivalents only |
| Writing `PropertyChanged?.Invoke(...)` by hand | Use `[ObservableProperty]` source generator |
| Adding MAUI references to test projects | Test projects target `net10.0`; abstract MAUI types |
| Collapsing layers (HTTP logic in `Application`) | Respect the layer dependency rules in §3 |
| Duplicating the system prompt | Always reference `DentalAIConfig.SystemPrompt` |
| `new MainPage()` in `App.xaml.cs` | Resolve `MainPage` from DI constructor injection |

---

## 12. Build & Validation

Primary build command (Windows):

```powershell
dotnet build DentistAssistantAI.App/DentistAssistantAI.App.csproj -f net10.0-windows10.0.19041.0
```

Run all tests:

```powershell
dotnet test
```

Manual smoke test checklist before any PR:

- [ ] Send a text-only message → AI responds
- [ ] Pick a photo → thumbnail shown → send → AI responds with image analysis
- [ ] Take a photo (if device supports it) → same flow
- [ ] Send with empty input and no image → Send button disabled
- [ ] Simulate network error → friendly error message shown in chat

---

## 13. Configuration & Secrets

- API key: `DentistAssistantAI.App/Security/ApiKeys.cs` — **never commit a real key**.
- `appsettings.json` is bundled as a `MauiAsset` but is not currently loaded at startup.
  If you add config loading, do it in `MauiProgram.cs` and use MAUI's `IConfiguration` pattern.
- All AI model names and prompts are centralised in `DentalAIConfig` — do not scatter them across files.

---

## 14. Task Template

When an agent is given a task, structure the work as follows:

```markdown
### Task: <short name>

**Acceptance Criteria**
- [ ] ...

**Steps**
1. Read: <files to understand first>
2. Change: <specific files and what to change>
3. Test: <which test class / method to add or update>
4. Build: run dotnet build and dotnet test

**Files to Review**
- DentistAssistantAI.Core/...
- DentistAssistantAI.App/ViewModels/MainPageViewModel.cs
- MauiProgram.cs
```
