// NIGHT48 builder W: sets design.n48.climbTop48.numbers (G1's top-third lips; read by Assets/Scripts/Game/SurvivalSim.N48.Wick.cs) in BOTH config
// copies, touching nothing else: the row's object is re-written in place (its other keys kept, numbers replaced); everything outside
// design.n48.climbTop48.numbers must round-trip, and the two copies must be byte-identical after. Idempotent.
// Usage (worktree root): node Tools/night/w48-config.cjs [--dry]
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const COPIES = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];

const NUMBERS = {
  lipAfter: [4, 5, 6, 7, 8],
  lipAfter_why: "The panels whose far end gets a lip: every ride gap of the top third (panels 4 -> 5 .. 8 -> 9: section 2, rise 2.8, with extraTopPanels 2). NIGHT47 measured the three it had at 31 / 40, 30 / 40, 19 / 33 first tries (n47wick sloppy, 40 seeds); traced tonight (n48wick sloppy --steps): the 'late' rider held jump through the whole release, rode past the panel's end and fell (Research/2026-10-19-wave48/W.md). The last panel's leap is to the cap (a floor), not a ride gap.",
  extraTopPanels: 2,
  extraTopPanels_why: "Panels added to the top section (n47's design.n47.wick47.numbers.stack.panelCount 8 + this; the cap 26.3 -> 31.9 m): with the lips the sloppy climb fell less and took 20.7 s (G1 asks 25-45 s); one more panel measured 24.7 s, two 27.0 s with every G1 gate and the human band held (Research/2026-10-19-wave48/W.md). The lead's call DECIDE48 D-23: two panels.",
  lipOver: 0.8,
  lipOver_why: "m: the lip's top over its panel's bottom. Under the rider's feet at the panel's end (the catch line, design.n47.climb47.numbers.catchLine 1.0), so a late rider steps DOWN onto it and never hits its end face; over 0.7 so Wick37Pilot's next-panel pick (NextFromFeet: bottoms >= feet - 0.7) names the next panel, not the one just left; the hop from it to the next panel (2.8 m over this panel's bottom, attached from catchBelow 2.0 under its catch line) asks a 1.0 m rise, under a plain jump's 1.15 m (W.md, the reach table).",
  lipArcDeg: 22,
  lipArcDeg_why: "deg of the ring the lip spans from its start: past the panel's end the next panel starts 36.8 deg on (slots 90 deg apart, each face 53.1 deg at apothem 8); 22 deg (3.1 m at 8 m) leaves 14 deg (about 2 m) of open wall before the next panel: the hop is a jump, the lip never touches the next face.",
  lipStartDeg: 0,
  lipStartDeg_why: "deg past the panel's end angle (its face's half span at the apothem) where the lip begins: at 0 the lip meets the face's end, so the rider leaves the panel straight over it.",
  lipDepth: 2.0,
  lipDepth_why: "m from the shaft wall in (the lip's inner edge 7.0 m from the axis): a rider leaves a panel's end against the wall (the face's end is 8.94 m from the axis) and the hot wall pushes inward; 2.0 m holds a rider pushed in, and is NIGHT47's 2.6 m step depth less a margin (the shelf reads as a lip, not a ledge ring).",
};

const dry = process.argv.includes("--dry");
const texts = [];
for (const rel of COPIES) {
  const p = path.join(ROOT, rel);
  const raw = fs.readFileSync(p, "utf8");
  const before = JSON.parse(raw);
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  const n48 = raw.indexOf('"n48": {');
  const key = raw.indexOf('"climbTop48": {', n48);
  if (n48 < 0 || key < 0) throw new Error(rel + ": design.n48.climbTop48 not found");
  // the row object's span
  const open = raw.indexOf("{", key);
  let depth = 0, end = -1;
  for (let j = open; j < raw.length; j++) {
    const ch = raw[j];
    if (ch === '"') { j++; while (raw[j] !== '"') { if (raw[j] === "\\") j++; j++; } continue; }
    if (ch === "{") depth++;
    else if (ch === "}") { depth--; if (depth === 0) { end = j; break; } }
  }
  const lineStart = raw.lastIndexOf("\n", key) + 1;
  const indent = raw.slice(lineStart, key);
  const row = Object.assign({}, before.design.n48.climbTop48, { numbers: NUMBERS });
  const body = JSON.stringify(row, null, 2).split("\n").map((l, n) => (n === 0 ? l : indent + l)).join(eol);
  const text = raw.slice(0, open) + body + raw.slice(end + 1);
  const after = JSON.parse(text);
  // the guard: only design.n48.climbTop48.numbers moved
  const strip = (o) => { const c = JSON.parse(JSON.stringify(o)); delete c.design.n48.climbTop48.numbers; return JSON.stringify(c); };
  if (strip(before) !== strip(after)) throw new Error(rel + ": something outside design.n48.climbTop48.numbers changed");
  if (JSON.stringify(after.design.n48.climbTop48.numbers) !== JSON.stringify(NUMBERS)) throw new Error(rel + ": numbers not written");
  texts.push([p, text, raw === text]);
}
if (texts[0][1] !== texts[1][1]) throw new Error("the two copies differ after the patch");
for (const [p, text, same] of texts) {
  if (same) { console.log(path.relative(ROOT, p) + ": already in"); continue; }
  if (dry) console.log(path.relative(ROOT, p) + ": would write design.n48.climbTop48.numbers");
  else { fs.writeFileSync(p, text); console.log(path.relative(ROOT, p) + ": wrote design.n48.climbTop48.numbers"); }
}
