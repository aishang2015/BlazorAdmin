#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#cd src\BlazorAdmin
#docker build -f BlazorAdmin/Dockerfile -t blazoradmin:latest .
#docker run --name BlazorAdmin2 -p :37218 -d blazoradmin:latest

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 37218

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["BlazorAdmin/BlazorAdmin.csproj", "BlazorAdmin/"]
RUN dotnet restore "BlazorAdmin/BlazorAdmin.csproj"
COPY . .
WORKDIR "/src/BlazorAdmin"
RUN dotnet build "BlazorAdmin.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlazorAdmin.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BlazorAdmin.dll"]