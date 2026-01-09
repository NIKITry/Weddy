FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Weddy.UI.Invitation/Weddy.UI.Invitation.csproj", "Weddy.UI.Invitation/"]
RUN dotnet restore "Weddy.UI.Invitation/Weddy.UI.Invitation.csproj"

COPY . .
WORKDIR "/src/Weddy.UI.Invitation"
RUN dotnet build "Weddy.UI.Invitation.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Weddy.UI.Invitation.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Weddy.UI.Invitation.dll"]

