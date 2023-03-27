FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src/Simulator
COPY . .
WORKDIR SlimTwit
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
EXPOSE 80
EXPOSE 5236
ENV connection_string ""
WORKDIR /src/Simulator
COPY --from=build /src/Simulator/SlimTwit/out .

ENTRYPOINT ["dotnet", "SlimTwit.dll", "--urls","http://0.0.0.0:5236"]
