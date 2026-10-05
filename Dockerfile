FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Önce sadece proje dosyaları: restore katmanı cache'lenir.
COPY Directory.Build.props ./
COPY src/Test.Domain/Test.Domain.csproj src/Test.Domain/
COPY src/Test.Application/Test.Application.csproj src/Test.Application/
COPY src/Test.Infrastructure/Test.Infrastructure.csproj src/Test.Infrastructure/
COPY src/Test.API/Test.API.csproj src/Test.API/
RUN dotnet restore src/Test.API/Test.API.csproj

COPY src/ src/
RUN dotnet publish src/Test.API/Test.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Root olmayan kullanıcı; log klasörüne yazabilsin.
RUN mkdir -p /app/logs && chown -R $APP_UID /app/logs
USER $APP_UID

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Test.API.dll"]
