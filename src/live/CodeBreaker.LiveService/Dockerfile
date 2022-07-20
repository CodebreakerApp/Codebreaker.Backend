#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["LiveService/LiveService.csproj", "LiveService/"]
RUN dotnet restore "LiveService/LiveService.csproj"
COPY . .
WORKDIR "/src/LiveService"
RUN dotnet build "LiveService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LiveService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LiveService.dll"]
