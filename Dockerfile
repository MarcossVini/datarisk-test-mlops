FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/DatariskMLOps.API/DatariskMLOps.API.csproj", "src/DatariskMLOps.API/"]
COPY ["src/DatariskMLOps.Domain/DatariskMLOps.Domain.csproj", "src/DatariskMLOps.Domain/"]
COPY ["src/DatariskMLOps.Infrastructure/DatariskMLOps.Infrastructure.csproj", "src/DatariskMLOps.Infrastructure/"]

RUN dotnet restore "src/DatariskMLOps.API/DatariskMLOps.API.csproj"
COPY . .
WORKDIR "/src/src/DatariskMLOps.API"
RUN dotnet build "DatariskMLOps.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DatariskMLOps.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DatariskMLOps.API.dll"]
