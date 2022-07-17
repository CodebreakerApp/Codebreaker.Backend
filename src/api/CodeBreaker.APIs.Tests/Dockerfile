#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["api/CodeBreaker.APIs.Tests/CodeBreaker.APIs.Tests.csproj", "api/CodeBreaker.APIs.Tests/"]
COPY ["api/CodeBreaker.APIs/CodeBreaker.APIs.csproj", "api/CodeBreaker.APIs/"]
COPY ["shared/CodeBreaker.Shared/CodeBreaker.Shared.csproj", "shared/CodeBreaker.Shared/"]
RUN dotnet restore "api/CodeBreaker.APIs.Tests/CodeBreaker.APIs.Tests.csproj"
COPY . .
WORKDIR "/src/api/CodeBreaker.APIs.Tests"
RUN dotnet build "CodeBreaker.APIs.Tests.csproj" -c Release -o /app/build

# run unit tests on docker build
RUN dotnet test

# run unit tests on docker run
ENTRYPOINT ["dotnet", "test"]