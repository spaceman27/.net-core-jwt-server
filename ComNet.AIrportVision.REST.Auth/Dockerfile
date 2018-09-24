FROM microsoft/dotnet:2.1.2-aspnetcore-runtime-alpine3.7 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.1.302-sdk-alpine3.7 AS build
WORKDIR /src
RUN mkdir .nuget
COPY .nuget .nuget
COPY ComNet.AIrportVision.REST.Auth/ComNet.AirportVision.REST.Auth.csproj ComNet.AIrportVision.REST.Auth/
COPY Vision.Core.REST.Auth.Service/Vision.Core.REST.Auth.Service.csproj Vision.Core.REST.Auth.Service/
COPY Vision.Core.REST.Auth.Data/Vision.Core.REST.Auth.Data.csproj Vision.Core.REST.Auth.Data/
COPY Vision.Core.REST.Auth.DTO/Vision.Core.REST.Auth.DTO.csproj Vision.Core.REST.Auth.DTO/
RUN dotnet restore ComNet.AIrportVision.REST.Auth/ComNet.AirportVision.REST.Auth.csproj --configfile .nuget/NuGet.config
COPY . .
WORKDIR /src/ComNet.AIrportVision.REST.Auth
RUN dotnet build ComNet.AirportVision.REST.Auth.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish ComNet.AirportVision.REST.Auth.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
COPY ComNet.AIrportVision.REST.Auth/ComNet.AirportVision.REST.Auth.pfx  /root/.aspnet/https/ComNet.AirportVision.REST.Auth.pfx
ENV Kestrel:Certificates:Default:Path=/root/.aspnet/https/ComNet.AirportVision.REST.Auth.pfx
ENV Kestrel:Certificates:Default:Password=7373952b-d659-4439-9694-56b02e12b250
ENV ASPNETCORE_URLS=https://+:443;http://+:80
ENTRYPOINT ["dotnet", "ComNet.AirportVision.REST.Auth.dll"]
