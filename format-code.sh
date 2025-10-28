#!/bin/bash

echo "🔧 Форматирование и анализ кода в .NET проекте..."

cd "$(dirname "$0")"

if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK не найден. Установите .NET SDK для продолжения."
    exit 1
fi

echo "📦 Восстановление пакетов..."
dotnet restore --verbosity quiet || exit 1

echo "🔍 Анализ и сборка проекта..."
dotnet build --no-restore --verbosity minimal || exit 1

echo "✨ Форматирование кода согласно .editorconfig..."
dotnet format CrmBack.sln --verify-no-changes || {
    echo "❌ Код не соответствует правилам форматирования!"
    echo "🧹 Применение автоформатирования..."
    dotnet format CrmBack.sln --verbosity minimal
}

echo ""
echo "🔎 Проверка стиля кода (строгие правила)..."
dotnet format CrmBack.sln --verify-no-changes --verbosity diagnostic 2>&1 | grep -E "(error|warning|violating)" || true

echo ""
echo "✅ Форматирование завершено!"

