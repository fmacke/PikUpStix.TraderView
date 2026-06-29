# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["PikUpStix.TraderView.sln", "./"]
COPY ["traderview/traderview.Server/traderview.Server.csproj", "traderview/traderview.Server/"]
COPY ["IKBR_Report_Puller/PikUpStix.TraderView.csproj", "IKBR_Report_Puller/"]

# Restore dependencies
RUN dotnet restore "traderview/traderview.Server/traderview.Server.csproj"

# Copy all source files
COPY . .
WORKDIR "/src/traderview/traderview.Server"

# Build the application (BuildingInsideDocker=true skips JS project build)
RUN dotnet build "traderview.Server.csproj" -c Release -o /app/build /p:BuildingInsideDocker=true

# Publish stage
FROM build AS publish
RUN dotnet publish "traderview.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:BuildingInsideDocker=true

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

# Create the documents directory for IBKR reports
RUN mkdir -p /app/documents && chmod -R 755 /app/documents

# Set environment to Docker so it uses appsettings.Docker.json
ENV ASPNETCORE_ENVIRONMENT=Docker

ENTRYPOINT ["dotnet", "traderview.Server.dll"]
