FROM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim AS build

WORKDIR /app

COPY ["src/Api/Rinha.Api.csproj", "Api/"]
RUN dotnet restore "Api/Rinha.Api.csproj"

# Copia todo o código-fonte
COPY . .

# Publica a aplicação
# -c Release: Compila em modo Release
# -o /app/publish: Define o diretório de saída dentro do contêiner
WORKDIR /app/src/Api

RUN dotnet build "Rinha.Api.csproj" -c Release -o /app/build
# Estágio Final (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim AS final
WORKDIR /app

# Copia os artefatos publicados do estágio de build para o estágio final
COPY --from=build /app/build .

# Define o ponto de entrada da aplicação
ENTRYPOINT ["dotnet", "Rinha.Api.dll"]
