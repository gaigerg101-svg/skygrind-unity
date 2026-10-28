#!/usr/bin/env bash
# NIGHT52 (the lead's): a room's top-down SVG (Tools/night/level-svg.cjs) rasterized to PNG with headless Edge (no ImageMagick / cairo on this
# machine), so a reviewer or the lead's Read sees it as an image.   bash Tools/night/n52-svg-png.sh <plan> <room> <out.png> [scale]
cd "$(git rev-parse --show-toplevel)" || exit 1
plan="${1:?plan}"; room="${2:?room}"; out="${3:?out.png}"; scale="${4:-14}"
svg="${out%.png}.svg"
node Tools/night/level-svg.cjs "$plan" "$svg" --rooms "$room" --scale "$scale" | tail -1
w=$(grep -o 'width="[0-9]*"' "$svg" | head -1 | tr -dc 0-9); h=$(grep -o 'height="[0-9]*"' "$svg" | head -1 | tr -dc 0-9)
E="/c/Program Files (x86)/Microsoft/Edge/Application/msedge.exe"
W=$(cygpath -w "$PWD/$svg"); O=$(cygpath -w "$PWD/$out")
timeout 60 "$E" --headless=new --disable-gpu --hide-scrollbars --screenshot="$O" --window-size="${w:-900},${h:-760}" "file:///${W//\\//}" 2>&1 | tail -1
