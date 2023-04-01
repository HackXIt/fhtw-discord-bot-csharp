FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["BIC-FHTW.WebApp/BIC-FHTW.WebApp.csproj", "BIC-FHTW.WebApp/"]
RUN dotnet restore "BIC-FHTW.WebApp/BIC-FHTW.WebApp.csproj"
COPY . .
WORKDIR "/src/BIC-FHTW.WebApp"
RUN dotnet build "BIC-FHTW.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BIC-FHTW.WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BIC-FHTW.WebApp.dll"]
