﻿FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ChampionsOfKhazad.Bot.csproj", "."]
RUN dotnet restore "ChampionsOfKhazad.Bot.csproj"
COPY . .
RUN dotnet build "ChampionsOfKhazad.Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChampionsOfKhazad.Bot.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:7.0
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChampionsOfKhazad.Bot.dll"]
