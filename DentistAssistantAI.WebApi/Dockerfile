# ── Build stage ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first — keeps NuGet restore layer cached
COPY ["DentistAssistantAI.WebApi/DentistAssistantAI.WebApi.csproj",          "DentistAssistantAI.WebApi/"]
COPY ["DentistAssistantAI.Core/DentistAssistantAI.Core.csproj",              "DentistAssistantAI.Core/"]
COPY ["DentistAssistantAI.Application/DentistAssistantAI.Application.csproj","DentistAssistantAI.Application/"]
COPY ["DentistAssistantAI.Infrastructure/DentistAssistantAI.Infrastructure.csproj","DentistAssistantAI.Infrastructure/"]
RUN dotnet restore "DentistAssistantAI.WebApi/DentistAssistantAI.WebApi.csproj"

# Copy source and publish
COPY . .
WORKDIR /src/DentistAssistantAI.WebApi
RUN dotnet publish "DentistAssistantAI.WebApi.csproj" \
    -c Release -o /app/publish /p:UseAppHost=false

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=${PORT:-8080}

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DentistAssistantAI.WebApi.dll"]
