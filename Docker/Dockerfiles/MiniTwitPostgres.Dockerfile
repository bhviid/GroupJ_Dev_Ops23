FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
# DO NOT CHANGE THE WORKDIRS THEY ARE VERY PRONE TO ERRORS
# EVEN THOUGH IT SAYS THAT THEY SHOULD BE ABSOLUTE PATHS DO NOT LISTEN TO ITS LIES. THIS IS CORRECT.
WORKDIR /src/minitwit
COPY . .
WORKDIR Server
RUN dotnet restore && dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
EXPOSE 80
EXPOSE 5235
ENV connection_string ""
WORKDIR /src/minitwit
COPY --from=build /src/minitwit/Server/out .

ENTRYPOINT ["dotnet", "MiniTwit.Server.dll"]
