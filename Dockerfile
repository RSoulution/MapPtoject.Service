FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TechnicalTask.Service.sln .
COPY TechnicalTask.Service.Console/TechnicalTask.Service.Console.csproj ./TechnicalTask.Service.Console/
COPY TechnicalTask.Service.DAL/TechnicalTask.Service.DAL.csproj ./TechnicalTask.Service.DAL/
COPY TechnicalTask.Service.Stub/TechnicalTask.Service.Stub.csproj ./TechnicalTask.Service.Stub/

COPY TechnicalTask.Entities/TechnicalTask.Entities.csproj ./TechnicalTask.Entities/

# Виконуємо restore для всього рішення
RUN dotnet restore TechnicalTask.Service.sln

# Копіюємо весь код
COPY . .

# Збираємо додаток
RUN dotnet publish TechnicalTask.Service.sln -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /publish ./

ENTRYPOINT ["dotnet", "TechnicalTask.Service.Console.dll"]