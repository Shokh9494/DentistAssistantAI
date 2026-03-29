# Agent Configuration

## Purpose
This file defines the baseline rules for any AI assistant working in this repository.

## Required Stack
- Target framework: `.NET 10`
- UI framework: `.NET MAUI`
- Architecture baseline: `MVVM`

## Core Rules
1. Use `MVVM` for UI-facing features and changes.
2. Keep architectural changes incremental; do not rewrite large parts of the app unless explicitly requested.
3. Prefer existing project structure, naming, and dependency injection patterns.
4. Use `async`/`await` for I/O and network operations.
5. Preserve nullable-safe code and avoid introducing nullability warnings.
6. Make minimal changes required to solve the task.
7. Validate changes with build/tests when possible.

## .NET MAUI Rules
- Use `.NET MAUI` patterns and APIs only.
- Do not suggest or introduce `Xamarin.Forms` code.
- Keep platform-specific code inside `Platforms/` when needed.
- Prefer shared UI logic in `ViewModels` over code-behind.

## MVVM Rules
- Put presentation logic in `ViewModels`.
- Keep views focused on layout and bindings.
- Use services for business logic and external integrations.
- Use dependency injection for services and view models where appropriate.
- Avoid tightly coupling UI code to infrastructure details.

## AI and Service Integration Rules
- Do not hardcode API keys, tokens, or secrets in source files.
- Do not store production secrets in client-side MAUI app code.
- Prefer backend-mediated access for third-party AI providers.
- If configuration is needed, use typed options or configuration binding where practical.

## Code Quality Rules
- Follow the existing code style of the repository.
- Avoid adding unnecessary comments.
- Reuse existing libraries and abstractions before adding new ones.
- Keep methods small and focused when modifying code.

## Safety Rules
- Do not expose sensitive data in logs, exceptions, or UI messages.
- Treat any committed secret as compromised and recommend rotation.
- Avoid destructive file or architectural changes unless requested.

## Default Assistant Behavior
- Analyze existing files before editing them.
- Prefer minimal, targeted patches.
- Explain tradeoffs briefly and clearly.
- When multiple approaches exist, prefer the one most consistent with the current repository.
