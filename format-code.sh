#!/bin/bash

# Скрипт для форматирования кода в .NET проекте

echo "🔧 Форматирование кода в .NET проекте..."

# Переходим в директорию проекта
cd "$(dirname "$0")"

# Проверяем наличие dotnet
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK не найден. Установите .NET SDK для продолжения."
    exit 1
fi

echo "📦 Восстановление пакетов..."
dotnet restore

echo "🔍 Анализ кода..."
dotnet build --verbosity quiet

echo "✨ Форматирование кода..."
# Форматируем все .cs файлы в проекте
find . -name "*.cs" -not -path "./bin/*" -not -path "./obj/*" -not -path "./node_modules/*" | while read -r file; do
    echo "  Форматирование: $file"
    dotnet format "$file" --verbosity quiet
done

echo "🎯 Применение правил форматирования..."
# Применяем форматирование ко всему проекту
dotnet format src/CrmBack.csproj --verbosity quiet

echo "✅ Форматирование завершено!"
echo ""
echo "💡 Полезные команды:"
echo "  dotnet format src/CrmBack.csproj --verify-no-changes  # Проверить форматирование без изменений"
echo "  dotnet format src/CrmBack.csproj --verbosity diagnostic  # Подробный вывод"
echo "  dotnet format src/CrmBack.csproj --include-generated  # Включить автогенерированные файлы"
