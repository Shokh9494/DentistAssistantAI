# Deploy WebAPI to Render.com

## Context
Deploy the DentistAssistantAI ASP.NET Core WebAPI to Render.com so the MAUI app can reach it from anywhere. Docker is already in place. The OpenAI API key must never be committed — it lives only in Render's env vars dashboard. The MAUI App's `appsettings.json` (URL only, no secrets) should be committed so the production URL is baked into Release builds.

---

## Task 1 — Fix Dockerfile PORT handling
**File:** `DentistAssistantAI.WebApi/Dockerfile`

Render injects a dynamic `$PORT` env var. Replace the hardcoded port line in the runtime stage:
```dockerfile
ENV ASPNETCORE_HTTP_PORTS=${PORT:-8080}
```

---

## Task 2 — Add /health endpoint
**File:** `DentistAssistantAI.WebApi/Program.cs`

Add before `app.Run()`:
```csharp
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
```

---

## Task 3 — Create render.yaml
**New file:** `render.yaml` (repo root)

```yaml
services:
  - type: web
    name: dentist-assistant-api
    runtime: docker
    dockerfilePath: DentistAssistantAI.WebApi/Dockerfile
    healthCheckPath: /health
    envVars:
      - key: ASPNETCORE_ENVIRONMENT
        value: Production
      - key: OpenAI__ApiKey
        sync: false
```

`sync: false` → value never stored in YAML; Render dashboard prompts for it.

---

## Task 4 — Commit MAUI appsettings.json (URL only)
**File:** `DentistAssistantAI.App/appsettings.json`  
**File:** `.gitignore`

- Remove `/DentistAssistantAI.App/appsettings.json` from `.gitignore`
- Set content to production Render URL (after deploy)
- No secrets in this file — safe to commit

---

## Task 5 — Dev override: appsettings.Development.json
**New file:** `DentistAssistantAI.App/appsettings.Development.json` (gitignored)  
**File:** `DentistAssistantAI.App/MauiProgram.cs`

- In `#if DEBUG` block, load `appsettings.Development.json` on top of base config
- Local devs set `BaseUrl` to `http://localhost:5186` in their gitignored file

---

## Deployment Steps (manual)
1. Push branch to GitHub
2. Render dashboard → New Web Service → Docker → connect repo
3. Set `OpenAI__ApiKey` env var in Render dashboard (secret)
4. Set Health Check Path `/health`
5. Deploy → copy `https://<name>.onrender.com` URL
6. Update `appsettings.json` with that URL → commit

---

## Verification
- `GET /health` → 200 `{"status":"healthy"}`
- `POST /api/chat` with message → AI response
- MAUI Release build uses Render URL
- `git grep -i "sk-"` → no results
