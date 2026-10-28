// FORGE71 (2026-10-26, the lead, right after NIGHT70's close): writes NIGHT71's night-bound tools as renamed copies of NIGHT70's (the
// forges' method: forge70-make-n70.cjs). NIGHT71 keeps NIGHT70's 8-hour clock and close; beyond the rename (n70 -> n71, NIGHT70 -> NIGHT71,
// Logs/n70 -> Logs/n71, u3-n70 -> u3-n71; data names stay: n67 levels, design.n67.*67, slots67.json, --off67) it changes:
//   - n71-clock.cjs: the close's heavy steps may be tagged n71-freeze OR n71-play (GG's play build is the night's deliverable);
//   - n71-close-check.cjs: a `play` check (GG 2026-10-26: "Bots guard, I judge"): Builds/n71-play/SKYGRIND.exe exists, its smoke log
//     Logs/player/n71-play-smoke.log reads "DONE: all steps passed", and PLAY71.md (the play card) is committed and clean; OR the guard's
//     refusal of the play build is quoted under ## Blocked and PLAY71.md is still committed. The minimum close may miss it like the
//     Player lines. Self-test cases for it are added and the existing ones updated.
// Only the night-bound tools are copied (clock, close-check, slate tools, runner, gates, reels, prune, freeze-rows); the room tools
// (n70-area, n70-deco, n70-patch-share, n70-throws, n70-round*, gen-slots) are used under their own names as GUARDS.
// Each special edit must apply exactly once (or, with 'all', at least once), else the tool stops without writing that file.
// Refuses to overwrite an n71 file.   node Tools/night/forge71-make-n71.cjs
'use strict';
const fs = require('fs');
const path = require('path');
const DIR = __dirname;
const FILES = ['n70-clock.cjs', 'n70-close-check.cjs', 'n70-slate.cjs', 'n70-slate-set.cjs', 'n70-slate-add.cjs', 'n70-runner.sh',
  'n70-gates.sh', 'n70-reels.sh', 'n70-prune.sh', 'n70-freeze-rows.cjs'];

const common = (s) => s.replace(/\bn70\b/g, 'n71').replace(/NIGHT70/g, 'NIGHT71');

const PLAY_CHECK = [
  "const gatesTxt = rd('Logs/' + NIGHT + '/gates-flow-close.txt');",
  "// FORGE71 (GG 2026-10-26, \"Bots guard, I judge\"): the night's deliverable is GG's play build: Builds/<night>-play/SKYGRIND.exe, its\n" +
  "// smoke DONE (Logs/player/<night>-play-smoke.log) and the play card PLAY<NN>.md committed and clean; OR the guard's refusal of the play\n" +
  "// build quoted under ## Blocked (the card is still owed: it says what was not built and why)\n" +
  "const playExe = fs.existsSync(path.join(ROOT, 'Builds', NIGHT + '-play', 'SKYGRIND.exe'));\n" +
  "const playSmoke = /DONE: all steps passed/.test(rd('Logs/player/' + NIGHT + '-play-smoke.log') || '');\n" +
  "const playCard = (() => { try { return git('ls-files', '--', 'PLAY' + NN + '.md') !== '' && git('status', '--porcelain', '--', 'PLAY' + NN + '.md') === ''; } catch (e) { return false; } })();\n" +
  "const blockedPlay = guardQuoted && /play build/i.test(blocked);\n" +
  "R.play = [playCard && ((playExe && playSmoke) || blockedPlay), (playCard ? 'PLAY' + NN + '.md committed; ' : 'no committed, clean PLAY' + NN + '.md; ') + (playExe && playSmoke ? 'Builds/' + NIGHT + '-play and its smoke DONE' : (blockedPlay ? \"the guard's refusal of the play build quoted under ## Blocked\" : 'play build ' + (playExe ? 'present' : 'missing') + ', its smoke ' + (playSmoke ? 'DONE' : 'not DONE')))];\n" +
  "const gatesTxt = rd('Logs/' + NIGHT + '/gates-flow-close.txt');",
];

// A special entry is [from, to] (exactly once) or [from, to, 'all'] (at least once); `from` is matched after the rename.
const SPECIAL = {
  'n70-clock.cjs': [
    ["closeTags: ['n71-freeze'] }", "closeTags: ['n71-freeze', 'n71-play'] }"],
  ],
  'n70-close-check.cjs': [
    PLAY_CHECK,
    ["const PRE = ['start', 'files', 'frozen', 'player', 'gates', 'slate', 'row', 'running'];",
      "const PRE = ['start', 'files', 'frozen', 'player', 'gates', 'play', 'slate', 'row', 'running'];"],
    ["const FULL = ['clock', 'start', 'files', 'frozen', 'tag', 'player', 'gates', 'slate', 'row', 'running'];",
      "const FULL = ['clock', 'start', 'files', 'frozen', 'tag', 'player', 'gates', 'play', 'slate', 'row', 'running'];"],
    ["failed.every((k) => k === 'player' || k === 'gates');", "failed.every((k) => k === 'player' || k === 'gates' || k === 'play');"],
    // (the final check's MINIMUM PASS already ignores which checks failed: minOk reads the clock, files, tag, running, slate and row)
    // the self-test: the play files in a correct close; PLAY71.md in the close commit
    ["W(d, 'HANDOFF.md', '# h\\n'); if (!o.coreEarly)", "if (o.card !== false) W(d, 'PLAY71.md', '# play\\n'); W(d, 'HANDOFF.md', '# h\\n'); if (!o.coreEarly)"],
    ["G(d, null, 'add', '--', 'NIGHT71-REPORT.md', 'NIGHT71-LOG.md', 'HANDOFF.md', 'docs/memory/CORE.md');",
      "G(d, null, 'add', '--', 'NIGHT71-REPORT.md', 'NIGHT71-LOG.md', 'HANDOFF.md', 'docs/memory/CORE.md', ...(o.card !== false ? ['PLAY71.md'] : []));"],
    ["W(d, 'Logs/player/n56-smoke-steal-n71-freeze.log', '[smoke] 232.0s DONE: all steps passed\\n'); }",
      "W(d, 'Logs/player/n56-smoke-steal-n71-freeze.log', '[smoke] 232.0s DONE: all steps passed\\n'); }\n" +
      "    if ((o.player === 'ok' || o.player === 'no-vn') && o.play !== 'none') { W(d, 'Builds/n71-play/SKYGRIND.exe', 'exe'); W(d, 'Logs/player/n71-play-smoke.log', '[smoke] 231.0s DONE: all steps passed\\n'); }"],
    // the existing cases' expected lines with the play line
    ["(r) => r.last === 'PRE-CHECK n71: FAIL files, player, gates' && r.code === 1]", "(r) => r.last === 'PRE-CHECK n71: FAIL files, player, gates, play' && r.code === 1]"],
    ["(r) => r.last === 'PRE-CHECK n71: FAIL player, gates' && r.code === 1]", "(r) => r.last === 'PRE-CHECK n71: FAIL player, gates, play' && r.code === 1]"],
    ["(r) => r.last === 'CLOSE-CHECK n71: FAIL player, gates']", "(r) => r.last === 'CLOSE-CHECK n71: FAIL player, gates, play']"],
    ["\\n- (e) the gates: the same' }", "\\n- (e) the gates: the same\\n- (f) the play build: the same' }"],
    // two new cases (FORGE71)
    ["    ['a commit after the tag fails the tag', { afterTag: true }, null, (r) => r.last === 'CLOSE-CHECK n71: FAIL tag'],",
      "    ['a commit after the tag fails the tag', { afterTag: true }, null, (r) => r.last === 'CLOSE-CHECK n71: FAIL tag'],\n" +
      "    ['no play build and no quoted refusal fails play (FORGE71)', { play: 'none' }, null, (r) => r.last === 'CLOSE-CHECK n71: FAIL play'],\n" +
      "    ['a play build without a committed play card fails play (FORGE71)', { card: false }, null, (r) => r.last === 'CLOSE-CHECK n71: FAIL play'],\n" +
      "    ['the guard\\'s refusal of the play build quoted under Blocked, the card committed, passes (FORGE71)', { play: 'none', blocked: '- (f) the play build: refused: ' + GUARDQ }, null, (r) => r.last === 'CLOSE-CHECK n71: PASS'],"],
  ],
};

let wrote = 0, bad = 0;
for (const f of FILES) {
  const src = path.join(DIR, f);
  const dstName = f.replace(/^n70-/, 'n71-');
  const dst = path.join(DIR, dstName);
  if (fs.existsSync(dst)) { console.log('SKIP ' + dstName + ': exists (never overwritten)'); continue; }
  let s = common(fs.readFileSync(src, 'utf8'));
  let ok = true;
  for (const [from, to, mode] of (SPECIAL[f] || [])) {
    const n = s.split(from).length - 1;
    if (mode === 'all' ? n < 1 : n !== 1) { console.log('FAIL ' + dstName + ': a special edit matched ' + n + ' times: ' + from.slice(0, 90)); ok = false; break; }
    s = s.split(from).join(to);
  }
  if (!ok) { bad++; continue; }
  fs.writeFileSync(dst, s);
  try { fs.chmodSync(dst, fs.statSync(src).mode); } catch (e) { /* Windows */ }
  console.log('wrote ' + dstName);
  wrote++;
}
console.log('forge71-make-n71: ' + wrote + ' written, ' + bad + ' failed');
process.exit(bad ? 1 : 0);
