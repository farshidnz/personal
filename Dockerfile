# Stage 1 build
# FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
FROM mcr.microsoft.com/dotnet/sdk:5.0-focal AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY source/Cashrewards3API/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY source/Cashrewards3API/ ./

# Take a copy of appsettings for use by new jenkins pipeline so we don't break ramtin
RUN cp appsettings.json appsettings.devops4.json

# Ramtin pipeline does it's own transform
RUN rm appsettings.json

# RUN dotnet publish -c Release -o out
RUN dotnet publish -r linux-x64 --self-contained true /p:PublishSingleFile=true -c Release -o out

# Stage 2 run
FROM mcr.microsoft.com/dotnet/aspnet:5.0

RUN apt update; \
    apt install -y nginx

WORKDIR /app

COPY nginx.default.conf /etc/nginx/sites-available/default

COPY --from=build-env /app/out /app

EXPOSE 80

ADD source/docker-entrypoint.sh /app

ENV ASPNETCORE_URLS http://+:5000

RUN ["chmod", "+x", "/app/docker-entrypoint.sh"]
ENTRYPOINT ["/app/docker-entrypoint.sh"]

