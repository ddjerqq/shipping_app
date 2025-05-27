ARG USER_ID=1000
ARG GROUP_ID=1000
ARG USERNAME="shipping"

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
ARG USER_ID
ARG GROUP_ID
ARG USERNAME

RUN groupadd -g ${GROUP_ID} ${USERNAME} && \
    useradd -u ${USER_ID} -g ${GROUP_ID} -m -s /bin/bash ${USERNAME} && \
    chown -R ${USERNAME}:${USERNAME} /home/${USERNAME}

RUN apt update && \
    apt install -y ca-certificates curl gnupg && \
    mkdir -p /etc/apt/keyrings && \
    curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg && \
    NODE_MAJOR=20 && \
    echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_$NODE_MAJOR.x nodistro main" | tee /etc/apt/sources.list.d/nodesource.list && \
    apt update && \
    apt install -y curl && \
    apt clean && \
    rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

RUN apt update && \
    apt install -y ca-certificates curl gnupg && \
    mkdir -p /etc/apt/keyrings && \
    curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg && \
    NODE_MAJOR=20 && \
    echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_$NODE_MAJOR.x nodistro main" | tee /etc/apt/sources.list.d/nodesource.list && \
    apt update && \
    apt install -y nodejs && \
    apt clean && \
    rm -rf /var/lib/apt/lists/*

RUN dotnet workload install wasm-tools

COPY ["source_generators/StrongIdGenerator/StrongIdGenerator.csproj", "source_generators/StrongIdGenerator/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Persistence/Persistence.csproj", "src/Persistence/"]
COPY ["src/Presentation/Presentation.csproj", "src/Presentation/"]

RUN dotnet restore "source_generators/StrongIdGenerator/StrongIdGenerator.csproj"
RUN dotnet restore "src/Domain/Domain.csproj"
RUN dotnet restore "src/Application/Application.csproj"
RUN dotnet restore "src/Infrastructure/Infrastructure.csproj"
RUN dotnet restore "src/Persistence/Persistence.csproj"
RUN dotnet restore "src/Presentation/Presentation.csproj"

COPY ["src/Presentation/package.json", "src/Presentation/package-lock.json*", "./src/Presentation/"]
COPY ["src/Presentation/webpack.config.js", "./src/Presentation/"]
COPY ["src/Presentation/tsconfig.json", "./src/Presentation/"]

WORKDIR /app/src/Presentation
RUN npm install

WORKDIR /app
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
WORKDIR /app/src/Presentation
RUN npm run tw:build
RUN npm run pack

WORKDIR /app

RUN dotnet publish -c Release -o /app/publish --no-restore --no-build "./src/Presentation/Presentation.csproj"

FROM base AS final
ARG USERNAME
USER ${USERNAME}
WORKDIR /home/${USERNAME}

EXPOSE 1080
EXPOSE 1443

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=10s CMD curl --insecure --silent --fail https://127.0.0.1:1443/api/v1/health || exit 1

ENTRYPOINT ["dotnet", "Presentation.dll"]
