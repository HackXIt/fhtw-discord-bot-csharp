FROM mcr.microsoft.com/dotnet/aspnet:6.0.22 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0.414 AS build
WORKDIR /src

# Check network connectivity
#RUN apt-get update && apt-get install -y curl
#RUN curl https://api.nuget.org/v3/index.json
# Copy the solution file
COPY ["bic-fhtw-discord-bot.sln", "."]

# Copy each csproj and restore as distinct layers
COPY ["BIC-FHTW.WebApp/BIC-FHTW.WebApp.csproj", "BIC-FHTW.WebApp/"]
COPY ["BIC-FHTW.Database/BIC-FHTW.Database.csproj", "BIC-FHTW.Database/"]
COPY ["BIC-FHTW.DiscordBot/BIC-FHTW.DiscordBot.csproj", "BIC-FHTW.DiscordBot/"]
COPY ["BIC-FHTW.Scraper/BIC-FHTW.Scraper.csproj", "BIC-FHTW.Scraper/"]
COPY ["BIC-FHTW.Shared/BIC-FHTW.Shared.csproj", "BIC-FHTW.Shared/"]
COPY ["BIC-FHTW.ThirdParty/BIC-FHTW.ThirdParty.csproj", "BIC-FHTW.ThirdParty/"]
COPY ["BIC-FHTW.WebClient/BIC-FHTW.WebClient.csproj", "BIC-FHTW.WebClient/"]

# Restore the entire solution
RUN dotnet restore

# Copy the rest of the files
COPY . .

# Build the WebApp project
WORKDIR "/src/BIC-FHTW.WebApp"
RUN dotnet build "BIC-FHTW.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BIC-FHTW.WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BIC-FHTW.WebApp.dll"]