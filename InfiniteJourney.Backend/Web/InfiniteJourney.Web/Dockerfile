FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Web/InfiniteJourney.Web/InfiniteJourney.Web.csproj", "Web/InfiniteJourney.Web/"]
COPY ["Application/InfiniteJourney.Application/InfiniteJourney.Application.csproj", "Application/InfiniteJourney.Application/"]
COPY ["Domain/InfiniteJourney.Domain/InfiniteJourney.Domain.csproj", "Domain/InfiniteJourney.Domain/"]
COPY ["Infrustructure/InfiniteJourney.Infrustructure/InfiniteJourney.Infrustructure.csproj", "Infrustructure/InfiniteJourney.Infrustructure/"]
COPY ["AppShared/InfiniteJourney.Global.Shared/InfiniteJourney.Global.Shared.csproj", "AppShared/InfiniteJourney.Global.Shared/"]

RUN dotnet restore "Web/InfiniteJourney.Web/InfiniteJourney.Web.csproj"

COPY . .
RUN dotnet publish "Web/InfiniteJourney.Web/InfiniteJourney.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InfiniteJourney.Web.dll"]
