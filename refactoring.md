# Unit Test Creation Plan for Existing Logic

## Goal
Create a focused unit-test strategy for the current solution without changing app behavior. The first target is the logic that already exists in the `Core`, `Application`, `Infrastructure`, and testable parts of the `App` layer.

## Research Summary

### Current projects
- `DentistAssistantAI.Core` (`net10.0`)
- `DentistAssistantAI.Application` (`net10.0`)
- `DentistAssistantAI.Infrastructure` (`net10.0`)
- `DentistAssistantAI.App` (`.NET MAUI`, multi-targeted including `net10.0-windows10.0.19041.0`)

### Important existing logic found
- `DentistAssistantAI.Application/Services/AIManager.cs`
  - Thin orchestration layer over `IOpenAIService`
  - Contains user-visible error mapping logic
- `DentistAssistantAI.Infrastructure/Services/OpenAIService.cs`
  - Builds text and vision request payloads
  - Reads image files
  - Calls OpenAI HTTP API
  - Parses API responses and maps transport/runtime failures
- `DentistAssistantAI.App/ViewModels/MainPageViewModel.cs`
  - Contains chat flow logic, command state, message list mutations, and image prompt fallback
- `DentistAssistantAI.App/Models/ChatMessageItem.cs`
  - Small presentation model with derived properties
- `DentistAssistantAI.App/Templates/ChatMessageTemplateSelector.cs`
  - Simple UI template selection logic
- `DentistAssistantAI.Core/Configuration/DentalAIConfig.cs`
  - Central source of models and prompts

### Testability assessment

| Component | Logic value | Testability now | Notes |
|---|---:|---:|---|
| `AIManager` | High | High | Pure application logic with mockable dependency (`IOpenAIService`) |
| `OpenAIService` | Very high | High | Can be tested with fake `HttpMessageHandler` and temp files |
| `MainPageViewModel` | High | Medium | Send flow is testable; media/file static APIs make photo commands harder |
| `ChatMessageItem` | Low/Medium | Medium | Simple derived properties; timestamp is time-coupled |
| `ChatMessageTemplateSelector` | Low | High | Straightforward selector behavior |
| `MauiProgram` / `App.xaml.cs` / `MainPage.xaml.cs` | Low for unit tests | Low | Better covered by smoke/integration/manual checks |

## Recommended Test Project Structure

### Phase 1: Add test projects for pure .NET layers first
1. `DentistAssistantAI.Application.Tests`
   - Target: `net10.0`
   - References:
     - `DentistAssistantAI.Application`
     - `DentistAssistantAI.Core`
2. `DentistAssistantAI.Infrastructure.Tests`
   - Target: `net10.0`
   - References:
     - `DentistAssistantAI.Infrastructure`
     - `DentistAssistantAI.Core`
3. `DentistAssistantAI.App.Tests` (second wave)
   - Preferred target: `net10.0-windows10.0.19041.0`
   - Reference:
     - `DentistAssistantAI.App`
   - Use only after deciding whether to keep MAUI types directly in unit tests or extract seams for media/file access.

### Suggested packages for each test project
Keep package count minimal.
- `Microsoft.NET.Test.Sdk`
- `xunit`
- `xunit.runner.visualstudio`
- `coverlet.collector`

### Mocking approach
Prefer simple fakes/stubs first.
- For `IOpenAIService`: handwritten test fake or lightweight mock
- For `HttpClient`: custom `HttpMessageHandler` stub
- Avoid adding extra mocking libraries unless handwritten fakes become noisy

## Detailed Test Plan by Component

## 1. `AIManager` tests
File: `DentistAssistantAI.Application/Services/AIManager.cs`

### Why this is first
This class contains the cleanest business-facing logic and has almost no framework coupling.

### Behaviors to test
1. Returns response content when `IOpenAIService.SendAsync(...)` succeeds
2. Returns empty string when service succeeds with `Content = null`
3. Returns quota-specific message when error text contains `quota`
4. Returns generic unavailability message for any other failure
5. Forwards both `question` and `imagePath` to the dependency unchanged

### Example test cases
- `AskDentistAI_ReturnsContent_WhenServiceSucceeds`
- `AskDentistAI_ReturnsEmptyString_WhenContentIsNull`
- `AskDentistAI_ReturnsQuotaMessage_WhenErrorContainsQuota`
- `AskDentistAI_ReturnsGenericMessage_WhenErrorDoesNotContainQuota`
- `AskDentistAI_ForwardsQuestionAndImagePath_ToOpenAIService`

### Risks / notes
- Current quota detection is case-sensitive (`Contains("quota")`). Tests should document the current behavior first, not silently redefine it.

## 2. `OpenAIService` tests
File: `DentistAssistantAI.Infrastructure/Services/OpenAIService.cs`

### Why this is critical
This is the highest-risk logic in the solution because it assembles external API payloads, reads files, and parses responses.

### Test categories

### A. Constructor behavior
1. Sets `Authorization` header to `Bearer <apiKey>`

### B. Text-only request behavior
2. Posts to `https://api.openai.com/v1/chat/completions`
3. Uses `DentalAIConfig.TextModel`
4. Includes system prompt and raw user prompt in request JSON
5. Returns parsed AI content when API response is valid

### C. Vision request behavior
6. Reads image file and encodes it as base64
7. Uses `DentalAIConfig.VisionModel`
8. Prefixes prompt with `DentalAIConfig.ImageAnalysisInstruction`
9. Builds `image_url` content block with `detail = "high"`
10. Uses `image/png` for `.png`, `image/jpeg` for all other current paths
11. Preserves prompt text after injected image instruction

### D. Error mapping behavior
12. Returns `API Error: ...` when HTTP status is non-success
13. Returns `Invalid response format` when `choices` property is missing
14. Returns `Network error: ...` for `HttpRequestException`
15. Returns `Request timeout` for `TaskCanceledException`
16. Returns `Unexpected error: ...` for any other exception

### E. Response parsing edge cases
17. Valid response with `choices[0].message.content`
18. Empty `choices` array should be documented as current behavior
19. Missing `message` or `content` should be documented as current behavior

### Recommended test technique
- Build `HttpClient` with a fake `HttpMessageHandler`
- Capture outgoing `HttpRequestMessage`
- Read request body and verify JSON using `JsonDocument`
- For image tests, create temp `.png` and `.jpg` files during the test and delete them after

### Important implementation note
`BuildTextRequest` and `BuildVisionRequest` are private. Do not test private methods directly. Validate them through `SendAsync(...)` by asserting on the outgoing HTTP body.

## 3. `MainPageViewModel` tests
File: `DentistAssistantAI.App/ViewModels/MainPageViewModel.cs`

### Current value
This class holds most of the user-facing chat behavior:
- seeded welcome message
- command enablement
- send flow
- image-only fallback prompt
- error display
- busy-state transitions

### Tests that can be added with minimal friction
1. Constructor seeds the initial AI welcome message
2. `SendMessageCommand.CanExecute` is `false` when no text and no pending image
3. `SendMessageCommand.CanExecute` is `true` when text exists
4. `SendMessageCommand.CanExecute` is `true` when pending image exists
5. `SendMessageAsync` trims user input before sending
6. `SendMessageAsync` clears `UserInput` and `PendingImagePath`
7. `SendMessageAsync` adds the user message before AI response
8. `SendMessageAsync` appends AI answer on success
9. `SendMessageAsync` uses `DentalAIConfig.DefaultImagePrompt` when only image is sent
10. `SendMessageAsync` appends `Error: ...` message when dependency throws
11. `SendMessageAsync` always resets `IsBusy` in `finally`
12. `ClearPendingImageCommand` clears the pending image

### Tests that are harder with current design
13. `PickPhotoAsync` success path
14. `TakePhotoAsync` success path
15. `CopyToLocalCacheAsync` file-copy behavior

### Why those are harder
The view model currently uses:
- `MediaPicker.Default`
- `FileSystem.CacheDirectory`
- private static `CopyToLocalCacheAsync(...)`
- `ImageSource.FromFile(...)`

These are MAUI/static APIs, which make isolated unit testing more difficult and more environment-dependent.

### Minimal refactor proposed before writing the harder tests
Keep this small and local to the `App` layer.
1. Introduce an `IMediaPickerService` wrapper for:
   - `PickPhotoAsync()`
   - `CapturePhotoAsync()`
   - `IsCaptureSupported`
2. Introduce an `ILocalImageCache` or `IMediaFileCache` wrapper for copying selected media into cache
3. Inject those services into `MainPageViewModel`
4. Keep command behavior unchanged

This preserves MVVM and avoids putting MAUI static APIs directly inside unit-tested logic.

### Priority decision
- Test send/chat behavior first
- Defer photo/camera command tests until wrappers exist

## 4. `ChatMessageItem` tests
File: `DentistAssistantAI.App/Models/ChatMessageItem.cs`

### Useful tests
1. Constructor stores `Text`, `IsFromUser`, and `ImagePath`
2. `HasImage` is `true` only when `ImagePath` is non-empty
3. `HasText` is `true` only when `Text` is non-empty
4. `TimestampText` is formatted as `HH:mm`

### Notes
- `Timestamp` uses `DateTime.Now`, so exact time assertions should allow tolerance or only validate formatting.
- `ImageSource` can be tested only if the chosen test target supports MAUI types reliably.

## 5. `ChatMessageTemplateSelector` tests
File: `DentistAssistantAI.App/Templates/ChatMessageTemplateSelector.cs`

### Useful tests
1. Returns `UserTemplate` when `item.IsFromUser == true`
2. Returns `AiTemplate` when `item.IsFromUser == false`
3. Non-`ChatMessageItem` values currently fall back to `AiTemplate` by implementation shape; test should document that current behavior

## 6. `DentalAIConfig` contract tests
File: `DentistAssistantAI.Core/Configuration/DentalAIConfig.cs`

### These are low-value but useful guard tests
1. `TextModel` is not empty
2. `VisionModel` is not empty
3. `SystemPrompt` contains the required image disclaimer text
4. `DefaultImagePrompt` is not empty
5. `ImageAnalysisInstruction` contains the structured section headings

### Why keep these limited
These are configuration constants, not dynamic logic. Add only a few contract tests to catch accidental prompt deletion or model-name blanking.

## What Not to Unit Test First

### `MainPage.xaml.cs`
- Contains dispatcher-driven scroll behavior tied to UI controls
- Better handled by manual verification or UI/integration tests

### `App.xaml.cs`
- Thin MAUI startup object
- Low payoff for unit tests

### `MauiProgram.cs`
- Service registration is better validated by smoke tests or a lightweight composition test if needed later

## Proposed Execution Order

### Iteration 1: Quick wins, no production refactor
1. Add `DentistAssistantAI.Application.Tests`
2. Add `AIManager` tests
3. Add `DentistAssistantAI.Infrastructure.Tests`
4. Add `OpenAIService` tests for text path, image path, and error mapping
5. Add a few `DentalAIConfig` guard tests

### Iteration 2: App-layer tests with minimal friction
6. Add `DentistAssistantAI.App.Tests`
7. Add tests for `ChatMessageItem`
8. Add tests for `ChatMessageTemplateSelector`
9. Add tests for `MainPageViewModel` send flow and command state

### Iteration 3: Small refactor to unlock harder view-model tests
10. Wrap `MediaPicker.Default`
11. Wrap cache-copy behavior
12. Add unit tests for `PickPhotoAsync`, `TakePhotoAsync`, and cache-copy outcomes

## Suggested Folder Layout

```text
/tests
  DentistAssistantAI.Application.Tests
    /Services
      AIManagerTests.cs
  DentistAssistantAI.Infrastructure.Tests
    /Services
      OpenAIServiceTests.cs
    /TestDoubles
      StubHttpMessageHandler.cs
  DentistAssistantAI.App.Tests
    /ViewModels
      MainPageViewModelTests.cs
    /Models
      ChatMessageItemTests.cs
    /Templates
      ChatMessageTemplateSelectorTests.cs
```

## Naming Convention for Tests
Use `MethodName_State_ExpectedResult`.
Examples:
- `AskDentistAI_ServiceSucceeds_ReturnsContent`
- `SendAsync_ImagePathProvided_UsesVisionRequest`
- `SendMessageAsync_ImageOnly_UsesDefaultImagePrompt`

## Build and Validation Plan

### After each test-project addition
1. Restore packages
2. Build the solution
3. Run only the new test project first
4. Then run all tests

### Minimum success criteria
- All new tests pass consistently
- No change to existing runtime behavior
- No hardcoded secrets added
- MAUI app still builds for Windows target

## Practical Constraints to Respect
- Keep existing layer boundaries
- Do not move UI logic into code-behind
- Do not duplicate prompt text outside `DentalAIConfig`
- Prefer small seams/wrappers over large architectural changes
- Avoid over-testing framework plumbing

## Final Recommendation
If only one slice is implemented first, start with:
1. `AIManager` tests
2. `OpenAIService` tests
3. `MainPageViewModel` send-flow tests

That sequence gives the best coverage of real behavior with the least disruption.

## Actionable Refactoring Tasks

## Task 1: Create `DentistAssistantAI.Application.Tests`

### Acceptance Criteria
- A new `DentistAssistantAI.Application.Tests` project exists and targets `net10.0`
- The project references `DentistAssistantAI.Application` and `DentistAssistantAI.Core`
- Required test packages are installed
- The test project builds successfully

### Steps to Solve
1. Create a new test project named `DentistAssistantAI.Application.Tests`
2. Target the project to `net10.0`
3. Add project references to `DentistAssistantAI.Application` and `DentistAssistantAI.Core`
4. Add minimal test packages: `Microsoft.NET.Test.Sdk`, `xunit`, `xunit.runner.visualstudio`, `coverlet.collector`
5. Organize the project with a `Services` folder for `AIManager` tests
6. Build the test project to verify setup

### Files to Review
- `DentistAssistantAI.Application/DentistAssistantAI.Application.csproj`
- `DentistAssistantAI.Core/DentistAssistantAI.Core.csproj`
- solution file if one is later added to the workspace

## Task 2: Add unit tests for `AIManager`

### Acceptance Criteria
- Tests cover success, null content, quota error, generic error, and parameter forwarding
- Tests document current quota behavior as case-sensitive
- All `AIManager` tests pass

### Steps to Solve
1. Create `Services/AIManagerTests.cs` inside `DentistAssistantAI.Application.Tests`
2. Add a small fake implementation of `IOpenAIService` or use a lightweight mock approach
3. Write a test for successful content return
4. Write a test for `Content = null` returning empty string
5. Write a test for quota-specific error mapping
6. Write a test for generic error mapping
7. Write a test confirming `question` and `imagePath` are passed through unchanged
8. Run the test project and fix any failing assertions

### Files to Review
- `DentistAssistantAI.Application/Services/AIManager.cs`
- `DentistAssistantAI.Core/Interfaces/IOpenAIService.cs`
- `DentistAssistantAI.Core/Models/AIResult.cs`

## Task 3: Create `DentistAssistantAI.Infrastructure.Tests`

### Acceptance Criteria
- A new `DentistAssistantAI.Infrastructure.Tests` project exists and targets `net10.0`
- The project references `DentistAssistantAI.Infrastructure` and `DentistAssistantAI.Core`
- Required test packages are installed
- The test project builds successfully

### Steps to Solve
1. Create a new test project named `DentistAssistantAI.Infrastructure.Tests`
2. Target the project to `net10.0`
3. Add project references to `DentistAssistantAI.Infrastructure` and `DentistAssistantAI.Core`
4. Add the minimal test packages
5. Add folders for `Services` and `TestDoubles`
6. Build the test project to verify setup

### Files to Review
- `DentistAssistantAI.Infrastructure/DentistAssistantAI.Infrastructure.csproj`
- `DentistAssistantAI.Core/DentistAssistantAI.Core.csproj`
- solution file if one is later added to the workspace

## Task 4: Add text-path tests for `OpenAIService`

### Acceptance Criteria
- Tests verify POST target URL
- Tests verify `Authorization` header uses `Bearer <apiKey>`
- Tests verify text requests use `DentalAIConfig.TextModel`
- Tests verify request JSON contains the system prompt and user prompt
- Tests verify valid API responses are parsed into successful `AIResult`

### Steps to Solve
1. Add a stub `HttpMessageHandler` that captures outgoing requests and returns controlled responses
2. Create `Services/OpenAIServiceTests.cs`
3. Instantiate `OpenAIService` with `HttpClient` backed by the stub handler
4. Write a success test for text-only requests
5. Parse the outgoing JSON using `JsonDocument`
6. Assert request URL, HTTP method, model name, and message structure
7. Assert the returned `AIResult` contains parsed content
8. Run the infrastructure test project

### Files to Review
- `DentistAssistantAI.Infrastructure/Services/OpenAIService.cs`
- `DentistAssistantAI.Core/Configuration/DentalAIConfig.cs`
- `DentistAssistantAI.Core/Models/AIResult.cs`

## Task 5: Add image-path tests for `OpenAIService`

### Acceptance Criteria
- Tests verify image requests use `DentalAIConfig.VisionModel`
- Tests verify image data is sent as a base64 data URL
- Tests verify `.png` maps to `image/png`
- Tests verify non-`.png` current behavior maps to `image/jpeg`
- Tests verify `DentalAIConfig.ImageAnalysisInstruction` is prefixed before the original prompt
- Tests verify `detail` is set to `high`

### Steps to Solve
1. Create temporary image files during tests
2. Invoke `SendAsync(prompt, imagePath)` with `.png` and `.jpg` samples
3. Capture the outgoing JSON payload from the stub handler
4. Parse the `messages` structure and inspect the multimodal `content` array
5. Assert the model, instruction prefix, image URL format, and detail level
6. Clean up temp files after each test
7. Run the infrastructure test project

### Files to Review
- `DentistAssistantAI.Infrastructure/Services/OpenAIService.cs`
- `DentistAssistantAI.Core/Configuration/DentalAIConfig.cs`

## Task 6: Add error-handling tests for `OpenAIService`

### Acceptance Criteria
- Tests cover non-success HTTP status handling
- Tests cover missing `choices` response handling
- Tests cover `HttpRequestException`, `TaskCanceledException`, and generic exception mapping
- Tests document current parsing edge cases without changing behavior

### Steps to Solve
1. Extend the stub handler to return error responses and throw exceptions
2. Add tests for HTTP non-success responses returning `API Error: ...`
3. Add tests for invalid JSON shape returning `Invalid response format`
4. Add tests for thrown `HttpRequestException`
5. Add tests for thrown `TaskCanceledException`
6. Add tests for other thrown exceptions
7. Optionally add documentation-style tests for current behavior with malformed `choices[0]` shape
8. Run the infrastructure test project

### Files to Review
- `DentistAssistantAI.Infrastructure/Services/OpenAIService.cs`
- `DentistAssistantAI.Core/Models/AIResult.cs`

## Task 7: Add guard tests for `DentalAIConfig`

### Acceptance Criteria
- Tests verify `TextModel` and `VisionModel` are not empty
- Tests verify `SystemPrompt` contains the mandatory disclaimer line
- Tests verify `DefaultImagePrompt` is not empty
- Tests verify `ImageAnalysisInstruction` contains expected structured headings

### Steps to Solve
1. Decide whether these tests live in `DentistAssistantAI.Infrastructure.Tests` or a separate `Core` test project
2. Create a test file for prompt/config guards
3. Write simple assertions against constant values
4. Keep the tests lightweight and avoid duplicating full prompt text in assertions
5. Run the test project containing these tests

### Files to Review
- `DentistAssistantAI.Core/Configuration/DentalAIConfig.cs`
- the selected test project `.csproj`

## Task 8: Create `DentistAssistantAI.App.Tests`

### Acceptance Criteria
- A new `DentistAssistantAI.App.Tests` project exists
- The project targets a MAUI-compatible test target, preferably `net10.0-windows10.0.19041.0`
- The project references `DentistAssistantAI.App`
- The test project builds on the Windows development environment

### Steps to Solve
1. Create a new test project named `DentistAssistantAI.App.Tests`
2. Choose the target framework appropriate for exercising MAUI-dependent types on Windows
3. Add a project reference to `DentistAssistantAI.App`
4. Add the standard test packages
5. Validate that the project can build in the current environment
6. Add folders for `Models`, `Templates`, and `ViewModels`

### Files to Review
- `DentistAssistantAI.App/DentistAssistantAI.App.csproj`
- `DentistAssistantAI.App/MauiProgram.cs`
- solution file if one is later added to the workspace

## Task 9: Add tests for `ChatMessageItem`

### Acceptance Criteria
- Tests verify constructor-assigned values
- Tests verify `HasImage` behavior
- Tests verify `HasText` behavior
- Tests verify `TimestampText` format is `HH:mm`

### Steps to Solve
1. Create `Models/ChatMessageItemTests.cs`
2. Add tests for constructor state
3. Add tests for image/text derived properties
4. Add a format-based assertion for `TimestampText` instead of exact time equality
5. If MAUI image-source assertions are unstable, skip `ImageSource` checks in the first pass
6. Run the app test project

### Files to Review
- `DentistAssistantAI.App/Models/ChatMessageItem.cs`

## Task 10: Add tests for `ChatMessageTemplateSelector`

### Acceptance Criteria
- Tests verify `UserTemplate` is returned for user messages
- Tests verify `AiTemplate` is returned for AI messages
- Tests document fallback behavior for non-`ChatMessageItem` inputs

### Steps to Solve
1. Create `Templates/ChatMessageTemplateSelectorTests.cs`
2. Instantiate the selector with distinct `DataTemplate` instances
3. Pass a user `ChatMessageItem` and assert `UserTemplate`
4. Pass a non-user `ChatMessageItem` and assert `AiTemplate`
5. Pass a non-`ChatMessageItem` object and document/assert current fallback behavior
6. Run the app test project

### Files to Review
- `DentistAssistantAI.App/Templates/ChatMessageTemplateSelector.cs`
- `DentistAssistantAI.App/Models/ChatMessageItem.cs`

## Task 11: Add send-flow tests for `MainPageViewModel`

### Acceptance Criteria
- Tests verify the constructor seeds the welcome message
- Tests verify send command enablement for text and image scenarios
- Tests verify text is trimmed before sending
- Tests verify `UserInput` and `PendingImagePath` are cleared on send
- Tests verify user message and AI reply are appended in the expected order
- Tests verify image-only sends use `DentalAIConfig.DefaultImagePrompt`
- Tests verify thrown exceptions produce `Error: ...` chat messages
- Tests verify `IsBusy` is reset after completion

### Steps to Solve
1. Create `ViewModels/MainPageViewModelTests.cs`
2. Provide a fake `AIManager` dependency path suitable for testing current behavior
3. Add tests for constructor initialization and command `CanExecute`
4. Add tests for successful send flow with text input
5. Add tests for image-only send flow
6. Add tests for exception handling
7. Add assertions for message order and busy-state reset
8. Run the app test project

### Files to Review
- `DentistAssistantAI.App/ViewModels/MainPageViewModel.cs`
- `DentistAssistantAI.Application/Services/AIManager.cs`
- `DentistAssistantAI.Core/Configuration/DentalAIConfig.cs`
- `DentistAssistantAI.App/Models/ChatMessageItem.cs`

## Task 12: Refactor media dependencies in `MainPageViewModel` to improve testability

### Acceptance Criteria
- Static `MediaPicker.Default` usage is wrapped behind an injected abstraction
- Cache-copy behavior is wrapped behind an injected abstraction or isolated service
- Existing UI behavior remains unchanged
- Existing send-flow tests continue to pass
- New seams are small and stay inside the current project boundaries

### Steps to Solve
1. Introduce a small wrapper interface for media picking and capture support
2. Introduce a small wrapper interface for copying media into local cache
3. Implement these wrappers in the `App` layer using MAUI APIs
4. Register the wrappers in `MauiProgram.cs`
5. Update `MainPageViewModel` to use the wrappers instead of static APIs directly
6. Keep command names and user-facing behavior unchanged
7. Build the app and run existing tests

### Files to Review
- `DentistAssistantAI.App/ViewModels/MainPageViewModel.cs`
- `DentistAssistantAI.App/MauiProgram.cs`
- any new wrapper files created under `DentistAssistantAI.App`

## Task 13: Add tests for `PickPhotoAsync` and `TakePhotoAsync`

### Acceptance Criteria
- Tests verify `PickPhotoAsync` sets `PendingImagePath` when a file is returned
- Tests verify `TakePhotoAsync` respects `IsCaptureSupported`
- Tests verify null results do not modify pending image state incorrectly
- Tests verify exceptions are swallowed as currently intended

### Steps to Solve
1. Use the new wrapper abstractions added in the previous task
2. Add fake media picker and cache services in tests
3. Write tests for successful photo pick
4. Write tests for successful capture
5. Write tests for unsupported capture
6. Write tests for null results and thrown exceptions
7. Run the app test project

### Files to Review
- `DentistAssistantAI.App/ViewModels/MainPageViewModel.cs`
- wrapper interfaces and implementations added for media access

## Task 14: Validate the full test suite and build flow

### Acceptance Criteria
- All added test projects restore and build successfully
- All implemented tests pass locally
- The MAUI app still builds for Windows target `net10.0-windows10.0.19041.0`
- No new hardcoded secrets are introduced

### Steps to Solve
1. Restore NuGet packages for all projects
2. Build each test project independently
3. Run each test project independently
4. Run the full test suite
5. Build `DentistAssistantAI.App` for the Windows target
6. Perform a quick smoke test of chat send and image flow if production code changed

### Files to Review
- `DentistAssistantAI.App/DentistAssistantAI.App.csproj`
- all new test project `.csproj` files
- `DentistAssistantAI.App/MauiProgram.cs`
