#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$(realpath "$SCRIPT_DIR/../ChangeTrace.csproj")"
CONFIGURATION="Release"
OUTPUT_DIR="$(realpath "$SCRIPT_DIR/../publish")"

ALL_RUNTIMES=("win-x64" "linux-x64" "osx-x64" "linux-arm64" "osx-arm64")

SELF_CONTAINED=false
SELECTED_RUNTIMES=()

declare -A PUBLISH_TIMES
declare -A PUBLISH_SIZES

color_echo() {
  local color=$1
  shift
  echo -e "\033[${color}m$*\033[0m"
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

  read -r -p "Choice [1/2]: " choice

  if [[ "${choice:-1}" == "2" ]]; then
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
    printf " %2d) %s\n" "$((i+1))" "${ALL_RUNTIMES[$i]}"
  done

  read -r -p "Enter numbers (e.g., 1,3,5): " input

  if [[ -z "${input:-}" ]]; then
    SELECTED_RUNTIMES=("${ALL_RUNTIMES[@]}")
  else
    IFS=',' read -ra nums <<< "$input"

    for n in "${nums[@]}"; do
      idx=$((n-1))

      if [[ $idx -ge 0 && $idx -lt ${#ALL_RUNTIMES[@]} ]]; then
        SELECTED_RUNTIMES+=("${ALL_RUNTIMES[$idx]}")
      else
        color_echo "31" "Invalid runtime index: $n"
        exit 1
      fi
    done
  fi

  color_echo "36" "Selected runtimes: ${SELECTED_RUNTIMES[*]}"
}

echo "=============================="
color_echo "32" "ChangeTrace Build & Publish Script"
echo "=============================="

read -r -p "Do you want to clean publish directory first? (y/N): " clean_choice
if [[ "$clean_choice" =~ ^[Yy]$ ]]; then
  clean_output
else
  mkdir -p "$OUTPUT_DIR"
fi

prompt_self_contained
prompt_runtimes

echo ""
color_echo "34" "Restoring packages..."
dotnet restore "$PROJECT"

echo ""
color_echo "34" "Building project..."

BUILD_START=$(date +%s)
dotnet build "$PROJECT" -c "$CONFIGURATION"
BUILD_END=$(date +%s)

BUILD_TIME=$((BUILD_END - BUILD_START))

echo ""
color_echo "34" "Preparing runtime results..."

for RUNTIME in "${SELECTED_RUNTIMES[@]}"; do
  PUBLISH_TIMES[$RUNTIME]=0
  PUBLISH_SIZES[$RUNTIME]="?"
done

echo ""
color_echo "34" "Starting parallel publish..."

TMP_FILE="$OUTPUT_DIR/publish_times.tmp"
rm -f "$TMP_FILE"

for RUNTIME in "${SELECTED_RUNTIMES[@]}"; do
(
  START=$(date +%s)

  LOG_FILE="$OUTPUT_DIR/$RUNTIME.log"

  dotnet publish "$PROJECT" \
    -c "$CONFIGURATION" \
    -r "$RUNTIME" \
    --self-contained "$SELF_CONTAINED" \
    --no-build \
    -o "$OUTPUT_DIR/$RUNTIME" \
    > "$LOG_FILE" 2>&1

  END=$(date +%s)
  TIME=$((END - START))

  SIZE=$(du -sh "$OUTPUT_DIR/$RUNTIME" | cut -f1)

  echo "$RUNTIME|$TIME|$SIZE" >> "$TMP_FILE"

  color_echo "36" "Finished $RUNTIME (${TIME}s)"
) &
done

wait

if [[ -f "$TMP_FILE" ]]; then
  while IFS='|' read -r runtime time size; do
    PUBLISH_TIMES[$runtime]=$time
    PUBLISH_SIZES[$runtime]=$size
  done < "$TMP_FILE"

  rm "$TMP_FILE"
fi

echo ""
echo "=============================="
color_echo "32" "Build & Publish Summary"
echo "=============================="

echo "Project: $PROJECT"
echo "Configuration: $CONFIGURATION"
echo "Build time: ${BUILD_TIME}s"

if [[ "$SELF_CONTAINED" == true ]]; then
  echo "Build type: Self-contained"
else
  echo "Build type: Framework-dependent"
fi

echo ""
echo "Published runtimes:"

for RUNTIME in "${SELECTED_RUNTIMES[@]}"; do
  printf "  %-12s -> %s (%ss, %s)\n" \
    "$RUNTIME" \
    "$OUTPUT_DIR/$RUNTIME" \
    "${PUBLISH_TIMES[$RUNTIME]}" \
    "${PUBLISH_SIZES[$RUNTIME]}"
done

echo ""
color_echo "32" "All done! Artifacts in: $OUTPUT_DIR"