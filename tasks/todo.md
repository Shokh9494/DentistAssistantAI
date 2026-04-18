# TODO — Render.com Deployment

## Phase 1: WebAPI changes
- [x] T1: Fix Dockerfile — use `${PORT:-8080}` instead of hardcoded 8080
- [x] T2: Add `GET /health` endpoint to Program.cs
- [x] T3: Create `render.yaml` at repo root with `OpenAI__ApiKey sync: false`

## Phase 2: MAUI app config
- [x] T4: Remove App `appsettings.json` from `.gitignore`; set placeholder production URL
- [x] T5: Add `appsettings.Development.json` (gitignored) + load it in `MauiProgram.cs` under `#if DEBUG`

## Phase 3: Deploy (manual steps)
- [ ] T6: Push to GitHub
- [ ] T7: Create Web Service on Render → connect repo → set `OpenAI__ApiKey` env var
- [ ] T8: Copy deployed URL → update `appsettings.json` with real URL → commit
- [ ] T9: Smoke-test `/health` and `/api/chat` on live URL
