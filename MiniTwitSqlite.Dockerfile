# Use the official Microsoft .NET SDK image as the base image
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

# Set the working directory to /app
WORKDIR /app

# Copy the project file(s) to the container
COPY ./ ./

WORKDIR /app/Server/

# Restore dependencies
RUN dotnet restore

# Set the environment variable for the PostgreSQL database
ENV ConnectionString ""

WORKDIR /app
# Build the application
RUN dotnet build -c Release -o /app/build

# Publish the application
RUN dotnet publish -c Release -o out

# Use the official Microsoft .NET runtime image as the base image for the final image
FROM mcr.microsoft.com/dotnet/aspnet:7.0

# Set the working directory to /app
WORKDIR /app

COPY /tmp /tmp
# Copy the published output from the build stage to the runtime container
COPY --from=build-env /app/out .

# Expose port 80 for HTTP traffic
EXPOSE 80
EXPOSE 5235

# Start the application
ENTRYPOINT ["dotnet", "MiniTwit.Server.dll"]
