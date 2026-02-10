FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY lms-be.sln .
COPY Lms.Api/Lms.Api.csproj Lms.Api/
COPY Lms.Tests/Lms.Tests.csproj Lms.Tests/
RUN dotnet restore

COPY . .
RUN dotnet publish Lms.Api/Lms.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Lms.Api.dll"]
