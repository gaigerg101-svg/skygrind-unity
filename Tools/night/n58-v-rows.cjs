const fs = require("fs");
const path = require("path");

const root = path.join(__dirname, "..", "..");
const files = [path.join(root, "Design", "unity-config.json"), path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json")];

const SETS = require("./n58-v-sets.cjs");
const want = process.argv.slice(2);
const pick = (k) => !want.length || want.includes(k.split(".")[1]);
const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const before = JSON.parse(raw);

function fmt(v, ind) {
  const pad = " ".repeat(ind);
  if (Array.isArray(v)) {
    if (!v.length) return "[]";
    if (v.every((x) => typeof x !== "object" || x === null)) return "[" + v.map((x) => JSON.stringify(x)).join(", ") + "]";
    return "[" + eol + v.map((x) => pad + "  " + fmt(x, ind + 2)).join("," + eol) + eol + pad + "]";
  }
  if (v && typeof v === "object") {
    const keys = Object.keys(v);
    if (!keys.length) return "{}";
    return "{" + eol + keys.map((k) => pad + "  " + JSON.stringify(k) + ": " + fmt(v[k], ind + 2)).join("," + eol) + eol + pad + "}";
  }
  return JSON.stringify(v);
}

function setPath(o, keys, value, why) {
  let t = o;
  for (let i = 0; i < keys.length - 1; i++) {
    if (!t[keys[i]] || typeof t[keys[i]] !== "object") throw new Error("no object at " + keys.slice(0, i + 1).join("."));
    t = t[keys[i]];
  }
  const k = keys[keys.length - 1];
  t[k] = value;
  if (why) t[k + "_why"] = why;
}

const targets = {};
for (const [row, sets] of Object.entries(SETS)) {
  if (!pick(row)) continue;
  const [grp, name] = row.split(".");
  if (!before.design[grp] || !before.design[grp][name]) throw new Error("no design." + row);
  const num = JSON.parse(JSON.stringify(before.design[grp][name].numbers || {}));
  for (const [keys, value, why] of sets) setPath(num, keys, value, why);
  targets[row] = num;
}

let lines = raw.split(eol);
for (const [row, num] of Object.entries(targets)) {
  const [grp, name] = row.split(".");
  const g = lines.findIndex((l) => l.trim() === '"' + grp + '": {');
  if (g < 0) throw new Error("no group line for " + grp);
  const open = lines.findIndex((l, k) => k > g && l.trim() === '"' + name + '": {');
  if (open < 0) throw new Error("no row line for " + row);
  const ind = lines[open].match(/^ */)[0].length;
  const end = lines.findIndex((l, k) => k > open && l.match(/^ */)[0].length === ind && l.trim().startsWith("}"));
  const n = lines.findIndex((l, k) => k > open && k < end && l.match(/^ */)[0].length === ind + 2 && l.trim().startsWith('"numbers": '));
  if (n < 0) throw new Error(row + ": its numbers key not found");
  let last = n;
  if (!/^"numbers": \{\},?$/.test(lines[n].trim())) {
    last = lines.findIndex((l, k) => k > n && l.match(/^ */)[0].length === ind + 2 && l.trim().startsWith("}"));
    if (last < 0) throw new Error(row + ": the numbers object's end not found");
  }
  const comma = lines[last].trim().endsWith(",") ? "," : "";
  const text = (" ".repeat(ind + 2) + '"numbers": ' + fmt(num, ind + 2) + comma).split(eol);
  lines = lines.slice(0, n).concat(text, lines.slice(last + 1));
}
const out = lines.join(eol);
const after = JSON.parse(out);
for (const [row, num] of Object.entries(targets)) {
  const [grp, name] = row.split(".");
  if (JSON.stringify(after.design[grp][name].numbers) !== JSON.stringify(num)) throw new Error(row + ": written numbers differ from the script's");
  if (after.design[grp][name].enabled !== before.design[grp][name].enabled) throw new Error(row + ": enabled changed");
  after.design[grp][name].numbers = before.design[grp][name].numbers;
}
const norm = (o) => JSON.stringify(o, (k, v) => (v && typeof v === "object" && !Array.isArray(v) ? Object.fromEntries(Object.entries(v).sort()) : v));
if (norm(after) !== norm(before)) throw new Error("something outside the rows' numbers changed");
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
console.log("n58-v-rows: numbers written for " + Object.keys(targets).join(", ") + " (both copies; enabled untouched)");
