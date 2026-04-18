\# agent-skills



This is the agent-skills project — a collection of production-grade engineering skills for AI coding agents.



\## Project Structure

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


```

skills/       → Core skills (SKILL.md per directory)

agents/       → Reusable agent personas (code-reviewer, test-engineer, security-auditor)

hooks/        → Session lifecycle hooks

.claude/commands/ → Slash commands (/spec, /plan, /build, /test, /review, /code-simplify, /ship)

references/   → Supplementary checklists (testing, performance, security, accessibility)

docs/         → Setup guides for different tools

```



\## Skills by Phase



\*\*Define:\*\* spec-driven-development

\*\*Plan:\*\* planning-and-task-breakdown

\*\*Build:\*\* incremental-implementation, test-driven-development, context-engineering, source-driven-development, frontend-ui-engineering, api-and-interface-design

\*\*Verify:\*\* browser-testing-with-devtools, debugging-and-error-recovery

\*\*Review:\*\* code-review-and-quality, code-simplification, security-and-hardening, performance-optimization

\*\*Ship:\*\* git-workflow-and-versioning, ci-cd-and-automation, deprecation-and-migration, documentation-and-adrs, shipping-and-launch



\## Conventions



\- Every skill lives in `skills/<name>/SKILL.md`

\- YAML frontmatter with `name` and `description` fields

\- Description starts with what the skill does (third person), followed by trigger conditions ("Use when...")

\- Every skill has: Overview, When to Use, Process, Common Rationalizations, Red Flags, Verification

\- References are in `references/`, not inside skill directories

\- Supporting files only created when content exceeds 100 lines



\## Commands



\- `npm test` — Not applicable (this is a documentation project)

\- Validate: Check that all SKILL.md files have valid YAML frontmatter with name and description



\## Boundaries



\- Always: Follow the skill-anatomy.md format for new skills

\- Never: Add skills that are vague advice instead of actionable processes

\- Never: Duplicate content between skills — reference other skills instead

