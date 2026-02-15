#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$SCRIPT_DIR/../ChangeTrace.csproj"
CONFIGURATION="Release"
OUTPUT_DIR="$SCRIPT_DIR/../publish"

ALL_RUNTIMES=("win-x64" "linux-x64" "osx-x64" "linux-arm64" "osx-arm64")

SELF_CONTAINED=false
SELECTED_RUNTIMES=()

declare -A PUBLISH_TIMES

color_echo() {
  local color=$1
  shift
  echo -e "\033[${color}m$@\033[0m"
}

clean_output() {
  color_echo "33" "Cleaning publish directory..."
  rm -rf "$OUTPUT_DIR"
  mkdir -p "$OUTPUT_DIR"
}

prompt_self_contained() {
  echo "Select build type:"
  echo " 1) Framework-dependent (uses installed .NET) [default]"
  echo " 2) Self-contained (bundles .NET runtime)"
  read -p "Choice [1/2]: " choice
  if [[ "$choice" == "2" ]]; then
    SELF_CONTAINED=true
    color_echo "36" "Selected: Self-contained"
  else
    color_echo "36" "Selected: Framework-dependent"
  fi
}

prompt_runtimes() {
  echo "Select runtime(s) to publish (comma separated, default: all):"
  echo "Available runtimes:"
  for i in "${!ALL_RUNTIMES[@]}"; do
    echo "  $((i+1))) ${ALL_RUNTIMES[$i]}"
  done
  read -p "Enter numbers (e.g., 1,3,5): " input
  if [[ -z "$input" ]]; then
    SELECTED_RUNTIMES=("${ALL_RUNTIMES[@]}")
  else
    IFS=',' read -ra nums <<< "$input"
    for n in "${nums[@]}"; do
      n=$((n-1))
      SELECTED_RUNTIMES+=("${ALL_RUNTIMES[$n]}")
    done
  fi
  color_echo "36" "Selected runtimes: ${SELECTED_RUNTIMES[*]}"
}

echo "=============================="
color_echo "32" "ChangeTrace Build & Publish Script"
echo "=============================="
read -p "Do you want to clean publish directory first? (y/N): " clean_choice
if [[ "$clean_choice" =~ ^[Yy]$ ]]; then
  clean_output
fi

prompt_self_contained
prompt_runtimes

BUILD_START=$(date +%s)
color_echo "34" "Building project..."
dotnet build "$PROJECT" -c "$CONFIGURATION"
BUILD_END=$(date +%s)
BUILD_TIME=$((BUILD_END - BUILD_START))

for RUNTIME in "${SELECTED_RUNTIMES[@]}"; do
  color_echo "36" "Publishing for $RUNTIME..."
  START=$(date +%s)
  dotnet publish "$PROJECT" \
    -c "$CONFIGURATION" \
    -r "$RUNTIME" \
    --self-contained "$SELF_CONTAINED" \
    -o "$OUTPUT_DIR/$RUNTIME"
  END=$(date +%s)
  PUBLISH_TIMES[$RUNTIME]=$((END - START))
done

echo "=============================="
color_echo "32" "Build & Publish Summary"
echo "=============================="
echo "Project: $PROJECT"
echo "Configuration: $CONFIGURATION"
echo "Build time: ${BUILD_TIME}s"
echo "Build type: $([[ "$SELF_CONTAINED" == true ]] && echo "Self-contained" || echo "Framework-dependent")"
echo ""
echo "Published runtimes:"
for RUNTIME in "${SELECTED_RUNTIMES[@]}"; do
  echo "  - $RUNTIME -> $OUTPUT_DIR/$RUNTIME (Time: ${PUBLISH_TIMES[$RUNTIME]}s)"
done
echo ""
color_echo "32" "All done! Published artifacts are in $OUTPUT_DIR/"
