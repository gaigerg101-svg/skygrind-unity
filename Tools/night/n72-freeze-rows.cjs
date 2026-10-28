// NIGHT72 (the forge FORGE62, 2026-10-22; a copy of n60-freeze-rows.cjs with the NIGHT72 label; NIGHT57.md section 0's rule plus NIGHT72's
// visible rule): turn the rows that passed ON in the REPO's config (both copies, byte identical), each with an enabled_why starting
// "ON at NIGHT72 by the rule: <reason>"; asserts that nothing else changes. Off switches stay: -skygrindOffNN for each row's night.
// It refuses a row unless the slate (Logs/n72/slate.json, or --slate=<path>) holds a slice whose `row` names design.<night.row>, with
// passes true, state pass and a verdict file whose first line is "<id>: PASS" (FORGE62 red team: the tool used to flip any row).
//   node Tools/night/n72-freeze-rows.cjs <night.row>=<reason> [<night.row>=<reason> ...] [--dry] [--slate=<path>]
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const A = path.join(ROOT, "Design/unity-config.json"), B = path.join(ROOT, "Assets/StreamingAssets/Design/unity-config.json");
const LABEL = "ON at NIGHT72 by the rule: ";
const dry = process.argv.includes("--dry");
const slateArg = process.argv.find((a) => a.startsWith("--slate="));
const SLATE = slateArg ? slateArg.slice("--slate=".length) : path.join(ROOT, "Logs/n72/slate.json");
const pairs = process.argv.slice(2).filter((a) => !a.startsWith("--")).map((a) => { const i = a.indexOf("="); return [a.slice(0, i), a.slice(i + 1)]; });
const CONFIGS = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];
// the slice's own files, the two config copies left out (every flip rewrites them; the row's block is checked on its own)
function ownFiles(list) { return list.map(String).filter((f) => !CONFIGS.includes(f.replace(/\\/g, "/"))); }
// FORGE66 (the NIGHT65 integrated review's S2, Logs/n65/review-n65.md): a LEVEL row's PASS binds its level, so passFiles must name the
// level file in both copies and its slot set (Design/levels/<px>/slotsNN.json); else the level could be re-assembled after the PASS
function levelFilesMissing(r, list) {
  if (!r || !r.levelTo) return [];
  const have = list.map((f) => String(f).replace(/\\/g, "/"));
  const need = ["Design/levels/" + r.levelTo + ".json", "Assets/StreamingAssets/Design/levels/" + r.levelTo + ".json"];
  const miss = need.filter((p) => !have.includes(p));
  if (!have.some((p) => /^Design\/levels\/.+\/slots\d+\.json$/.test(p))) miss.push("Design/levels/<px>/slotsNN.json");
  return miss;
}
// true when design.<night>.<row> differs between two config texts, its enabled / enabled_why aside
function rowBlockChanged(oldText, newText, key) {
  const [night, row] = key.split(".");
  const pick = (t) => { const j = JSON.parse(t); const r = j.design && j.design[night] && j.design[night][row]; if (!r) return null; const c = Object.assign({}, r); delete c.enabled; delete c.enabled_why; return JSON.stringify(c); };
  return pick(oldText) !== pick(newText);
}
if (process.argv.includes("--selftest")) {
  // FORGE64 red team 1's planted case: two rows; row a flipped first (its enabled changes), then row b's flip must still pass; a change to
  // row b's numbers after its PASS must refuse; the config copies never count as the slice's own files
  const base = { design: { n64: { a64: { _why: "x", enabled: false, numbers: { k: 1 } }, b64: { _why: "y", enabled: false, numbers: { k: 2 } } } } };
  const t0 = JSON.stringify(base);
  const t1 = JSON.parse(t0); t1.design.n64.a64.enabled = true; t1.design.n64.a64.enabled_why = "ON ...";
  const t2 = JSON.parse(t0); t2.design.n64.b64.numbers.k = 3;
  const ok = [
    ["b's block unchanged after a's flip", rowBlockChanged(t0, JSON.stringify(t1), "n64.b64") === false],
    ["a's own flip is not a change", rowBlockChanged(t0, JSON.stringify(t1), "n64.a64") === false],
    ["b's numbers edited after its PASS refuses", rowBlockChanged(t0, JSON.stringify(t2), "n64.b64") === true],
    ["the config copies are never own files", ownFiles(["Design/unity-config.json", "Assets\\StreamingAssets\\Design\\unity-config.json", "Assets/Scripts/Game/Route47View.cs"]).join() === "Assets/Scripts/Game/Route47View.cs"],
    ["a level row's PASS without its level files refuses; with them it passes", levelFilesMissing({ levelTo: "n65-foundry" }, ["Logs/x.md"]).length === 3 && levelFilesMissing({ levelTo: "n65-foundry" }, ["Design/levels/n65-foundry.json", "Assets/StreamingAssets/Design/levels/n65-foundry.json", "Design/levels/n65/slots65.json"]).length === 0 && levelFilesMissing({ enabled: false }, []).length === 0],
  ];
  for (const [name, pass] of ok) console.log((pass ? "PASS " : "FAIL ") + name);
  const bad = ok.filter(([, p]) => !p).length;
  console.log(bad ? "n72-freeze-rows selftest: " + bad + " FAIL" : "n72-freeze-rows selftest: PLANTED GREEN (" + ok.length + " cases)");
  process.exit(bad ? 1 : 0);
}
if (!pairs.length) throw new Error("usage: n72-freeze-rows.cjs <night.row>=<reason> ...");
let slices;
try { const s = JSON.parse(fs.readFileSync(SLATE, "utf8")); slices = Array.isArray(s) ? s : s.slices; } catch (e) { throw new Error("cannot read the slate " + SLATE + ": " + e.message); }
for (const [key] of pairs) {
  const hit = (slices || []).find((s) => String(s.row).includes("design." + key) && s.passes === true && s.state === "pass");
  if (!hit) throw new Error(key + ": no slate slice with row design." + key + ", passes true and state pass in " + SLATE + ": a row turns ON only after its critic PASS");
  const first = hit.verdict && fs.existsSync(hit.verdict) ? fs.readFileSync(hit.verdict, "utf8").split(/\r?\n/)[0].trim() : "";
  if (first !== hit.id + ": PASS") throw new Error(key + ": " + hit.id + "'s verdict file does not start '" + hit.id + ": PASS'");
  // FORGE64 (NIGHT63's T63c: a fix after a PASS shipped unreviewed and lost a lesson; GitHub's dismiss-stale-approvals): a PASS binds to the
  // sha the critic judged (the slice's passSha). Two checks: the slice's own files (passFiles: the union of `git show --name-only --format=`
  // over its own picked shas, never a pick range, which holds other slices' commits) unchanged since passSha, the two config copies left out
  // (every flip rewrites them: FORGE64 red team 1); and the row's own config block at passSha equal to HEAD's, enabled / enabled_why aside.
  if (!hit.passSha || !Array.isArray(hit.passFiles)) throw new Error(key + ": " + hit.id + " has no passSha / passFiles in the slate (the sha the critic judged; the slice's own files)");
  const sha = String(hit.passSha);
  const { execFileSync } = require("child_process");
  const files = ownFiles(hit.passFiles);
  { const [nn, rr] = key.split("."); const cur = JSON.parse(fs.readFileSync(A, "utf8")).design; const miss = levelFilesMissing(cur[nn] && cur[nn][rr], hit.passFiles);
    if (miss.length) throw new Error(key + ": a level row's PASS must bind its level files: passFiles lacks " + miss.join(", ") + " (FORGE66; the NIGHT65 review's S2)"); }
  const changed = files.length ? execFileSync("git", ["diff", "--name-only", sha, "--", ...files], { cwd: ROOT, encoding: "utf8" }).trim() : "";
  if (changed) throw new Error(key + ": files changed since the PASS at " + sha + " (" + changed.split("\n").join(", ") + "): GATE it again or flip at passSha");
  const atPass = execFileSync("git", ["show", sha + ":Design/unity-config.json"], { cwd: ROOT, encoding: "utf8", maxBuffer: 64 * 1024 * 1024 });
  if (rowBlockChanged(atPass, fs.readFileSync(A, "utf8"), key)) throw new Error(key + ": the row's config block changed since the PASS at " + sha + ": GATE it again");
}
let raw = fs.readFileSync(A, "utf8");
if (raw !== fs.readFileSync(B, "utf8")) throw new Error("the two config copies differ");
const before = JSON.parse(raw);
const after = JSON.parse(raw);
for (const [key, reason] of pairs) {
  const [night, row] = key.split(".");
  const r = after.design[night] && after.design[night][row];
  if (!r || typeof r.enabled !== "boolean") throw new Error(key + ": no such row with a bool enabled");
  if (!reason || reason.length < 20) throw new Error(key + ": give a measured reason");
  r.enabled = true;
  r.enabled_why = LABEL + reason;
}
// text edit per row (keeps the file's layout): the row's "enabled": false and its "enabled_why" line, found inside its own block
for (const [key, reason] of pairs) {
  const [night, row] = key.split(".");
  const nightAt = raw.indexOf('\n    "' + night + '": {');
  if (nightAt < 0) throw new Error(night + ": block not found");
  const rowAt = raw.indexOf('\n      "' + row + '": {', nightAt);
  if (rowAt < 0) throw new Error(key + ": row not found");
  const end = raw.indexOf("\n      }", rowAt);
  let block = raw.slice(rowAt, end);
  if (!/"enabled": false/.test(block)) throw new Error(key + ": not off now");
  block = block.replace('"enabled": false', '"enabled": true')
    .replace(/"enabled_why": "(?:[^"\\]|\\.)*"/, '"enabled_why": ' + JSON.stringify(LABEL + reason));
  raw = raw.slice(0, rowAt) + block + raw.slice(end);
}
if (JSON.stringify(JSON.parse(raw)) !== JSON.stringify(after)) throw new Error("the text edit and the parsed edit disagree");
const chk = JSON.parse(raw);
for (const [key] of pairs) { const [n, r] = key.split("."); chk.design[n][r].enabled = before.design[n][r].enabled; chk.design[n][r].enabled_why = before.design[n][r].enabled_why; }
if (JSON.stringify(chk) !== JSON.stringify(before)) throw new Error("something beyond the named rows changed");
if (dry) { console.log("DRY: would turn ON " + pairs.map((p) => p[0]).join(", ")); process.exit(0); }
fs.writeFileSync(A, raw); fs.writeFileSync(B, raw);
console.log("ON in both config copies: " + pairs.map((p) => p[0]).join(", "));
