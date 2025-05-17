# Verwende ein .NET Runtime-Image für ARM (Raspberry Pi)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
#EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY SRMAgent.csproj ./
RUN dotnet restore

# Kopiere den gesamten Code und baue das Projekt
COPY . ./
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SRMAgent.dll"]