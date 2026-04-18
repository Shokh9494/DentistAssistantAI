# Task List — AI Educational Web Service (MVP)

## Status Key: [ ] todo  [~] in progress  [x] done

---

## Task 1 — Scaffold + Chat Endpoint
- [ ] Create `DentistAssistantAI.WebApi.csproj`
- [ ] Write `Program.cs` (DI, static files, CORS, route groups)
- [ ] Create `Endpoints/ChatEndpoints.cs` — `POST /api/chat`
- [ ] Create `DTOs/ChatRequest.cs`
- [ ] Create `wwwroot/index.html` (two-tab shell)
- [ ] Create `wwwroot/app.js` (tab switching, chat fetch)
- [ ] Create `wwwroot/style.css`
- [ ] Add project to `DentistAssistantAI.sln`
- [ ] Verify: `dotnet run`, open browser, send chat message, receive response

---

## Task 2 — Teacher Content Generation
- [ ] Create `Core/Configuration/EducationAIConfig.cs`
  - [ ] `TeacherSystemPrompt`
  - [ ] `LecturePromptTemplate(topic, year)`
  - [ ] `TestPromptTemplate(topic, year, count)`
  - [ ] `TeacherCasePromptTemplate(topic, year)`
- [ ] Extend `Core/Interfaces/IOpenAIService.cs` — add `systemPrompt` param
- [ ] Update `Infrastructure/Services/OpenAIService.cs` — use param with fallback
- [ ] Add to `Application/Services/AIManager.cs`:
  - [ ] `GenerateLecture`
  - [ ] `GenerateTest`
  - [ ] `GenerateTeacherCase`
- [ ] Create `Endpoints/TeacherEndpoints.cs`
- [ ] Create `DTOs/GenerateContentRequest.cs`
- [ ] Update `wwwroot/` — Generate tab with 3 buttons + topic/year inputs
- [ ] Verify: lecture, test, case all return correct structured content

---

## Task 3 — Student AI Assistant
- [ ] Add to `EducationAIConfig.cs`: `StudentSystemPrompt`, `StudentAskTemplate`
- [ ] Add `AskStudent` to `AIManager.cs`
- [ ] Create `Endpoints/StudentEndpoints.cs` — `POST /api/student/ask`
- [ ] Create `DTOs/StudentAskRequest.cs`
- [ ] Update `wwwroot/` — Teacher/Student toggle in Chat tab
- [ ] Verify: student query returns simplified explanation; language matches input

---

## Task 4 — Interactive Clinical Cases
- [ ] Add to `EducationAIConfig.cs`: `StudentCasePromptTemplate`, `CaseEvaluationTemplate`
- [ ] Add to `AIManager.cs`: `GenerateStudentCase`, `EvaluateStudentAnswer`
- [ ] Create `Endpoints/ClinicalCaseEndpoints.cs`
  - [ ] `POST /api/cases/generate`
  - [ ] `POST /api/cases/evaluate`
- [ ] Create `DTOs/EvaluateCaseRequest.cs`
- [ ] Update `wwwroot/` — Cases tab (case display, inputs, feedback)
- [ ] Verify: full case flow — generate → submit → get feedback with grades

---

## Task 5 — Tests + Polish
- [ ] Add tests to `Application.Tests/Services/AIManagerTests.cs`:
  - [ ] `GenerateLecture_ReturnsContent`
  - [ ] `GenerateTest_ReturnsContent`
  - [ ] `GenerateTeacherCase_ReturnsContent`
  - [ ] `AskStudent_ReturnsContent`
  - [ ] `EvaluateStudentAnswer_ReturnsContent`
- [ ] Add loading spinner to `app.js`
- [ ] Add error display to `app.js`
- [ ] Add 400 validation in API endpoints (empty input)
- [ ] Verify: `dotnet test` all pass; spinner visible during calls
