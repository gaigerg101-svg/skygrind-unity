// NIGHT34 builder B (leftover 3): N34Blocks bit 5 combo34 (counts only with bend34 on, like the split), and design.n34.combo34 in
// Design/unity-config.json (enabled false). Run from the repo (or worktree) root: node Tools/night/n34b-combo-blocks.cjs   (the N34Blocks part is skipped when present; the config part refuses to run twice),
// then cp the config to Assets/StreamingAssets/Design/unity-config.json and cmp.
const fs = require("fs");
const path = require("path");
const root = process.cwd();

// 1. Core/N34Blocks.cs
const bf = path.join(root, "Assets/Scripts/Core/N34Blocks.cs");
let s = fs.readFileSync(bf, "utf8");
const blocksDone = s.includes("combo34"); // already patched (a cherry-pick): only the config part runs
const rep = (a, b) => {
  const n = s.split(a).length - 1;
  if (n !== 1) throw new Error("N34Blocks anchor found " + n + " times: " + a);
  s = s.replace(a, b);
};
if (!blocksDone) rep("public bool bend34, bendSplit34, flow34, reach34, air34;", "public bool bend34, bendSplit34, flow34, reach34, air34, combo34;");
if (!blocksDone) rep("public const int Count = 5;", "public const int Count = 6;");
if (!blocksDone) rep("public bool Any => bend34 || bendSplit34 || flow34 || reach34 || air34;", "public bool Any => bend34 || bendSplit34 || flow34 || reach34 || air34 || combo34;");
if (!blocksDone) rep("                air34 = cfg.Design(\"n34.air34.enabled\"),", "                air34 = cfg.Design(\"n34.air34.enabled\"),\n                combo34 = bend && cfg.Design(\"n34.combo34.enabled\"), // the named combo lives inside a bend: only with bend34 on");
if (!blocksDone) rep("4 air34).</summary>", "4 air34, 5 combo34).</summary>");
if (!blocksDone) rep("i == 4 && air34;", "i == 4 ? air34 : i == 5 && combo34;");
if (!blocksDone) rep("air34 = (b & 16) != 0,", "air34 = (b & 16) != 0, combo34 = (b & 32) != 0,");
if (!blocksDone) rep("\"reach34\", \"air34\" }", "\"reach34\", \"air34\", \"combo34\" }");
if (!blocksDone) rep("// air34 (NIGHT34 B leftover 1:", "// combo34 (NIGHT34 B leftover 3: a named combo for kills with two or more guns inside one bend, a style bonus), air34 (NIGHT34 B leftover 1:");
if (!blocksDone) fs.writeFileSync(bf, s);
console.log(blocksDone ? "Assets/Scripts/Core/N34Blocks.cs already has combo34: skipped" : "patched Assets/Scripts/Core/N34Blocks.cs");

// 2. design.n34.combo34
const file = path.join(root, "Design", "unity-config.json");
const raw = fs.readFileSync(file, "utf8");
const d = JSON.parse(raw);
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
if (d.design.n34.combo34) throw new Error("design.n34.combo34 already exists");
const web = "The web has no such number: it has no bend and no named combos across guns (its style meter's cross combos are pairs of verbs, not a bend).";
d.design.n34.combo34 = {
  _why: "Builder B (NIGHT34.md leftover 3, C-17): a NAMED COMBO: a grapple latch starts a bend, and kills inside that one bend with two or more different guns (the loadout's guns: ArsenalMath.WeaponOf of the kill's weapon, owned; the whip's hit does not count) stamp a name. The sim emits bend34Combo {name, guns, kills, points} each time the bend's count of different killing guns reaches a new step (2, 3, 4+); the style meter pays the step's bonus (StyleCore.Bonus, the name as its source). Builder H shows the name in hud33's one message slot. Needs bend34 (N34Blocks.combo34 counts only with it). " + web,
  enabled: false,
  names: ["BEND BREAKER", "TRIPLE BEND", "FULL ARSENAL"],
  names_why: "The combo's name by the number of different killing guns inside one bend: 2, 3, 4 or more (the last one repeats). Short and loud for hud33's one message slot (text >= 16 px). Unity-only names, the forge's choice (NIGHT34.md C-17: 'grapple -> bend -> two different guns -> kill stamps a combo name'). " + web,
  stylePoints: [150, 250, 400],
  stylePoints_why: "The style bonus per step (2, 3, 4+ guns), before the meter's multipliers. Read against PortExport/config.json: style.base.huskKill 30, style.bonus.multikill 25, airshot 12; so a 2-gun bend combo pays five husk kills' style, a 4-gun one about thirteen. The forge's choice, logged as such. " + web,
};
const block = JSON.stringify(d.design.n34, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
const start = raw.indexOf("\"n34\": {");
const tail = eol + "  }" + eol + "}";
if (start < 0 || !raw.endsWith(tail)) throw new Error("unexpected config layout");
fs.writeFileSync(file, raw.slice(0, start) + "\"n34\": " + block + tail);
const back = JSON.parse(fs.readFileSync(file, "utf8"));
if (back.design.n34.combo34.enabled !== false) throw new Error("combo34 must stay off");
console.log("design.n34.combo34 added (names, stylePoints; enabled false)");
