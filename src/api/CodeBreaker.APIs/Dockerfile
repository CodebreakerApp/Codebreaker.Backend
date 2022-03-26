#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["api/CodeBreaker.APIs/CodeBreaker.APIs.csproj", "api/CodeBreaker.APIs/"]
COPY ["shared/CodeBreaker.Shared/CodeBreaker.Shared.csproj", "shared/CodeBreaker.Shared/"]
RUN dotnet restore "api/CodeBreaker.APIs/CodeBreaker.APIs.csproj"
COPY . .
WORKDIR "/src/api/CodeBreaker.APIs"
RUN dotnet build "CodeBreaker.APIs.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CodeBreaker.APIs.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CodeBreaker.APIs.dll"]