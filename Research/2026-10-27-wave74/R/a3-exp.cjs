// NIGHT74 R: variants of a3's room file (the winner is written into rooms/a3.json by hand with the Edit tool).
// Usage: node Research/2026-10-27-wave74/R/a3-exp.cjs <name[+name]> <in.json> <out.json>
const fs = require("fs");
const [name, inp, out] = process.argv.slice(2);
const f = JSON.parse(fs.readFileSync(inp, "utf8"));
const r = f.row;
const box = (min0) => f.boxes.find((b) => b.min[0] === min0[0] && b.min[1] === min0[1] && b.min[2] === min0[2]);
// a slag pig: a low cast block (or a taller mould pillar), cover for the floor fighter and a stepping stone for the air route
const pig = (id, x0, x1, z0, z1, h, why) => ({ zone: "a3", kind: "hull", min: [x0, 0, z0], max: [x1, h, z1], job: "cover", why: id + " (hull " + h + " m, NIGHT74 R): " + why });
const E = {
  base() {},
  // GG 2026-10-26 "the mini roof ... remove it" (0e3f95ea5, N0): a3 has no roof slab; its ring is 15 m (N2: no ESCAPE left); nothing hangs from a roof that is not there
  roofless() {
    r.roof = false; r.height = 15;
    r.height_why = "NIGHT74 R: roofless as n52's a3 is (GG 2026-10-26 'the mini roof ... remove it'; 0e3f95ea5), the ring 15 m (NIGHT72 N2 reach audit: no ESCAPE left at 10 m)";
    f.lights = f.lights.filter((l) => l.kind !== "ceiling");
    f.props = f.props.filter((p) => !["a3-P3", "a3-P13", "a3-P4", "a3-P5"].includes(p.id));
    f.lights.push(
      { id: "a3-WL1", room: "a3", kind: "wall", at: [-9.95, 4.5, -64], normal: [1, 0, 0], size: [2.5, 0.4, 0.1], color: "white", range: 9, intensity: 0.9, job: "light", why: "NIGHT74 R: lights the entry and the ramp's foot from the west wall (the ceiling light a3-L1 went with the roof)" },
      { id: "a3-WL2", room: "a3", kind: "wall", at: [-9.95, 4.5, -76], normal: [1, 0, 0], size: [3, 0.4, 0.1], color: "white", range: 9, intensity: 1, job: "light", why: "NIGHT74 R: lights the swarm floor and the mould wall from the west wall (a3-L2 went with the roof)" },
      { id: "a3-WL3", room: "a3", kind: "wall", at: [9.95, 5, -82.5], normal: [-1, 0, 0], size: [3, 0.4, 0.1], color: "amber", range: 10, intensity: 1.2, job: "landmark", why: "NIGHT74 R: the landmark light over THE LANDING, now on the east wall (a3-L3 hung from the roof)" });
    f.teach35.push({ id: "l1.wallrun", room: "a3", rect: [1, -64, 4, -59], minY: 0.3 });
  },
  // the air hand-offs (n65-air: 19 against the old room's 101: pieces 6 against 14): seven more ledges, low cast blocks on the swarm floor and in the east lane
  pigs() {
    const w = "cover for the floor fighter against the swarm and a stepping stone on the air route (n65-air counts a hull top of 1 m or more as an air piece)";
    f.boxes.push(
      pig("THE SLAG PIG, door west", -6.5, -5, -63, -61.5, 1.2, w),
      pig("THE SLAG PIG, west south", -10, -8.2, -66, -64.2, 1.2, w),
      pig("THE SLAG PIG, west mid", -10, -8.5, -76, -74.5, 1.2, w),
      pig("THE SLAG PIG, west north", -10, -8.2, -82, -80.3, 1.2, w),
      pig("THE SLAG PIG, floor", -7, -5, -76, -74.6, 1.2, w),
      pig("THE SLAG PIG, east lane", 8.2, 10, -68, -66, 1.2, w),
      pig("THE MOULD PILLAR, east lane", 8.2, 10, -64, -62.2, 2.4, w));
  },
  // a second health pickup on the swarm floor (deaths A 6 against the bar 5): the floor fighter's reward, between the pigs and the exit apron
  hp2() {
    f.props.push({ id: "a3-P2", room: "a3", kind: "healthPickup", at: [-7.5, 0.9, -79], size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true, job: "reward",
      why: "THE SLAG PIG'S MEND (NIGHT74 R): the swarm floor's own health, north-west between the pigs and the exit apron, 1.5 m from wave 2's west node: the ride pays with the landing's pickup, the floor fighter with this one" });
  },
  // METER ONLY (never written into the room): the meter takes the floor from the spawn's y, and a3's spawn stands on the ramp at 1.45 m; this puts
  // the spawn on the floor so every plane / loop / exposure reading is taken from the real floor
  meterspawn() { r.spawn = { at: [2.5, 0, -60.3], yawDeg: r.spawn.yawDeg }; },
  // diagnosis only: the slag run's lava pit as floor
  nopit() { f.pits = []; },
  // the slag run's lava pit as short as 7 m (z -76..-69) / as narrow as 1.5 m (x 2.5..4): deaths A 6 -> ? (nopit read 0)
  pitshort() { f.pits[0].rect = [1.4, -76, 4, -69]; },
  pitnarrow() { f.pits[0].rect = [2.5, -79.5, 4, -67]; },
  // diagnosis only: the swarm den's wall (the mould wall) 1.2 m, wave 1 visible over it
  lowmould() { const b = box([-10, 0, -70]); b.max[1] = 1.2; },
  // a second pad: THE DEN HOIST from the mould wall's top across the swarm floor onto the landing's west end
  pad2() {
    f.pads.push({ zone: "a3", at: [-6, 1.8, -69.5], target: [3.4, 2.5, -82.8], apexAbove: 2, job: "pad",
      why: "THE DEN HOIST (NIGHT74 R): on THE MOULD WALL's top (1.8 m, a jump and a double jump up from the floor): across the swarm floor onto THE LANDING's west end, 14 m: the room keeps the old a3's two pads; a way over the swarm that is not the ride, never the line the ride pays for (RIDE KILL, the picket)" });
  },
};
for (const n of name.split("+")) {
  const pm = /^padt(-?[\d.]+)_(-?[\d.]+)$/.exec(n); // padt<x>_<z>: the target of the LAST pad (the den hoist), on the landing's height
  if (pm) { f.pads[f.pads.length - 1].target = [+pm[1], 2.5, +pm[2]]; continue; }
  const ym = /^yaw(-?\d+)$/.exec(n); // yaw<N>: the spawn's yaw in degrees (+ turns west)
  if (ym) { r.spawn.yawDeg = +ym[1]; continue; }
  if (!E[n]) { console.error("unknown experiment " + n + " (" + Object.keys(E).join(", ") + ")"); process.exit(2); }
  E[n]();
}
fs.writeFileSync(out, JSON.stringify(f, null, 1) + "\n");
console.log("wrote " + out + " (" + name + ")");
