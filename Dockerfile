FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

# Install PowerShell
RUN apt-get update \
    && apt-get install -y wget \
    && wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && apt-get update \
    && apt-get install -y powershell \
    && apt-get install libglib2.0-0 -y \
    && apt-get install libnss3 -y \
    && apt-get install libnspr4 -y \
    && apt-get install libatk1.0-0 -y \
    && apt-get install libatk-bridge2.0-0 -y \
    && apt-get install libcups2 -y \
    && apt-get install libdrm2 -y \
    && apt-get install libdbus-1-3 -y \
    && apt-get install libexpat1 -y \
    && apt-get install libxcb1 -y \
    && apt-get install libxkbcommon0 -y \
    && apt-get install libx11-6 -y \
    && apt-get install libxcomposite1 -y \
    && apt-get install libxdamage1 -y \
    && apt-get install libxext6 -y \
    && apt-get install libxfixes3 -y \
    && apt-get install libxrandr2 -y \
    && apt-get install libgbm1 -y \
    && apt-get install libgtk-3-0 -y \
    && apt-get install libpango-1.0-0 -y \
    && apt-get install libcairo2 -y \
    && apt-get install libasound2 -y \
    && apt-get install libatspi2.0-0 -y \
    && apt-get install libxshmfence1 -y


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
RUN pwsh -Command "./playwright.ps1 install"

CMD ASPNETCORE_URLS=http://*:$PORT dotnet TGBot_TW_Stock_Polling.dll