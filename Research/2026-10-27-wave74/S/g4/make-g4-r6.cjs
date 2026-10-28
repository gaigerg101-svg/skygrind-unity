// NIGHT74 builder S, slice g4 (the lead's: GG's NIGHT71 g4 THE ORB YARD is flatter than the shipped n52 g4 THE REACH; the blind spotter names the
// old room's raised blocks with cyan edges and its second ramp on the left; gg-proxy: "a flat floor with one ramp"). Writes
// Design/levels/n67/iceline/rooms/g4.json from t0b's file (idempotent: always from the git blob, never from the working file).
//   node Research/2026-10-27-wave74/S/g4/make-g4-r6.cjs            run from the worktree root
// The verb stays PARRY (the yellow orb) and the room keeps its ice glide floor, its lock wall ride, the gallery, the west ledge, both ice posts, the
// sluice head, the run-up block, the three hoists, both stairs, the waves and the lesson (PRINCIPLES rule 1). New: the pond is raised 0.9 m into THE ICE
// STAGE (a rimmed deck with the ice on top, two short ramps on it) and THE LEDGE RAMP climbs the west ledge from the door side, in the entry's view.
const fs = require("fs");
const cp = require("child_process");
const OUT = "Design/levels/n67/iceline/rooms/g4.json";
const base = JSON.parse(cp.execSync("git show a354c0e94:" + OUT, { maxBuffer: 1 << 26 }).toString("utf8"));
const room = JSON.parse(JSON.stringify(base));
const W = "NIGHT74 S g4 r6 (Research/2026-10-27-wave74/S/card-g4.md; the lead's slice: GG's g4 was flatter than THE REACH): ";
const eq = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const row = room.row;

// ---- the stage: the pond's rect raised 0.9 m (a walkers' tier: levelgen's 1 m deck top is not one, 0.9 m is)
const STAGE = { min: [-6, 0, -113], max: [4.5, 0.9, -107] };
room.boxes.push({
  zone: "g4", kind: "deck", min: STAGE.min, max: STAGE.max,
  job: "line: THE ICE STAGE (the pond raised 0.9 m: the glide lane under the orbs; the raised, rimmed block the room reads from the door)",
  why: W + "THE ICE POND (x -6..4.5, z -113..-107) stood on the floor, so the yard read flat from the door. It is now a deck 0.9 m high (10.5 x 6 m, rimmed like every deck, the ice on its top: surfaces minY 0.4 / maxY 1.4); two short ramps (south and north) put the walkers' and the skater's way onto it.",
  note: "THE ICE STAGE (0.9 m, 10.5 x 6 m: the ice pond raised)",
});
// ---- the ice stays: the glide's band follows the stage's top
const ice = room.surfaces.find((s) => s.kind === "ice");
if (!ice || !eq(ice.rect, [-6, -113, 4.5, -107])) throw new Error("no ice pond");
ice.minY = 0.4;
ice.maxY = 1.4;
ice.note = ice.note + "; NIGHT74 S g4 r6: the pond is THE ICE STAGE now (0.9 m): the band follows its top (0.4 .. 1.4)";

// ---- ramps: the stage's south and north ramps, and the west ledge's ramp from the door side (the second ramp, on the left in the entry's view)
room.ramps.push(
  {
    zone: "g4", from: [2, -103.5], to: [2, -107], width: 3, rise: 0.9,
    job: "climb: THE STAGE STEP (the door floor onto the ice stage: the skater's and the walkers' way up, 0.9 m over 3.5 m)",
    why: W + "the south ramp onto THE ICE STAGE (x 0.5..3.5), 2 m east of the ledge ramp and 0.9 m clear of THE EAST ICE POST (x 4.4).", note: "THE STAGE STEP",
  },
  {
    zone: "g4", from: [0, -116.5], to: [0, -113], width: 3, rise: 0.9,
    job: "climb: THE STAGE DROP (the far floor onto the ice stage and back: 0.9 m over 3.5 m)",
    why: W + "the north ramp off THE ICE STAGE onto the far floor, west of THE CANAL HOIST's pad (x 2..4) and east of THE WEST ICE POST (x -5.6..-4.4).", note: "THE STAGE DROP",
  },
  {
    zone: "g4", from: [-1.5, -105], to: [-6, -105], width: 3, rise: 2.5,
    job: "climb: THE LEDGE RAMP (the door floor onto THE WEST LEDGE: 2.5 m over 4.5 m, in the entry's view; the walkers' second way up)",
    why: W + "the west ledge's only stair (x -11..-8, z -114..-108) stood behind the ledge, out of the door's view; this second ramp climbs the ledge's east face from the door floor (z -106.5..-103.5), 2 m clear of the stage step.", note: "THE LEDGE RAMP",
  },
);

// ---- the line the tour frames, the proofs, the pickup and the light follow the stage
row.line65 = [[1.5, 0, -100], [2, 0.45, -105.25], [0, 0.9, -110], [0, 0.45, -114.75], [0, 0, -119]];
row.line65_why = W + "the parry line over the raised ice: the door floor, THE STAGE STEP, the stage, THE STAGE DROP, the far floor (y follows the surface)";
row.proofs = [
  { kind: "walk", points: [[0, 0, -96], [2, 0, -102], [2, 0.9, -108], [0, 0.9, -112], [0, 0, -118], [0, 0, -122]] },
  row.proofs[1],
  row.proofs[2],
  row.proofs[3],
  // the ledge ramp to its top and a step on (I4: THE LEDGE HOIST's disc at (-6.8, 2.5, -103) stays 2 m or more from every walk line)
  { kind: "walk", points: [[-0.5, 0, -105], [-6, 2.5, -105], [-6.5, 2.5, -106.5]] },
];
const pk = room.props.find((p) => p.id === "g4-P1");
if (!pk) throw new Error("no pickup");
pk.at = [0, 1.8, -110];
const L7 = room.lights.find((l) => l.id === "g4-L7");
if (!L7) throw new Error("no light L7");
L7.at = [-1, 0.925, -110];
room._why = W + "the pond raised into THE ICE STAGE (0.9 m, rimmed), THE STAGE STEP and THE STAGE DROP, and THE LEDGE RAMP; the verb (parry), the ice, the lock wall, the gallery, the ledge, the posts, the hoists and the waves are NIGHT71 R's, unchanged. " + room._why;
fs.writeFileSync(OUT, JSON.stringify(room, null, 1) + "\n");
console.log("wrote " + OUT + ": boxes " + room.boxes.length + ", ramps " + room.ramps.length + ", pads " + room.pads.length);
