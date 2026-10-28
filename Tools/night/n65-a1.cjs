// NIGHT65 A1 (the lead; the shared commit, L-049 / L-065; NIGHT57's recipe, Research/2026-10-23-forge65/code-scout.md section 5; modelled on
// Tools/night/n64-a1-rows.cjs). Into Design/unity-config.json and its byte copy Assets/StreamingAssets/Design/unity-config.json it writes:
//   1. design.n65 as design's last key: the block's _why and the LEVEL row foundry65 {levelFrom n52-foundry, levelTo n65-foundry, enabled
//      false} (data, no tape bit: NightRows.SwapLevels swaps it in, NightRows.BaseId reads n65-foundry as n52-foundry for older systems);
//   2. the maps.list row for n65-foundry right after n52-foundry's (an unlisted id boots the arena: SurvivalGame.Maps.cs:66-70);
//   3. "n65-foundry" beside "n52-foundry" in look49's levels list (design.n49.look49.numbers.levels: the grade is per listed level id).
// Then asserts nothing else changed (the parsed file minus those three equals the old one: keys, values and order) and the copies are
// byte-identical. It also writes the placeholder level Design/levels/n65-foundry.json and its Player copy: n52-foundry.json byte for byte
// with its name and _why changed (gen-n65-foundry.cjs, builder T's T1, remakes it from the slots). Run once; a second run is refused.
//   node Tools/night/n65-a1.cjs [--dry]
const fs = require("fs");
const path = require("path");

const dry = process.argv.includes("--dry");
const root = path.join(__dirname, "..", "..");
const files = [path.join(root, "Design", "unity-config.json"), path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json")];
const levelFrom = path.join(root, "Design", "levels", "n52-foundry.json");
const levels = [path.join(root, "Design", "levels", "n65-foundry.json"), path.join(root, "Assets", "StreamingAssets", "Design", "levels", "n65-foundry.json")];

const BLOCK = {
  _why: "NIGHT65 (THE FLIGHT FOUNDRY, NIGHT65.md; GG 2026-10-23 ~13:08: 'i wnat real progress and itterations and not just small polishes'; GG 2026-10-09: 'every single level needs a complete arena overhaul completely from scratch so don't preserve anything'): this night's design rows. foundry65 is a LEVEL row (data, no tape bit: NightRows.SwapLevels swaps it in, NightRows.BaseId reads n65-foundry as n52-foundry for older systems). It is OFF until its own slice LV passes (Logs/n65/slate.json LV) with a skygrind-critic PASS bound to the sha it judged, flipped by Tools/night/n65-freeze-rows.cjs. The web has no such row: its level 1 was one map.",
  foundry65: {
    _why: "Level row (the lead's A1; builders T, R and S fill its rooms): level 1 THE FOUNDRY rebuilt for flight as n65-foundry by Tools/night/gen-n65-foundry.cjs: six fight-room slots (a2 KESTREL, a3 RIDE, a5 CRUCIBLE, b1 THE YARD, a8 PIERCE, a4 PARRY), each holding n52-foundry's room until its rebuilt room passes its GATE (Design/levels/n65/slots65.json, written only by Tools/night/n65-slot-set.cjs); a1 INTAKE, b2 THE GANTRY, a6, a7 and a9 (THE WICK) kept bit for bit. On, the campaign order's n52-foundry becomes n65-foundry. Off: -skygrindOff65 (or older). The web has no such row.",
    enabled: false,
    enabled_why: "Off until slate LV passes NIGHT65.md section 4 step 5 (Logs/n65/slate.json LV): slots65.json holds at least one room set new by n65-slot-set.cjs and every other slot equals n52-foundry's room; levelgen --walk 0 ERROR; n46flow clears 10 / 10 in every new room; verbs37 n65-foundry 27 / 27 on its GATE build; -skygrindOff65 boots n52-foundry; gates GREEN; then LV's own skygrind-critic PASS, flipped by Tools/night/n65-freeze-rows.cjs.",
    levelFrom: "n52-foundry",
    levelTo: "n65-foundry",
  },
};

const MAP_OLD = '{"id": "n52-foundry", "label": "LEVEL 1 THE FOUNDRY", "file": "Design/levels/n52-foundry.json", "campaignOnly": true},';
const MAP_NEW = '{"id": "n65-foundry", "label": "LEVEL 1 THE FOUNDRY", "file": "Design/levels/n65-foundry.json", "campaignOnly": true},';
const LOOK_OLD = '"levels": ["n48-foundry", "n47-spire", "n52-foundry", "n52-spire"]';
const LOOK_NEW = '"levels": ["n48-foundry", "n47-spire", "n52-foundry", "n65-foundry", "n52-spire"]';

const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const before = JSON.parse(raw);
if (before.design.n65) throw new Error("design.n65 already exists (already run?)");
for (const f of levels) if (fs.existsSync(f)) throw new Error(f + " already exists (already run?)");
const last = Object.keys(before.design).pop();
if (NightOf(last) >= 65) throw new Error("design's last key is " + last + " (expected an older night)");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";

// 2. the maps.list row: once, on its own line, the new row after it with the same indent
const lines = raw.split(eol);
const mi = lines.map((l, i) => (l.trim() === MAP_OLD ? i : -1)).filter((i) => i >= 0);
if (mi.length !== 1) throw new Error("n52-foundry's maps.list row found " + mi.length + " times (expected 1)");
const indent = lines[mi[0]].slice(0, lines[mi[0]].indexOf("{"));
lines.splice(mi[0] + 1, 0, indent + MAP_NEW);
let out = lines.join(eol);

// 3. look49's levels list: once
if (out.split(LOOK_OLD).length !== 2) throw new Error("look49's levels list found " + (out.split(LOOK_OLD).length - 1) + " times (expected 1)");
out = out.replace(LOOK_OLD, LOOK_NEW);

// 1. design.n65 as design's last key
const tail = eol + "    }" + eol + "  }" + eol + "}"; // the end of design's last night, of design, of the file
if (!out.endsWith(tail)) throw new Error("unexpected config ending");
const block = JSON.stringify(BLOCK, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
out = out.slice(0, out.length - tail.length) + eol + "    }," + eol + "    \"n65\": " + block + eol + "  }" + eol + "}";

// asserts: only the three changes
const after = JSON.parse(out);
const dk = Object.keys(after.design);
if (dk[dk.length - 1] !== "n65") throw new Error("n65 is not design's last key");
if (JSON.stringify(after.design.n65) !== JSON.stringify(BLOCK)) throw new Error("design.n65 not written as the script says");
const list = after.design.maps.list;
const at = list.findIndex((m) => m.id === "n65-foundry");
if (at < 1 || list[at - 1].id !== "n52-foundry" || list.filter((m) => m.id === "n65-foundry").length !== 1) throw new Error("the maps.list row is not once, right after n52-foundry's");
if (JSON.stringify(list[at]) !== JSON.stringify({ id: "n65-foundry", label: "LEVEL 1 THE FOUNDRY", file: "Design/levels/n65-foundry.json", campaignOnly: true })) throw new Error("the maps.list row differs");
const lv = after.design.n49.look49.numbers.levels;
if (lv.join(",") !== "n48-foundry,n47-spire,n52-foundry,n65-foundry,n52-spire") throw new Error("look49's levels list: " + lv.join(","));
delete after.design.n65;
list.splice(at, 1);
lv.splice(lv.indexOf("n65-foundry"), 1);
if (JSON.stringify(after) !== JSON.stringify(before)) throw new Error("something outside the three changes changed (keys, values or order)");

// the placeholder level: n52-foundry.json with its name and _why changed, byte for byte otherwise
const lraw = fs.readFileSync(levelFrom, "utf8");
const NAME_OLD = '"name":"n52-foundry"', WHY_OLD = '{"_why":"NIGHT52 builder F';
if (lraw.split(NAME_OLD).length !== 2) throw new Error("the level's name field found " + (lraw.split(NAME_OLD).length - 1) + " times");
if (!lraw.startsWith(WHY_OLD)) throw new Error("the level does not start with its NIGHT52 _why");
const lout = lraw.replace(NAME_OLD, '"name":"n65-foundry"').replace(WHY_OLD, '{"_why":"NIGHT65 A1 placeholder (Tools/night/n65-a1.cjs: n52-foundry.json byte for byte with its name and this _why changed; builder T\'s Tools/night/gen-n65-foundry.cjs remakes it from Design/levels/n65/slots65.json): NIGHT52 builder F');
const lb = JSON.parse(lraw), la = JSON.parse(lout);
if (la.name !== "n65-foundry" || la.data.name !== lb.data.name) throw new Error("the placeholder's names are wrong");
la.name = lb.name; la._why = lb._why;
if (JSON.stringify(la) !== JSON.stringify(lb)) throw new Error("the placeholder differs from n52-foundry beyond its name and _why");

if (dry) {
  console.log("n65-a1 --dry: design.n65 after design." + last + ", the maps.list row, look49's list (" + out.length + " chars, was " + raw.length + "); the placeholder " + lout.length + " chars");
  process.exit(0);
}
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
for (const f of levels) fs.writeFileSync(f, lout);
console.log("n65-a1: design.n65.foundry65 (level row n52-foundry -> n65-foundry, enabled false) after design." + last + "; the maps.list row after n52-foundry's; look49's levels + n65-foundry (both config copies byte-identical; nothing else changed); the placeholder Design/levels/n65-foundry.json and its Player copy (n52-foundry's data.name '" + lb.data.name + "' kept)");

function NightOf(key) {
  const m = /^n(\d+)$/.exec(key || "");
  return m ? Number(m[1]) : -1;
}
