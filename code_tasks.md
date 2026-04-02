# DentistAssistantAI — Task Board

> **Agent rule:** Every task must pass through Backlog → In Progress → Done.
> Update this file at the **start** (move to In Progress) and **end** (move to Done) of every task.
> See `Agents.md §15` for the mandatory workflow.

---

## ✅ Done

### TASK-001 — Initial Project Setup
**Completed:** 2026-04-02
**Description:** Bootstrap 4-layer Clean Architecture solution (.NET 10 / MAUI).
**Deliverables:**
- `DentistAssistantAI.Core` — `IOpenAIService`, `AIResult`, `DentalAIConfig`
- `DentistAssistantAI.Application` — `AIManager`
- `DentistAssistantAI.Infrastructure` — `OpenAIService` (text + vision)
- `DentistAssistantAI.App` — MAUI chat UI, `MainPageViewModel`, `ChatMessageItem`
- `MauiProgram.cs` — DI wiring, `UseMauiCommunityToolkit`
- Platform targets: Android, iOS, macOS Catalyst, Windows

---

### TASK-002 — Media Picker & Image Flow
**Completed:** 2026-04-02
**Description:** Allow the clinician to pick or capture a photo and attach it to a message.
**Deliverables:**
- `IMediaPickerService` + `MauiMediaPickerService`
- `IMediaFileCache` + `MediaFileCache` (copy to `FileSystem.CacheDirectory`)
- `PendingImagePath` / `HasPendingImage` / `PendingImageSource` in view model
- `PickPhotoCommand` / `TakePhotoCommand` wired to toolbar
- Image thumbnail shown in input area before send
**Files changed:** `MainPageViewModel.cs`, `MainPage.xaml`, `MauiProgram.cs`

---

### TASK-003 — Unit Test Suite (all 3 projects)
**Completed:** 2026-04-02
**Description:** Add xUnit tests for all testable logic.
**Deliverables:**
- `DentistAssistantAI.App.Tests` — `MainPageViewModelTests`, `ChatMessageItemTests`, `ChatMessageTemplateSelectorTests`
- `DentistAssistantAI.Application.Tests` — `AIManagerTests`
- `DentistAssistantAI.Infrastructure.Tests` — `OpenAIServiceTests`, `StubHttpMessageHandler`
- All test projects target `net10.0` (not MAUI TFM)

---

### TASK-004 — README.md & Agents.md
**Completed:** 2026-04-02
**Description:** Write complete project documentation and agent context files.
**Deliverables:**
- `README.md` — features, architecture, stack, getting started, testing, contributing
- `Agents.md` — §1–14 agent context (layer rules, DI, MVVM, testing, task template)

---

### TASK-005 — Mandatory Planning Files Rule
**Completed:** 2026-04-02
**Description:** Add `§15` to `Agents.md` enforcing `code_plan.md` and `code_tasks.md` workflow.
**Deliverables:**
- `Agents.md §15` — rule, workflow order, file ownership table
- `code_plan.md` — initial architectural plan
- `code_tasks.md` — initial task board (this file)

---

## 🔄 In Progress

_No tasks currently in progress._

---

## 📋 Backlog

### TASK-006 — Load appsettings.json at Startup
**Priority:** Medium
**Description:** Wire `appsettings.json` (already a `MauiAsset`) into `IConfiguration` at startup in `MauiProgram.cs`.
**Acceptance Criteria:**
- [ ] `builder.Configuration` reads `appsettings.json` at runtime on all platforms
- [ ] `OpenAI:ApiKey` can be read from config (as an alternative to `ApiKeys.cs`)
- [ ] Existing `ApiKeys.cs` path still works as fallback
**Affected layers:** App
**Files to change:** `MauiProgram.cs`, `appsettings.json`

---

### TASK-007 — Secure API Key Storage
**Priority:** High
**Description:** Remove the hardcoded API key from `ApiKeys.cs` and store it securely.
**Acceptance Criteria:**
- [ ] API key is read from `SecureStorage` or environment variable, not source code
- [ ] First-run flow prompts the user to enter the key if absent
- [ ] `ApiKeys.cs` is deleted or replaced with a service abstraction
**Affected layers:** App, Infrastructure
**Files to change:** `ApiKeys.cs`, `MauiProgram.cs`, `OpenAIService.cs`

---

### TASK-008 — Streaming AI Responses
**Priority:** Medium
**Description:** Use OpenAI streaming (server-sent events) so the AI response appears word-by-word.
**Acceptance Criteria:**
- [ ] `IOpenAIService` supports a streaming overload returning `IAsyncEnumerable<string>`
- [ ] `AIManager` passes the stream up to the view model
- [ ] `MainPageViewModel` appends tokens to the last `ChatMessageItem` in real time
- [ ] Fallback to non-streaming on error
**Affected layers:** Core, Infrastructure, Application, App
**Files to change:** `IOpenAIService.cs`, `OpenAIService.cs`, `AIManager.cs`, `MainPageViewModel.cs`

---

### TASK-009 — Conversation History
**Priority:** Medium
**Description:** Send the full conversation thread with each request so the AI has context.
**Acceptance Criteria:**
- [ ] `OpenAIService.SendAsync` accepts a `IReadOnlyList<ChatMessage>` history parameter
- [ ] `AIManager` maintains conversation state and passes it on each call
- [ ] History is trimmed/truncated to stay within model token limits
**Affected layers:** Core, Infrastructure, Application, App
**Files to change:** `IOpenAIService.cs`, `OpenAIService.cs`, `AIManager.cs`, `MainPageViewModel.cs`

---

### TASK-010 — Image Compression Before Upload
**Priority:** Low
**Description:** Compress images to JPEG at ≤1280px longest side before base64 encoding to reduce token usage and cost.
**Acceptance Criteria:**
- [ ] Image is resized in `OpenAIService` or a new `IImageCompressor` service before encoding
- [ ] Original file is not modified
- [ ] Compressed bytes are used only for the API request
**Affected layers:** Infrastructure (or App/Services)
**Files to change:** `OpenAIService.cs` or new `ImageCompressor.cs`

---

### TASK-011 — Dark Mode Support
**Priority:** Low
**Description:** Add a dark colour scheme that respects the OS theme setting.
**Acceptance Criteria:**
- [ ] `App.xaml` defines `AppThemeBinding`-based resource dictionary for light/dark
- [ ] Chat bubbles, background, and input bar all adapt to the OS theme
- [ ] No hardcoded hex colours remain in `MainPage.xaml`
**Affected layers:** App
**Files to change:** `App.xaml`, `MainPage.xaml`, new `Styles/Colors.xaml`

---

### TASK-012 — CI/CD Pipeline (GitHub Actions)
**Priority:** Medium
**Description:** Add a GitHub Actions workflow that builds and runs all tests on push.
**Acceptance Criteria:**
- [ ] Workflow triggers on push to `master` and all pull requests
- [ ] `dotnet build` succeeds for `net10.0-windows10.0.19041.0`
- [ ] `dotnet test` runs all three test projects and reports results
- [ ] API key is injected from a GitHub secret (never from source)
**Affected layers:** DevOps
**Files to change:** `.github/workflows/ci.yml` (new)

---

### TASK-013 — Localisation of UI Strings
**Priority:** Low
**Description:** Externalise all user-visible UI strings into `.resx` resource files for Uzbek, Russian, and English.
**Acceptance Criteria:**
- [ ] All hardcoded strings in `MainPage.xaml` and `MainPageViewModel.cs` moved to resources
- [ ] Three locales provided: `uz`, `ru`, `en`
- [ ] Language switches with OS locale automatically
**Affected layers:** App
**Files to change:** New `Resources/Strings/`, `MainPage.xaml`, `MainPageViewModel.cs`
