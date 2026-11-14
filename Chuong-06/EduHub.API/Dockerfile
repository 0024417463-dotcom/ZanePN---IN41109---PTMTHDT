# Stage 1: Build (Alpine SDK)
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
# Copy csproj trước để cache lớp restore
COPY ["EduHub.API/EduHub.API.csproj", "EduHub.API/"]
COPY ["EduHub.Application/EduHub.Application.csproj", "EduHub.Application/"]
COPY ["EduHub.Domain/EduHub.Domain.csproj", "EduHub.Domain/"]
COPY ["EduHub.Infrastructure/EduHub.Infrastructure.csproj", "EduHub.Infrastructure/"]
RUN dotnet restore "EduHub.API/EduHub.API.csproj"
COPY EduHub.API/ EduHub.API/
COPY EduHub.Application/ EduHub.Application/
COPY EduHub.Domain/ EduHub.Domain/
COPY EduHub.Infrastructure/ EduHub.Infrastructure/
RUN dotnet publish "EduHub.API/EduHub.API.csproj" \
    -c Release -o /app/publish --no-restore

# Stage 2: Runtime (Alpine ASP.NET, nhỏ hơn nhiều)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "EduHub.API.dll"]
