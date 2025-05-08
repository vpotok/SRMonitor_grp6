# Use the official .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Add local tools to PATH
ENV PATH="${PATH}:/app/.config/dotnet-tools"

# Copy the project file and restore dependencies
COPY ./SRMCore.csproj ./
RUN dotnet restore

# Copy the rest of the application files
COPY . ./
RUN dotnet publish -c Release -o out

# Use the runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/out .

# Expose the port the application runs on
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
EXPOSE 443

# Run the application
ENTRYPOINT ["dotnet", "SRMCore.dll"]