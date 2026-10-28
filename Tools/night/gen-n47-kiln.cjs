// NIGHT47 builder B (row kiln47: n46-kiln -> n47-kiln; G33 / G35; Research/2026-10-18-wave47/B-cards.md sections 4.2 / 4.3; the card gate PASS).
// LEVEL 3 as n47-kiln. n46-kiln, its plan and its generator are untouched (this file reads n46's plan only), so --off47 loads n46-kiln bit for
// bit. From n46's PLAN (Design/levels/n46/kiln.plan.json -> Design/levels/n47/kiln.plan.json), then levelgen, then --finish. Round 1:
//   K5: wyrmArena = Design/maps/wyrm47-arena.json (Tools/night/gen-n47-wyrm-arena.cjs: wyrm46's 35 boxes + THE SLAG ISLANDS 35-37 at 6.0 m);
//       crumble31 keeps its indices (27 / 28). Every other room is n46's bit for bit (asserted).
//   (round 2: the three foreshadow beats' data, G35)
// Run (from the worktree root):
//   node Tools/night/gen-n47-wyrm-arena.cjs && node Tools/night/gen-n47-kiln.cjs
//   dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n47/kiln.plan.json Design/levels/n47/kiln.json
//   node Tools/night/gen-n47-kiln.cjs --finish
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN_IN = "Design/levels/n46/kiln.plan.json", PLAN_OUT = "Design/levels/n47/kiln.plan.json";
const GEN_OUT = "Design/levels/n47/kiln.json";
const ARENA = "Design/maps/wyrm47-arena.json";

if (process.argv[2] === "--finish") {
  const src = path.join(ROOT, process.argv[3] || GEN_OUT);
  const text = fs.readFileSync(src, "utf8");
  const level = JSON.parse(text);
  if (level.name !== "n47-kiln") throw new Error(`${src}: name ${level.name}, expected n47-kiln`);
  level.data.name = "n40-kiln"; // the older systems' name for level 3 (as n46-kiln's: MapModelJson reads the outer name first)
  level.generatedBy = "Tools/night/gen-n47-kiln.cjs (plan " + PLAN_OUT + " -> levelgen -> --finish)";
  const eol = text.includes("\r\n") ? "\r\n" : "\n";
  const out = JSON.stringify(level) + (text.endsWith(eol) ? eol : "");
  const outs = ["Design/levels/n47-kiln.json", "Assets/StreamingAssets/Design/levels/n47-kiln.json"];
  for (const f of outs) fs.writeFileSync(path.join(ROOT, f), out);
  console.log(`n47-kiln: finished ${path.relative(ROOT, src)} -> ${outs.join(", ")} (${out.length} bytes)`);
  process.exit(0);
}

const text = fs.readFileSync(path.join(ROOT, PLAN_IN), "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
if (plan.id !== "n46-kiln") throw new Error(`${PLAN_IN}: id ${plan.id}`);
plan.id = "n47-kiln";
plan._why = "NIGHT47 B (row kiln47; Tools/night/gen-n47-kiln.cjs): n46-kiln's plan with k5's worm arena swapped for wyrm47-arena.json (THE SLAG ISLANDS, G33). Before it: " + plan._why;
const k5 = plan.rooms.find((r) => r.id === "k5");
if (!k5 || k5.wyrmArena !== "Design/maps/wyrm46-arena.json") throw new Error("k5's arena is not n46's: re-read");
k5.wyrmArena = ARENA;
k5.wyrmArena_why = "NIGHT47 B (G33, B-cards.md 4.2; Tools/night/gen-n47-wyrm-arena.cjs): wyrm46-arena.json + THE SLAG ISLANDS (boxes 35-37, top 6.0 m, R 12: only the THUNDERCLAP's boost chain reaches them). Before: " + k5.wyrmArena_why;
// G35 beat 3 THE CAMEO's card (kb3 had no lesson): it shows from the room's entry until the first kill, so it is on screen when the breach
// comes (cameoAfterS 5 s); the beat itself is sim (SurvivalSim.N47.Kiln.cs), keyed by the room id
const kb3 = plan.rooms.find((r) => r.id === "kb3");
if (!kb3 || kb3.lessons || kb3.lessons31) throw new Error("kb3 has lessons upstream: re-read");
const card = "When the floor glows, jump.";
kb3.lessons = [{ promptPad: card, kind: "husksDead", n: 1, prompt: card }];
kb3.lessons31 = [{ promptPad: card, kind: "husksDead", n: 1, prompt: card }];
kb3.kiln47_why = "NIGHT47 B G35: the cameo's card (copy for F; <= 6 words, LevelValidate): the worm's breach tell is taught here before the fight";
const CHANGED = ["k5", "kb3"];
for (let i = 0; i < plan.rooms.length; i++) if (!CHANGED.includes(plan.rooms[i].id) && JSON.stringify(plan.rooms[i]) !== JSON.stringify(before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
for (const k of Object.keys(before)) if (!["_why", "id", "rooms"].includes(k) && JSON.stringify(before[k]) !== JSON.stringify(plan[k])) throw new Error(`${k} changed`);
fs.mkdirSync(path.dirname(path.join(ROOT, PLAN_OUT)), { recursive: true });
fs.writeFileSync(path.join(ROOT, PLAN_OUT), JSON.stringify(plan, null, 1) + "\n");
console.log(`gen-n47-kiln: wrote ${PLAN_OUT} (k5's arena ${ARENA})`);
