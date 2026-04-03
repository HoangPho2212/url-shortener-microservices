# Stage 1: Build the application
# Use the official .NET 9.0 SDK image for building the source code
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
# Including the Shared project is mandatory as the API depends on it
COPY ["UserManagement.Api/UserManagement.Api.csproj", "UserManagement.Api/"]
COPY ["Shared/Shared.csproj", "Shared/"]

# Run restore to download NuGet packages
RUN dotnet restore "UserManagement.Api/UserManagement.Api.csproj"

# Copy the remaining source code into the container
COPY . .

# Change directory to the API folder and build the project
WORKDIR "/src/UserManagement.Api"
RUN dotnet build "UserManagement.Api.csproj" -c Release -o /app/build

# Stage 2: Publish the application
# Generate the executable files without the overhead of the build environment
FROM build AS publish
RUN dotnet publish "UserManagement.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Create the final production image
# Use the lightweight ASP.NET runtime image for better performance and security
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

# Copy the published output from the publish stage
COPY --from=publish /app/publish .

# Define the entry point for the container to run the API
ENTRYPOINT ["dotnet", "UserManagement.Api.dll"]