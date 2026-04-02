# DentistAssistantAI — Code Plan

> **Agent rule:** Update this file before and after every feature, refactor, or fix.
> See `Agents.md §15` for the mandatory workflow.

---

## 1. Project Goal

Build a cross-platform **.NET 10 / .NET MAUI** clinical AI assistant for dental professionals.
The app sends text and clinical images to the **OpenAI Chat Completions API** and renders structured
diagnostic responses in a native mobile/desktop chat UI. The language of the response automatically
matches the language the clinician writes in (Uzbek, Russian, or English).

---

## 2. Architectural Decisions

### 2.1 Why Clean Architecture (4 layers)?

| Decision | Rationale |
|---|---|
| Separate `Core` layer | Contracts and models must be stable; no framework dependency leaks |
| Separate `Application` layer | Use-case orchestration stays testable without HTTP or MAUI |
| Separate `Infrastructure` layer | OpenAI HTTP code is swappable and testable with a stub handler |
| MAUI only in `App` layer | Platform APIs (`MediaPicker`, `FileSystem`) cannot be used in unit tests |

### 2.2 Why CommunityToolkit.Mvvm?

- Source generators eliminate `INotifyPropertyChanged` boilerplate.
- `[RelayCommand]` auto-generates `ICommand` + async wrappers.
- `[ObservableProperty]` + `[NotifyCanExecuteChangedFor]` keeps `CanExecute` always in sync.
- Standard across the MAUI ecosystem; well-maintained by Microsoft.

### 2.3 Why hand-written fakes over Moq/NSubstitute?

- No additional NuGet dependencies in test projects.
- Fakes are explicit and readable; behaviour is obvious without magic.
- Test projects target `net10.0` (not MAUI TFM) to avoid MAUI initialisation overhead.

### 2.4 Why two AI models?

| Model | Usage | Reason |
|---|---|---|
| `gpt-4o-mini` | Text-only queries | Fast and cost-efficient; no vision needed |
| `gpt-4o` | Image/X-ray analysis | Best vision accuracy for medical imaging |

### 2.5 Image caching strategy

`MauiMediaPickerService` returns a `FileResult` from the platform picker.
`MediaFileCache.CopyToLocalCacheAsync()` copies it into `FileSystem.CacheDirectory` before use.
This is required for cross-platform safety — the original `FileResult` path is not guaranteed
to be readable across all platforms after the picker closes.

---

## 3. Layer Dependency Map

```
App  →  Application  →  Core  ←  Infrastructure
         ↓                         ↓
    AIManager               OpenAIService
         ↓
  MainPageViewModel
```

Strict rule: arrows only point inward. `Core` has zero outward dependencies.

---

## 4. Current Implementation Status

| Component | Status | Notes |
|---|---|---|
| `DentalAIConfig` | ✅ Done | System prompt, model names, image instruction |
| `IOpenAIService` / `AIResult` | ✅ Done | Core contracts stable |
| `OpenAIService` | ✅ Done | Text + vision requests, base64 encoding, error handling |
| `AIManager` | ✅ Done | Thin facade, error mapping |
| `MainPageViewModel` | ✅ Done | Send, pick photo, take photo, busy guard, CanExecute |
| `ChatMessageItem` | ✅ Done | Immutable, timestamp, image support |
| `ChatMessageTemplateSelector` | ✅ Done | User vs AI bubble selection |
| `MainPage.xaml` | ✅ Done | Compiled bindings, CollectionView, toolbar |
| `MainPage.xaml.cs` | ✅ Done | Auto-scroll only |
| `MauiProgram.cs` | ✅ Done | DI wiring, `UseMauiCommunityToolkit` |
| `App.xaml.cs` | ✅ Done | DI-resolved `MainPage` via `CreateWindow` |
| `IMediaPickerService` + impl | ✅ Done | Abstracted for testability |
| `IMediaFileCache` + impl | ✅ Done | Cross-platform cache copy |
| Unit tests — App | ✅ Done | ViewModel, ChatMessageItem, TemplateSelector |
| Unit tests — Application | ✅ Done | AIManager |
| Unit tests — Infrastructure | ✅ Done | OpenAIService, StubHttpMessageHandler |
| `appsettings.json` loading | ⏳ Not wired | File exists as MauiAsset but not loaded at startup |
| API key management | ⚠️ Hardcoded | `ApiKeys.cs` — needs env var or secure storage |

---

## 5. Key File Locations

| Concern | File |
|---|---|
| AI prompts + model names | `DentistAssistantAI.Core/Configuration/DentalAIConfig.cs` |
| OpenAI HTTP integration | `DentistAssistantAI.Infrastructure/Services/OpenAIService.cs` |
| Use-case orchestration | `DentistAssistantAI.Application/Services/AIManager.cs` |
| UI state + commands | `DentistAssistantAI.App/ViewModels/MainPageViewModel.cs` |
| DI registrations + startup | `DentistAssistantAI.App/MauiProgram.cs` |
| Chat bubble model | `DentistAssistantAI.App/Models/ChatMessageItem.cs` |
| Template selector | `DentistAssistantAI.App/Templates/ChatMessageTemplateSelector.cs` |
| API key | `DentistAssistantAI.App/Security/ApiKeys.cs` |

---

## 6. Design Constraints

1. **No Xamarin.Forms APIs** — `.NET MAUI` only.
2. **No Shell navigation** — single-page window model.
3. **No mocking frameworks** — hand-written fakes only.
4. **Compiled XAML bindings** — `x:DataType` required on every `DataTemplate`.
5. **`DentalAIConfig` is the single source of truth** for all prompt text and model names.
6. **Test projects target `net10.0`**, not the MAUI TFM, to keep tests fast and dependency-free.

---

## 7. Planned Improvements (Backlog)

| # | Improvement | Affected layers |
|---|---|---|
| 1 | Load `appsettings.json` at startup via `IConfiguration` | App |
| 2 | Move API key to secure storage / environment variable | App, Infrastructure |
| 3 | Streaming response support (server-sent events) | Core, Infrastructure, Application, App |
| 4 | Conversation history — send full message thread per request | Core, Infrastructure, Application |
| 5 | Export chat as PDF | App |
| 6 | Dark mode support | App (XAML resources) |
| 7 | Localisation of UI strings (Uzbek / Russian / English) | App |
| 8 | Offline error state with retry | Infrastructure, App |
| 9 | Image compression before base64 encoding | Infrastructure |
| 10 | CI/CD pipeline (GitHub Actions) | DevOps |

---

## 8. Change Log

| Date | Change | Author |
|---|---|---|
| 2026-04-02 | Initial project setup — 4-layer Clean Architecture, MAUI chat UI | Initial |
| 2026-04-02 | README.md and Agents.md populated | Agent |
| 2026-04-02 | code_plan.md and code_tasks.md created | Agent |
