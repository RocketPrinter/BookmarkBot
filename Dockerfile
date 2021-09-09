# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:5.0 as build

WORKDIR /app
COPY ./Server .

#build
RUN dotnet restore -v n -s https://api.nuget.org/v3/index.json -s https://nuget.emzi0767.com/api/v3/index.json
RUN dotnet build -v n --no-restore 

# install dotnet ef
RUN dotnet tool install --global dotnet-ef
ENV PATH $PATH:/root/.dotnet/tools

# entrypoint script
RUN chmod +x ./entrypoint.sh
CMD /bin/bash ./entrypoint.sh 