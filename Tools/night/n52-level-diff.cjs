// NIGHT52 (the lead's): compares two level JSONs and lists what differs, by room: each campaign room row and each top-level array entry
// (boxes, ramps, lava, zones, props, lights ...) grouped by its zone / room / id where it has one. Read-only.
//   node Tools/night/n52-level-diff.cjs <a.json> <b.json> [--ignore name,_why,generatedBy]
const fs = require("fs");
const [a, b, ...rest] = process.argv.slice(2);
const ig = new Set((rest[rest.indexOf("--ignore") + 1] || "name,_why,generatedBy").split(","));
const A = JSON.parse(fs.readFileSync(a, "utf8")), B = JSON.parse(fs.readFileSync(b, "utf8"));
const out = [];
function walk(x, y, p) {
  if (JSON.stringify(x) === JSON.stringify(y)) return;
  if (Array.isArray(x) && Array.isArray(y)) {
    if (x.length !== y.length) { out.push(`${p}: length ${x.length} -> ${y.length}`); return; }
    for (let i = 0; i < x.length; i++) walk(x[i], y[i], `${p}[${i}${x[i] && x[i].id ? ":" + x[i].id : x[i] && x[i].zone ? ":" + x[i].zone : ""}]`);
    return;
  }
  if (x && y && typeof x === "object" && typeof y === "object") {
    for (const k of new Set([...Object.keys(x), ...Object.keys(y)])) {
      if (ig.has(k)) continue;
      if (!(k in x)) out.push(`${p}.${k}: added`);
      else if (!(k in y)) out.push(`${p}.${k}: removed`);
      else walk(x[k], y[k], `${p}.${k}`);
    }
    return;
  }
  out.push(`${p}: ${JSON.stringify(x).slice(0, 80)} -> ${JSON.stringify(y).slice(0, 80)}`);
}
walk(A, B, "$");
console.log(out.length ? out.slice(0, 60).join("\n") + (out.length > 60 ? `\n... ${out.length - 60} more` : "") : "EQUAL (ignoring " + [...ig].join(", ") + ")");
