#!/bin/sh

set -eu

usage() {
  echo "Usage: sh push.sh -m \"commit message\"" >&2
  exit 1
}

message=""

while getopts "m:" opt; do
  case "$opt" in
    m)
      message=$OPTARG
      ;;
    *)
      usage
      ;;
  esac
done

shift $((OPTIND - 1))

if [ -z "$message" ] || [ "$#" -ne 0 ]; then
  usage
fi

if ! command -v zip >/dev/null 2>&1; then
  echo "Error: 'zip' is required but not installed." >&2
  exit 1
fi

repo_root=$(git rev-parse --show-toplevel 2>/dev/null) || {
  echo "Error: not inside a git repository." >&2
  exit 1
}

cd "$repo_root"

scenes_dir="Assets/Scenes"

set -- "$scenes_dir"/*.unity

if [ ! -f "$1" ]; then
  echo "Error: no scene files found in $scenes_dir" >&2
  exit 1
fi

branch_name=$(git symbolic-ref --quiet --short HEAD 2>/dev/null) || {
  echo "Error: detached HEAD. Check out a branch before pushing." >&2
  exit 1
}

for scene_path in "$@"; do
  zip_path="$scene_path.zip"
  rm -f "$zip_path"
  zip -q -j "$zip_path" "$scene_path"
done

git add -A
git reset -- "$@"

if git diff --cached --quiet; then
  echo "No staged changes to commit after excluding scene files in $scenes_dir."
  echo "Pushing existing commits on $branch_name..."
else
  git commit -m "$message"
fi

git push origin "$branch_name"
