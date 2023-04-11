FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src/minitwit
COPY . .
WORKDIR /src/minitwit/Server/
RUN dotnet restore && dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
EXPOSE 80
EXPOSE 5235
ENV connection_string ""
WORKDIR /src/minitwit
COPY --from=build /src/minitwit/Server/out .

ENTRYPOINT ["dotnet", "MiniTwit.Server.dll"]
