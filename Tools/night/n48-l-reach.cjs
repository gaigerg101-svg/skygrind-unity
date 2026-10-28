// NIGHT48 builder L: design.n48.loomReach48 (DECIDE48 D-34; THE LOOM's hit capsules) in both config copies (Design/unity-config.json and
// Assets/StreamingAssets/Design/unity-config.json, byte-identical). Re-runnable, on the lead's port/stage3 too:
//   - the row missing (a builder branch from before the lead added it): it is created after design.n48.lineup48 with the lead's text;
//   - the row there without "sim": true (the lead's first version): "sim": true is put first (a SIM row: NightRows reads only rows with it);
//   - its "numbers" replaced by builder L's below, each with its _why.
// Throws if anything outside that one row would change. `--dry` prints, writes nothing.
// Usage: node Tools/night/n48-l-reach.cjs [--dry]
"use strict";
const fs = require("fs");
const path = require("path");
const ROOT = path.resolve(__dirname, "..", "..");
const FILES = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"].map((f) => path.join(ROOT, f));
const dry = process.argv.includes("--dry");
const ROW = "loomReach48";

const ROW_DEF = {
  sim: true,
  _why: "Sim row (NightRows.On): THE LOOM's hit test. The drawn hull (view, loomHull48) runs 13.6 m long and its outer 3.8 m of each end lie outside the 3 m hit sphere (design.n37.loom37 bodyHalfW 3), so a shot at a visible point passes through (L's n48loom reach, DECIDE48 D-34). The row adds hit capsules along the hull's long axis, from the middle to each point; numbers come from the measurement, not from taste. Off: NIGHT47's sphere (--off48 keeps it).",
  enabled: false,
  enabled_why: "Off until builder L measures it: reach (every point hittable) and the human band (n47lhuman --level n47-span, 40 seeds, median 90-150 s, p90 <= 180, min >= 60) with it on, against off; the lead turns it on at the freeze only if both hold.",
  numbers: {},
};

const W = (v, why) => [v, why];
const NUMBERS = {
  radiusM: W(1.2, "m: each capsule's radius = the drawn spindle's half HEIGHT over its straight middle (UnityTuning.L48.cs: Loom37BodyH 3 x Hull48HeightMul 0.8 / 2; its half width across the near faces is 1.165 m), so a shot at the drawn body's flank or belly lands; the 3 m sphere still covers the middle."),
  innerM: W(0, "m along the axis from the centre where each capsule starts: the middle (the two capsules meet there: one segment point to point)."),
  outerM: W(5.6, "m along the axis from the centre where each capsule's segment ends: lengthM / 2 - radiusM, so its round end reaches the drawn point at 6.8 m and no further."),
  lengthM: W(13.6, "m point to point of the drawn hull (Loom37BodyL 8 x Hull48LenMul 1.7): the check outerM + radiusM = lengthM / 2 (n48loom reach asserts it); not read by the sim."),
  yawFollow: W(0.35, "share of the angle the long axis turns from world x toward the shooter's broadside: the view's own rule (UnityTuning.L48.cs Hull48YawFollow, which must stay equal), so the capsules lie where the hull is drawn."),
  yawMaxDeg: W(20, "deg the long axis turns at most (the view's Hull48YawMaxDeg, which must stay equal)."),};

function span(out, at) {
  let i = out.indexOf("{", at), depth = 0, j = i, inStr = false;
  for (; j < out.length; j++) {
    const ch = out[j];
    if (inStr) { if (ch === "\\") j++; else if (ch === '"') inStr = false; continue; }
    if (ch === '"') inStr = true;
    else if (ch === "{" || ch === "[") depth++;
    else if (ch === "}" || ch === "]") { depth--; if (depth === 0) break; }
  }
  return [i, j];
}

function patch(raw) {
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  let out = raw;
  let at = out.indexOf('"' + ROW + '": {');
  if (at < 0) {
    const after = out.indexOf('"lineup48": {');
    if (after < 0) throw new Error("design.n48.lineup48 not found (the anchor)");
    const [, j] = span(out, after);
    const lineStart = out.lastIndexOf("\n", after) + 1;
    const indent = out.slice(lineStart, after);
    const body = JSON.stringify(ROW_DEF, null, 2).split("\n").map((l, k) => (k === 0 ? l : indent + l)).join(eol);
    out = out.slice(0, j + 1) + "," + eol + indent + '"' + ROW + '": ' + body + out.slice(j + 1);
    at = out.indexOf('"' + ROW + '": {');
  }
  let [i, j] = span(out, at);
  if (out.slice(i, j).indexOf('"sim": true') < 0) {
    const firstKey = out.indexOf('"', i + 1);
    const lineStart = out.lastIndexOf("\n", firstKey) + 1;
    const indent = out.slice(lineStart, firstKey);
    out = out.slice(0, firstKey) + '"sim": true,' + eol + indent + out.slice(firstKey);
    [i, j] = span(out, at);
  }
  const key = out.indexOf('"numbers": ', i);
  if (key < 0 || key > j) throw new Error(ROW + ": no numbers key inside it");
  const lineStart = out.lastIndexOf("\n", key) + 1;
  const indent = out.slice(lineStart, key);
  const [ni, nj] = span(out, key);
  const o = {};
  for (const [k, [v, why]] of Object.entries(NUMBERS)) { o[k] = v; o[k + "_why"] = why; }
  const block = JSON.stringify(o, null, 2).split("\n").map((l, k) => (k === 0 ? l : indent + l)).join(eol);
  return out.slice(0, ni) + block + out.slice(nj + 1);
}

const raws = FILES.map((f) => fs.readFileSync(f, "utf8"));
if (raws[0] !== raws[1]) throw new Error("the two config copies differ before the patch");
const next = patch(raws[0]);
const a = JSON.parse(raws[0]), b = JSON.parse(next);
const want = {};
for (const [k, [v, why]] of Object.entries(NUMBERS)) { want[k] = v; want[k + "_why"] = why; }
if (JSON.stringify(b.design.n48[ROW].numbers) !== JSON.stringify(want)) throw new Error("numbers did not round-trip");
if (b.design.n48[ROW].sim !== true) throw new Error("sim: true did not land");
if (b.design.n48[ROW].enabled !== false) throw new Error("the row must stay off (the lead turns it on)");
delete a.design.n48[ROW];
delete b.design.n48[ROW];
if (JSON.stringify(a) !== JSON.stringify(b)) throw new Error("something outside design.n48." + ROW + " changed");
console.log(`n48-l-reach: design.n48.${ROW}: ${Object.keys(NUMBERS).length} numbers; ${next === raws[0] ? "already in" : dry ? "would change (dry run)" : "written to both copies"}`);
if (!dry && next !== raws[0]) for (const f of FILES) fs.writeFileSync(f, next);
