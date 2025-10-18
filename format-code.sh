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
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -not -path "./node_modules/*" | while read -r file; do
    echo "  Форматирование: $file"
    dotnet format "$file" --verbosity quiet
done

echo "🎯 Применение правил форматирования..."
dotnet format src/CrmBack.csproj --verbosity quiet

echo "✅ Форматирование завершено!"
echo ""
echo "💡 Полезные команды:"
echo "  dotnet format src/CrmBack.csproj --verify-no-changes  # Проверить форматирование без изменений"
echo "  dotnet format src/CrmBack.csproj --verbosity diagnostic  # Подробный вывод"
echo "  dotnet format src/CrmBack.csproj --include-generated  # Включить автогенерированные файлы"
