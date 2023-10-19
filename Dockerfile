FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY "Zeepkist.GTR.Backend.csproj" .
RUN dotnet restore
COPY . .
WORKDIR /src
RUN dotnet build -c Release -o /app/build

FROM build AS publish
WORKDIR "/src"
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Add healthcheck
HEALTHCHECK --interval=30s --timeout=3s --retries=3 CMD curl --fail http://localhost/healthcheck || exit 1

ENTRYPOINT ["dotnet", "TNRD.Zeepkist.GTR.Backend.dll"]
