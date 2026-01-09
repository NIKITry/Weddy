FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/Weddy.API/Weddy.API.csproj", "Weddy.API/"]
COPY ["src/Weddy.Application/Weddy.Application.csproj", "Weddy.Application/"]
COPY ["src/Weddy.Domain/Weddy.Domain.csproj", "Weddy.Domain/"]
COPY ["src/Weddy.Infrastructure/Weddy.Infrastructure.csproj", "Weddy.Infrastructure/"]

RUN dotnet restore "Weddy.API/Weddy.API.csproj"

# Copy everything else and build
COPY src/ .
WORKDIR "/src/Weddy.API"
RUN dotnet build "Weddy.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Weddy.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Установка PostgreSQL клиента для применения миграций
RUN apt-get update && \
    apt-get install -y postgresql-client && \
    rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .
COPY database/init.sql /app/database/init.sql
COPY database/apply-migrations.sh /app/apply-migrations.sh
RUN chmod +x /app/apply-migrations.sh

ENTRYPOINT ["/bin/sh", "-c", "/app/apply-migrations.sh && dotnet Weddy.API.dll"]

