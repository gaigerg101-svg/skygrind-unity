// NIGHT52 (the lead, P0; from n47-flow-table.cjs: the level ids come from the arguments): the flow table from n52-flow-table.sh's runs
// (read-only; prints Markdown). A pair with no candidate prints the base alone (seeds 1-10 / 11-20).
//   node Tools/night/n52-flow-table.cjs <outDir> <commit> <base>:<cand> ...
const fs = require("fs");
const path = require("path");
const [dir, commit = "", ...pairs] = process.argv.slice(2);
const LV = pairs.map((p) => { const [b, c] = p.split(":"); return [b, c || ""]; });
// metric: label, regex on the SUMMARY line, lower is better?
const M = [
  ["walks", /(?:GREEN|SHORT|RED) (\d+)\/10/, false],
  ["stall %", /stall ([\d.]+) %/, true],
  ["longest stall s", /longest stall ([\d.]+) s/, true],
  ["dwell s", /dwell ([\d.]+) s/, true],
  ["dead s", /dead ([\d.]+) s/, true],
  ["deaths", /deaths (\d+)/, true],
  ["clear s", /clear ([\d.]+) s/, true],
];
function summary(f) {
  const p = path.join(dir, f);
  if (!fs.existsSync(p)) return null;
  const t = fs.readFileSync(p, "utf8").split("\n").find((l) => l.includes(" SUMMARY: "));
  if (!t) return null;
  return M.map(([, re]) => { const m = re.exec(t); return m ? +m[1] : NaN; });
}
const data = LV.map(([b, c]) => ({ b, c, bA: summary(`base-A-${b}.txt`), bB: summary(`base-B-${b}.txt`), cA: c ? summary(`cand-A-${c}.txt`) : null, cB: c ? summary(`cand-B-${c}.txt`) : null }));
const band = M.map((_, i) => Math.max(0, ...data.filter((d) => d.bA && d.bB).map((d) => Math.abs(d.bA[i] - d.bB[i]))));
const f1 = (x) => (Number.isFinite(x) ? (Math.round(x * 10) / 10).toString() : "-");
const out = [];
out.push(`## The flow table (n46flow; base = the named level with --off52 (NIGHT51's game), candidate = the n52 level with the config's rows; commit ${commit || "?"}; ${new Date().toISOString().slice(0, 16)}Z)`);
out.push("");
out.push("Each cell: base -> candidate, the mean of seeds 1-10 and 11-20 [candidate seeds 1-10 / 11-20]; walks are the sum of both sets (of 20).");
out.push("A check = better past the band, a cross = worse past it (the band: the base's seeds 1-10 vs 11-20, the largest gap over the levels run).");
out.push("");
out.push("| level | " + M.map(([l]) => l).join(" | ") + " |");
out.push("|---|" + M.map(() => "---").join("|") + "|");
for (const d of data) {
  if (!d.bA || !d.bB) { out.push(`| ${d.b} | ` + M.map(() => "missing run").join(" | ") + " |"); continue; }
  if (!d.c) {
    out.push(`| ${d.b} (base only) | ` + M.map(([], i) => (i === 0 ? `${d.bA[0] + d.bB[0]}/20` : `${f1((d.bA[i] + d.bB[i]) / 2)} [${f1(d.bA[i])} / ${f1(d.bB[i])}]`)).join(" | ") + " |");
    continue;
  }
  if (!d.cA || !d.cB) { out.push(`| ${d.b} -> ${d.c} | ` + M.map(() => "missing candidate run").join(" | ") + " |"); continue; }
  const cells = M.map(([label, , lower], i) => {
    if (i === 0) {
      const b = d.bA[0] + d.bB[0], c = d.cA[0] + d.cB[0];
      const mark = c > b ? " ✓" : c < b ? " ✗" : "";
      return `${b} -> ${c}/20${mark}`;
    }
    const b = (d.bA[i] + d.bB[i]) / 2, c = (d.cA[i] + d.cB[i]) / 2, diff = c - b;
    let mark = "";
    if (Math.abs(diff) > band[i]) mark = (lower ? diff < 0 : diff > 0) ? " ✓" : " ✗";
    return `${f1(b)} -> ${f1(c)}${mark} [${f1(d.cA[i])} / ${f1(d.cB[i])}]`;
  });
  out.push(`| ${d.b} -> ${d.c} | ` + cells.join(" | ") + " |");
}
out.push("| band | " + M.map(([l], i) => (i === 0 ? "-" : f1(band[i]))).join(" | ") + " |");
out.push("");
out.push("Per-room rows (seeds 1-10): the files base-A-<id>.txt / cand-A-<id>.txt in this folder (n52-room-check.py --flow-before / --flow reads them).");
out.push("Runs that stopped (the pilot's caps; a capped room is left out of its own numbers):");
for (const d of data)
  for (const [k, f] of [["base", `base-A-${d.b}.txt`], ["base", `base-B-${d.b}.txt`], ["candidate", `cand-A-${d.c}.txt`], ["candidate", `cand-B-${d.c}.txt`]]) {
    const p = path.join(dir, f);
    if (!d.c && k === "candidate") continue;
    if (!fs.existsSync(p)) continue;
    const caps = fs.readFileSync(p, "utf8").split("\n").filter((l) => / seed \d+: /.test(l)).map((l) => l.trim().replace(/^\S+ /, "").slice(0, 160));
    if (caps.length) out.push(`- ${k} ${f}: ${caps.join("; ")}`);
  }
console.log(out.join("\n"));
