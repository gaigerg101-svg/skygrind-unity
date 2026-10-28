// NIGHT65 builder T (iso65's proof): renders n52-foundry's six slots (the red: Research/2026-10-23-wave65/T/iso-red-<id>.png), then checks
// (1) the same input twice gives the same bytes, (2) a planted box shows (the picture changes; its PNG saved), (3) a planted line65 /
// frames65 / grapple anchor draws (a scratch copy of the plan; its PNG saved), (4) n65-foundry assembled with --slots=latest renders all six
// slots (into a scratch folder, removed) and, with every room n52's, draws the same pieces as the red, (5) a level file is drawn from its plan.
//   node Tools/night/n65-iso-proof.cjs [--out Research/2026-10-23-wave65/T]
const fs = require("fs");
const path = require("path");
const os = require("os");
const cp = require("child_process");
const crypto = require("crypto");
const ROOT = path.join(__dirname, "../..");
const ISO = path.join(__dirname, "n65-iso.cjs");
const SLOTS = ["a2", "a3", "a4", "a5", "b1", "a8"];
const oi = process.argv.indexOf("--out");
const OUT = oi > 0 ? process.argv[oi + 1] : "Research/2026-10-23-wave65/T";
const N52 = "Design/levels/n52/foundry.plan.json";
const tmp = fs.mkdtempSync(path.join(os.tmpdir(), "n65isoproof-"));
let bad = 0;
const say = (ok, m) => { console.log((ok ? "PASS " : "FAIL ") + m); if (!ok) bad++; };
const iso = (a) => { const r = cp.spawnSync(process.execPath, [ISO, ...a], { cwd: ROOT, encoding: "utf8" }); if (r.status !== 0) throw new Error("n65-iso " + a.join(" ") + ": " + (r.stderr || r.stdout)); return r.stdout.trim(); };
const sha = (p) => crypto.createHash("sha256").update(fs.readFileSync(path.resolve(ROOT, p))).digest("hex");
const svgBody = (p) => fs.readFileSync(p, "utf8").split("\n").filter((l) => !/<text /.test(l)).join("\n"); // the drawing without its labels
try {
  // the red: n52-foundry's six slots
  for (const id of SLOTS) console.log("  " + iso([N52, id, OUT + "/iso-red-" + id + ".png"]).slice(0, 220));
  // (1) the same input twice: the same bytes (PNG and SVG)
  const a = path.join(tmp, "a5-1.png"), b = path.join(tmp, "a5-2.png");
  iso([N52, "a5", a, "--svg", path.join(tmp, "a5-1.svg")]); iso([N52, "a5", b, "--svg", path.join(tmp, "a5-2.svg")]);
  say(sha(a) === sha(b) && fs.readFileSync(path.join(tmp, "a5-1.svg"), "utf8") === fs.readFileSync(path.join(tmp, "a5-2.svg"), "utf8") && sha(a) === sha(OUT + "/iso-red-a5.png"),
    "the same input three times (the red's run and two more): the same PNG bytes (sha256 " + sha(a).slice(0, 16) + ") and the same SVG");
  // (2) a planted box (a 2 x 3 x 2 m hull in a2's middle): the picture changes, saved
  const pp = OUT + "/iso-planted-box-a2.png";
  console.log("  " + iso([N52, "a2", pp, "--plant-box", "-1,0,-46,1,3,-44"]).slice(0, 200));
  say(sha(pp) !== sha(OUT + "/iso-red-a2.png"), "PLANTED a hull box (x -1..1, y 0..3, z -46..-44) in a2: the PNG differs from the red's (" + pp + ": the white box in both views and both sections)");
  // (3) a planted line65, frames65 and grapple anchor on a scratch copy of the plan: they draw, saved
  const plan = JSON.parse(fs.readFileSync(path.join(ROOT, N52), "utf8"));
  const a2 = plan.rooms.find((r) => r.id === "a2");
  a2.line65 = [[0, 0, -37], [-4, 0, -41], [-5.5, 2, -47], [-3, 2, -51], [0, 0, -53]];
  a2.frames65 = { entry: [0, 0, -36, 0, 0.1], deck: [-4, 2, -51, 160, -0.2] };
  plan.grapple = (plan.grapple || []).concat([{ zone: "a2", at: [5, 5.5, -48] }]);
  const tp = path.join(tmp, "plan-planted.json");
  fs.writeFileSync(tp, JSON.stringify(plan));
  const lp = OUT + "/iso-planted-line-a2.png";
  const line = iso([tp, "a2", lp]);
  console.log("  " + line.slice(0, 260));
  say(/line65 5, frames65 2/.test(line) && /1 grapple/.test(line) && sha(lp) !== sha(OUT + "/iso-red-a2.png"), "PLANTED a2 line65 (5 points), frames65 (2 poses), one grapple anchor on a scratch plan: drawn (" + lp + ")");
  // (4) n65-foundry with --slots=latest (the plan step only: the renderer reads the plan), all six slots; with every room n52's the drawing
  // is the red's (the labels aside: the level's id is in the title)
  const g = cp.spawnSync(process.execPath, [path.join(__dirname, "gen-n65-foundry.cjs"), "--slots=latest"], { cwd: ROOT, encoding: "utf8" });
  if (g.status !== 0) throw new Error("gen-n65-foundry --slots=latest: " + g.stderr);
  const rebuilt = (g.stdout.match(/rebuilt: ([^)]*)\)/) || [])[1] || "?";
  let same = 0;
  for (const id of SLOTS) {
    const o = path.join(tmp, "n65-" + id + ".png"), s1 = path.join(tmp, "n65-" + id + ".svg"), s0 = path.join(tmp, "red-" + id + ".svg");
    console.log("  " + iso(["Design/levels/n65/foundry.plan.json", id, o, "--svg", s1]).slice(0, 160));
    iso([N52, id, path.join(tmp, "red-" + id + ".png"), "--svg", s0]);
    if (svgBody(s0) === svgBody(s1)) same++;
  }
  say(rebuilt !== "?", "n65-foundry --slots=latest (rebuilt: " + rebuilt + ") renders all six slots; " + same + " / 6 draw exactly the red's pieces (labels aside)" + (rebuilt === "none" ? " (every slot is n52's room now: 6 expected)" : ""));
  if (rebuilt === "none" && same !== 6) say(false, "with no room rebuilt every slot should draw the red's pieces");
  // (5) a level file is drawn from its plan
  const lv = iso(["Design/levels/n52-foundry.json", "a5", path.join(tmp, "lv-a5.png")]);
  say(/a5: slot rect 20 x 32/.test(lv), "a level file (Design/levels/n52-foundry.json) is drawn from its plan (Design/levels/n52/foundry.plan.json)");
} finally {
  fs.rmSync(tmp, { recursive: true, force: true });
}
console.log("n65-iso proof: " + (bad ? bad + " FAIL" : "all PASS"));
process.exit(bad ? 1 : 0);
