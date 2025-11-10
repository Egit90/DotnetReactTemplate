# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Crystal.WebApi/Crystal.WebApi.csproj", "Crystal.WebApi/"]
COPY ["../Crystal.Core/Crystal.Core.csproj", "Crystal.Core/"]
COPY ["Directory.Packages.props", "./"]
RUN dotnet restore "Crystal.WebApi/Crystal.WebApi.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Crystal.WebApi"
RUN dotnet build "Crystal.WebApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Crystal.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Crystal.WebApi.dll"]