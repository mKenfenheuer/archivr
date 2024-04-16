#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build "Archivr.UI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Archivr.UI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apt-get update && apt-get -y install libgdiplus libc6-dev wget
RUN wget "https://github.com/wkhtmltopdf/packaging/releases/download/0.12.6.1-2/wkhtmltox_0.12.6.1-2.bullseye_$(dpkg --print-architecture).deb" -O wkhtmltox.deb 
RUN apt install -y ./wkhtmltox.deb 
RUN mkdir -p /app/Lib/Linux/ && cp /usr/local/bin/wkhtmltopdf /app/Lib/Linux/wkhtmltopdf
RUN chmod 755 /app/Lib/Linux/wkhtmltopdf
RUN chmod +x /app/Lib/Linux/wkhtmltopdf

ENTRYPOINT ["dotnet", "Archivr.UI.dll"]