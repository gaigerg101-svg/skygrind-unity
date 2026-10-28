// NIGHT34 builder A: builder A's own design.n34 rows in Design/unity-config.json (anchored text edits, so the rest of the file keeps its layout), then
// the Player's copy (Assets/StreamingAssets/Design/unity-config.json) by copy + byte compare. Never sets `enabled` (the lead turns rows on).
// Usage, from the repo (worktree) root:
//   node Tools/night/n34a-config.cjs calm                 design.n34.calm34: keepKinds + their _why
//   node Tools/night/n34a-config.cjs swap foundry kiln    design.n34.lines34.levelSwap: n33-<name> -> n34-<name> for each name (the whole map rewritten)
//   [--file <unity-config.json>]  edit another copy (a dry run on a scratch copy; no StreamingAssets copy then)
'use strict';
const fs = require('fs');
const args = process.argv.slice(2);
const fi = args.indexOf('--file');
const file = fi >= 0 ? args[fi + 1] : 'Design/unity-config.json';
const rest = fi >= 0 ? args.filter((a, i) => i !== fi && i !== fi + 1) : args;
const mode = rest[0];
let t = fs.readFileSync(file, 'utf8');
const nl = t.includes('\r\n') ? '\r\n' : '\n';

function once(text, a, b) {
  const n = text.split(a).length - 1;
  if (n !== 1) throw new Error(`anchor found ${n} times: ${a.slice(0, 120)}`);
  return text.replace(a, b);
}
// the n34 block's start: every anchor below is searched after it, so an older night's row of the same name never matches
const n34At = t.indexOf('"n34": {');
if (n34At < 0) throw new Error('no design.n34 block');
const head = t.slice(0, n34At);
let body = t.slice(n34At);

if (mode === 'calm') {
  const KEEP = ['wash', 'doorFrame', 'lintelLight', 'hero', 'lamp', 'lampPost', 'chain', 'truss', 'kilnArch', 'lavaChannel', 'hazardStripe'];
  const start = body.indexOf('"calm34": {');
  if (start < 0) throw new Error('no design.n34.calm34');
  const end = body.indexOf('}', start);
  const block = body.slice(start, end);
  const ind = '        ';
  const why = 'The dress33 kinds kept with calm34 on (Calm34.Keep; every other dress33 row is cut). research/arenas.md rule 1 (ULTRAKILL devs, 80.lv: "the geometry of a level is simple and clean"; Neon White: "a clean, low detail look with lots of negative space"): cut every prop that is not a surface you use. Kept, each for a purpose: wash (the wall surface itself, K1 5.2: no wall reads as one flat colour), doorFrame + lintelLight (a door reads at a glance), hero (the room\'s one landmark: where am I), lamp / lampPost / chain / truss (the fixtures that light a room; a lamp keeps its chain and truss, never floating), kilnArch (the kiln mouths\' glow), lavaChannel / hazardStripe (danger, red-orange). Cut: skirt, cornice, rib, pilaster, fin, recess, banner, pipes, wallPipe, edgeLight, inlay, debris, scorch, drift, crystal, ingots, stalactite, halo. Never touched: every row with a collider (cover, crates, pillars, consoles), devices, pickups (not dress33 rows). A view list (no sim effect): the web has no dressing at all.';
  // the block rebuilt from its parsed keys (so a second run gives the same text): every other key first, as it was, then keepKinds_why, keepKinds
  const obj = JSON.parse(block.slice(block.indexOf('{')) + '}');
  delete obj.keepKinds_why; delete obj.keepKinds;
  const lines = Object.entries(obj).map(([k, v]) => ind + JSON.stringify(k) + ': ' + JSON.stringify(v));
  lines.push(ind + '"keepKinds_why": ' + JSON.stringify(why), ind + '"keepKinds": ' + JSON.stringify(KEEP).replace(/","/g, '", "'));
  const nb = '"calm34": {' + nl + lines.join(',' + nl) + nl + '      ';
  body = body.slice(0, start) + nb + body.slice(end);
} else if (mode === 'swap') {
  const names = rest.slice(1);
  if (names.length === 0) throw new Error('swap: name at least one level (foundry kiln span spire iceline)');
  for (const n of names) if (!/^[a-z]+$/.test(n) || !fs.existsSync(`Design/levels/n34-${n}.json`)) throw new Error(`swap: Design/levels/n34-${n}.json does not exist`);
  const start = body.indexOf('"lines34": {');
  if (start < 0) throw new Error('no design.n34.lines34');
  const end = body.indexOf('"calm34": {', start);
  const block = body.slice(start, end);
  const map = '{ ' + names.map((n) => `"n33-${n}": "n34-${n}"`).join(', ') + ' }';
  const nb = block.replace(/"levelSwap": \{[^}]*\}/, '"levelSwap": ' + map);
  if (nb === block && !block.includes('"levelSwap": ' + map)) throw new Error('lines34.levelSwap not found');
  body = body.slice(0, start) + nb + body.slice(end);
} else throw new Error('usage: n34a-config.cjs calm | swap <name> ... [--file f]');

const out = head + body;
JSON.parse(out); // still JSON
fs.writeFileSync(file, out);
console.log(`n34a-config ${mode}: wrote ${file}`);
if (fi < 0) {
  const sa = 'Assets/StreamingAssets/Design/unity-config.json';
  fs.copyFileSync(file, sa);
  if (!fs.readFileSync(file).equals(fs.readFileSync(sa))) throw new Error('COPY MISMATCH: ' + sa);
  console.log('  copies identical: ' + sa);
}
