FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["ERP.slnx", "./"]
COPY ["NuGet.Config", "./"]
COPY ["src/Domain/ERP.Domain.csproj", "src/Domain/"]
COPY ["src/Application/ERP.Application.csproj", "src/Application/"]
COPY ["src/Infrastructure/ERP.Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/API/ERP.API.csproj", "src/API/"]

RUN dotnet restore "src/API/ERP.API.csproj" --configfile NuGet.Config

COPY . .
RUN dotnet publish "src/API/ERP.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

EXPOSE 10000

ENTRYPOINT ["dotnet", "ERP.API.dll"]
