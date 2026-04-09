FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["MetaComptaSAS.sln", "."]
COPY ["src/MetaCompta.Domain/MetaCompta.Domain.csproj", "src/MetaCompta.Domain/"]
COPY ["src/MetaCompta.Infrastructure/MetaCompta.Infrastructure.csproj", "src/MetaCompta.Infrastructure/"]
COPY ["src/MetaCompta.Application/MetaCompta.Application.csproj", "src/MetaCompta.Application/"]
COPY ["src/MetaCompta.API/MetaCompta.API.csproj", "src/MetaCompta.API/"]
COPY ["src/MetaCompta.BackOffice/MetaCompta.BackOffice.csproj", "src/MetaCompta.BackOffice/"]
COPY ["src/MetaCompta.Extranet/MetaCompta.Extranet.csproj", "src/MetaCompta.Extranet/"]
COPY ["src/MetaCompta.Admin/MetaCompta.Admin.csproj", "src/MetaCompta.Admin/"]
COPY ["tests/MetaCompta.BackOffice.Tests/MetaCompta.BackOffice.Tests.csproj", "tests/MetaCompta.BackOffice.Tests/"]

RUN dotnet restore "MetaComptaSAS.sln"

COPY . .

FROM build AS publish-api
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/MetaCompta.API/MetaCompta.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM build AS publish-backoffice
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/MetaCompta.BackOffice/MetaCompta.BackOffice.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM build AS publish-extranet
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/MetaCompta.Extranet/MetaCompta.Extranet.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM build AS publish-admin
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/MetaCompta.Admin/MetaCompta.Admin.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS api
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
COPY --from=publish-api /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
HEALTHCHECK --interval=15s --timeout=5s --retries=3 CMD curl -sf http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "MetaCompta.API.dll"]

FROM base AS backoffice
WORKDIR /app
COPY --from=publish-backoffice /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "MetaCompta.BackOffice.dll"]

FROM base AS extranet
WORKDIR /app
COPY --from=publish-extranet /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "MetaCompta.Extranet.dll"]

FROM base AS admin
WORKDIR /app
COPY --from=publish-admin /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "MetaCompta.Admin.dll"]
