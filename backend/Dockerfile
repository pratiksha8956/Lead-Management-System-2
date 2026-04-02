FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY LeadManagement.sln ./
COPY LeadManagement.Domain/LeadManagement.Domain.csproj LeadManagement.Domain/
COPY LeadManagement.Application/LeadManagement.Application.csproj LeadManagement.Application/
COPY LeadManagement.Infrastructure/LeadManagement.Infrastructure.csproj LeadManagement.Infrastructure/
COPY LeadManagement.Api/LeadManagement.Api.csproj LeadManagement.Api/
COPY LeadManagement.Tests/LeadManagement.Tests.csproj LeadManagement.Tests/

RUN dotnet restore LeadManagement.Api/LeadManagement.Api.csproj

COPY . .
WORKDIR /src/LeadManagement.Api
RUN dotnet publish -c Release -o /app/publish   # ✅ FIXED

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "LeadManagement.Api.dll"]