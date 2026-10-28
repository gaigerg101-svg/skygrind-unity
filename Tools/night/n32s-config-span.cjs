// NIGHT32 (builder S): writes design.n32.span32.levelSwap = {"n30-span": "n32-span"} in Design/unity-config.json (a text edit inside the span32 block only;
// the row stays `enabled: false` until the lead turns it on). Idempotent. Run from the repo root: node Tools/night/n32s-config-span.cjs
const fs = require("fs");
const file = "Design/unity-config.json";
let t = fs.readFileSync(file, "utf8");
const at = t.indexOf('"span32": {');
if (at < 0) throw new Error("no span32 block");
const end = t.indexOf("\n      },", at);
let block = t.slice(at, end);
const want = '"levelSwap": { "n30-span": "n32-span" }';
if (block.includes(want)) { console.log("span32.levelSwap already set"); process.exit(0); }
if (!block.includes('"levelSwap": {}')) throw new Error("span32.levelSwap is not {} (edited by someone else?)");
block = block.replace('"levelSwap": {}', want);
t = t.slice(0, at) + block + t.slice(end);
JSON.parse(t); // still valid JSON
fs.writeFileSync(file, t);
console.log("span32.levelSwap set: n30-span -> n32-span");
