FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Archdraw.Presentation/Archdraw.Presentation.csproj", "Archdraw.Presentation/"]
COPY ["Archdraw.CrossCutting/Archdraw.CrossCutting.csproj", "Archdraw.CrossCutting/"]
COPY ["Archdraw.Application/Archdraw.Application.csproj", "Archdraw.Application/"]
COPY ["Archdraw.Domain/Archdraw.Domain.csproj", "Archdraw.Domain/"]
COPY ["Archdraw.BuildingBlocks/Archdraw.BuildingBlocks.csproj", "Archdraw.BuildingBlocks/"]
COPY ["Archdraw.Infrastructure/Archdraw.Infrastructure.csproj", "Archdraw.Infrastructure/"]
COPY ["Archdraw.BackgroundJobs/Archdraw.BackgroundJobs.csproj", "Archdraw.BackgroundJobs/"]
RUN dotnet restore "Archdraw.Presentation/Archdraw.Presentation.csproj"
COPY . .
WORKDIR "/src/Archdraw.Presentation"
RUN dotnet publish "./Archdraw.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Archdraw.Presentation.dll"]
