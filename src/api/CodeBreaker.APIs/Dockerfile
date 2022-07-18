#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["api/CodeBreaker.APIs/CodeBreaker.APIs.csproj", "api/CodeBreaker.APIs/"]
COPY ["shared/CodeBreaker.Shared/CodeBreaker.Shared.csproj", "shared/CodeBreaker.Shared/"]
COPY ["api/CodeBreaker.APIs.Tests/CodeBreaker.APIs.Tests.csproj", "api/CodeBreaker.APIs.Tests/"]
RUN dotnet restore "api/CodeBreaker.APIs/CodeBreaker.APIs.csproj"
RUN dotnet restore "api/CodeBreaker.APIs.Tests/CodeBreaker.APIs.Tests.csproj"
COPY . .
WORKDIR "/src/api/CodeBreaker.APIs.Tests"
RUN dotnet test "CodeBreaker.APIs.Tests.csproj"
WORKDIR "/src/api/CodeBreaker.APIs"
RUN dotnet build "CodeBreaker.APIs.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CodeBreaker.APIs.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CodeBreaker.APIs.dll"]
