// NIGHT55 builder R (P1-doorDark; critic-ggproxy #9; GG w37: doors "not visible now and you just run through it and it's like dark"):
// kiln kb2b THE STACK gets lamps that reach its floor. It is a 16 x 52 m link with a 32 m roof: its ceiling panels hang at 31.95 m with a
// 14 m range (they light nothing below ~18 m), so through the door d3 the entry floor, the run-up and G2a's 12 m cliff face render in the fog
// colour: n54-freeze vk verbs37 f0950 ("DOOR" is the HUD's zone plate in this link) is the camera at the unlit cliff face, world luma std
// 0.014 (Tools/night/n55-r-flat.py; flat < 0.03); f1000 shows the face dark violet from the floor, the floor and west wall lit orange.
// Added (level data only, lights: no collider, no sim change), each flush to a face (levelgen's light check):
//   kb2b-L55a  white wall lamp on the west liner at the cliff's foot: lights the cliff face's west half and the take-off floor
//   kb2b-L55b  white wall lamp on the east liner over the cyan run-up panel: the face's east half and the panel's run (kept off the
//              panel: a ride face carries only its own cyan)
//   kb2b-L55c  white floor pool at the cliff's foot: the face lit from below (the climb's wall reads as a wall, not a void)
//   kb2b-L55d  white wall lamp on the west liner at the door landing: the entry floor lit as you come through d3
// The room's one accent stays the amber kb2b-L1 (LevelValidate: one accent per room; the first cut made these two amber: 3 accents, ERROR); cyan stays the ride panel's and the climb lip's (kb2b-L2), the exit pool's (L10).
// Asserts kb2b's lights are NIGHT52's (8 rows) and not yet run; every other row of the plan bit for bit.
// Run: node Tools/night/n55-r-kb2b.cjs [--dry], then levelgen into Design/levels/n52-kiln.json + the StreamingAssets copy (see R.md)
const fs = require("fs");
const path = require("path");
const PLAN = path.join(__dirname, "../../Design/levels/n52/kiln.plan.json");
const text = fs.readFileSync(PLAN, "utf8");
const plan = JSON.parse(text);
const before = JSON.parse(text);
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const R = "kb2b";
const mine = plan.lights.filter((l) => l.room === R);
if (mine.some((l) => /^kb2b-L55/.test(l.id))) throw new Error("kb2b already lit by NIGHT55");
if (mine.length !== 8) throw new Error("kb2b's lights: " + mine.length);
const add = [
  { id: "kb2b-L55a", room: R, kind: "wall", at: [-7.65, 3.5, -130], normal: [1, 0, 0], size: [2.5, 0.3, 0.1], color: "white", range: 11, intensity: 1.4, job: "light", why: "NIGHT55 (n55-r-kb2b.cjs, P1-doorDark): the STOKE LAMP on the west liner at the cliff's foot: the 12 m climb face's west half and the take-off floor lit (the ceiling panels at 32 m reach nothing below ~18 m)" },
  { id: "kb2b-L55b", room: R, kind: "wall", at: [7.65, 8, -130], normal: [-1, 0, 0], size: [2.5, 0.3, 0.1], color: "white", range: 11, intensity: 1.2, job: "light", why: "NIGHT55 (P1-doorDark): the east STOKE LAMP on the liner over the cyan run-up panel (2 m above its top, off the ride face): the climb face's east half and the panel's run lit" },
  { id: "kb2b-L55c", room: R, kind: "pool", at: [0, 0.025, -132], normal: [0, 1, 0], size: [3, 1, 0.05], color: "white", range: 9, intensity: 1, job: "light", why: "NIGHT55 (P1-doorDark): the cliff's FOOT LAMP (a floor pool 3 m before the face): the climb face lit from below, so it reads as a wall to climb, not a dark void" },
  { id: "kb2b-L55d", room: R, kind: "wall", at: [-7.65, 3.5, -113], normal: [1, 0, 0], size: [2.5, 0.3, 0.1], color: "white", range: 10, intensity: 1, job: "light", why: "NIGHT55 (P1-doorDark): the DOOR LANDING LAMP on the west liner 5.5 m in from d3: the entry floor lit as you come through the door (GG w37: 'you just run through it and it's like dark')" },
];
const last = plan.lights.map((l, i) => (l.room === R ? i : -1)).filter((i) => i >= 0).pop();
plan.lights.splice(last + 1, 0, ...add);

for (const k of Object.keys(before)) {
  if (k === "lights") {
    const keep = (x) => !/^kb2b-L55/.test(x.id || "");
    if (!same(before.lights, plan.lights.filter(keep))) throw new Error("lights: an old row changed");
  } else if (!same(before[k], plan[k])) throw new Error(k + " changed");
}
console.log("kb2b: lights " + mine.length + " -> " + plan.lights.filter((l) => l.room === R).length);
if (process.argv.includes("--dry")) { console.log("DRY RUN"); process.exit(0); }
const eol = text.includes("\r\n") ? "\r\n" : "\n";
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1).replace(/\n/g, eol) + (text.endsWith("\n") ? eol : ""));
console.log("wrote " + PLAN);
