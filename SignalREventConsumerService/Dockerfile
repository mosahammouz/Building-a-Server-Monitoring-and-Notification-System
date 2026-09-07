FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore SignalREventConsumerService/SignalREventConsumerService.csproj

RUN dotnet publish SignalREventConsumerService/SignalREventConsumerService.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SignalREventConsumerService.dll"]