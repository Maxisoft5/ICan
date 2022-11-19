FROM ubuntu

RUN apt-get update

# CMD ["echo", "Hello World"]
# FROM mcr.microsoft.com/dotnet/core/sdk:3.1.101-alpine3.10 AS build
#FROM arey/mysql-client AS build 
#WORKDIR /src
#EXPOSE 80
 
#COPY ["ICan", "ICan"]
#WORKDIR "/src/ICan/ICan"
 
#RUN dotnet publish -c Release  -o /app
#WORKDIR /app
#ENV ASPNETCORE_ENVIRONMENT Development
# ENTRYPOINT ["dotnet", "ICan.dll"]