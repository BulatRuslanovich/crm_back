#!/bin/bash

RED="\033[0;31m"
GREEN="\033[0;32m"
YELLOW="\033[0;33m"
BLUE="\033[0;34m"
CYAN="\033[0;36m"
BOLD="\033[1m"
RESET="\033[0m"

log_step() {
    printf "%b%s%b\n" "${BOLD}${CYAN}" "$1" "${RESET}"
}

log_success() {
    printf "%b%s%b\n" "${BOLD}${GREEN}" "$1" "${RESET}"
}

log_error() {
    printf "%b%s%b\n" "${BOLD}${RED}" "$1" "${RESET}"
}

echo -e "${BOLD}${BLUE}Code formatting and analysis${RESET}"

cd "$(dirname "$0")"

if ! command -v dotnet &> /dev/null; then
    log_error ".NET SDK not found"
    exit 1
fi

log_step "Restoring packages..."
dotnet restore --verbosity quiet || exit 1

log_step "Building and analyzing project..."
dotnet build --no-restore --verbosity minimal || exit 1

log_step "Formatting code"
dotnet format CrmBack.csproj --verify-no-changes || {
    log_error "Code does not follow formatting rules!"
    log_step "Applying auto-formatting..."
    dotnet format CrmBack.csproj --verbosity minimal
}

log_step "Checking code style..."
dotnet format CrmBack.csproj --verify-no-changes --verbosity diagnostic 2>&1 | grep -E "(error|warning|violating)" || true

log_success "Formatting finished!"
dotnet clean

