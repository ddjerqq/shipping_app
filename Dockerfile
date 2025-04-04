ARG USER_ID=1000
ARG GROUP_ID=1000
ARG USERNAME="appuser"
ARG TARGETARCH
ARG TAILWIND_VERSION=3.4.17

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
ARG USER_ID
ARG GROUP_ID
ARG USERNAME
ARG TARGETARCH
ARG TAILWIND_VERSION

ENV TAILWIND_VERSION=${TAILWIND_VERSION}

RUN groupadd -g ${GROUP_ID} ${USERNAME} && \
    useradd -u ${USER_ID} -g ${GROUP_ID} -m -s /bin/bash ${USERNAME} && \
    chown -R ${USERNAME}:${USERNAME} /home/${USERNAME}

RUN apt update && apt install -y curl && \
    ARCH_SUFFIX=$([ "$TARGETARCH" = "arm64" ] && echo "arm64" || echo "x64") && \
    curl -sLo /usr/bin/tailwindcss "https://github.com/tailwindlabs/tailwindcss/releases/download/v${TAILWIND_VERSION}/tailwindcss-linux-${ARCH_SUFFIX}" && \
    chmod +x /usr/bin/tailwindcss

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /app

COPY ["src/Presentation/Presentation.csproj", "src/Presentation/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Persistence/Persistence.csproj", "src/Persistence/"]
COPY ["source_generators/StrongIdGenerator/StrongIdGenerator.csproj", "source_generators/StrongIdGenerator/"]

RUN dotnet restore "src/Presentation/Presentation.csproj"

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY --from=base /usr/bin/tailwindcss /usr/bin/tailwindcss
COPY --from=restore /app /app
COPY . .

WORKDIR /app/source_generators
RUN dotnet build -c Release --no-restore "StrongIdGenerator/StrongIdGenerator.csproj"

WORKDIR /app/src
RUN dotnet build -c Release --no-restore "Domain/Domain.csproj"
RUN dotnet build -c Release --no-restore "Application/Application.csproj"
RUN dotnet build -c Release --no-restore "Infrastructure/Infrastructure.csproj"
RUN dotnet build -c Release --no-restore "Persistence/Persistence.csproj"
RUN dotnet build -c Release --no-restore "Presentation/Presentation.csproj"

FROM build AS publish
WORKDIR /app/src

RUN /usr/bin/tailwindcss \
    --input src/Presentation/wwwroot/styles/app.css \
    --output src/Presentation/wwwroot/styles/app.min.css \
    --config src/Presentation/tailwind.config.cjs \
    --content "./src/Presentation/**/*.{cs,razor,js,css,html}" \
    --minify

RUN dotnet publish -c Release -o /app/publish --no-restore --no-build "Presentation/Presentation.csproj"

FROM base AS final
ARG USERNAME
USER ${USERNAME}
WORKDIR /home/${USERNAME}

EXPOSE 1080
EXPOSE 1443

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=10s CMD curl --insecure --silent --fail https://127.0.0.1:1443/api/v1/health || exit 1

ENTRYPOINT ["dotnet", "Presentation.dll"]
