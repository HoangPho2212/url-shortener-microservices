# Stage 1: Build Aplication
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy file .csproj of API and Shared to restore library
COPY ["UrlManagement.Api/UrlManagement.Api.csproj", "UrlManagement.Api/"]
COPY ["Shared/Shared.csproj", "Shared/"]

# Restore command (dowloading NuGet)
RUN dotnet restore "UrlManagement.Api/UrlManagement.Api.csproj"

# Copy the whole code in container
COPY . .

# Navigate to the API directory and build
WORKDIR "/src/UrlManagement.Api"
RUN dotnet build "UrlManagement.Api.csproj" -c Release -o /app/build

# Stage 2: Publish (Package executable file)
FROM build AS publish
RUN dotnet publish "UrlManagement.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .

# Launch command - Note that the .dll file name must be exact
ENTRYPOINT ["dotnet", "UrlManagement.Api.dll"]