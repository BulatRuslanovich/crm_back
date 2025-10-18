#!/bin/bash


echo "🔧 Форматирование кода в .NET проекте..."

cd "$(dirname "$0")"

if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK не найден. Установите .NET SDK для продолжения."
    exit 1
fi

echo "📦 Восстановление пакетов..."
dotnet restore

echo "🔍 Анализ кода..."
dotnet build --verbosity quiet

echo "✨ Форматирование кода..."
dotnet format src/CrmBack.csproj --verbosity quiet

echo "🎯 Форматирование тестов..."
dotnet format Tests/Tests.csproj --verbosity quiet

echo "✅ Форматирование завершено!"
echo ""
echo "💡 Полезные команды:"
echo "  dotnet format src/CrmBack.csproj --verify-no-changes  # Проверить форматирование без изменений"
echo "  dotnet format src/CrmBack.csproj --verbosity diagnostic  # Подробный вывод"
echo "  dotnet format src/CrmBack.csproj --include-generated  # Включить автогенерированные файлы"
