#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 37318

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["BlazorAdmin.Web/BlazorAdmin.Web.csproj", "BlazorAdmin.Web/"]
COPY ["BlazorAdmin.Core/BlazorAdmin.Core.csproj", "BlazorAdmin.Core/"]
COPY ["BlazorAdmin.Data/BlazorAdmin.Data.csproj", "BlazorAdmin.Data/"]
COPY ["BlazorAdmin.Modules/BlazorAdmin.Log/BlazorAdmin.Log.csproj", "BlazorAdmin.Modules/BlazorAdmin.Log/"]
COPY ["BlazorAdmin.Modules/BlazorAdmin.Rbac/BlazorAdmin.Rbac.csproj", "BlazorAdmin.Modules/BlazorAdmin.Rbac/"]
COPY ["BlazorAdmin.Modules/BlazorAdmin.Setting/BlazorAdmin.Setting.csproj", "BlazorAdmin.Modules/BlazorAdmin.Setting/"]
RUN dotnet restore "./BlazorAdmin.Web/./BlazorAdmin.Web.csproj"
COPY . .
WORKDIR "/src/BlazorAdmin.Web"
RUN dotnet build "./BlazorAdmin.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BlazorAdmin.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlazorAdmin.Web.dll"]