# ---- Stage 1: Build ----
# Use .NET 10 SDK image to compile the source code
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy .csproj file to restore package (caching)
COPY TriviaAPI/TriviaAPI.csproj TriviaAPI/
RUN dotnet restore TriviaAPI/TriviaAPI.csproj

# Copy the other codes and then build
COPY TriviaAPI/ TriviaAPI/
RUN dotnet publish TriviaAPI/TriviaAPI.csproj -c Release -o /app/publish

# ---- Stage 2: Runtime ----
# Run on light weight ASP.NET Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Copy the one is built
COPY --from=build /app/publish .

# Create a folder for media upload
RUN mkdir -p /app/wwwroot/media

# Port config
EXPOSE 8080

# Run App
ENTRYPOINT ["dotnet", "TriviaAPI.dll"]
