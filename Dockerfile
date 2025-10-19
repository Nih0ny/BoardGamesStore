# Етап 1: Збірка проєкту (Build Stage)
# Використовуємо офіційний образ .NET SDK для збірки
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Копіюємо файли .csproj та відновлюємо залежності
# Це кешується окремо від коду, щоб пришвидшити майбутні збірки
COPY *.sln .
COPY *.csproj ./
RUN dotnet restore "./BoardGamesStore.csproj"

# Копіюємо решту файлів проєкту та збираємо його
COPY . .
WORKDIR "/source"
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet publish -c Release -o /app/publish

# Етап 2: Створення кінцевого образу (Final Stage)
# Використовуємо легкий образ ASP.NET Core Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Вказуємо порт, на якому буде працювати застосунок (за замовчуванням 8080 для .NET 8)
EXPOSE 8080

# Точка входу для запуску застосунку
CMD ["/bin/sh", "-c", "dotnet ef database update && dotnet BoardGamesStore.dll"]
