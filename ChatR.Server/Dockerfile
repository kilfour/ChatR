# ----------------------------
# Build stage
# ----------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj first for better caching
COPY ./ChatR.Server.csproj ./
RUN dotnet restore ./ChatR.Server.csproj

# copy the rest
COPY . ./
RUN dotnet publish ./ChatR.Server.csproj -c Release -o /app/publish /p:UseAppHost=false

# ----------------------------
# Runtime stage
# ----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

COPY --from=build /app/publish ./

EXPOSE 8080

ENTRYPOINT ["dotnet", "ChatR.Server.dll"]