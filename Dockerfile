FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
COPY . . /
WORKDIR "/Server"
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
EXPOSE 80
EXPOSE 5235
WORKDIR "/Server"
COPY --from=build /Server/out .

ENTRYPOINT ["dotnet", "MiniTwit.Server.dll"]
