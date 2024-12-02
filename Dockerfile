FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DeskMotion/DeskMotion.csproj", "DeskMotion/"]
RUN dotnet restore "./DeskMotion/DeskMotion.csproj"
COPY . .
WORKDIR "/src/DeskMotion"
RUN dotnet build "DeskMotion.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DeskMotion.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Generate dev certificate
RUN apt-get update && \
    apt-get install -y openssl && \
    openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
        -keyout /app/aspnetapp.pfx -out /app/aspnetapp.pfx \
        -subj "/CN=localhost"

ENV ASPNETCORE_URLS=http://+:8080;https://+:8081
ENV ASPNETCORE_HTTPS_PORT=8081
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/app/aspnetapp.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=

ENTRYPOINT ["dotnet", "DeskMotion.dll"]
