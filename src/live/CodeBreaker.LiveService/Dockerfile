#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["live/CodeBreaker.LiveService/CodeBreaker.LiveService.csproj", "live/CodeBreaker.LiveService/"]
COPY ["shared/CodeBreaker.LiveService.Shared/CodeBreaker.LiveService.Shared.csproj", "shared/CodeBreaker.LiveService.Shared/"]
COPY ["shared/CodeBreaker.Shared/CodeBreaker.Shared.csproj", "shared/CodeBreaker.Shared/"]
RUN dotnet restore "live/CodeBreaker.LiveService/CodeBreaker.LiveService.csproj"
COPY . .
WORKDIR "/src/live/CodeBreaker.LiveService"
RUN dotnet build "CodeBreaker.LiveService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CodeBreaker.LiveService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CodeBreaker.LiveService.dll"]