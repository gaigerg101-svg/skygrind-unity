// NIGHT46 builder F1: the flow table from n46-flow-table.sh's 20 runs (read-only; prints Markdown).
//   node Tools/night/n46-flow-table.cjs <outDir> [rows] [commit]
const fs = require("fs");
const path = require("path");
const [dir, rows = "", commit = ""] = process.argv.slice(2);
const LV = [["L1", "n45-foundry", "n46-foundry"], ["L2", "n45-iceline", "n46-iceline"], ["L3", "n45-kiln", "n46-kiln"], ["L4", "n40-span", "n46-span"], ["L5", "n41-spire", "n46-spire"]];
// metric: label, regex on the SUMMARY line, lower is better?
const M = [
  ["walks", /(?:GREEN|SHORT) (\d+)\/10/, false],
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
const data = LV.map(([tag, b, c]) => ({ tag, b, c, bA: summary(`base-A-${b}.txt`), bB: summary(`base-B-${b}.txt`), cA: summary(`cand-A-${c}.txt`), cB: summary(`cand-B-${c}.txt`) }));
// the band: this run's baseline A against B, the largest gap over the levels (walks: none, judged by count)
const band = M.map((_, i) => Math.max(0, ...data.filter((d) => d.bA && d.bB).map((d) => Math.abs(d.bA[i] - d.bB[i]))));
const f1 = (x) => (Number.isFinite(x) ? (Math.round(x * 10) / 10).toString() : "-");
const out = [];
out.push(`## The flow table (n46flow; baseline = NIGHT45's levels with --off46, candidate = the n46 levels with rows ${rows || "-"}; commit ${commit || "?"}; ${new Date().toISOString().slice(0, 16)}Z)`);
out.push("");
out.push("Each cell: baseline -> candidate, the mean of seeds 1-10 and 11-20 [seeds 1-10 / 11-20 for the candidate]; walks are the sum of both sets (of 20).");
out.push("A check = better past the band, a cross = worse past it (the band: this run's baseline seeds 1-10 vs 11-20, the largest gap of the five levels).");
out.push("");
out.push("| level | " + M.map(([l]) => l).join(" | ") + " |");
out.push("|---|" + M.map(() => "---").join("|") + "|");
for (const d of data) {
  if (!d.bA || !d.bB || !d.cA || !d.cB) { out.push(`| ${d.tag} ${d.c} | ` + M.map(() => "missing run").join(" | ") + " |"); continue; }
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
  out.push(`| ${d.tag} ${d.c} | ` + cells.join(" | ") + " |");
}
out.push("| band | " + M.map(([l], i) => (i === 0 ? "-" : f1(band[i]))).join(" | ") + " |");
out.push("");
out.push("Runs that stopped (the pilot's caps; a capped room is left out of its own numbers):");
for (const d of data)
  for (const [k, f] of [["baseline", `base-A-${d.b}.txt`], ["baseline", `base-B-${d.b}.txt`], ["candidate", `cand-A-${d.c}.txt`], ["candidate", `cand-B-${d.c}.txt`]]) {
    const p = path.join(dir, f);
    if (!fs.existsSync(p)) continue;
    const caps = fs.readFileSync(p, "utf8").split("\n").filter((l) => / seed \d+: /.test(l)).map((l) => l.trim().replace(/^\S+ /, "").slice(0, 160));
    if (caps.length) out.push(`- ${k} ${f}: ${caps.join("; ")}`);
  }
console.log(out.join("\n"));
