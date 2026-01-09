FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Weddy.UI.Admin/Weddy.UI.Admin.csproj", "Weddy.UI.Admin/"]
RUN dotnet restore "Weddy.UI.Admin/Weddy.UI.Admin.csproj"

COPY . .
WORKDIR "/src/Weddy.UI.Admin"
RUN dotnet build "Weddy.UI.Admin.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Weddy.UI.Admin.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Weddy.UI.Admin.dll"]

