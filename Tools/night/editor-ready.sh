#!/usr/bin/env bash
# Polls the Unity Pipeline until status is ready (15 s steps, 5 minutes max); prints the Unity window title.
cd "$(dirname "$0")/../.." || exit 2
for i in $(seq 1 20); do
  out=$(bash Tools/unity-loop.sh status 2>&1)
  if echo "$out" | grep -q '"status": "ready"'; then
    echo "EDITOR READY (poll $i)"
    powershell -NoProfile -Command "Get-Process Unity -ErrorAction SilentlyContinue | Where-Object { \$_.MainWindowTitle } | ForEach-Object { 'window: ' + \$_.MainWindowTitle }"
    exit 0
  fi
  echo "poll $i not ready"
  powershell -NoProfile -Command "Get-Process Unity -ErrorAction SilentlyContinue | Where-Object { \$_.MainWindowTitle } | ForEach-Object { 'window: ' + \$_.MainWindowTitle }"
  sleep 15
done
echo "EDITOR NOT READY after 5 minutes"
exit 1
