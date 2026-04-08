FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files and Shared folder
COPY ["UserManagement.Api/UserManagement.Api.csproj", "UserManagement.Api/"]
COPY ["Shared/", "Shared/"]

# Restore dependencies
RUN dotnet restore "UserManagement.Api/UserManagement.Api.csproj"

# Copy everything else and build
COPY UserManagement.Api/ UserManagement.Api/
WORKDIR "/src/UserManagement.Api"
RUN dotnet build "UserManagement.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserManagement.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserManagement.Api.dll"]
