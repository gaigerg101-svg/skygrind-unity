// NIGHT74 builder S, slice v5 (NIGHT71 R's THE BELL WALLS r4, left out of the play build: the spire's verbs37 read RED on rows 9 and 11, "longest single ride
// 0.00 s on a 17 m wall 11.5 m high", with v2 + v5 new). Writes Design/levels/n67/spire/rooms/v5.json from t0b's file (idempotent: from the git blob).
//   node Research/2026-10-27-wave74/S/v5/make-v5-r5.cjs        run from the worktree root
// WHY IT WAS RED (traced tonight with Research/2026-10-27-wave74/S/wallprobe, which opens every zone first like the Player: it reproduces NIGHT71's two
// Player spire lines exactly): the reel's wall pick (VerbsWall39.Pick) is the longest straight run along a wall seen by 16 rays from each room's centre.
// In v5 the pick starts where the ray from the centre meets the west wall's corner by the door, (-21.3, -40.0), and runs 17 m north along x -21.3: the
// perimeter wall's face. THE STAIR WALL (x -22..-21.4, z -58..-42) ends 2 m short of that corner, so the lane's rays (zero thickness) slip past the
// panel's end while the player's capsule (0.35 m) meets it: the bot stops dead at z -41.7 against the panel's south end, 0 rides. The shipped v5's own
// ride wall ran on to the corner (the pick there starts (-20.7, -40.6) on the panel's face and rides 1.32 s).
// r5: THE STAIR WALL runs on to z -40.5 (17.5 m), the base's start again. Nothing else changes (the verb, both hoists, the belfry, the chamber, the stairs,
// the cheeks, the waves). The ride's light strip and its ride36 range follow the wall.
const fs = require("fs");
const cp = require("child_process");
const OUT = "Design/levels/n67/spire/rooms/v5.json";
const base = JSON.parse(cp.execSync("git show a354c0e94:" + OUT, { maxBuffer: 1 << 26 }).toString("utf8"));
const room = JSON.parse(JSON.stringify(base));
const W = "NIGHT74 S v5 r5 (Research/2026-10-27-wave74/S/card-v5.md): ";
const eq = (a, b) => JSON.stringify(a) === JSON.stringify(b);

const w = room.boxes.find((b) => b.ride && eq(b.min, [-22, 0, -58]) && eq(b.max, [-21.4, 8, -42]));
if (!w) throw new Error("no stair wall");
w.max[2] = -40.5;
w.ride36.lz = [7, 24.5];
w.note = "THE STAIR WALL (ride, cyan, 17.5 m x 8 m on the west wall)";
w.why = W + "z -42 -> -40.5 (16 -> 17.5 m): the wall runs on to the corner by the door, as the shipped ride face did. The verbs37 wall pick starts at the west wall's door-side corner and runs north 17 m; the wall ended 2 m short of it, the bot met its south end (0 rides, Player RED at NIGHT71). " + (w.why || "");
const rw = room.lights.find((l) => l.id === "v5-RW");
if (!rw) throw new Error("no light RW");
rw.at = [-21.35, 1.2, -49.25];
rw.size = [17.5, 0.2, 0.1];

// ---- the raised middle: NIGHT71's hall was two corner decks and an open floor (the shipped THE RINGERS' STAIR had a 2.5 m mass across its east half: the spotter
// and gg-proxy name "flat floor" against rebuilt rooms). THE BELL STAGE (2.5 m, 6 x 6 m) in the middle of the hall, ahead of the door, with a ramp from
// the spawn's side; the chamber hoist's pad moves out of the ramp's way to the hall's south-west corner (its target and job unchanged).
room.boxes.push(
  {
    zone: "v5", kind: "deck", min: [-7.5, 0, -51], max: [-1.5, 2.5, -45.4],
    job: "line: THE BELL STAGE (the mid tier: the ramp's top ahead of the door, a step between the floor, the belfry and the bell wall)",
    why: W + "the hall's middle was open floor: a 2.5 m deck (6 x 5.6 m, rimmed) ahead of the door, its north face touching the belfry's south face (z -51), so the hall reads in three planes (0 / 2.5 / 4 and 5) from the entry. Its near edge is 8.4 m inside the door (not within R5's 4-8 m band): it is a tier, not a foothold, so the room check does not read IN VIEW from its flanks.",
    note: "THE BELL STAGE (2.5 m, 6 x 5.6 m, mid-hall)",
  },
  {
    zone: "v5", kind: "hull", min: [-6, 0, -62], max: [-4, 2.2, -60],
    job: "cover: THE CLAPPER (a 2.2 m bell-weight pillar on the far floor: the one free-standing piece to circle under the spitters)",
    why: W + "the hall had no free-standing cover (the meter's COVER RED): a 2 x 2 x 2.2 m pillar (high cover, 1.75 m or more) in the north-east of the far floor, 1.5 m from the exit's lane (x -10), clear of the belfry and of every pad's flight.",
    note: "THE CLAPPER (2.2 m, 2 x 2 m, far floor)",
  },
);
room.ramps.push({
  zone: "v5", from: [-12.5, -48], to: [-7.5, -48], width: 3, rise: 2.5,
  job: "climb: THE STAGE STEP (the walkers' and the player's way onto the bell stage, 2.5 m over 5 m, from the spawn's side)",
  why: W + "the ramp onto THE BELL STAGE: a route enemies can climb to every floor (GG, NIGHT20); it starts 2 m ahead of the spawn.",
  note: "THE STAGE STEP",
});
const hoist = room.pads.find((p) => eq(p.at, [-12, 0, -50.5]));
if (!hoist) throw new Error("no chamber hoist");
hoist.at = [-18.5, 0, -45];
hoist.why = W + "THE CHAMBER HOIST's pad (-12, 0, -50.5) -> (-18.5, 0, -45): off THE STAGE STEP's foot (the ramp's north edge z -49.5); the target (the chamber, (-19, 5, -61.5)) and the job are unchanged. " + hoist.why;
room.row.proofs.push({ kind: "walk", points: [[-12.5, 0, -48], [-7.5, 2.5, -48], [-4.5, 2.5, -48]] });
// wave 1's node (-7, 0, -58) lies behind THE BELFRY STAIR from the entry eye (the ramp rises 4 m over x -13..-4 across z -56..-53; the n56 reading was
// 3.2 of 4 seen with the wedges correctly turned): it moves west of the ramp's foot, in the entry's view, 17 m from the commit feet
const NO = [-7, 0, -58], NN = [-12.5, 0, -59.5];
let moved = 0;
const swap = (n) => (eq(n, NO) ? (moved++, NN.slice()) : n);
room.row.nodes = room.row.nodes.map(swap);
room.row.waveNodes43 = room.row.waveNodes43.map((w) => w.map(swap));
if (moved !== 2) throw new Error("expected node (-7, 0, -58) in nodes and wave 1, got " + moved);
room.row.nodes_why = W + "node (-7, 0, -58) -> (-12.5, 0, -59.5): hidden from the entry eye by THE BELFRY STAIR (the wedge's real orientation; the entry frame read 3.2 of wave 1's 4 seen). " + (room.row.nodes_why || "");
room._why = W + "THE STAIR WALL runs on to z -40.5 (the verbs37 pick's lane meets the wall's face, not its end); THE BELL STAGE, THE STAGE STEP and THE CLAPPER make the hall's middle; the rest is NIGHT71 R's. " + room._why;
fs.writeFileSync(OUT, JSON.stringify(room, null, 1) + "\n");
console.log("wrote " + OUT);
