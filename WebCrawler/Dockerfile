FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Install Node.js
RUN curl -fsSL https://deb.nodesource.com/setup_14.x | bash - \
    && apt-get install -y \
        nodejs \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src
COPY ["WebCrawler/WebCrawler.csproj", "WebCrawler/"]
RUN dotnet restore "WebCrawler/WebCrawler.csproj"
COPY . .
WORKDIR "/src/WebCrawler"
RUN dotnet build "WebCrawler.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebCrawler.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebCrawler.dll"]
