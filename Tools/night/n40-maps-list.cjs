// NIGHT40 lead: registers the four NIGHT40 level ids in design.maps.list (SurvivalGame.Maps.cs loads a level id only through this list:
// the campaign and -skygrindMap both). campaignOnly, like the n38 rows; inert until a level row (kiln40 / iceline40 / loom40 / crown40)
// swaps the id into the campaign order, so --off40 is unchanged. Edits only the list, both config copies (byte-identical after).
// Run: node Tools/night/n40-maps-list.cjs
const fs = require("fs");
const file = "C:/Users/gaige/dev/SKYGRIND-unity/Design/unity-config.json";
const copy = "C:/Users/gaige/dev/SKYGRIND-unity/Assets/StreamingAssets/Design/unity-config.json";
let raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const rows = [
  ["n40-iceline", "LEVEL 2 THE ICE LINE"],
  ["n40-kiln", "LEVEL 3 THE KILN"],
  ["n40-span", "LEVEL 4 THE SPAN"],
  ["n40-spire", "LEVEL 5 THE SPIRE"],
];
const anchorRe = /(\{\s*"id":\s*"n38-spire",[^}]*\})/;
const m = raw.match(anchorRe);
if (!m) throw new Error("the n38-spire row was not found");
if (raw.includes('"id": "n40-')) throw new Error("n40 rows already present");
const indent = raw.slice(raw.lastIndexOf(eol, m.index) + eol.length, m.index);
const add = rows.map(([id, label]) => "," + eol + indent + JSON.stringify({ id, label, file: "Design/levels/" + id + ".json", campaignOnly: true }).replace(/","/g, '", "').replace(/":/g, '": ').replace(/,"campaignOnly"/, ', "campaignOnly"')).join("");
raw = raw.replace(anchorRe, m[1] + add);
const d = JSON.parse(raw);
const ids = d.design.maps.list.map((x) => x.id);
for (const [id] of rows) if (!ids.includes(id)) throw new Error("missing " + id);
fs.writeFileSync(file, raw);
fs.copyFileSync(file, copy);
console.log("design.maps.list now " + ids.length + " rows; added " + rows.map((r) => r[0]).join(", "));
