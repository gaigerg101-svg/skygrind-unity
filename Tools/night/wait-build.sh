#!/usr/bin/env bash
# NIGHT15: poll build_status every 30 s until 'completed' (at most 40 min), then print the result's summary lines.
cd /c/Users/gaige/dev/SKYGRIND-unity || exit 1
out="Logs/build-status.json"
for i in $(seq 1 80); do
  s=$(unity command build_status --caller plugin --skill unity-cli --no-banner --format json --result-only 2>&1)
  echo "$s" > "$out"
  if echo "$s" | grep -q '"status": "completed"'; then break; fi
  sleep 30
done
date +%H:%M:%S
node -e "
const t=require('fs').readFileSync(process.argv[1],'utf8');
try{const j=JSON.parse(t);const r=j.report||j.Report||j.buildReport||j;const s=r.summary||r.Summary||{};
 console.log('status '+j.status+'; result '+(s.result||s.Result)+'; errors '+(s.totalErrors??s.TotalErrors)+'; warnings '+(s.totalWarnings??s.TotalWarnings)+'; size '+(s.totalSize??s.TotalSize)+'; time '+(s.totalTime??s.TotalTime));
 const errs=(r.errors||r.Errors||[]);for(const e of errs.slice(0,10))console.log('  ERROR '+JSON.stringify(e).slice(0,300));}
catch(e){console.log('not JSON: '+t.slice(0,600))}" "$out"
