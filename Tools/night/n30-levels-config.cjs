// NIGHT30 (builder L): the rebuilt levels' config rows in Design/unity-config.json (this worktree's copy; the lead syncs StreamingAssets with n24-sync.sh):
//  1. the MAP list gets n30-foundry / n30-iceline / n30-kiln (campaignOnly, files Design/levels/n30-*.json) after n29-kiln;
//  2. design.n30.levels gets `order`: the CAMPAIGN's play order while design.n30.levels.enabled is on (Core/Curriculum25.cs reads it in place of
//     design.n25.curriculum.order); off, the n29 order stands. n30-span joins the order when its map exists (a later run of this script with SPAN=1).
// Run from the worktree root: node Tools/night/n30-levels-config.cjs   (idempotent: a row already present is left alone; the file's bytes elsewhere are kept)
const fs = require("fs");
const path = require("path");
const file = path.join(__dirname, "..", "..", "Design", "unity-config.json");
let raw = fs.readFileSync(file, "utf8");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const span = process.env.SPAN === "1";

// 1. the map list
const kilnLine = `        { "id": "n29-kiln", "label": "LEVEL 3 THE KILN", "file": "Design/levels/n29-kiln.json", "campaignOnly": true },`;
if (!raw.includes(kilnLine)) throw new Error("the n29-kiln map line was not found");
const maps = [
  ["n30-foundry", "LEVEL 1 THE FOUNDRY"], ["n30-iceline", "LEVEL 2 THE ICE LINE"], ["n30-kiln", "LEVEL 3 THE KILN"],
].concat(span ? [["n30-span", "LEVEL 4 THE SPAN"]] : []);
let insert = "";
for (const [id, label] of maps) {
  if (raw.includes(`"id": "${id}"`)) continue;
  insert += eol + `        { "id": "${id}", "label": "${label}", "file": "Design/levels/${id}.json", "campaignOnly": true },`;
}
if (insert) raw = raw.replace(kilnLine, kilnLine + insert);

// 2. design.n30.levels.order
const order = ["n30-foundry", "n30-iceline", "n30-kiln"].concat(span ? ["n30-span"] : []).concat(["level3", "level4", "level5", "level06", "level07", "level08", "level09", "level10"]);
const orderLine = `        "order": ${JSON.stringify(order).replace(/","/g, "\", \"")}`;
const levelsHead = `      "levels": {${eol}        "_why": "The rebuilt levels 1-4 (plans Design/levels/n30/*.plan.json, ids n30-*): while on, design.n25.curriculum.order points at them; off: the n29-* order. A view / flow row (no sim bit: the level a run loads is already on its tape).",${eol}        "enabled": false`;
const levelsHeadOn = levelsHead.replace(`"enabled": false`, `"enabled": true`);
const head = raw.includes(levelsHead) ? levelsHead : raw.includes(levelsHeadOn) ? levelsHeadOn : null;
if (!head) throw new Error("design.n30.levels not found in the expected shape");
const at = raw.indexOf(head) + head.length;
const rest = raw.slice(at);
if (rest.startsWith("," + eol + `        "order_why": `)) {
  // replace the existing order line (the line after order_why)
  const o = rest.indexOf(eol + `        "order": `);
  const end = rest.indexOf(eol, o + eol.length);
  raw = raw.slice(0, at) + rest.slice(0, o) + eol + orderLine + rest.slice(end);
} else {
  raw = raw.slice(0, at) + "," + eol + `        "order_why": "NIGHT30 (builder L): the CAMPAIGN's play order while this row is on (Core/Curriculum25.cs reads it in place of design.n25.curriculum.order): the rebuilt n30 levels, then levels 4-10 as before (n30-span is inserted when its map exists).",` + eol + orderLine + rest;
}
JSON.parse(raw); // still valid
fs.writeFileSync(file, raw);
console.log("patched", file, insert ? "maps +" + maps.length : "maps unchanged", "order", order.join(","));
