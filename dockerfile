FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY MediaRating.Api/*.csproj MediaRating.Api/
COPY MediaRating.Domain/*.csproj MediaRating.Domain/
COPY MediaRating.Infrastructure/*.csproj MediaRating.Infrastructure/

# RESTORE AUF PROJEKT, NICHT AUF SLN
RUN dotnet restore MediaRating.Api/MediaRatings.Api.csproj

COPY . .

RUN dotnet publish MediaRating.Api/MediaRatings.Api.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MediaRatings.Api.dll"]
