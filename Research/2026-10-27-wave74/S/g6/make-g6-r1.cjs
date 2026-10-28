// NIGHT74 builder S, slice g6 (the lead's: gg-proxy-start ranks g6 THE ARMOURY a flat floor: "the floor is empty at x0-1920, y600-1080"; the rooms-rules meter reads
// PLANES 1, COVER none, LOOPS 0, EXPOSE 100 %). Writes Design/levels/n67/iceline/rooms/g6.json from t0b's file (the shipped export: idempotent, from the git blob).
//   node Research/2026-10-27-wave74/S/g6/make-g6-r1.cjs        run from the worktree root
// g6 2.6 THE ARMOURY is level 2's KESTREL room (job36 reward): the KESTREL comes back on its plinth (the door's axis, 3.5 m in), wave 1 two husks at the far
// end, wave 2 one husk, two lessons ("KESTREL BACK: FIRE THE FAR ONES", "DASH INTO IT. FIRE THE GAUNTLET."). Kept exactly (PRINCIPLES rule 1): the rect, the
// door, the spawn, the exit, the waves, the nodes, both lessons, the plinth and the KESTREL pickup. The shipped room had no piece at all.
// The verb: LAUNCH (a dash-punch throws a husk into a rack or into its partner and it bursts; NIGHT71 R's slate). New (each with a job): THE WEST SHELF (a
// 2 m tier the husks climb, the player rides the pad or the ramp), its ramp, THE GUN LIFT (a pad onto the shelf), THE EAST RACK (a free-standing 2.5 m rack: the
// burst wall and the cover from the far husks), THE PIER (2.2 m), THE CART (1.2 m low cover).
const fs = require("fs");
const cp = require("child_process");
const OUT = "Design/levels/n67/iceline/rooms/g6.json";
const base = JSON.parse(cp.execSync("git show a354c0e94:" + OUT, { maxBuffer: 1 << 26 }).toString("utf8"));
const room = JSON.parse(JSON.stringify(base));
const W = "NIGHT74 S g6 r1 (Research/2026-10-27-wave74/S/card-g6.md): ";
const row = room.row;
if (room.boxes.length || room.ramps.length || room.pads.length) throw new Error("the shipped g6 should hold no piece");

const box = (kind, min, max, job, why, note) => ({ zone: "g6", kind, min, max, job, why: W + why, note });
room.boxes.push(
  box("deck", [-7, 0, -177], [-4, 2, -168], "line: THE WEST SHELF (2 m: the armoury's raised shelf over the dash lane: shoot down the far husks, throw a husk at its face)",
    "the room was one bare floor: a 3 x 9 m shelf along the west wall, 2 m high and rimmed, the second floor plane; the ramp and the gun lift both end on it.",
    "THE WEST SHELF (2 m, 3 x 9 m, west wall)"),
  box("hull", [4.3, 0, -176], [5.5, 2.5, -169], "line: THE EAST RACK (a free-standing 2.5 m gun rack: the burst wall beside the dash lane and the cover from the far husks' line)",
    "NIGHT71 R's east rack idea, free-standing (1.2 x 7 m): a husk thrown at it bursts on its face; a circuit round it.",
    "THE EAST RACK (2.5 m, 1.2 x 7 m, free-standing)"),
  box("hull", [-3.6, 0, -170], [-2, 2.2, -168.4], "cover: THE PIER (a 2.2 m pillar west of the plinth: high cover on the way to the far husks)",
    "a 1.6 x 1.6 x 2.2 m pillar (high cover, 1.75 m or more) 1.4 m west of the plinth's line, 8.4 m inside the door (not R5's 4-8 m foothold band); clear of the entry's sight lines to both wave-1 nodes.",
    "THE PIER (2.2 m, 1.6 x 1.6 m)"),
  box("hull", [2, 0, -171.5], [3.6, 1.2, -170], "cover: THE CART (1.2 m low cover east of the dash lane: hop it or shelter behind it from the far husks)",
    "a 1.6 x 1.5 x 1.2 m low piece (1.0-1.25 m) 2 m east of the axis, clear of the walk line.", "THE CART (1.2 m, 1.6 x 1.5 m)"),
);
room.ramps.push({
  zone: "g6", from: [-5.5, -164], to: [-5.5, -168], width: 2.5, rise: 2,
  job: "climb: THE SHELF STAIR (the walkers' and the player's way onto the west shelf: 2 m over 4 m, beside the door)",
  why: W + "a route enemies can climb to every floor (GG, NIGHT20): on the shelf's south end, in the door's view on the left.", note: "THE SHELF STAIR",
});
room.pads.push({
  zone: "g6", at: [-2.5, 0, -173.5], target: [-5.5, 2, -172.5], apexAbove: 1.5,
  job: "line: THE GUN LIFT (a jump pad from the floor onto the west shelf: the launch room's way up)",
  why: W + "the shelf's second way up (the stair is the walkers'): a pad 2.5 m west of the dash lane, its flight clear of the pier.", note: "THE GUN LIFT",
});

// ---- the verb: launch (NIGHT71 R's slate verb for g6), its line, the proofs
row.bot43 = false;
row.verb65 = "launch";
row.line65 = [[0, 0, -164.5], [0, 0, -170], [0.5, 0, -175], [3, 0, -176.5]];
row.line65_why = W + "the launch line: the dash lane from the door past the plinth to the far husks, the throw at the east rack";
row.card65 = "Research/2026-10-27-wave74/S/card-g6.md";
row.proofs = [
  row.proofs[0],
  { kind: "walk", points: [[-5.5, 0, -164], [-5.5, 2, -168.5], [-5.5, 2, -173]] },
];
room._why = W + "THE WEST SHELF + its stair + THE GUN LIFT, THE EAST RACK, THE PIER, THE CART; the rect, the waves, the lessons, the plinth and the KESTREL are the shipped room's. " + room._why;
fs.writeFileSync(OUT, JSON.stringify(room, null, 1) + "\n");
console.log("wrote " + OUT + ": boxes " + room.boxes.length + ", ramps " + room.ramps.length + ", pads " + room.pads.length);
