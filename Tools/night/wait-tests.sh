#!/usr/bin/env bash
# NIGHT17: poll test_status every 20 s until completed (at most 30 min); save it; print the summary and the failures.
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
out="${1:-Logs/n17-suite.json}"
for i in $(seq 1 150); do
  unity command test_status --caller plugin --skill unity-cli --no-banner --format json --result-only > "$out" 2>&1
  if grep -q '"status": "completed"' "$out"; then break; fi
  sleep 20
done
date +%H:%M:%S
node -e "
const j=JSON.parse(require('fs').readFileSync(process.argv[1],'utf8'));const s=j.summary||{};
console.log('tests: '+s.passed+' / '+s.total+' passed, '+s.failed+' failed, '+s.skipped+' skipped ('+j.duration+' s)');
const r=j.results||[];const tr=r.filter(x=>/Trajectory_ReplaysExactly/.test(x.FullName));if(tr.length)console.log('replays: '+tr.filter(x=>x.Status==='Passed').length+' / '+tr.length);
const g=r.filter(x=>/Golden/.test(x.FullName));if(g.length)console.log('golden tests: '+g.filter(x=>x.Status==='Passed').length+' / '+g.length);
for(const x of r.filter(x=>x.Status==='Failed').slice(0,20))console.log('  FAIL '+x.FullName+': '+String(x.Message).replace(/\s+/g,' ').slice(0,300));" "$out"
