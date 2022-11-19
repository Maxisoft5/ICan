#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ICan.Jobs.Runner/ICan.Jobs.Runner.csproj", "ICan.Jobs.Runner/"]
RUN dotnet restore "ICan.Jobs.Runner/ICan.Jobs.Runner.csproj"
COPY . .
WORKDIR "/src/ICan.Jobs.Runner"
RUN dotnet build "ICan.Jobs.Runner.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ICan.Jobs.Runner.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ICan.Jobs.Runner.dll"]