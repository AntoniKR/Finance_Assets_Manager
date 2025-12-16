# --- Этап сборки ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем всё и собираем проект
COPY . .
RUN dotnet restore

RUN dotnet publish FinancialAssetsApp.csproj -c Release -o /app/out

# --- Этап выполнения ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Запускаем приложение
ENTRYPOINT ["dotnet", "FinancialAssetsApp.dll"]

# Render по умолчанию слушает 10000 порт
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
