#!/bin/sh
set -eu

usage() {
  cat <<'EOF'
Usage: ./resizeTexturesByHalf.sh <folder>

Recursively finds .png, .jpg, .jpeg, and .tga files under <folder> and
resizes each image to 50% of its current dimensions in place using ImageMagick.
EOF
}

require_command() {
  command_name="$1"

  if ! command -v "$command_name" >/dev/null 2>&1; then
    echo "Error: required command not found: $command_name" >&2
    exit 1
  fi
}

if [ "$#" -ne 1 ]; then
  usage >&2
  exit 1
fi

target_dir="$1"

if [ ! -d "$target_dir" ]; then
  echo "Error: folder does not exist: $target_dir" >&2
  exit 1
fi

require_command find
require_command magick
require_command mktemp
require_command xargs
require_command tr
require_command awk

list_file=$(mktemp)
trap 'rm -f "$list_file"' EXIT HUP INT TERM

find "$target_dir" -type f \
  \( -iname '*.png' -o -iname '*.jpg' -o -iname '*.jpeg' -o -iname '*.tga' \) \
  -print0 > "$list_file"

resized_count=$(tr -cd '\000' < "$list_file" | wc -c | awk '{ print $1 }')

if [ "$resized_count" -eq 0 ]; then
  echo "Finished. Resized 0 image(s)."
  exit 0
fi

xargs -0 -n 1 sh -c '
  image_path=$1
  echo "Resizing: $image_path"
  magick mogrify -resize 50% "$image_path"
' sh < "$list_file"

echo "Finished. Resized $resized_count image(s)."
