#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#USER app
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Biblioteca/Biblioteca.csproj", "Biblioteca/"]
RUN dotnet restore "./Biblioteca/./Biblioteca.csproj"
COPY . .
WORKDIR "/src/Biblioteca"
#RUN dotnet build "./Biblioteca.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet build "Biblioteca.csproj" -c Release -o /app/build

FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./Biblioteca.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
RUN dotnet publish "Biblioteca.csproj" -c Release -o /app/build 

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Biblioteca.dll"]