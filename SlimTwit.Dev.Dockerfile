FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

CMD ["dotnet", "watch", "run", "--project", "./SlimTwit", "--urls", "http://0.0.0.0:5551"]