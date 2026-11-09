# Etapa 1 - Compilação
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copia os arquivos da solução
COPY . .

# Restaura as dependências
RUN dotnet restore "src/btgBank.Worker/btgBank.Worker.csproj"

# Publica o Worker
RUN dotnet publish "src/btgBank.Worker/btgBank.Worker.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# Etapa 2 - Execução
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final

WORKDIR /app

# Copia somente os arquivos publicados
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "btgBank.Worker.dll"]