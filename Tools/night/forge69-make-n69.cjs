// FORGE69 (2026-10-25, the lead, right after NIGHT68's close): writes NIGHT69's tools as renamed copies of NIGHT68's (the forges' method).
// NIGHT69 is a 4-hour night, so beyond the rename (n68- -> n69-, Logs/n68 -> Logs/n69, NIGHT68 -> NIGHT69, u3-n68 -> u3-n69; the data names
// stay: the level ids n67-iceline / n67-kiln / n67-spire, the rows design.n67.*67, Design/levels/n67/, slots67.json, levels67.json, --off67):
//   - n69-clock.cjs: freezeMin 195 (3:15), capMin 240 (4:00), staleMin 420 (a long usage stop still reads CAP); close-check derives its windows
//     from the clock's capMin (the close starts at cap - 60 = 3:00 or later, the clock stops from cap - 30 = 3:30 to 4:00), so it needs no
//     other change;
//   - n69-close-check.cjs: its self-test clock takes a `cap` option and one new planted case proves a 4-hour close passes.
// Each planned special edit must apply exactly once, or the tool stops without writing that file. Refuses to overwrite an n69 file.
//   node Tools/night/forge69-make-n69.cjs
'use strict';
const fs = require('fs');
const path = require('path');
const DIR = __dirname;
const FILES = fs.readdirSync(DIR).filter((f) => /^n68-.*\.(cjs|sh)$/.test(f)).sort();

const common = (s) => s
  .replace(/\bn68-/g, 'n69-')
  .replace(/Logs\/n68\b/g, 'Logs/n69')
  .replace(/'Logs', 'n68'/g, "'Logs', 'n69'")
  .replace(/NIGHT68/g, 'NIGHT69');

// A special entry is [from, to] (must apply exactly once) or [from, to, 'all'] (at least once).
// n69-gates.sh (FORGE69 scout D, NIGHT68's closing gates were reaped for low memory at the --off57 fingerprint): it called `dotnet run
// --project` 14 times, each one re-evaluating MSBuild; now it builds both harnesses once and runs their DLLs directly (as n65-verbs.sh does).
const SPECIAL = {
  'n68-gates.sh': [
    ['export MSBUILDDISABLENODEREUSE=1\n',
      'export MSBUILDDISABLENODEREUSE=1\n' +
      '# FORGE69: both harnesses built ONCE (two MSBuild evaluations, not 14), then their DLLs run directly\n' +
      'dotnet build Tools/SimHarness -c Release -nologo -v q > /dev/null 2>&1 || { echo "GATE FAIL: SimHarness build"; exit 1; }\n' +
      'dotnet build Tools/CoreHarness -c Release -nologo -v q > /dev/null 2>&1 || { echo "GATE FAIL: CoreHarness build"; exit 1; }\n' +
      'SIM="dotnet Temp/SimHarness/bin/Release/net10.0/SimHarness.dll"; CORE="dotnet Temp/CoreHarness/bin/Release/net10.0/CoreHarness.dll"\n'],
    ['dotnet run --project Tools/SimHarness -c Release -- ', '$SIM ', 'all'],
    ['dotnet run --project Tools/CoreHarness -c Release ', '$CORE ', 'all'],
  ],
  // FORGE69 scout X4: a room measured ALONE (only it new in its level; the others old): SLOTS=<id[,id]> (default latest: every slot). A
  // room that reads 1.22 alone and 1.12 beside three other new rooms (R, NIGHT68) is measured against its red like for like when alone.
  'n68-round.sh': [
    ['if [ "$mode" = "--shipped" ]; then gen "$out/gen-latest.txt" --run --out="$out"; else gen "$out/gen-latest.txt" --run --slots=latest --out="$out"; fi',
      '# FORGE69: SLOTS=<id[,id]> assembles only those slots latest (the rest old: the red\'s level plus the room); each must be in ROOMS\n' +
      'SLOTS="${SLOTS:-latest}"\n' +
      'if [ "$SLOTS" != latest ]; then for s in ${SLOTS//,/ }; do case " $ROOMS " in *" $s "*) ;; *) echo "n69-round $tag: SLOTS id $s is not in ROOMS ($ROOMS)"; exit 1 ;; esac; done; fi\n' +
      'echo "n69-round $tag: slots $SLOTS"\n' +
      'if [ "$mode" = "--shipped" ]; then gen "$out/gen-latest.txt" --run --out="$out"; else gen "$out/gen-latest.txt" --run --slots="$SLOTS" --out="$out"; fi'],
  ],
  'n68-clock.cjs': [
    ["night: 'n68', freezeMin: 435, capMin: 480, staleMin: 720", "night: 'n69', freezeMin: 195, capMin: 240, staleMin: 420"],
  ],
  'n68-close-check.cjs': [
    ["freezeMin: 435, capMin: 480, staleMin: 720", "freezeMin: (o.cap || 480) - 45, capMin: o.cap || 480, staleMin: 720"],
    ["G(d, at(o.slotsLate ? 437 : 399), 'commit', '-q', '-m', 'frozen');",
      "G(d, at(o.frozenMin || (o.slotsLate ? 437 : 399)), 'commit', '-q', '-m', 'frozen');"],
    ["    ['a verbs37 side missing fails the Player',",
      "    ['a 4-hour night (cap 240): a close started at 3:05 and a clock stopped at 3:32 pass (FORGE69)', { cap: 240, startMin: 185, frozenMin: 150, closeMin: 200, stopMin: 212 }, null, (r) => r.last === 'CLOSE-CHECK n69: PASS'],\n    ['a verbs37 side missing fails the Player',"],
  ],
};

let bad = 0;
for (const f of FILES) {
  const dst = path.join(DIR, f.replace(/^n68-/, 'n69-'));
  if (fs.existsSync(dst)) { console.log('exists, not overwritten: ' + dst); bad++; continue; }
  let s = fs.readFileSync(path.join(DIR, f), 'utf8').replace(/\r\n/g, '\n');
  s = common(s);
  if (f === 'n68-close-check.cjs') s = s.replace(/'n68'/g, "'n69'").replace(/n68cc-/g, 'n69cc-').replace(/u3-n68/g, 'u3-n69').replace(/CLOSE-CHECK n68/g, 'CLOSE-CHECK n69').replace(/PRE-CHECK n68/g, 'PRE-CHECK n69');
  if (f === 'n68-clock.cjs') s = s.replace(/night: 'n68'/g, "night: 'n69'").replace(/closeTags: \['n68-freeze'\]/g, "closeTags: ['n69-freeze']");
  const missed = [];
  for (const [from, to, mode] of (SPECIAL[f] || [])) {
    // the clock's DEF carries 'n68' before the rename; match the renamed text
    const fromR = from.replace("night: 'n68'", "night: 'n69'");
    const n = s.split(fromR).length - 1;
    if (mode === 'all' ? n < 1 : n !== 1) { missed.push('(' + n + 'x) ' + fromR.slice(0, 70)); continue; }
    s = s.split(fromR).join(to);
  }
  if (missed.length) { console.log('NOT WRITTEN ' + f + ': ' + missed.join(' || ')); bad++; continue; }
  fs.writeFileSync(dst, s);
  if (f.endsWith('.sh')) fs.chmodSync(dst, 0o755);
  const left = s.split('\n').filter((l) => /\bn68\b|n68-|Logs\/n68/.test(l)).length;
  console.log(path.relative(path.join(DIR, '..', '..'), dst).replace(/\\/g, '/') + ': ' + s.split('\n').length + ' lines' + (SPECIAL[f] ? ', ' + SPECIAL[f].length + ' special edits' : '') + '; lines still naming n68: ' + left);
}
process.exit(bad ? 1 : 0);
