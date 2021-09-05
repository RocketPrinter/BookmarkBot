# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:5.0 as build
COPY . /src
WORKDIR /src
RUN dotnet restore -v n -s https://api.nuget.org/v3/index.json -s https://nuget.emzi0767.com/api/v3/index.json
RUN dotnet publish -v n -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet","Server.dll"]

# EXPOSE 80/tcp
# RUN chmod +x ./entrypoint.sh
# CMD /bin/bash ./entrypoint.sh 