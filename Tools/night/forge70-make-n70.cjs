// FORGE70 (2026-10-25, the lead, right after NIGHT69's close): writes NIGHT70's tools as renamed copies of NIGHT69's (the forges' method).
// NIGHT70 is an 8-hour night across TWO usage windows, so beyond the rename (n69- -> n70-, Logs/n69 -> Logs/n70, NIGHT69 -> NIGHT70,
// u3-n69 -> u3-n70; the data names stay: the level ids n67-iceline / n67-kiln / n67-spire, the rows design.n67.*67, Design/levels/n67/,
// slots67.json, levels67.json, --off67):
//   - n70-clock.cjs: back to 8 hours: freezeMin 435 (7:15), capMin 480 (8:00), staleMin 720 (a long usage stop still reads CAP); the
//     close-check derives its windows from the clock's capMin (the close starts at 7:00 or later, the clock stops from 7:30 to 8:00);
//   - n70-close-check.cjs: git's stderr piped (NIGHT69: "fatal: Needed a single revision" printed before the tag existed: noise in the
//     final check's output), and one planted 8-hour case beside the 4-hour one;
//   - n70-next.cjs: two windows (work 0:00-2:20, the stop with only detached chains 2:20-5:00, work 5:00-7:20, the close), and a room-check
//     piece (a room-check item that FAILs in the round but PASSes in the red: NIGHT69's R5 / I4 / I2 regressions the printer did not see).
// Each planned special edit must apply exactly once, or the tool stops without writing that file. Refuses to overwrite an n70 file.
//   node Tools/night/forge70-make-n70.cjs
'use strict';
const fs = require('fs');
const path = require('path');
const DIR = __dirname;
const FILES = fs.readdirSync(DIR).filter((f) => /^n69-.*\.(cjs|sh)$/.test(f)).sort();

const common = (s) => s
  .replace(/\bn69-/g, 'n70-')
  .replace(/Logs\/n69\b/g, 'Logs/n70')
  .replace(/'Logs', 'n69'/g, "'Logs', 'n70'")
  .replace(/NIGHT69/g, 'NIGHT70');

// A special entry is [from, to] (must apply exactly once) or [from, to, 'all'] (at least once); `from` is matched after the rename.
const SPECIAL = {
  'n69-clock.cjs': [
    ["night: 'n70', freezeMin: 195, capMin: 240, staleMin: 420", "night: 'n70', freezeMin: 435, capMin: 480, staleMin: 720"],
  ],
  'n69-close-check.cjs': [
    ["const git = (...a) => execFileSync('git', a, { cwd: ROOT, encoding: 'utf8', maxBuffer: 64 << 20 }).trim();",
      "const git = (...a) => execFileSync('git', a, { cwd: ROOT, encoding: 'utf8', maxBuffer: 64 << 20, stdio: ['ignore', 'pipe', 'pipe'] }).trim();   // FORGE70: stderr piped (a missing tag printed 'fatal: ...')"],
    ["    ['a 4-hour night (cap 240): a close started at 3:05 and a clock stopped at 3:32 pass (FORGE69)',",
      "    ['an 8-hour night (cap 480): a close started at 7:05 and a clock stopped at 7:32 pass (FORGE70)', { cap: 480, startMin: 425, closeMin: 440, stopMin: 452 }, null, (r) => r.last === 'CLOSE-CHECK n70: PASS'],\n    ['a 4-hour night (cap 240): a close started at 3:05 and a clock stopped at 3:32 pass (FORGE69)',"],
  ],
  'n69-next.cjs': [
    ["const STOP_MIN = 140;   // the usage window's likely stop, minutes after t0 (a fresh window: 2:20)",
      "// FORGE70: an 8-hour night is two usage windows (NIGHT66-69 measured 2:10-2:25 of work per fresh window; the window resets 5:00 after\n" +
      "// its first use): work 1 to 2:20, the stop (only detached chains compute) to 5:00, work 2 to 7:20, the close. A stop's real time is in\n" +
      "// Logs/n70/stops.txt when the lead writes it; this is the plan, not a measurement.\n" +
      "const STOP_MIN = 140, RESUME_MIN = 300, STOP2_MIN = 440;"],
    ["    out.push(m < STOP_MIN ?",
      "    if (m >= STOP_MIN && m < STOP2_MIN) out.push(m < RESUME_MIN ? 'window: inside the planned stop (2:20-5:00): if you are reading this, the window resumed early: work 2 starts now (agents resumed by id from STATE.md)' : 'window 2: the likely second stop is at about ' + hm(STOP2_MIN) + ' (' + hm(STOP2_MIN - m) + ' left): critics and agents before it; the close at 7:00');\n    else out.push(m < STOP_MIN ?"],
    ["    const vm = dir && (rd(dir + '/verbs.md') || '');",
      "    // FORGE70: a room-check item that FAILs in the round but PASSes in the red is a GUARD lost (NIGHT69: kb3 R5 / I4, g8 I2)\n" +
      "    const rcF = (t) => new Set(((t || '').match(/^\\s*FAIL (\\S+)/gm) || []).map((x) => x.trim().split(/\\s+/)[1]));\n" +
      "    const rcNow = dir && rd(dir + '/' + id + '/room-check.txt'), rcRed = rd('Logs/' + NIGHT + '/red-' + L + '/' + id + '/room-check.txt');\n" +
      "    const lost = rcNow ? [...rcF(rcNow)].filter((k) => !rcF(rcRed).has(k)) : ['no room-check.txt'];\n" +
      "    ok('room check', rcNow && !lost.length, lost.length ? 'FAIL here, PASS in the red: ' + lost.join(' ') : '');\n" +
      "    const vm = dir && (rd(dir + '/verbs.md') || '');"],
    // the self-test's complete room gets its room checks (a red FAIL item it keeps is not a loss), and its pattern the new piece
    ["  W('Logs/n70/r2-kiln/verbs.md',",
      "  W('Logs/n70/r2-kiln/X1/room-check.txt', '  PASS R1 x\\n  FAIL R5 foothold x\\nROOM X1: FAIL R5 foothold\\n'); W('Logs/n70/red-kiln/X1/room-check.txt', '  FAIL R5 foothold x\\n');\n  W('Logs/n70/r2-kiln/verbs.md',"],
    ["ok M4 deco \\| ok patch \\| ok M3", "ok M4 deco \\| ok patch \\| ok room check \\| ok M3"],
    ["fs.mkdtempSync(path.join(os.tmpdir(), 'n69next-'))", "fs.mkdtempSync(path.join(os.tmpdir(), 'n70next-'))"],
  ],
};

let bad = 0;
for (const f of FILES) {
  const dst = path.join(DIR, f.replace(/^n69-/, 'n70-'));
  if (fs.existsSync(dst)) { console.log('exists, not overwritten: ' + dst); bad++; continue; }
  let s = fs.readFileSync(path.join(DIR, f), 'utf8').replace(/\r\n/g, '\n');
  s = common(s);
  if (f === 'n69-close-check.cjs') s = s.replace(/'n69'/g, "'n70'").replace(/n69cc-/g, 'n70cc-').replace(/u3-n69/g, 'u3-n70').replace(/CLOSE-CHECK n69/g, 'CLOSE-CHECK n70').replace(/PRE-CHECK n69/g, 'PRE-CHECK n70');
  if (f === 'n69-clock.cjs') s = s.replace(/night: 'n69'/g, "night: 'n70'").replace(/closeTags: \['n69-freeze'\]/g, "closeTags: ['n70-freeze']");
  if (f === 'n69-next.cjs') s = s.replace(/'n69'/g, "'n70'");
  const missed = [];
  for (const [from, to, mode] of (SPECIAL[f] || [])) {
    const n = s.split(from).length - 1;
    if (mode === 'all' ? n < 1 : n !== 1) { missed.push('(' + n + 'x) ' + from.slice(0, 70)); continue; }
    s = s.split(from).join(to);
  }
  if (missed.length) { console.log('NOT WRITTEN ' + f + ': ' + missed.join(' || ')); bad++; continue; }
  fs.writeFileSync(dst, s);
  if (f.endsWith('.sh')) fs.chmodSync(dst, 0o755);
  const left = s.split('\n').filter((l) => /\bn69\b|n69-|Logs\/n69/.test(l)).length;
  console.log(path.relative(path.join(DIR, '..', '..'), dst).replace(/\\/g, '/') + ': ' + s.split('\n').length + ' lines' + (SPECIAL[f] ? ', ' + SPECIAL[f].length + ' special edits' : '') + '; lines still naming n69: ' + left);
}
process.exit(bad ? 1 : 0);
