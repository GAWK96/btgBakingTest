# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore "src/btgBank.Api/btgBank.Api.csproj"

RUN dotnet publish "src/btgBank.Api/btgBank.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "btgBank.Api.dll"]