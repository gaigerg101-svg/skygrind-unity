// NIGHT34 builder B (B1): adds the bend's numbers to design.n34.bend34 in Design/unity-config.json (enabled stays false; the lead turns the
// row on). Each number carries a _why naming its source (Research/2026-10-11-wave34/RESEARCH.md table B) and why the web has none.
// Run from the repo (or worktree) root: node Tools/night/n34b-bend-config.cjs   (refuses to run twice), then
// cp Design/unity-config.json Assets/StreamingAssets/Design/unity-config.json and cmp them.
const fs = require("fs");
const path = require("path");
const file = path.join(process.cwd(), "Design", "unity-config.json");
const raw = fs.readFileSync(file, "utf8");
const d = JSON.parse(raw);
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const b = d.design.n34.bend34;
if (b.scale !== undefined) throw new Error("design.n34.bend34 already has its numbers");
const web = "The web has no slow motion of its own (its killSlowMo is a view effect), so it has no such number.";
const add = {
  scale: 0.25,
  scale_why: "The world's time share inside a bend (global: the loop's share; split: the world steps 1 in round(1 / scale)). RESEARCH.md table B: not found in any game (research/slowmo.md 3); the forge's choice, judged by frames f1-f4. " + web,
  inS: 0.08,
  inS_why: "Ease in, real seconds. RESEARCH.md table B (research/slowmo.md 9, 2). Global: whole sim steps at the ramp's mean share (1 + scale) / 2; split: whole player steps. " + web,
  outS: 0.3,
  outS_why: "Ease out, real seconds (research/slowmo.md 9: a 0.3 s ramp out; 2024 paper: 0.2-0.4 s reads heavy). Converted as inS. " + web,
  shortS: 1.0,
  shortS_why: "The SHORT BEND on every latch (node, wall, the whip on an enemy), free, real seconds. RESEARCH.md table B: the red team's finding 13 (GG's 'the whole point' is real-time swaps on the grapple). The grapple's own charges (NIGHT32) are its limit. " + web,
  holdS: 2.5,
  holdS_why: "The FULL BEND's length, real seconds, paid by a full meter. RESEARCH.md table B: research/slowmo.md 9 (3 s), shortened for small rooms. " + web,
  killExtendS: 0.5,
  killExtendS_why: "Each kill inside a FULL bend adds this, real seconds. RESEARCH.md table B: research/slowmo.md 9 (+0.5 s per kill). " + web,
  maxS: 5,
  maxS_why: "A FULL bend's cap, real seconds (in + hold). RESEARCH.md table B: research/slowmo.md 9 (cap 6 s), shortened for small rooms. " + web,
  refillPerKill: 0.25,
  refillPerKill_why: "The BEND METER (0..1, starts full at every run start; nothing refills it over time) gains this per player kill outside a full bend (kills inside one extend it instead). RESEARCH.md table B: research/slowmo.md 1, 6 (Max Payne 2, Severed Steel); 0.25 is the forge's choice (no source found). " + web,
  rankDoubleMult: 2,
  rankDoubleMult_why: "A kill at style rank rankDoubleLetter or higher refills x this. RESEARCH.md table B: the forge's choice (no source found). " + web,
  rankDoubleLetter: "S",
  rankDoubleLetter_why: "The style rank letter (config style.ranks, D C B A S SS) from which a kill counts double. RESEARCH.md table B: 'x2 at rank S+', the forge's choice. " + web,
};
for (const k of Object.keys(add)) b[k] = add[k];
const block = JSON.stringify(d.design.n34, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
const start = raw.indexOf("\"n34\": {");
if (start < 0) throw new Error("no design.n34");
const tail = eol + "  }" + eol + "}";
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
fs.writeFileSync(file, raw.slice(0, start) + "\"n34\": " + block + tail);
const back = JSON.parse(fs.readFileSync(file, "utf8"));
if (back.design.n34.bend34.enabled !== false) throw new Error("bend34 must stay off");
console.log("design.n34.bend34 numbers: " + Object.keys(add).filter((k) => !k.endsWith("_why")).join(", "));
