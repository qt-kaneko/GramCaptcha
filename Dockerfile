FROM mcr.microsoft.com/dotnet/sdk:9.0

COPY . /app
WORKDIR /app

RUN dotnet build

ENTRYPOINT ["dotnet", "bin/GramCaptcha.dll"]