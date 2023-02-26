# Use the official Microsoft .NET SDK image as the base image
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Set the working directory to /app
WORKDIR /app

# Copy the project file(s) to the container
COPY Server/* ./
COPY Shared/* ./
COPY Client/* ./
COPY MiniTwit.sln ./

# Restore dependencies
RUN dotnet restore

# Set the environment variable for the PostgreSQL database
ENV ConnectionString ""

# Build the application
RUN dotnet build -c Release -o /app/build

# Publish the application
RUN dotnet publish -c Release -o /app/publish

# Use the official Microsoft .NET runtime image as the base image for the final image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime

# Set the working directory to /app
WORKDIR /app

# Copy the published output from the build stage to the runtime container
COPY --from=build /app/publish .

# Expose port 80 for HTTP traffic
EXPOSE 80

# Start the application
ENTRYPOINT ["dotnet", "YourProjectName.dll"]
