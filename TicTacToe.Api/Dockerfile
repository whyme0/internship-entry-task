FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TicTacToe.Api/TicTacToe.Api.csproj", "TicTacToe.Api/"]
RUN dotnet restore "./TicTacToe.Api/TicTacToe.Api.csproj"
COPY . .
WORKDIR "/src/TicTacToe.Api"
RUN dotnet build "./TicTacToe.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TicTacToe.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

USER root
RUN mkdir -p /app/database && chown -R $APP_UID:$APP_UID /app/database
USER $APP_UID

ENTRYPOINT ["dotnet", "TicTacToe.Api.dll"]