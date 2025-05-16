FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar los archivos csproj y restaurar dependencias
COPY ["DemianzxBackend.sln", "DemianzxBackend.sln"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Web/Web.csproj", "src/Web/"]
COPY ["tests/Application.UnitTests/Application.UnitTests.csproj", "tests/Application.UnitTests/"]
COPY ["tests/Domain.UnitTests/Domain.UnitTests.csproj", "tests/Domain.UnitTests/"]
COPY ["tests/Application.FunctionalTests/Application.FunctionalTests.csproj", "tests/Application.FunctionalTests/"]
COPY ["tests/Infrastructure.IntegrationTests/Infrastructure.IntegrationTests.csproj", "tests/Infrastructure.IntegrationTests/"]
COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["Directory.Packages.props", "Directory.Packages.props"]
COPY ["global.json", "global.json"]

RUN dotnet restore "DemianzxBackend.sln"

# Copiar todo el c�digo y compilar
COPY . .
WORKDIR "/src"
RUN dotnet build "DemianzxBackend.sln" -c Release -o /app/build

# Publicar
FROM build AS publish
RUN dotnet publish "src/Web/Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configurar variables de entorno
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Exponer el puerto
EXPOSE 8080

# Definir el punto de entrada
ENTRYPOINT ["dotnet", "DemianzxBackend.Web.dll"]