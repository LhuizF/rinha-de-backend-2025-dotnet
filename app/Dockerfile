FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build

WORKDIR /app

COPY ["src/Api/Rinha.Api.csproj", "Api/"]
RUN dotnet restore "Api/Rinha.Api.csproj"

COPY . .

WORKDIR /app/src/Api

RUN dotnet build "Rinha.Api.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

COPY --from=build /app/build .

ENTRYPOINT ["dotnet", "Rinha.Api.dll"]
