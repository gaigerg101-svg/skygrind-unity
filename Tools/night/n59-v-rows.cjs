// NIGHT59 builder V (slate V2 yellow56's halo): writes ONLY design.n56.yellow56.numbers.dimYellow58.sKeep / sKeepVMin (and their _why) into
// Design/unity-config.json and its byte copy Assets/StreamingAssets/Design/unity-config.json, then asserts nothing else changed (every other
// key, value and order equal; the two copies byte-identical). A view number (the cap shader's), so Unity-side config, never the web's.
//   node Tools/night/n59-v-rows.cjs
// Re-running is a no-op when the numbers are already written. Note: Tools/night/n58-v-rows.cjs rewrites the whole dimYellow58 object from
// n58-v-sets.cjs: run THIS script after it, or the keep bound is lost.
const fs = require("fs");
const path = require("path");

const root = path.join(__dirname, "..", "..");
const files = [path.join(root, "Design", "unity-config.json"), path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json")];

const ADD = [
  ["sKeep", 0.85, "NIGHT59 builder V (slate V2; gate-x.md's FAIL: the foundry a5 husk's parry halo kept 20 % of its yellow because its V 0.84 sits on vFull): a yellow at or over this saturation (HSV on the sRGB frame) is untouched by the dim-yellow rule whatever its value, when its value is at least sKeepVMin. Saturation splits what value cannot: the halo S 0.99 (median 0.95 in its box on the -nocap twin), the lava trench's edge 0.64-0.70 (median), the column plate 0.56. Simulated on the n58-freeze a358 -nocap twins (Tools/night/n59-v-halo.py --sim, n58-v-yellow.py's measure on the simulated frames): the halo 38 -> 71 % kept at 0.85 (84 % at 0.75, 67 % at 0.90) and every frame's non-parry yellow <= 0.46 % at 0.85 (0.75 put foundry a5-entry at 0.58 % and spire v2-mid at 0.71 %: the meter counts this halo as non-parry, gate-x.md (4)). Cap57.shader's _Cap57YH.w via Yellow59.Keep; off with -skygrindOff59. The web has no such number (no parry colour rule)."],
  ["sKeepVMin", 0.3, "NIGHT59 builder V: the keep bound's value floor (Cap57.shader's _Cap57.w): spire's near-black olive reads S 1.0 at V 0.18-0.19 (a dim yellow); with sKeep alone (0.90) the simulation let it through (spire v6-mid non-parry 0.05 -> 0.72 %, v5-entry 0.06 -> 0.32 %); with this floor 0.10 % / 0.09 %. The halo's dim edge is V 0.32-0.39, over it."],
];

const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const before = JSON.parse(raw);
const d58b = before.design.n56.yellow56.numbers.dimYellow58;
if (!d58b) throw new Error("no design.n56.yellow56.numbers.dimYellow58");

let out = raw;
const todo = ADD.filter(([k, v]) => d58b[k] !== v);
if (todo.some(([k]) => k in d58b)) throw new Error("a key exists with another value: edit this script's ADD, then restore the file and re-run");
if (todo.length) {
  const lines = raw.split(eol);
  const g = lines.findIndex((l) => l.trim() === '"yellow56": {');
  const open = lines.findIndex((l, k) => k > g && l.trim() === '"dimYellow58": {');
  if (g < 0 || open < 0) throw new Error("dimYellow58's line not found");
  const ind = lines[open].match(/^ */)[0].length;
  const end = lines.findIndex((l, k) => k > open && l.match(/^ */)[0].length === ind && l.trim().startsWith("}"));
  if (end < 0) throw new Error("dimYellow58's end not found");
  const pad = " ".repeat(ind + 2);
  const last = end - 1;
  if (lines[last].trim().endsWith(",")) throw new Error("unexpected comma on dimYellow58's last key");
  const add = [];
  todo.forEach(([k, v, why], i) => {
    add.push(pad + JSON.stringify(k) + ": " + JSON.stringify(v) + ",");
    add.push(pad + JSON.stringify(k + "_why") + ": " + JSON.stringify(why) + (i === todo.length - 1 ? "" : ","));
  });
  lines[last] = lines[last] + ",";
  out = lines.slice(0, end).concat(add, lines.slice(end)).join(eol);
}

const after = JSON.parse(out);
const d58a = after.design.n56.yellow56.numbers.dimYellow58;
for (const [k, v, why] of ADD) {
  if (d58a[k] !== v || d58a[k + "_why"] !== why) throw new Error(k + ": not written as the script says");
  delete d58a[k];
  delete d58a[k + "_why"];
}
for (const [k] of ADD) {
  delete d58b[k];
  delete d58b[k + "_why"];
}
if (JSON.stringify(after) !== JSON.stringify(before)) throw new Error("something outside dimYellow58.sKeep / sKeepVMin changed (keys, values or order)");
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
console.log("n59-v-rows: dimYellow58 " + ADD.map(([k, v]) => k + " " + v).join(", ") + (todo.length ? " written" : " already there (no change)") + " (both copies byte-identical; nothing else changed)");
