#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["api/MM.APIs/MM.APIs.csproj", "api/MM.APIs/"]
COPY ["shared/mastermindlib/MastermindLib.csproj", "shared/mastermindlib/"]
RUN dotnet restore "api/MM.APIs/MM.APIs.csproj"
COPY . .
WORKDIR "/src/api/MM.APIs"
RUN dotnet build "MM.APIs.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MM.APIs.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MM.APIs.dll"]