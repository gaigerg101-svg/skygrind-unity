// NIGHT74 builder S, slice kb2c (NIGHT71 R's THE ASH FLOOR r5, left out of the play build: the kiln's verbs37 read RED at NIGHT71 on a wall pick that is
// not the room's wall any more). Writes Design/levels/n67/kiln/rooms/kb2c.json from t0b's file (idempotent: always from the git blob).
//   node Research/2026-10-27-wave74/S/kb2c/make-kb2c-r6.cjs        run from the worktree root
// r6 keeps the verb (ride), the 19.5 m cooling wall (r5's fix, untouched), the hearth, the catwalk, the flue wall, the bellows housing, the three
// pads, both stairs, the door liners, the waves and the lesson. Changes, each with a job:
//   1. the plan check's one problem: ground node 7 (-1, 0, -180.75) sat 0.25 m from the iron plate's edge (a capsule overlap): it moves to z -180.25.
//   2. the three iron plates 0.4 -> 0.9 m (burrow-safe iron the player hops onto and sees from the door: a walkers' tier, not a kerb).
//   3. THE KILN HOOD (a 2.2 m stack, 2 x 2 m, mid-room): the circuit round the burrower's fight and a break in the door-to-hearth line.
//   (no slag bin: see the comment at the hood; the verbs37 wall pick is judged by Research/2026-10-27-wave74/S/run-wall.sh with zones OPENED)
const fs = require("fs");
const cp = require("child_process");
const OUT = "Design/levels/n67/kiln/rooms/kb2c.json";
const base = JSON.parse(cp.execSync("git show a354c0e94:" + OUT, { maxBuffer: 1 << 26 }).toString("utf8"));
const room = JSON.parse(JSON.stringify(base));
const W = "NIGHT74 S kb2c r6 (Research/2026-10-27-wave74/S/card-kb2c.md): ";
const eq = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const row = room.row;

// ---- 1. node 7
const OLD = [-1, 0, -180.75], NEW = [-1, 0, -180.25];
let moved = 0;
const swap = (list) => list.map((n) => (eq(n, OLD) ? (moved++, NEW.slice()) : n));
row.nodes = swap(row.nodes);
row.waveNodes43 = row.waveNodes43.map((w) => swap(w));
if (moved !== 4) throw new Error("expected node (-1, 0, -180.75) in nodes and in each of the 3 waves (4 uses), got " + moved);
row.nodes_why = W + "ground node (-1, 0, -180.75) -> (-1, 0, -180.25): the plan check named it (a capsule overlap with the mid iron plate's edge, z -181). " + row.nodes_why;

// ---- 2. the plates
let plates = 0;
for (const b of room.boxes) {
  if (b.burrowSafe43 && b.kind === "deck" && Math.abs(b.max[1] - 0.4) < 1e-9) {
    b.max[1] = 0.9;
    b.note = b.note.replace("0.4 m", "0.9 m");
    b.why = W + "the plate 0.4 -> 0.9 m: a raised iron block the player hops onto (walkers climb 0.9 m), rimmed, seen from the door; the burrow rule is the same. " + b.why;
    plates++;
  }
}
if (plates !== 3) throw new Error("expected 3 iron plates, found " + plates);

// the cyan pool light on the mid plate is flush to the plate's top (levelgen: "not flush to a surface"): up with the plate
const L6 = room.lights.find((l) => l.id === "kb2c-L6");
if (!L6 || Math.abs(L6.at[1] - 0.425) > 1e-9 || !L6.marks.includes("0.4,-181")) throw new Error("no light L6 on the mid plate");
L6.at = [L6.at[0], 0.925, L6.at[2]];
L6.marks = L6.marks.replace("0.4,-181", "0.9,-181");

// ---- 3. the hood (and, with --bin x0,z0 the slag bin: NOT in the shipped r6: a 1.2 m piece at x 3..5 blocked the pick ray (the room centre's
// 1.0 m eye, 22.5 deg north of east) to the cooling wall's z -182 start: the verbs37 pick fell to a 14 m run, ride 1.02 s, row 9 RED under the
// Player's open zones; at x 4.5..6.5 it stood on the ride-kill pilot's husk spot)
room.boxes.push(
  {
    zone: "kb2c", kind: "hull", min: [0.5, 0, -176], max: [2.5, 2.2, -174],
    job: "cover: THE KILN HOOD (a 2.2 m stack mid-room: the burrower fight's pillar to circle; it breaks the door-to-hearth line)",
    why: W + "a free-standing 2 x 2 x 2.2 m stack 7 m beyond the door: high cover (1.75 m or more) to circle while the ash glows, west of the east plate (x 3.5) with a 1 m lane between, clear of the entry's sight line to wave 1's east node and of the verbs37 pick's rays to the cooling wall.",
    note: "THE KILN HOOD (2.2 m, 2 x 2 m, mid-room)",
  },
);
const bi = process.argv.indexOf("--bin");
if (bi > 0) {
  const [bx, bz] = process.argv[bi + 1].split(",").map(Number);
  room.boxes.push({
    zone: "kb2c", kind: "hull", min: [bx, 0, bz], max: [bx + 2, 1.2, bz + 2],
    job: "cover: THE SLAG BIN (1.2 m low cover)", why: W + "probe piece (--bin " + bx + "," + bz + ")", note: "THE SLAG BIN (1.2 m, 2 x 2 m)",
  });
}

// ---- the proofs: the first walk passes the mid plate (now 0.9 m): round its east end, between the plate and the ash vent
// (I4: the ash vent's disc at (2.5, 0, -184.5) stays 2 m or more from every walk line: down x 0.2, 0.7 m east of the plate, 2.3 m west of the pad)
row.proofs[0] = { kind: "walk", points: [[-2, 0, -168], [-1.5, 0, -173], [1.5, 0, -180], [0.2, 0, -183], [0.2, 0, -185.5], [-4, 0, -185.5], [-4, 0, -190.5], [0, 0, -191.5]] };
room._why = W + "node 7 off the plate's edge, the iron plates 0.9 m, THE KILN HOOD added; the verb (ride), the cooling wall (19.5 m, r5), the hearth, the catwalk, the flue wall, the pads and the waves are NIGHT71 R's. " + room._why;
fs.writeFileSync(OUT, JSON.stringify(room, null, 1) + "\n");
console.log("wrote " + OUT + ": boxes " + room.boxes.length + ", ramps " + room.ramps.length + ", pads " + room.pads.length + ", nodes moved " + moved + ", plates " + plates);
