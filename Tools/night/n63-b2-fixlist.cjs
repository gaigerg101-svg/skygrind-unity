// NIGHT63 builder G (B2b): B2's fix list COMPUTED FROM THE MEASURE, not a hand list. Reads the boss63 reel's per-attack cue lines from the
// "before" Player logs (bossTell63 off) and applies BossCue63.Verdict's rule (the same rule the reel prints, mirrored here and pinned by the
// self-test): an attack is on the list when its cue is SHORT (no danger- or parry-hue tell, or one under 300 ms before the hit) or OFF THE
// DANGER HUE (a cue found whose strongest riser is not in the danger band, on an attack the sim does not mark parryable: kind not yellow).
// The danger band is the colour table's (design.n63.colours63.numbers.bands; review S3), applied to each line's printed hue and saturation.
// A parryable attack (kind yellow) is never listed: its yellow tell is right. Prints the list per log and the union; with --write it puts the
// union into design.n63.bossTell63.numbers (fix + fix_why) in both config copies (byte-identical), asserting nothing else changed, AND writes
// the logs' cue lines to the test fixture Assets/Tests/EditMode/Game/Fixtures/n63-b2-before.txt (its source logs on line 1): N63BossTellTests
// computes the list from that fixture with BossCue63.Verdict and compares it to the config's, so the two only change together (B2c, DECIDE63 D-11).
//   node Tools/night/n63-b2-fixlist.cjs <before Player.log> [...] [--write]
//   node Tools/night/n63-b2-fixlist.cjs --selftest
'use strict';
const fs = require('fs');
const path = require('path');

const LEAD_MIN_MS = 300; // UnityTuning.Boss63CueLeadMinMs
const MIN_SAT = 0.5; // UnityTuning.Boss63CueMinSat

/** Review S3 (Logs/n63/review-n63.md): the danger band is the colour table's (design.n63.colours63.numbers.bands in Design/unity-config.json),
 * one band set with the meter (BossCue63.Use) and C1, never typed here. */
let BANDS = null;
function bands() {
  if (!BANDS) {
    const cfg = JSON.parse(fs.readFileSync(path.join(__dirname, '..', '..', 'Design', 'unity-config.json'), 'utf8'));
    const b = cfg.design.n63.colours63.numbers.bands;
    if (typeof b.dangerLo !== 'number' || typeof b.dangerHi !== 'number') throw new Error('no design.n63.colours63.numbers.bands.dangerLo / dangerHi');
    BANDS = { dangerLo: b.dangerLo, dangerHi: b.dangerHi };
  }
  return BANDS;
}
const inArc = (h, lo, hi) => (lo <= hi ? h >= lo && h <= hi : h >= lo || h <= hi);

/** One "[boss63] cue <boss> <attack> start .. hit .. lead .. hue .. sat .. parry .. danger .. tell .. kind .." line, or null. Its danger is
 * recomputed from the printed hue (whole degrees) and saturation with the table's band (a line printed under older bands says what those
 * said); its tell is the line's own. */
function parse(line) {
  const m = /\[boss63\] cue (\S+) (\S+) start (\S+) hit (\d+) lead (\d+) hue (\S+) sat (\S+) parry (yes|no) danger (yes|no) tell (\S+) kind (\S+)/.exec(line);
  if (!m) return null;
  const found = m[3] !== 'none', b = bands();
  const danger = found && m[6] !== '-' && Number(m[7]) >= MIN_SAT && inArc(Number(m[6]), b.dangerLo, b.dangerHi);
  return { boss: m[1], attack: m[2], found, danger, tell: m[10] === 'none' ? null : Number(m[10]), kind: m[11], hue: m[6] };
}

/** BossCue63.Verdict's rule. */
function verdict(c) {
  if (c.tell === null || c.tell < LEAD_MIN_MS) return 'short';
  if (c.found && !c.danger && c.kind !== 'yellow') return 'offhue';
  return 'ok';
}

function fixList(text) {
  const out = [];
  for (const l of text.split(/\r?\n/)) {
    const c = parse(l);
    if (!c) continue;
    const v = verdict(c);
    if (v !== 'ok') out.push({ id: c.boss + ':' + c.attack, why: `${v} (lead ${c.tell === null ? 'none' : c.tell} ms, hue ${c.hue}, kind ${c.kind})` });
  }
  return out;
}

function write(ids, why) {
  const root = path.join(__dirname, '..', '..');
  const files = [path.join(root, 'Design', 'unity-config.json'), path.join(root, 'Assets', 'StreamingAssets', 'Design', 'unity-config.json')];
  const raw = fs.readFileSync(files[0], 'utf8');
  if (raw !== fs.readFileSync(files[1], 'utf8')) throw new Error('the two config copies differ');
  const before = JSON.parse(raw);
  const row = before.design && before.design.n63 && before.design.n63.bossTell63;
  if (!row) throw new Error('no design.n63.bossTell63');
  const eol = raw.includes('\r\n') ? '\r\n' : '\n';
  const at = raw.indexOf('"bossTell63": {');
  if (at < 0) throw new Error('no "bossTell63": { in the text');
  // the row's own "numbers" (the first after its key, at the row's indent)
  const re = /\n( *)"numbers": (\{\}|\{[\s\S]*?\n\1\})/g;
  re.lastIndex = at;
  const m = re.exec(raw);
  if (!m) throw new Error("bossTell63's numbers not found");
  const indent = m[1];
  const numbers = { fix: ids, fix_why: why };
  const block = JSON.stringify(numbers, null, 2).split('\n').map((l, i) => (i === 0 ? l : indent + l)).join(eol);
  const start = m.index + 1 + indent.length;
  const end = m.index + m[0].length;
  const out = raw.slice(0, start) + '"numbers": ' + block + raw.slice(end);
  const after = JSON.parse(out);
  if (JSON.stringify(after.design.n63.bossTell63.numbers) !== JSON.stringify(numbers)) throw new Error('bossTell63.numbers not written as asked');
  after.design.n63.bossTell63.numbers = before.design.n63.bossTell63.numbers;
  if (JSON.stringify(after) !== JSON.stringify(before)) throw new Error('something outside design.n63.bossTell63.numbers changed');
  for (const f of files) fs.writeFileSync(f, out);
  if (fs.readFileSync(files[0], 'utf8') !== fs.readFileSync(files[1], 'utf8')) throw new Error('the copies differ after the write');
}

/** A log's path as the repo names it (Logs/player/<name> when it lives there). */
function repoName(f) {
  const p = path.resolve(f).replace(/\\/g, '/');
  const at = p.lastIndexOf('Logs/player/');
  return at >= 0 ? p.slice(at) : p;
}

/** The fixture's text: a source line, then each log's name and its parsed cue lines, verbatim, in order. */
function fixtureText(logs) {
  const out = [`# source: ${logs.map(repoName).join(', ')} (the boss63 before set: bossTell63 off); written by Tools/night/n63-b2-fixlist.cjs --write together with design.n63.bossTell63.numbers.fix; N63BossTellTests computes the list from these lines with BossCue63.Verdict`];
  for (const f of logs) {
    out.push('# ' + repoName(f));
    for (const l of fs.readFileSync(f, 'utf8').split(/\r?\n/)) if (parse(l)) out.push(l.trimEnd());
  }
  return out.join('\n') + '\n';
}

function writeFixture(logs) {
  const file = path.join(__dirname, '..', '..', 'Assets', 'Tests', 'EditMode', 'Game', 'Fixtures', 'n63-b2-before.txt');
  fs.mkdirSync(path.dirname(file), { recursive: true });
  fs.writeFileSync(file, fixtureText(logs));
  return file;
}

function selftest() {
  const log = [
    '[boss63] cue crown37 beamCut start 7650 hit 9133 lead 1483 hue 39 sat 0.65 parry yes danger no tell 1483 kind red objects 169 obj crownLook47/crown point edge',
    '[boss63] cue crown37 gemShard start 3950 hit 5125 lead 1175 hue 46 sat 0.76 parry yes danger no tell 1175 kind yellow objects 143 obj light THE CROWN/crown light',
    '[boss63] cue wick wickSlam start 4367 hit 5467 lead 1100 hue 10 sat 0.92 parry no danger yes tell 1100 kind red objects 2 obj the wick/flame core',
    '[boss63] cue wyrm spit start 23750 hit 24492 lead 742 hue 32 sat 0.84 parry no danger no tell 742 kind ? objects 6 obj KILN WYRM/trim 9',
    '[boss63] cue wyrm aggro start none hit 13408 lead 0 hue - sat - parry no danger no tell none kind ? objects 0 obj -',
    '[boss63] cue loom37 lash start 7067 hit 8392 lead 1325 hue 16 sat 0.88 parry no danger yes tell 200 kind red objects 46 obj x',
  ].join('\n');
  const got = fixList(log).map(x => x.id).join(',');
  const want = 'crown37:beamCut,wyrm:aggro,loom37:lash';
  console.log(`selftest: ${got === want ? 'PASS' : 'FAIL'} (want ${want}; got ${got}; danger band ${bands().dangerLo}-${bands().dangerHi} from the colour table): a red attack with a 39 deg cue is off-hue, no cue and a 200 ms tell are short; a parryable yellow, a danger cue and the 32 deg amber (danger in the table, review S3) are fine`);
  process.exit(got === want ? 0 : 1);
}

const args = process.argv.slice(2);
if (args[0] === '--selftest') selftest();
const doWrite = args.includes('--write');
const logs = args.filter(a => a !== '--write');
if (logs.length === 0) { console.log('usage: node Tools/night/n63-b2-fixlist.cjs <before Player.log> [...] [--write] | --selftest'); process.exit(2); }
const all = new Map();
for (const f of logs) {
  const list = fixList(fs.readFileSync(f, 'utf8'));
  console.log(`== ${f}: ${list.length ? list.map(x => x.id + ' ' + x.why).join('; ') : 'none'}`);
  for (const x of list) if (!all.has(x.id)) all.set(x.id, x.why);
}
const ids = [...all.keys()];
console.log(`fix list (${ids.length}): ${ids.join(', ') || 'none'}`);
if (doWrite) {
  const why = 'B2 fix list COMPUTED by Tools/night/n63-b2-fixlist.cjs from the boss63 before logs (bossTell63 off) with BossCue63.Verdict\'s rule: an attack whose cue is short (no danger- or parry-hue tell >= 300 ms before its hit) or off the danger hue (its strongest riser outside the colour table\'s danger band, design.n63.colours63.numbers.bands ' + bands().dangerLo + '-' + bands().dangerHi + ', recomputed from each line\'s hue: review S3; on an attack the sim does not mark parryable); parryable attacks never listed. Source: ' + logs.map(f => path.basename(f)).join(', ') + '. Each: ' + ids.map(i => i + ' ' + all.get(i)).join('; ') + '. Re-run the tool on a newer before set and commit its write.';
  write(ids, why);
  console.log('written: design.n63.bossTell63.numbers.fix in both config copies (byte-identical; nothing else changed)');
  const fx = writeFixture(logs);
  const cues = fs.readFileSync(fx, 'utf8').split('\n').filter(l => parse(l)).length;
  console.log(`written: ${path.relative(path.join(__dirname, '..', '..'), fx).replace(/\\/g, '/')} (${cues} cue lines; N63BossTellTests checks the config's list against it)`);
}
