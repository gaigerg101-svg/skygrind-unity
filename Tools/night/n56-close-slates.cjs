// NIGHT56 close (the lead): every NIGHT56 slate entry's final state and every NIGHT55 slate entry's NIGHT56 result, from one results file
// (Logs/n56/close-results.json). Only a critic's PASS sets passes true (the file says which gate); everything else closes SHORT with its
// measured reason. NIGHT55's entries keep their own state and get a new field `n56` (tonight's result); their doneWhen is never rewritten.
//   node Tools/night/n56-close-slates.cjs [--check]   (--check: print what is missing, write nothing)
const fs = require("fs");
const root = "C:/Users/gaige/dev/SKYGRIND-unity/";
const res = JSON.parse(fs.readFileSync(root + "Logs/n56/close-results.json", "utf8"));
const check = process.argv.includes("--check");
const s56 = JSON.parse(fs.readFileSync(root + "Logs/n56/slate.json", "utf8"));
const s55 = JSON.parse(fs.readFileSync(root + "Logs/n55/slate.json", "utf8"));
const missing = [];
for (const e of s56.entries) {
  const r = res.n56[e.id];
  if (!r) { missing.push("n56 " + e.id); continue; }
  if (r.passes && !/gate-r\d[ab]?\.md/.test(r.gate || "")) throw new Error(e.id + ": passes true needs the gate file that passed it");
  if (!r.passes && !/^SHORT/.test(r.state)) throw new Error(e.id + ": not passed, so its state must start with SHORT");
  e.passes = !!r.passes;
  e.state = r.state;
  e.notes = (e.notes ? e.notes + " | " : "") + "CLOSE: " + r.close;
}
for (const e of s55.entries) {
  const r = res.n55[e.id];
  if (!r) { missing.push("n55 " + e.id); continue; }
  e.n56 = r;
}
if (missing.length) { console.log("missing: " + missing.join(", ")); if (!check) throw new Error("every entry needs a result"); }
if (check) { console.log("check only: " + (missing.length ? "incomplete" : "complete")); process.exit(0); }
fs.writeFileSync(root + "Logs/n56/slate.json", JSON.stringify(s56, null, 2) + "\n");
fs.writeFileSync(root + "Logs/n55/slate.json", JSON.stringify(s55, null, 2) + "\n");
for (const e of s56.entries) console.log("n56 " + [e.id, e.state, "passes " + e.passes].join(" | "));
for (const e of s55.entries) console.log("n55 " + e.id + " | tonight: " + e.n56.slice(0, 90));
