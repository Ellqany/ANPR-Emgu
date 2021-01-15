FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ANPR.csproj", "./"]
RUN dotnet restore "ANPR.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ANPR.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ANPR.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ANPR.dll"]
