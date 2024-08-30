FROM mcr.microsoft.com/dotnet/aspnet:6.0.22 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
# https://devblogs.microsoft.com/dotnet/improving-multiplatform-container-support/
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:6.0.414 AS build
ARG TARGETARCH
WORKDIR /src

# Check network connectivity
#RUN apt-get update && apt-get install -y curl
#RUN curl https://api.nuget.org/v3/index.json
# Copy the solution file
COPY ["fhtw-discord-bot.sln", "."]

# Copy each csproj and restore as distinct layers
COPY ["FHTW.WebApp/FHTW.WebApp.csproj", "FHTW.WebApp/"]
COPY ["FHTW.Database/FHTW.Database.csproj", "FHTW.Database/"]
COPY ["FHTW.DiscordBot/FHTW.DiscordBot.csproj", "FHTW.DiscordBot/"]
COPY ["FHTW.Scraper/FHTW.Scraper.csproj", "FHTW.Scraper/"]
COPY ["FHTW.Shared/FHTW.Shared.csproj", "FHTW.Shared/"]
COPY ["FHTW.ThirdParty/FHTW.ThirdParty.csproj", "FHTW.ThirdParty/"]
COPY ["FHTW.WebClient/FHTW.WebClient.csproj", "FHTW.WebClient/"]

# Restore the entire solution
RUN dotnet restore

# Copy the rest of the files
COPY . .

# Build the WebApp project
WORKDIR "/src/FHTW.WebApp"
RUN dotnet build "FHTW.WebApp.csproj" -c Release -o /app/build -a $TARGETARCH

FROM build AS publish
RUN dotnet publish "FHTW.WebApp.csproj" -c Release -o /app/publish -a $TARGETARCH

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FHTW.WebApp.dll"]