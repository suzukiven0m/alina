FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Shared/ Shared/
COPY src/ShipEdge/ ShipEdge/
WORKDIR /src/ShipEdge
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CargoShipMonitoring.ShipEdge.dll"]
