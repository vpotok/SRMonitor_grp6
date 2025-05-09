# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SRMCore/*.csproj ./SRMCore/
RUN dotnet restore ./SRMCore/SRMCore.csproj

COPY . .
WORKDIR /src/SRMCore
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# HTTPS support
ENV ASPNETCORE_URLS=https://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
ENTRYPOINT ["dotnet", "SRMCore.dll"]
