# Task List — AI Educational Web Service (MVP)

## Status Key: [ ] todo  [~] in progress  [x] done

---

## Task 1 — Scaffold + Chat Endpoint
- [x] Create `DentistAssistantAI.WebApi.csproj`
- [x] Write `Program.cs` (DI, static files, CORS, route groups)
- [x] Create `Endpoints/ChatEndpoints.cs` — `POST /api/chat`
- [x] Create `DTOs/ChatRequest.cs`
- [x] Create `wwwroot/index.html` (two-tab shell)
- [x] Create `wwwroot/app.js` (tab switching, chat fetch)
- [x] Create `wwwroot/style.css`
- [x] Add project to `DentistAssistantAI.slnx`

---

## Task 2 — Teacher Content Generation
- [x] Create `Core/Configuration/EducationAIConfig.cs`
- [x] Extend `Core/Interfaces/IOpenAIService.cs` — add `systemPrompt` param
- [x] Update `Infrastructure/Services/OpenAIService.cs` — use param with fallback
- [x] Add to `Application/Services/AIManager.cs`: `GenerateLecture`, `GenerateTest`, `GenerateTeacherCase`
- [x] Create `Endpoints/TeacherEndpoints.cs`
- [x] Create `DTOs/GenerateContentRequest.cs`
- [x] Update `wwwroot/` — Generate tab with 3 buttons + topic/year inputs

---

## Task 3 — Student AI Assistant
- [x] Add to `EducationAIConfig.cs`: `StudentSystemPrompt`, `StudentAskTemplate`
- [x] Add `AskStudent` to `AIManager.cs`
- [x] Create `Endpoints/StudentEndpoints.cs` — `POST /api/student/ask`
- [x] Create `DTOs/StudentAskRequest.cs`
- [x] Update `wwwroot/` — Teacher/Student toggle in Chat tab

---

## Task 4 — Interactive Clinical Cases
- [x] Add to `EducationAIConfig.cs`: `StudentCasePromptTemplate`, `CaseEvaluationTemplate`
- [x] Add to `AIManager.cs`: `GenerateStudentCase`, `EvaluateStudentAnswer`
- [x] Create `Endpoints/ClinicalCaseEndpoints.cs` (`/generate` + `/evaluate`)
- [x] Create `DTOs/EvaluateCaseRequest.cs`
- [x] Update `wwwroot/` — Cases tab (case display, inputs, feedback)

---

## Task 5 — Tests + Polish
- [x] `GenerateLecture_ReturnsContent` + `GenerateLecture_UsesTeacherSystemPrompt`
- [x] `GenerateTest_ReturnsContent` + `GenerateTest_ReturnsErrorMessage_WhenServiceFails`
- [x] `GenerateTeacherCase_ReturnsContent` + `GenerateTeacherCase_UsesClinicalCaseSystemPrompt`
- [x] `AskStudent_ReturnsContent` + `AskStudent_UsesStudentSystemPrompt`
- [x] `EvaluateStudentAnswer_ReturnsContent` + `EvaluateStudentAnswer_ReturnsQuotaMessage`
- [x] `GenerateStudentCase_ReturnsContent`
- [x] Loading spinner in `app.js`
- [x] Error display in `app.js`
- [x] 400 validation in all API endpoints

**Total: 42 tests passing (17 Application + 25 Infrastructure)**
