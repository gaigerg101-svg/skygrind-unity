// NIGHT47 builder L (design.n47.spire47; NIGHT47.md G42 "level 5 makes sense"; Research/2026-10-18-wave47/L-cards.md section 4, the card gate's PASS):
// Level 5 as n47-spire: n46-spire's plan (Design/levels/n46/spire.plan.json, written by Tools/night/gen-n46-spire.cjs, which this file does not run or
// edit) copied whole, plus THE ROUTE: the CROWN's light seen from every room. Every room, door, box and piece is n46-spire's bit for bit (the guard at
// the end throws if anything but the `lights` rows this file adds, the id and the _why would differ), so `--off47` (n46-spire) and this file differ in
// those lights only.
//
// Round 1 (this file): THE CROWN'S WINDOWS. The premise (L-cards.md section 4): the cathedral stands on the chasm's lip and every room's far wall faces
// the bell shaft where the CROWN hangs; its red-gold light comes through a tall window over each room's exit door. Each window is a `wall` light row
// (LightKit29 draws the lit slab and stands its light: data only, no new view code), "white" (the pale violet-white boundary: a window onto the chasm IS the boundary; LevelValidate29 allows the colours cyan|green|amber|white|orange and one accent (amber / orange) a room, and six rooms have theirs; the CROWN's red-gold tint is crownLook47's, a view, round 2), not a usable thing and not the
// danger tone), on the exit wall's inner face (the room's -z edge: the level runs toward -z) over the door (x -10, the door axis), from the door's top
// (4.5 m) up to 1 m under the roof, so from the room's spawn (its entry door) the way on is the brightest thing ahead (A-6, the wienie: L.md section 1).
// v8 THE VESTRY's is the widest (the picture spot before the exam rooms). The windows sit on the exit walls only: never on a fight floor's footprint,
// never on an enemy climb route (the lead's card-gate note 1).
// SHORT, measured (round 1, for round 2): the card's roof slots + a 300 m light column at the core need the roofs opened, and LevelValidate29's
// closedRoom check (rays up from under the roof meet it within 0.35 m in every room but the boss's) refuses an open roof; a glass pane would pass the
// check but RoofView27's view ceiling and the plan roof both render opaque. Not built.
// Coordinates: web (x east, y up, z south); every door is at y 0 (levelgen doors carry no height).
// Run from the repo (or worktree) root: bash Tools/night/n47-spire-regen.sh
const path = require("path");
const fs = require("fs");

const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/levels/n46/spire.plan.json");
const OUT = path.join(ROOT, "Design/levels/n47/spire.plan.json");

const plan = JSON.parse(fs.readFileSync(SRC, "utf8"));
if (plan.id !== "n46-spire") throw new Error(`unexpected source id ${plan.id}`);
const before = JSON.parse(JSON.stringify(plan));
plan.id = "n47-spire";
plan._why = "NIGHT47 builder L (design.n47.spire47 swaps n46-spire for this file while the row is on): Tools/night/gen-n47-spire.cjs copies n46-spire's "
  + "plan whole and adds THE CROWN'S WINDOWS (one lit window over every room's exit door: the way on, toward the CROWN's light; L-cards.md section 4). "
  + "n46-spire's _why: " + plan._why;

const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const DOOR_TOP = 4.5;

// ---- THE CROWN'S WINDOWS -------------------------------------------------------------------------------------------------------------------------
// the rooms with an exit door on the level's axis (v1..v10: each room's exit is its door toward -z)
const ROUTE = ["v1", "v2", "v3", "v4", "v5", "v6", "v7", "v8", "v9", "v10"];
const JOB = {
  v1: "take the TORCH and see where you are going: the light over the way out",
  v2: "the TORCH up close round the fallen bell; the light over the apse door says the bell fell from where you are going",
  v3: "learn to fly over the fallen belfry floor toward the light",
  v4: "fly onto a higher ledge, the light ahead",
  v5: "a breather fight in the TORCH's lane, the light over its far door",
  v6: "fly between three heights and punch the drones' orbs back; the light behind the organ loft's door",
  v7: "the exam at the jump-dash's limit, the light over the landing's door",
  v8: "rest, heal and see the light close: the widest window (the picture spot before the stones and the ring)",
  v9: "keep moving across stones that fall under you, the light over the far door",
  v10: "rehearse the pass through the ring, the light beyond it",
};
const added = [];
for (const id of ROUTE) {
  const r = plan.rooms.find((q) => q.id === id);
  if (!r) throw new Error(`room ${id} missing`);
  const [x0, z0, x1] = [r.rect[0], r.rect[1], r.rect[2]];
  const h = r.height;
  // a gate room's flight ceiling (v3 / v4 / v7: boxes with `ceiling`, from y 4) hides a window over the door from the entry: measured round 1,
  // `n47spire route` BLOCKED in those three, so there the windows go beside the door, under the ceiling's underside
  const ceil = plan.boxes.filter((b) => b.zone === id && b.ceiling).reduce((m, b) => Math.min(m, b.min[1]), Infinity);
  const top = Math.min(h - 1.0, ceil - 0.3);
  // a room too low for a window over the door (v8 THE VESTRY, 6 m; the gates' 4 m ceiling) gets two tall windows flanking it instead (x 4 m either side)
  const over = top - DOOR_TOP >= 1.2;
  const spots = over ? [[-10, DOOR_TOP + 0.5, top, 3, ""]] : [[-14, 0.8, top, 2.5, "-w"], [-6, 0.8, top, 2.5, "-e"]];
  if (!over && (x0 > -15.5 || x1 < -4.5)) throw new Error(`${id}: no wall beside the door for the flanking windows`);
  for (const [x, lo, hi, wide, tag] of spots) {
    const tall = hi - lo;
    const row = {
      id: `${id}-crown47${tag}`, room: id, kind: "wall",
      at: [x, +(lo + tall / 2).toFixed(2), +(z0 + 0.05).toFixed(2)],
      normal: [0, 0, 1], size: [wide, +tall.toFixed(2), 0.1], color: "white", range: 7, intensity: id === "v8" ? 2.0 : 1.7,
      job: "route",
      why: `THE CROWN'S WINDOW (spire47, G42): ${JOB[id]}; on the exit wall ${over ? "over" : "beside"} the door (A-6: the way on is lit), off the fight floor and every climb route`,
    };
    row.note = row.why;
    added.push(row);
  }
}
// after the room's own lights (the arrays keep every n46 row's order: the new rows go at the end of each room's run)
for (const row of added) {
  let at = -1;
  plan.lights.forEach((l, i) => { if (l.room === row.room) at = i; });
  plan.lights.splice(at < 0 ? plan.lights.length : at + 1, 0, row);
}

// ---- the guard: only the added lights, the id and the _why differ --------------------------------------------------------------------------------
const lightsBack = plan.lights.filter((l) => !/-crown47(-[we])?$/.test(l.id));
if (!same(lightsBack, before.lights)) throw new Error("guard: an n46-spire light changed");
for (const k of Object.keys(before)) {
  if (k === "lights" || k === "id" || k === "_why") continue;
  if (!same(before[k], plan[k])) throw new Error(`guard: top-level ${k} changed`);
}
if (Object.keys(plan).length !== Object.keys(before).length) throw new Error("guard: a top-level key was added");

fs.mkdirSync(path.dirname(OUT), { recursive: true });
fs.writeFileSync(OUT, JSON.stringify(plan, null, 1) + "\n");
console.log("wrote", path.relative(ROOT, OUT), plan.rooms.length, "rooms;", added.length, "crown windows:", added.map((r) => `${r.room} ${r.size[0]}x${r.size[1]} @y${r.at[1]}`).join(", "));
