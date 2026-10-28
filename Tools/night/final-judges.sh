#!/usr/bin/env bash
# NIGHT20 close (U3-34.12): the final judges (range, chase, climb on both maps) and the final farm (THE MARATHON 40 seeds,
# THE PARITY FARM 7 guns x 20 seeds, the free farm 40 seeds) as one farm batch: 4 processes from a snapshot of the harness
# build (Temp/sh-<tag>), each waited for by its PID; then sbreport (SANDBOX.md).   bash final-judges.sh <tag>
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
tag="${1:-final20}"
dir="Logs/sandbox/$tag"; mkdir -p "$dir"
snap="Temp/sh-$tag"; rm -rf "$snap"; cp -r Temp/SimHarness/bin/Release/net10.0 "$snap"
H() { dotnet "$snap/SimHarness.dll" "$@"; }
echo "$(date +%H:%M:%S) final judges $tag: 4 processes ($(git log -1 --format=%h))"
( H marathon --seeds 20 --from 1 --tag $tag; H pfarm --gun zapper --seeds 20 --from 1 --tag $tag; H pfarm --gun revolver --seeds 20 --from 1 --tag $tag ) > "$dir/p1.log" 2>&1 &
p1=$!
( H marathon --seeds 20 --from 21 --tag $tag; H pfarm --gun bouncer --seeds 20 --from 1 --tag $tag; H pfarm --gun rocket --seeds 20 --from 1 --tag $tag ) > "$dir/p2.log" 2>&1 &
p2=$!
( H pfarm --gun rail --seeds 20 --from 1 --tag $tag; H pfarm --gun shotgun --seeds 20 --from 1 --tag $tag; H pfarm --gun nailgun --seeds 20 --from 1 --tag $tag; H chase --tag $tag ) > "$dir/p3.log" 2>&1 &
p3=$!
( H climb --tag $tag; H climb --map drydock --tag $tag; H range --tag $tag; H farm 40 normal 1 "$dir/farm.jsonl" ) > "$dir/p4.log" 2>&1 &
p4=$!
for p in $p1 $p2 $p3 $p4; do wait $p; echo "$(date +%H:%M:%S) pid $p done"; done
H sbreport --tag $tag > "$dir/sbreport.log" 2>&1
grep -h "runs in the report" "$dir"/p1.log "$dir"/p2.log # the later of the two marathon processes merged both parts
date +%H:%M:%S > "$dir/done.txt"
echo "$(date +%H:%M:%S) final judges $tag: all done"
