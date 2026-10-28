#!/usr/bin/env bash
# The Editor loop helper used through NIGHT7 (it lived in a session scratchpad; committed for NIGHT8+). It encodes the
# Unity spike's traps (SPIKE-RESULTS Task 6) and NIGHT7's:
# - never recompile in Play mode (this stops Play first)
# - recompile_status can say "completed" before the DLLs land, so it checks their mtimes
# - the first command after a domain reload often times out, so it retries once
# - run_tests in the live Editor, never `unity test`
# - the console's error tail keeps old entries: trust "compilationFailed" / "consoleErrors" in `errors`, not the old lines
#   bash Tools/unity-loop.sh status | compile | tests [filter] | errors [n] | eval-file <file.cs> [timeout] | play | stop
cd "$(dirname "$0")/.." || exit 1
OUT="Logs"
mkdir -p "$OUT"
uc() { unity command "$@" --caller plugin --skill unity-cli --no-banner --format json --result-only 2>&1; }
# retry once after a domain reload
ucr() { local out; out=$(uc "$@"); if echo "$out" | grep -q -i -E 'timed out|ECONNREFUSED|No Unity Editor|not reachable'; then sleep 10; out=$(uc "$@"); fi; echo "$out"; }
playmode() { ucr editor_status | grep -o '"playMode": *"[a-z]*"' | grep -o '[a-z]*"$' | tr -d '"'; }
newest() { stat -c %Y Library/ScriptAssemblies/Skygrind.*.dll Library/ScriptAssemblies/Assembly-CSharp*.dll 2>/dev/null | sort -n | tail -1; }
case "$1" in
  status) ucr editor_status ;;
  stop) ucr editor_stop ;;
  play) ucr editor_play ;;
  errors) ucr console --level error --tail "${2:-30}" ;;
  compile)
    pm=$(playmode)
    if [ "$pm" != "stopped" ]; then echo "playMode=$pm: stopping first"; ucr editor_stop >/dev/null; sleep 3; fi
    before=$(newest)
    ucr recompile >/dev/null
    st=""
    for i in $(seq 1 90); do
      sleep 2
      st=$(uc recompile_status 2>/dev/null | grep -o '"status": *"[a-z_]*"' | head -1 | grep -o '[a-z_]*"$' | tr -d '"')
      if [ "$st" = "completed" ] || [ "$st" = "up_to_date" ]; then break; fi
    done
    for i in $(seq 1 60); do
      s=$(uc editor_status 2>/dev/null)
      if echo "$s" | grep -q '"compiling": false' && echo "$s" | grep -q '"domainReloadInProgress": false'; then break; fi
      sleep 2
    done
    after=$(newest)
    if [ "$after" != "$before" ]; then echo "compile: $st, DLLs rewritten ($(date -d @"$after" +%H:%M:%S))"; else echo "compile: $st, DLLs NOT rewritten (no change, or a compile error)"; fi
    # The ground truth for "did it compile": these two fields, not the console's older error lines.
    ucr console --level error --tail 40 | grep -E '"compilationFailed"|"consoleErrors"|error CS' | tail -12
    ;;
  tests)
    out="$OUT/tests-last.json"
    if [ -n "$2" ]; then uc run_tests --mode EditMode --filter "$2" --timeout 600 > "$out"; else uc run_tests --mode EditMode --timeout 600 > "$out"; fi
    node -e "
      const fs=require('fs');const t=fs.readFileSync(process.argv[1],'utf8');
      try{const j=JSON.parse(t);const s=j.Summary;console.log('tests: '+s.Passed+' / '+s.Total+' passed, '+s.Failed+' failed, '+s.Skipped+' skipped');
        const tr=j.Results.filter(r=>/Trajectory_ReplaysExactly/.test(r.FullName));if(tr.length)console.log('replays: '+tr.filter(r=>r.Status==='Passed').length+' / '+tr.length);
        const g=j.Results.filter(r=>/Golden/.test(r.FullName));if(g.length)console.log('golden tests: '+g.filter(r=>r.Status==='Passed').length+' / '+g.length);
        for(const r of j.Results.filter(r=>r.Status!=='Passed'&&r.Status!=='Skipped').slice(0,15))console.log('  FAIL '+r.FullName+': '+String(r.Message).slice(0,300));}
      catch(e){console.log('tests: no JSON: '+t.slice(0,600))}" "$out"
    ;;
  eval-file) ucr eval_file --file "$2" --timeout "${3:-60}" ;;
  *) echo "usage: status | compile | tests [filter] | errors [n] | eval-file <file.cs> [timeout] | play | stop" ;;
esac
