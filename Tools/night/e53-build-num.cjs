// EXT53 (the lead): sets one number in a BUILD's runtime config copy only (Builds/<tag>/SKYGRIND_Data/StreamingAssets/Design/unity-config.json),
// for a proof that needs an item on before the repo's config turns it on (n46-build-rows.cjs toggles a row's `enabled` only). The repo's
// configs are never touched. Refuses a path that does not exist or a value of another type.
//   node Tools/night/e53-build-num.cjs <tag> <design.path.to.key> <json value>     e.g. n53-r3 design.n53.polish53.numbers.firstShot.on 1
const fs = require("fs");
const path = require("path");
const [tag, key, val] = process.argv.slice(2);
if (!tag || !key || val === undefined) throw new Error("usage: e53-build-num.cjs <tag> <path> <json value>");
const file = path.join(__dirname, "../../Builds", tag, "SKYGRIND_Data/StreamingAssets/Design/unity-config.json");
const d = JSON.parse(fs.readFileSync(file, "utf8"));
const parts = key.split(".");
let o = d;
for (const p of parts.slice(0, -1)) { if (!(p in o)) throw new Error("no " + p + " in " + key); o = o[p]; }
const last = parts[parts.length - 1];
if (!(last in o)) throw new Error("no " + key);
const v = JSON.parse(val);
if (typeof v !== typeof o[last]) throw new Error(key + " is a " + typeof o[last] + ", not a " + typeof v);
o[last] = v;
fs.writeFileSync(file, JSON.stringify(d, null, 2) + "\n");
console.log(tag + ": " + key + " = " + JSON.stringify(v));
