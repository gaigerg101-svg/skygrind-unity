#!/usr/bin/env bash
# PG3: make the clean public-repo snapshot of the committed pg3 branch (GG's uncommitted drift never goes in: git archive
# reads commits only). Leaves out Logs/ (4,097 force-tracked reel and night outputs), Assets/_Recovery/ and the night
# worktree scratch; keeps the project, the tools, the docs and the night prompts / logs that the prompt log cites.
# Usage: bash Tools/pg3/snapshot.sh [<out dir>]   (default ../skygrind-unity-pg3; refuses a non-empty dir)
set -euo pipefail
repo="$(cd "$(dirname "$0")/../.." && pwd)"
out="${1:-$repo/../skygrind-unity-pg3}"
if [ -e "$out" ] && [ -n "$(ls -A "$out" 2>/dev/null)" ]; then echo "snapshot: $out is not empty"; exit 2; fi
mkdir -p "$out"
cd "$repo"
sha="$(git rev-parse --short HEAD)"
git archive --format=tar HEAD | tar -x -C "$out" \
  --exclude='Logs' --exclude='Logs/*' --exclude='Assets/_Recovery' --exclude='Assets/_Recovery/*' --exclude='Assets/_Recovery.meta' \
  --exclude='.claude/worktrees'
# the proof frames the PG3 docs link to are copied into docs/pg3/ before the archive, so nothing under Logs/ is needed.
# The night prompts stay untracked in this repo (CLAUDE.md rule 1); the snapshot carries copies as prompt-log evidence.
mkdir -p "$out/prompts"
for f in NIGHT*.md DECIDE*.md FORGE*.md EXT*.md; do
  case "$f" in *-LOG.md|*-REPORT.md) continue ;; esac
  [ -f "$f" ] && cp "$f" "$out/prompts/"
done
[ -d nights ] && cp nights/*.md "$out/prompts/" 2>/dev/null || true   # NIGHT7-NIGHT43, moved out of the root earlier
cd "$out"
echo "prompts copied: $(ls prompts | wc -l)"
echo "snapshot of $sha -> $out"
du -sh . | tail -1
find . -type f -size +50M -printf '%s %p\n' | sort -rn | head -5
echo "files: $(find . -type f | wc -l)"
