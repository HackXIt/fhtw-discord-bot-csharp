FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["BIC-FHTW.DiscordBot/BIC-FHTW.DiscordBot.csproj", "BIC-FHTW.DiscordBot/"]
RUN dotnet restore "BIC-FHTW.DiscordBot/BIC-FHTW.DiscordBot.csproj"
COPY . .
WORKDIR "/src/BIC-FHTW.DiscordBot"
RUN dotnet build "BIC-FHTW.DiscordBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BIC-FHTW.DiscordBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BIC-FHTW.DiscordBot.dll"]
