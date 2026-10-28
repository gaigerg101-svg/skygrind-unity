// NIGHT30 builder B: adds THE LOOM's numbers (design.n30.span.loom) to Design/unity-config.json by text insertion after the span block's "enabled" line
// (keeps the line endings and the layout; idempotent: does nothing when "loom" is already in the block). Run from the repo root: node Tools/night/n30b-config-loom.cjs
const fs = require('fs');
const path = 'Design/unity-config.json';
let text = fs.readFileSync(path, 'utf8');
const eol = text.includes('\r\n') ? '\r\n' : '\n';
const at = text.indexOf('"span": {', text.indexOf('"n30": {'));
if (at < 0) throw new Error('no span block');
const end = text.indexOf(eol + '      }', at);
if (text.slice(at, end).includes('"loom": {')) { console.log('loom numbers already present'); process.exit(0); }
const m = /"enabled": (true|false)/.exec(text.slice(at));
if (!m || at + m.index > end) throw new Error('no enabled line in span');
const lineEnd = at + m.index + m[0].length;
const I = '        ', J = '          ';
const rows = [
  `"_why": "THE LOOM (DESIGN30 C4 / D; Core/Enemies/Loom30.cs): LEVEL 4's boss in room s8, a blocky crane-spider under the span's deck, four legs gripping the deck's corners (the plan's loomLegs), two joints a leg that are grapple nodes; nails land in full on a joint while you hang on that leg's line or stand by the joint, lightScale elsewhere; its first attack CUT plucks the line you hang on. First pass: the numbers are first picks (no kill band measured yet)."`,
  `"name": "THE LOOM"`, `"name_why": "The boss bar's name."`,
  `"pool": 2400`, `"pool_why": "Hit points (x the fight's hp scale). A first pick: the kill band 90-150 s (D-5) is not measured yet (no grapple-and-nail pilot)."`,
  `"bodyDrop": 3`, `"bodyDrop_why": "The body hangs this far under the deck's surface (m): under the span, its top just below the deck."`,
  `"bodyHalfW": 4`, `"bodyHalfW_why": "The body's half width (m): an 8 m block."`,
  `"bodyHalfH": 2`, `"bodyHalfH_why": "The body's half height (m)."`,
  `"legHalf": 9`, `"legHalf_why": "With no loomLegs in the plan, the grips sit at the deck's centre +- this (m): L's 18 x 18 deck."`,
  `"kneeOut": 3`, `"kneeOut_why": "The knee joint stands this far OUT past its grip, over the lava ring (m): the leg arches over the deck's edge."`,
  `"kneeUp": 9`, `"kneeUp_why": "The knee joint's height over the deck (m): near L's static nodes at y 12, so a swing from one reaches it."`,
  `"hipShare": 0.45`, `"hipShare_why": "The hip joint sits this share of the way from the body's side to the knee."`,
  `"jointRadius": 1`, `"jointRadius_why": "A joint's hit sphere (m): the nails' target."`,
  `"fullReach": 3`, `"fullReach_why": "Standing within this of a joint (m, chest to joint) also lands full damage on it (DESIGN30 D: within 3 m)."`,
  `"lineReach": 2`, `"lineReach_why": "The rope's anchor within this of a leg (m: its grip, knee, hip, the body's side) counts as hanging on that leg's line."`,
  `"lightScale": 0.2`, `"lightScale_why": "The share that lands on a joint off its line, and on the body (DESIGN30 D: light damage 0.2 elsewhere)."`,
  `"cutFirstSeconds": 6`, `"cutFirstSeconds_why": "The first CUT can come this long after the intro (the first swings read first)."`,
  `"cutEverySeconds": 6`, `"cutEverySeconds_why": "After a cut (or a dodged one) the next waits this long; it only tells while you hang on a rope."`,
  `"cutTellSeconds": 1`, `"cutTellSeconds_why": "The red tell on your line before it is plucked (DESIGN30 D: a 1.0 s tell; tells 1-1.5 s)."`,
  `"cutDropSpeed": 2`, `"cutDropSpeed_why": "The plucked line drops you at this downward speed (m/s), half your swing's horizontal speed kept: grab another node."`,
  `"introHold": 3`, `"introHold_why": "No damage and no cut for this long after it rises (its intro card)."`,
  `"dieTime": 1.5`, `"dieTime_why": "The finisher's fall (s)."`,
  `"legThick": 0.8`, `"legThick_why": "The legs' box thickness (m; the view's)."`,
];
let ins = ',' + eol + I + '"loom": {';
ins += rows.map((r, i) => eol + J + r + (i < rows.length - 1 ? ',' : '')).join('');
ins += eol + I + '}';
text = text.slice(0, lineEnd) + ins + text.slice(lineEnd);
fs.writeFileSync(path, text);
console.log('span.loom: added');
