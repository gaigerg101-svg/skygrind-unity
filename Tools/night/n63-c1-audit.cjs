#!/usr/bin/env node
// NIGHT63 builder D (slate C1, THE COLOUR TABLE; NIGHT63.md section 3): the audit. Lists every colour written by hand in Assets/Scripts/Game (a
// literal, not read from config's role colours), with file:line, what makes it (Color / Color32 / a named Color / a 0xRRGGBB hex / a "#RRGGBB"
// string / HSVToRGB), its RGB, its HSV and its ROLE BAND, plus where it lives (ui, effect, enemy, boss, world, viewmodel, tool). Read only.
//   node Tools/night/n63-c1-audit.cjs [--root <repo>] [--out <file>] [--all] [--json <file>]
//   node Tools/night/n63-c1-audit.cjs planted      (the classifier on known colours: every role, each off hue, neutral, and the parser's cases)
// THE ROLE BANDS (hue in degrees on the sRGB value as written; CLAUDE.md rule 8, DECIDE63 D-1, Colour56's header bands, widened by 5 deg where
// colour56's cap protects them): danger red-orange 345-35 (colors.enemy #FF4A1C is 11), parry yellow 45-70 (D-1: the parry tell only),
// player cyan 170-200 (colors.usable #19D3E6 is 186), rival magenta 285-335, boundary pale violet-white (hue 230-300, S < 0.40, V >= 0.75;
// barrier.boundaryColor #C9BCFF); NEUTRAL (no hue: S < 0.15 or V < 0.08: white, grey, black). Everything else is OFF, named by its hue
// (amber 35-45, lime 70-100, green 100-150, teal 150-170, blue 200-250, violet 250-285, rose 335-345). A read of config's role colours
// (Look.C(cfg, "colors.usable" | "colors.enemy"), barrier.boundaryColor) is counted as READ, not listed. "The red" (slate C1) = the hue-bearing
// literals (every band but neutral) in game code (tools excluded); OFF = those outside every role band.
const fs = require('fs');
const path = require('path');

const ROLE_READS = /Look\.C\([^,]+,\s*"(colors\.usable|colors\.enemy|barrier\.boundaryColor)"/g;

function hsv(r, g, b) {
  const mx = Math.max(r, g, b), mn = Math.min(r, g, b), d = mx - mn;
  let h = 0;
  if (d > 1e-9) {
    if (mx === r) h = (60 * ((g - b) / d) + 360) % 360;
    else if (mx === g) h = 60 * ((b - r) / d) + 120;
    else h = 60 * ((r - g) / d) + 240;
  }
  return { h, s: mx > 0 ? d / mx : 0, v: mx };
}

/** The role band of an sRGB colour (0-1 channels). */
function role(r, g, b) {
  const { h, s, v } = hsv(r, g, b);
  if (s < 0.15 || v < 0.08) return 'neutral';
  if (h >= 230 && h <= 300 && s < 0.40 && v >= 0.75) return 'boundary';
  if (h >= 345 || h <= 35) return 'danger';
  if (h >= 45 && h <= 70) return 'parry';
  if (h >= 170 && h <= 200) return 'player';
  if (h >= 285 && h <= 335) return 'rival';
  if (h > 35 && h < 45) return 'OFF amber';
  if (h > 70 && h < 100) return 'OFF lime';
  if (h >= 100 && h < 150) return 'OFF green';
  if (h >= 150 && h < 170) return 'OFF teal';
  if (h > 200 && h < 250) return 'OFF blue';
  if (h >= 250 && h < 285) return 'OFF violet';
  return 'OFF rose';
}

/** Where a file lives (its name tells). */
function where(rel) {
  const f = path.basename(rel);
  if (/^(FeelReel|SliceSmoke|PlayerChecks|ShotTour|FeelStrip|FeelProbe|FeelAB|Ui\.Reel|SmokeGuard|LevelSelect29Check|Prewarm|Hitch24)/.test(f) || /[\\/](Bot|MapAuthoring|Tape|Recap)[\\/]/.test(rel)) return 'tool';
  if (/^(Ui|Hud|UiStyle|Title|Menu|Kiosk|Lesson|Card|Banner)/.test(f)) return 'ui';
  if (/^(Boss|Wick|Crown|Brawl|Ice41|Wyrm|Loom|Stag|Glacier|Lantern|Tell36|Bulwark|Monolith|Rival|Hulk|Kit37)/.test(f)) return 'boss';
  if (/^(EnemyViews|Husk|Swarmer|Spitter|Enemy)/.test(f)) return 'enemy';
  if (/^(Viewmodel|Gun|Pistol|Weapon|Hands|Charge)/.test(f)) return 'viewmodel';
  if (/(Fx|Chunks|Particles|Juice|Impact|Decals|HitSpark|Spark|Tracer|Muzzle|Flash|Trail|Burst|KillConfirm|Crunch|Slam)/.test(f)) return 'effect';
  return 'world';
}

/** The file's text twice, positions kept: `noComments` (comments blanked, strings kept) and `codeOnly` (strings blanked too). */
function code(text) {
  const nc = text.split(''), co = text.split('');
  const blank = (arr, a, b) => { for (let k = a; k < b && k < arr.length; k++) if (arr[k] !== '\n') arr[k] = ' '; };
  let i = 0;
  const n = text.length;
  while (i < n) {
    const c = text[i], c2 = text[i + 1];
    if (c === '/' && c2 === '/') { const s = i; while (i < n && text[i] !== '\n') i++; blank(nc, s, i); blank(co, s, i); continue; }
    if (c === '/' && c2 === '*') { const s = i; while (i < n && !(text[i] === '*' && text[i + 1] === '/')) i++; i = Math.min(n, i + 2); blank(nc, s, i); blank(co, s, i); continue; }
    if (c === '@' && c2 === '"') { const s = i; i += 2; while (i < n && !(text[i] === '"' && text[i + 1] !== '"')) i += text[i] === '"' ? 2 : 1; i++; blank(co, s + 1, i - 1); continue; }
    if (c === '$' && c2 === '"') { i++; continue; } // an interpolated string: its holes are code (rare for colours); the string body below
    if (c === '"') { const s = i; i++; while (i < n && text[i] !== '"' && text[i] !== '\n') i += text[i] === '\\' ? 2 : 1; i++; blank(co, s + 1, i - 1); continue; }
    if (c === '\'') { i++; while (i < n && text[i] !== '\'' && text[i] !== '\n') i += text[i] === '\\' ? 2 : 1; i++; continue; }
    i++;
  }
  return { noComments: nc.join(''), codeOnly: co.join('') };
}

/** A 0xRRGGBB at `idx` is a colour: passed to a Hex(...) call, or assigned to (or listed under) a name that says colour, and not in a bitwise mask. */
function hexIsColour(src, idx, lineText) {
  const before = src.slice(Math.max(0, idx - 24), idx), after = src.slice(idx + 8, idx + 12);
  if (/[&|^]\s*$/.test(before) || /^\s*[&|^]/.test(after)) return false;
  if (/Hex\s*\(\s*(\(uint\)\s*)?$/.test(before)) return true;
  return /\b\w*(hex|colou?r|tint|ink|glow|rgb|accent|palette|key)\w*\s*(\[\s*\]\s*\w+\s*)?=/i.test(lineText) || /\b(uint|int)\[\]\s*\w*(hex|colou?r|tint|palette)\w*/i.test(lineText);
}

/** A numeric literal expression ("0.5f", "1", "200f / 255f", "0.2f * 3") to a number, else NaN. */
function num(s) {
  const t = s.trim().replace(/(\d)[fFdDmM]\b/g, '$1');
  if (!/^[-+]?[\d.]+(\s*[*/+-]\s*[\d.]+)*$/.test(t)) return NaN;
  try { const v = Function('"use strict"; return (' + t + ');')(); return typeof v === 'number' && isFinite(v) ? v : NaN; } catch (e) { return NaN; }
}

/** The top-level arguments of a call starting at `open` (the index of its "("); null if unbalanced. */
function args(src, open) {
  let depth = 0, start = open + 1;
  const out = [];
  for (let i = open; i < src.length; i++) {
    const c = src[i];
    if (c === '(') depth++;
    else if (c === ')') { depth--; if (depth === 0) { out.push(src.slice(start, i)); return { list: out, end: i }; } }
    else if (c === ',' && depth === 1) { out.push(src.slice(start, i)); start = i + 1; }
  }
  return null;
}

const NAMED = { red: [1, 0, 0], green: [0, 1, 0], blue: [0, 0, 1], yellow: [1, 0.92, 0.016], cyan: [0, 1, 1], magenta: [1, 0, 1], white: [1, 1, 1], black: [0, 0, 0], gray: [0.5, 0.5, 0.5], grey: [0.5, 0.5, 0.5], clear: [0, 0, 0] };

/** Every literal colour in one file's text. */
function scan(rel, text) {
  const { noComments, codeOnly: src } = code(text);
  const lineAt = (() => { const starts = [0]; for (let i = 0; i < text.length; i++) if (text[i] === '\n') starts.push(i + 1); return idx => { let lo = 0, hi = starts.length - 1; while (lo < hi) { const m = (lo + hi + 1) >> 1; if (starts[m] <= idx) lo = m; else hi = m - 1; } return lo + 1; }; })();
  const hits = [];
  const add = (idx, kind, r, g, b, txt) => hits.push({ file: rel, line: lineAt(idx), kind, r, g, b, text: txt.replace(/\s+/g, ' ').slice(0, 90) });
  let m;
  const reColor = /new\s+(?:UnityEngine\.)?Color(32)?\s*\(/g;
  while ((m = reColor.exec(src))) {
    const open = m.index + m[0].length - 1;
    const a = args(src, open);
    if (!a || a.list.length < 3) continue;
    const v = a.list.slice(0, 3).map(num);
    if (v.some(isNaN)) continue; // computed from a variable: not a literal
    const k = m[1] ? 255 : 1;
    add(m.index, m[1] ? 'Color32' : 'Color', v[0] / k, v[1] / k, v[2] / k, src.slice(m.index, a.end + 1));
  }
  const reNamed = /\bColor\.(red|green|blue|yellow|cyan|magenta|white|black|gray|grey|clear)\b/g;
  while ((m = reNamed.exec(src))) { const c = NAMED[m[1]]; add(m.index, 'Color.' + m[1], c[0], c[1], c[2], m[0]); }
  const reHex = /0x([0-9A-Fa-f]{6})\b/g;
  const srcLines = src.split('\n');
  while ((m = reHex.exec(src))) {
    const ln = lineAt(m.index);
    const lineText = srcLines[ln - 1] || '';
    if (!hexIsColour(src, m.index, lineText)) continue; // a hash, a mask or a seed, not a colour
    const x = parseInt(m[1], 16);
    add(m.index, '0xRRGGBB', ((x >> 16) & 255) / 255, ((x >> 8) & 255) / 255, (x & 255) / 255, lineText.trim());
  }
  const reStr = /"#([0-9A-Fa-f]{6})([0-9A-Fa-f]{2})?"/g;
  while ((m = reStr.exec(noComments))) {
    const x = parseInt(m[1], 16);
    add(m.index, '"#RRGGBB"', ((x >> 16) & 255) / 255, ((x >> 8) & 255) / 255, (x & 255) / 255, m[0]);
  }
  const reHsv = /Color\.HSVToRGB\s*\(/g;
  while ((m = reHsv.exec(src))) {
    const a = args(src, m.index + m[0].length - 1);
    if (!a || a.list.length < 3) continue;
    const v = a.list.slice(0, 3).map(num);
    if (v.some(isNaN)) continue;
    const h = ((v[0] % 1) + 1) % 1 * 6, s = v[1], val = v[2], i = Math.floor(h), f = h - i, p = val * (1 - s), q = val * (1 - s * f), t = val * (1 - s * (1 - f));
    const rgb = [[val, t, p], [q, val, p], [p, val, t], [p, q, val], [t, p, val], [val, p, q]][i % 6];
    add(m.index, 'HSVToRGB', rgb[0], rgb[1], rgb[2], src.slice(m.index, a.end + 1));
  }
  let reads = 0;
  ROLE_READS.lastIndex = 0;
  while (ROLE_READS.exec(noComments)) reads++;
  return { hits, reads };
}

function walk(dir, out) {
  for (const e of fs.readdirSync(dir, { withFileTypes: true })) {
    const p = path.join(dir, e.name);
    if (e.isDirectory()) walk(p, out);
    else if (e.name.endsWith('.cs')) out.push(p);
  }
  return out;
}

function audit(root) {
  const base = path.join(root, 'Assets', 'Scripts', 'Game');
  const rows = [];
  let reads = 0;
  for (const f of walk(base, [])) {
    const rel = path.relative(root, f).replace(/\\/g, '/');
    const r = scan(rel, fs.readFileSync(f, 'utf8'));
    reads += r.reads;
    for (const h of r.hits) {
      const { h: hue, s, v } = hsv(h.r, h.g, h.b);
      rows.push(Object.assign(h, { hue, s, v, role: role(h.r, h.g, h.b), where: where(rel) }));
    }
  }
  return { rows, reads };
}

const hex = r => '#' + [r.r, r.g, r.b].map(c => Math.max(0, Math.min(255, Math.round(c * 255))).toString(16).padStart(2, '0')).join('').toUpperCase();

function report(res, all) {
  const { rows, reads } = res;
  const game = rows.filter(r => r.where !== 'tool');
  const hued = game.filter(r => r.role !== 'neutral');
  const off = hued.filter(r => r.role.startsWith('OFF'));
  const lines = [];
  const by = (list, key) => { const m = {}; for (const r of list) m[r[key]] = (m[r[key]] || 0) + 1; return Object.entries(m).sort((a, b) => b[1] - a[1]).map(([k, n]) => k + ' ' + n).join(', '); };
  lines.push('C1 AUDIT: literal colours in Assets/Scripts/Game: ' + rows.length + ' (game code ' + game.length + ', tools ' + (rows.length - game.length) + '); role-colour reads from config ' + reads);
  lines.push('THE RED (hue-bearing literals in game code, not read from the role table): ' + hued.length + '; of them OFF every role band: ' + off.length + '; neutral (white / grey / black) ' + (game.length - hued.length));
  lines.push('  by role: ' + by(hued, 'role'));
  lines.push('  by place: ' + by(hued, 'where') + '   (OFF by place: ' + by(off, 'where') + ')');
  lines.push('  by kind: ' + by(hued, 'kind'));
  const files = {};
  for (const r of off) files[r.file] = (files[r.file] || 0) + 1;
  lines.push('  OFF by file (top 25): ' + Object.entries(files).sort((a, b) => b[1] - a[1]).slice(0, 25).map(([f, n]) => path.basename(f) + ' ' + n).join(', '));
  lines.push('');
  lines.push('file:line | kind | hex | hue S V | role | place | text');
  const list = (all ? rows : hued).slice().sort((a, b) => (a.role.startsWith('OFF') === b.role.startsWith('OFF') ? 0 : a.role.startsWith('OFF') ? -1 : 1) || a.file.localeCompare(b.file) || a.line - b.line);
  for (const r of list)
    lines.push(r.file + ':' + r.line + ' | ' + r.kind + ' | ' + hex(r) + ' | ' + r.hue.toFixed(0) + ' ' + r.s.toFixed(2) + ' ' + r.v.toFixed(2) + ' | ' + r.role + ' | ' + r.where + (r.where === 'ui' ? ' (HUD: GG 2026-10-20 "the UI looks good as it is": listed, not changed)' : '') + ' | ' + r.text);
  return lines;
}

function planted() {
  let bad = 0;
  const want = (name, got, exp) => { const ok = got === exp; bad += ok ? 0 : 1; console.log((ok ? 'PASS ' : 'FAIL ') + name + ': ' + got + (ok ? '' : ' (want ' + exp + ')')); };
  const c = x => [((x >> 16) & 255) / 255, ((x >> 8) & 255) / 255, (x & 255) / 255];
  want('colors.enemy #FF4A1C', role(...c(0xFF4A1C)), 'danger');
  want('colors.usable #19D3E6', role(...c(0x19D3E6)), 'player');
  want('barrier.boundaryColor #C9BCFF', role(...c(0xC9BCFF)), 'boundary');
  want('yellow56 parry #FFD000', role(...c(0xFFD000)), 'parry');
  want('RIVAL magenta #FF2BD6', role(...c(0xFF2BD6)), 'rival');
  want('span accent lime #8DD957', role(...c(0x8DD957)), 'OFF lime');
  want('iceline accent #86D9A2', role(...c(0x86D9A2)), 'OFF green');
  want('kiln accent blue #5A82E0', role(...c(0x5A82E0)), 'OFF blue');
  want('foundry accent violet #A267E6', role(...c(0xA267E6)), 'OFF violet');
  want('spire accent rose #E67793', role(...c(0xE67793)), 'OFF rose');
  want('white', role(1, 1, 1), 'neutral');
  want('mid grey', role(0.5, 0.5, 0.52), 'neutral');
  want('near black', role(0.05, 0.02, 0.0), 'neutral');
  const s = scan('Assets/Scripts/Game/WeaponFxX.cs', [
    'var a = new Color(0.55f, 0.85f, 0.35f); // lime literal',
    'var b = new Color(k, 0.2f, 0.1f); // computed: not a literal',
    'var c = new Color32(255, 74, 28, 255);',
    '// var d = new Color(1f, 0f, 1f); a comment: not counted',
    'var e = Color.magenta; var f = Look.Hex(0x8DD957); int mask = 0xFFFFFF00 & 0x00FF00;',
    'string g = "#A267E6"; Debug.Log("new Color(1,0,0) in a string");',
    'var h = Look.C(cfg, "colors.usable"); var i = new Color(200f / 255f, 1, 0.5f, 0.3f);',
  ].join('\n'));
  want('parser: literals found (lime, Color32, magenta, hex lime, "#A267E6", the 200/255 one)', s.hits.length, 6);
  want('parser: the role read counted', s.reads, 1);
  want('parser: the computed colour skipped', s.hits.some(h => h.line === 2), false);
  want('parser: the comment skipped', s.hits.some(h => h.line === 4), false);
  want('parser: line 5 gives Color.magenta and the Hex(0x8DD957), not the masks 0xFFFFFF00 / 0x00FF00', s.hits.filter(h => h.line === 5).length, 2);
  want('parser: the Color32 is danger', role(s.hits.find(h => h.kind === 'Color32').r, s.hits.find(h => h.kind === 'Color32').g, s.hits.find(h => h.kind === 'Color32').b), 'danger');
  console.log('C1 AUDIT PLANTED ' + (bad === 0 ? 'GREEN' : 'RED (' + bad + ' wrong)'));
  return bad ? 1 : 0;
}

function main(argv) {
  if (argv[0] === 'planted') return planted();
  let root = path.resolve(__dirname, '..', '..'), out = null, json = null, all = false;
  for (let i = 0; i < argv.length; i++) {
    if (argv[i] === '--root') root = path.resolve(argv[++i]);
    else if (argv[i] === '--out') out = argv[++i];
    else if (argv[i] === '--json') json = argv[++i];
    else if (argv[i] === '--all') all = true;
  }
  const res = audit(root);
  const lines = report(res, all);
  if (out) { fs.writeFileSync(out, lines.join('\n') + '\n'); console.log(lines.slice(0, 7).join('\n')); console.log('(full list: ' + out + ')'); }
  else console.log(lines.join('\n'));
  if (json) fs.writeFileSync(json, JSON.stringify(res.rows.map(r => ({ file: r.file, line: r.line, kind: r.kind, hex: hex(r), hue: +r.hue.toFixed(1), s: +r.s.toFixed(3), v: +r.v.toFixed(3), role: r.role, where: r.where })), null, 0));
  return 0;
}

process.exitCode = main(process.argv.slice(2));
