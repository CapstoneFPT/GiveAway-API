#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0.400 AS base
USER app
WORKDIR /app
EXPOSE 8081
EXPOSE 8080

VOLUME ["/etc/letsencrypt"]

FROM mcr.microsoft.com/dotnet/sdk:8.0.400 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WebApi/WebApi.csproj", "WebApi/"]
COPY ["Services/Services.csproj", "Services/"]
COPY ["Repositories/Repositories.csproj", "Repositories/"]
COPY ["BusinessObjects/BusinessObjects.csproj", "BusinessObjects/"]
RUN dotnet restore "./WebApi/WebApi.csproj"
COPY . .
COPY ["Services/MailTemplate", "/app/Services/MailTemplate"]
WORKDIR "/src/WebApi"
RUN dotnet build "./WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /app/Services/MailTemplate /app/Services/MailTemplate
ENTRYPOINT ["dotnet", "WebApi.dll"]