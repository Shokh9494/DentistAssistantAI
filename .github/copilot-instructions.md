# Copilot Instructions

## General Guidelines
- When breaking work into tasks for this repository, structure each task with acceptance criteria, steps to solve, and files to review.

## Stack and scope
- This repository uses `.NET 10` and `.NET MAUI`; do not introduce `Xamarin.Forms` guidance or APIs.
- Keep changes incremental. Preserve the current project split instead of collapsing layers.
- Use `MVVM` for UI work and keep view logic out of code-behind unless it is purely view behavior.

## Solution structure
- `DentistAssistantAI.App`: MAUI UI, DI setup, view models, XAML views, chat presentation models, and platform-specific files.
- `DentistAssistantAI.Application`: application orchestration; currently `AIManager` is the thin use-case layer over `IOpenAIService`.
- `DentistAssistantAI.Core`: stable contracts and shared models/config such as `IOpenAIService`, `AIResult`, and `DentalAIConfig`.
- `DentistAssistantAI.Infrastructure`: external integration code; `Services/OpenAIService.cs` builds OpenAI request payloads and performs HTTP calls.

## Current runtime flow
- App startup is wired in `DentistAssistantAI.App/MauiProgram.cs`.
- `MainPageViewModel` depends on `AIManager`; `AIManager` depends on `IOpenAIService`; `OpenAIService` is the concrete HTTP implementation.
- `DentalAIConfig` centralizes AI model names and the long system/image prompts. Reuse it instead of duplicating prompt text.
- `MainPageViewModel.SendMessageAsync()` adds the user message to `Messages`, calls `AIManager.AskDentistAI(...)`, then appends the AI response.
- Image flows go through `MediaPicker`, then `CopyToLocalCacheAsync()` copies the file into `FileSystem.CacheDirectory` before sending. Keep this pattern for cross-platform safety.

## MAUI and MVVM conventions used here
- View models use `CommunityToolkit.Mvvm` source generators (`[ObservableProperty]`, `[RelayCommand]`, `[NotifyCanExecuteChangedFor]`). Follow that pattern instead of manual `INotifyPropertyChanged`.
- The main chat UI is data-driven: `MainPage.xaml` binds to `ObservableCollection<ChatMessageItem>` and uses `Templates/ChatMessageTemplateSelector.cs` to choose user vs AI bubble templates.
- `MainPage.xaml.cs` is intentionally small and only handles view behavior (`CollectionView` auto-scroll on new messages).
- `App.xaml.cs` resolves and returns a DI-created `MainPage` window instead of constructing pages manually.

## Configuration and secrets
- The MAUI app project includes `appsettings.json` as a `MauiAsset`, but startup does not currently load it.
- The current OpenAI key is wired from `DentistAssistantAI.App/Security/ApiKeys.cs` into `OpenAIService` in `MauiProgram.cs`.
- Do not introduce more hardcoded secrets. If you touch configuration, keep the change minimal and consistent with MAUI startup/DI.

## Build and validation
- Primary project to build is `DentistAssistantAI.App/DentistAssistantAI.App.csproj`.
- On Windows, use the Windows target framework from the MAUI project when you need a desktop build: `net10.0-windows10.0.19041.0`.
- The app project also targets `net10.0-android`, `net10.0-ios`, and `net10.0-maccatalyst`; keep target-specific changes inside `Platforms/`.
- No automated test project is currently present in the open solution, so validate changes with a build plus a quick manual smoke test of chat send, image pick/capture, and AI response rendering.

## Change guidance for agents
- Prefer editing existing services and view models over adding new abstraction layers.
- Keep user-facing text and AI prompt behavior consistent with `DentalAIConfig` and the seeded welcome message in `MainPageViewModel`.
- Reuse existing DI registrations and service boundaries before adding new dependencies.
- When changing the chat UI, preserve compiled bindings (`x:DataType`) and the current `ChatMessageItem`-based data flow.
