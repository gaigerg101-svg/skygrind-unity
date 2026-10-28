// NIGHT43 lead (integration, K1's hand-off): LevelValidate29 had no wave-budget cost for A1's "burrower" (levelgen: "kind 'burrower' has
// no cost" on n43-kiln). Adds design.n43.burrow43.budgetCost 4 (a level-authoring number, no gameplay effect) to both config copies and
// one fallback in LevelValidate29's cost lookup. Run once: node Tools/night/n43-burrower-cost.cjs
const fs = require("fs");
const root = "C:/Users/gaige/dev/SKYGRIND-unity/";
const file = root + "Design/unity-config.json", copy = root + "Assets/StreamingAssets/Design/unity-config.json";
let raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("config copies differ");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const d0 = JSON.parse(raw);
if (d0.design.n43.burrow43.budgetCost !== undefined) throw new Error("already there");
const anchor = '      "burrow43": {' + eol;
if (raw.split(anchor).length !== 2) throw new Error("burrow43 anchor not unique");
raw = raw.replace(anchor, anchor + '        "budgetCost": 4,' + eol + '        "budgetCost_why": "The wave budget\'s cost of a BURROWER (LevelValidate29; K1\'s n43-kiln places it): 220 hp, a ground-only threat with a counter window: a charger\'s 4. A level-authoring number, no gameplay effect; Unity-only (the web has no wave budget).",' + eol);
const d = JSON.parse(raw);
if (d.design.n43.burrow43.budgetCost !== 4) throw new Error("patch failed");
fs.writeFileSync(file, raw);
fs.copyFileSync(file, copy);

const cs = root + "Assets/Scripts/Game/MapAuthoring/LevelValidate29.cs";
let s = fs.readFileSync(cs, "utf8");
const a = ': cfg.Has("design.n40.iceline40.budgetCost." + k) ? cfg.Num("design.n40.iceline40.budgetCost." + k) : double.NaN;';
if (s.split(a).length !== 2) throw new Error("LevelValidate29 anchor not unique");
s = s.replace(a, ': cfg.Has("design.n40.iceline40.budgetCost." + k) ? cfg.Num("design.n40.iceline40.budgetCost." + k) : k == "burrower" && cfg.Has("design.n43.burrow43.budgetCost") ? cfg.Num("design.n43.burrow43.budgetCost") : double.NaN;');
fs.writeFileSync(cs, s);
console.log("burrower cost 4 added (config + LevelValidate29)");
