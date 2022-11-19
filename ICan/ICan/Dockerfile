#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ICan/ICan.csproj", "ICan/"]
COPY ["ICan.Jobs/ICan.Jobs.csproj", "ICan.Jobs/"]
COPY ["ICan.DomainModel/ICan.Common.csproj", "ICan.DomainModel/"]
COPY ["ICan.Data/ICan.Data.csproj", "ICan.Data/"]
COPY ["ICan.Business/ICan.Business.csproj", "ICan.Business/"]
RUN dotnet restore "ICan/ICan.csproj"
COPY . .
 
WORKDIR "/src/ICan"
RUN dotnet build "ICan.csproj" -c Release -o /app/build /p:EnvironmentName=Docker

FROM build AS publish
RUN dotnet publish "ICan.csproj" -c Release -o /app/publish /p:EnvironmentName=Docker

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
USER root
RUN mkdir sitedata
WORKDIR /app
ENTRYPOINT ["dotnet", "ICan.dll"]