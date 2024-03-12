FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

# Install PowerShell
RUN apt-get update \
    && apt-get install -y wget \
    && wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y powershell \
    && apt-get upgrade

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY *.csproj ./
RUN dotnet restore 
COPY . .
WORKDIR "/src/."
RUN dotnet build  -c Release -o /app/build

FROM build AS publish
RUN dotnet publish  -c Release -o /app/publish 

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
RUN pwsh -Command "./playwright.ps1 install --with-deps chromium"

CMD ASPNETCORE_URLS=http://*:$PORT dotnet TGBot_TW_Stock_Polling.dll