#!/usr/bin/env bash
# FORGE70 (an 8-hour night is two usage windows with a ~2:40 stop between; NIGHT68's chain computed through its stop, NIGHT69 had none):
# the night's ONE dotnet queue in the main tree. It runs queued jobs one at a time, in name order, until Logs/n70/runner.stop exists or the
# clock reaches 7:00 (the close), so the stop's hours compute the rounds, fresh sets and throw charts the lead queued before it.
#   a job:    Logs/n70/queue/<NNN>-<name>.job   one line, the command (written with the Write tool), e.g. `bash Logs/n70/room-chain.sh 1 kb3 v2`
#   its run:  <job>.running while it runs (its output <name>.out), then <name>.done holding "rc <n> <start> <end>"; never re-run (a .done or
#             .running file blocks it)
#   the hold: Logs/n70/runner.hold (the lead's) stops the runner from STARTING a job; the lead creates it, waits until Logs/n70/runner.busy
#             is gone, picks / commits room files, then removes it (no tracked write while a round runs: FORGE66 red team 1)
#   alive:    Logs/n70/runner.alive holds the last loop's time and the job running; Logs/n70/runner.log one line per event
#   bash Tools/night/n70-runner.sh       (once, detached: `( bash Tools/night/n70-runner.sh > Logs/n70/runner-out.txt 2>&1 & )`)
#   bash Tools/night/n70-runner.sh --status
cd "$(git rev-parse --show-toplevel)" || exit 1
export MSBUILDDISABLENODEREUSE=1
Q=Logs/n70/queue
say() { echo "$(date '+%F %T') $*" >> Logs/n70/runner.log; }
mins() { node -e "const c=JSON.parse(require('fs').readFileSync('Logs/clock.json','utf8'));console.log(c.night==='n70'&&!c.stopped?Math.floor((Date.now()/1000-c.t0)/60):-1)" 2>/dev/null || echo -1; }
if [ "$1" = "--status" ]; then
  echo "alive: $(cat Logs/n70/runner.alive 2>/dev/null || echo none)"; [ -f Logs/n70/runner.busy ] && echo "busy: $(cat Logs/n70/runner.busy)"
  [ -f Logs/n70/runner.hold ] && echo "HOLD set"; for j in "$Q"/*.job; do [ -e "$j" ] || continue; b="${j%.job}"
    if [ -f "$b.done" ]; then echo "done    $(basename "$b"): $(cat "$b.done")"; elif [ -f "$b.running" ]; then echo "running $(basename "$b")"; else echo "queued  $(basename "$b")"; fi; done
  exit 0
fi
if [ -f Logs/n70/runner.pid ] && kill -0 "$(cat Logs/n70/runner.pid)" 2>/dev/null; then echo "n70-runner: already running (pid $(cat Logs/n70/runner.pid))"; exit 1; fi
mkdir -p "$Q"; echo $$ > Logs/n70/runner.pid; say "start pid $$"
while :; do
  [ -f Logs/n70/runner.stop ] && { say "stop file: exit"; break; }
  m=$(mins); if [ "$m" -ge 420 ]; then say "the clock reads $m min (7:00): the close; exit"; break; fi
  echo "$(date '+%F %T') idle" > Logs/n70/runner.alive
  if [ -f Logs/n70/runner.hold ]; then sleep 15; continue; fi
  next=""; for j in "$Q"/*.job; do [ -e "$j" ] || continue; b="${j%.job}"; [ -f "$b.done" ] || [ -f "$b.running" ] || { next="$j"; break; }; done
  if [ -z "$next" ]; then sleep 30; continue; fi
  b="${next%.job}"; name=$(basename "$b"); cmd=$(head -1 "$next")
  t1=$(date '+%T'); echo "$name $t1 $cmd" > Logs/n70/runner.busy; touch "$b.running"; echo "$(date '+%F %T') running $name" > Logs/n70/runner.alive
  say "run $name: $cmd"
  { echo "# command: $cmd"; echo "# sha: $(git rev-parse --short HEAD); start: $(date '+%F %T')"; bash -c "$cmd"; echo "# exit $?"; } > "$b.out" 2>&1
  rc=$(tail -1 "$b.out" | sed 's/# exit //')
  echo "rc $rc $t1 $(date '+%T')" > "$b.done"; rm -f "$b.running" Logs/n70/runner.busy
  say "done $name rc $rc"
done
rm -f Logs/n70/runner.pid Logs/n70/runner.busy; say "exit"
