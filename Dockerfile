# =============================================
# Stage 1: Build
# =============================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution file
COPY Social_Media_Chatting_APP.slnx ./

# Copy all csproj files first (layer caching — only re-restore if .csproj changes)
COPY Social-Media-Chatting-APP-Domain/Social-Media-Chatting-APP-Domain.csproj                         Social-Media-Chatting-APP-Domain/
COPY Social-Media-Chatting-APP-Service/Social-Media-Chatting-APP-Service.csproj                       Social-Media-Chatting-APP-Service/
COPY Social-Media-Chatting-APP-ServiceAbstraction/Social-Media-Chatting-APP-ServiceAbstraction.csproj Social-Media-Chatting-APP-ServiceAbstraction/
COPY Social-Media-Chatting-APP-Persistence/Social-Media-Chatting-APP-Persistence.csproj               Social-Media-Chatting-APP-Persistence/
COPY Social-Media-Chatting-APP-Presentation/Social-Media-Chatting-APP-Presentation.csproj             Social-Media-Chatting-APP-Presentation/
COPY Social-Media-Chatting-APP-SharedLibrary/Social-Media-Chatting-APP-SharedLibrary.csproj           Social-Media-Chatting-APP-SharedLibrary/
COPY Social-Media-Chatting-APP-Web/Social-Media-Chatting-APP-Web.csproj                               Social-Media-Chatting-APP-Web/

# Restore all packages
RUN dotnet restore Social_Media_Chatting_APP.slnx

# Copy the rest of the source code
COPY . .

# Build & publish the Web (entry point) project in Release mode
RUN dotnet publish Social-Media-Chatting-APP-Web/Social-Media-Chatting-APP-Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# =============================================
# Stage 2: Runtime
# =============================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create the uploads directory that will be mounted as a volume
RUN mkdir -p /app/wwwroot/uploads

# Copy published output from build stage
COPY --from=build /app/publish .

# Expose HTTP port
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Social-Media-Chatting-APP-Web.dll"]
