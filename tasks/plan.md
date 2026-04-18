# Plan: AI Educational Web Service (MVP) — Dental Faculty

## Context
Add a web application for dental education on top of the existing MAUI/AI codebase.
Teachers get lecture/test/case generation. Students get a Q&A assistant and interactive
clinical cases with diagnosis feedback. Reuses existing Core + Application + Infrastructure.

---

## Architecture

```
DentistAssistantAI.WebApi  (NEW — ASP.NET Core Minimal API + wwwroot SPA)
    └── depends on:
        DentistAssistantAI.Core           (add EducationAIConfig.cs)
        DentistAssistantAI.Application    (extend AIManager with 5 new methods)
        DentistAssistantAI.Infrastructure (extend IOpenAIService for custom systemPrompt)
```

### Layer Modifications (minimal, backward-compatible)

| File | Change |
|---|---|
| `Core/Interfaces/IOpenAIService.cs` | Add optional `string? systemPrompt = null` to `SendAsync` |
| `Core/Configuration/EducationAIConfig.cs` | **NEW** — educational system prompts + templates |
| `Infrastructure/Services/OpenAIService.cs` | Use systemPrompt param; fall back to DentalAIConfig.SystemPrompt |
| `Application/Services/AIManager.cs` | Add 5 new methods (below) |

### New AIManager Methods
```csharp
GenerateLecture(string topic, int courseYear)
GenerateTest(string topic, int courseYear, int questionCount = 10)
GenerateTeacherCase(string topic, int courseYear)
AskStudent(string question, int courseYear = 2)
GenerateStudentCase(string topic, int courseYear)
EvaluateStudentAnswer(string caseText, string diagnosis, string treatment)
```

---

## Tasks

### Task 1 — Scaffold + Chat Endpoint
**Vertical slice:** working chat from browser → API → AIManager → OpenAI → browser

Create:
- `DentistAssistantAI.WebApi.csproj` (.NET 8, Minimal API, refs Core+Application+Infrastructure)
- `Program.cs` (DI wiring, static files, CORS)
- `Endpoints/ChatEndpoints.cs` — `POST /api/chat`
- `DTOs/ChatRequest.cs`
- `wwwroot/index.html` + `app.js` + `style.css` — two-tab shell (Chat | Generate)

**Acceptance criteria:**
- `dotnet run` starts server; HTML page loads
- Chat tab sends message and renders AI response

---

### Task 2 — Teacher Content Generation
**Vertical slice:** topic input → button → API → AIManager → structured content rendered in UI

Create/modify:
- `Core/Configuration/EducationAIConfig.cs` — TeacherSystemPrompt, LecturePromptTemplate, TestPromptTemplate, TeacherCasePromptTemplate
- `Core/Interfaces/IOpenAIService.cs` — add `systemPrompt` param
- `Infrastructure/Services/OpenAIService.cs` — use param
- `Application/Services/AIManager.cs` — GenerateLecture, GenerateTest, GenerateTeacherCase
- `Endpoints/TeacherEndpoints.cs` — POST /api/teacher/lecture | /test | /case
- `DTOs/GenerateContentRequest.cs`
- `wwwroot/` — Generate tab: 3 buttons, topic input, year select, output area

**Acceptance criteria:**
- Lecture → ≥5 markdown headings
- Test → ≥10 numbered MCQ with answer key
- Case → includes Жалобы/Анамнез/Данные осмотра, no diagnosis revealed

---

### Task 3 — Student AI Assistant
**Vertical slice:** student question → simplified explanation

Modify:
- `EducationAIConfig.cs` — StudentSystemPrompt, StudentAskTemplate
- `AIManager.cs` — AskStudent
- `Endpoints/StudentEndpoints.cs` — POST /api/student/ask
- `DTOs/StudentAskRequest.cs`
- `wwwroot/` — Teacher/Student mode toggle in Chat tab

**Acceptance criteria:**
- Same topic asked in Student mode returns simpler, jargon-light response
- Language (RU/EN/UZ) matches input

---

### Task 4 — Interactive Clinical Cases (Student)
**Vertical slice:** case generated → student inputs diagnosis/treatment → feedback rendered

Modify:
- `EducationAIConfig.cs` — StudentCasePromptTemplate, CaseEvaluationTemplate
- `AIManager.cs` — GenerateStudentCase, EvaluateStudentAnswer
- `Endpoints/ClinicalCaseEndpoints.cs` — POST /api/cases/generate | /api/cases/evaluate
- `DTOs/EvaluateCaseRequest.cs`
- `wwwroot/` — Cases tab: display case, diagnosis/treatment inputs, feedback panel

**Acceptance criteria:**
- Case has all required sections, no diagnosis in text
- Evaluation shows ✅/❌/⚠️ per criterion + overall grade

---

### Task 5 — Tests + Polish
Extend:
- `tests/DentistAssistantAI.Application.Tests/` — add tests for all 5 new AIManager methods
- `wwwroot/app.js` — loading spinner, error display, disable during request
- API — 400 on empty input, proper error JSON

**Acceptance criteria:**
- `dotnet test` — all pass
- UI shows spinner during call; user-friendly error on failure

---

## Dependency Graph
```
Task 1
  ├── Task 2
  ├── Task 3
  └── Task 4
        └── Task 5 (after 2+3+4)
```

---

## Demo Script
1. `cd DentistAssistantAI.WebApi && dotnet run`
2. Open `http://localhost:5000`
3. Chat tab → "Что такое GIC?" → verify response
4. Generate tab → Lecture, topic "Стеклоиономерные цементы", year 3 → verify headings
5. Generate tab → Test → verify MCQ with answer key
6. Cases tab → generate case → enter diagnosis + treatment → verify ✅/❌ feedback
7. `dotnet test` → all green
