FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
# DO NOT CHANGE THE WORKDIRS THEY ARE VERY PRONE TO ERRORS
# EVEN THOUGH IT SAYS THAT THEY SHOULD BE ABSOLUTE PATHS DO NOT LISTEN TO ITS LIES. THIS IS CORRECT.
WORKDIR /src/Simulator
COPY . .
WORKDIR /src/Simulator/SlimTwit/
RUN dotnet restore && dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
EXPOSE 80
EXPOSE 5236
ENV connection_string ""
WORKDIR /src/Simulator
COPY --from=build /src/Simulator/SlimTwit/out .

ENTRYPOINT ["dotnet", "SlimTwit.dll", "--urls","http://0.0.0.0:5236"]
