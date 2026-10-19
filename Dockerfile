FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SecureCadPortal.slnx ./
COPY src/SecureCadPortal.Domain/SecureCadPortal.Domain.csproj src/SecureCadPortal.Domain/
COPY src/SecureCadPortal.Application/SecureCadPortal.Application.csproj src/SecureCadPortal.Application/
COPY src/SecureCadPortal.Infrastructure/SecureCadPortal.Infrastructure.csproj src/SecureCadPortal.Infrastructure/
COPY src/SecureCadPortal.Api/SecureCadPortal.Api.csproj src/SecureCadPortal.Api/

RUN dotnet restore src/SecureCadPortal.Api/SecureCadPortal.Api.csproj

COPY src/ src/

RUN dotnet publish src/SecureCadPortal.Api/SecureCadPortal.Api.csproj \
    --configuration Release \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "SecureCadPortal.Api.dll"]
