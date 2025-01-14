﻿FROM mcr.microsoft.com/dotnet/runtime:7.0-bullseye-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye-slim AS build
WORKDIR /src
COPY ["Koala.Activity.Game.Handler.Service.csproj", "./"]
RUN dotnet restore "Koala.Activity.Game.Handler.Service.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "Koala.Activity.Game.Handler.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Koala.Activity.Game.Handler.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Koala.ActivityGameHandlerService.dll"]
