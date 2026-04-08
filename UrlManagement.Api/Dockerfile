FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files and Shared folder
COPY ["UrlManagement.Api/UrlManagement.Api.csproj", "UrlManagement.Api/"]
COPY ["Shared/", "Shared/"]

# Restore dependencies
RUN dotnet restore "UrlManagement.Api/UrlManagement.Api.csproj"

# Copy everything else and build
COPY UrlManagement.Api/ UrlManagement.Api/
WORKDIR "/src/UrlManagement.Api"
RUN dotnet build "UrlManagement.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UrlManagement.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UrlManagement.Api.dll"]
