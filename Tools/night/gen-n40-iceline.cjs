// NIGHT40 builder L2 (NIGHT40.md card L2; GG 2026-10-13 asks 13 and 14, Research/2026-10-14-wave40/GG-REQUEST.md): LEVEL 2 THE ICE LINE rebuilt for
// the PULSE gauntlet, from its beat sheet Design/levels/n40/iceline.beats.md (written first). Writes Design/levels/n40/iceline.plan.json (id
// n40-iceline); Tools/night/n40l2-regen.sh runs the jobs checker, levelgen and the copies (Design/levels/n40-iceline.json + StreamingAssets).
// The campaign plays it behind design.n40.iceline40 (levelFrom n37-iceline -> levelTo n40-iceline, Curriculum25.N40Levels): startSet the
// gauntlet alone, awards [{room: "g6", weapon: "revolver"}] (the KESTREL returns in 2.6 THE ARMOURY). No older level file or generator is
// edited or required: n38-iceline's plan is read as data for the level's name and its n27 / wallHeight rows only.
// The gauntlet's curriculum (the card's order): g1 TANK ALONE, g2 THE SHIELD (WARDEN), g3 THE SWARM (the blast for the pack), g4 THE REACH
// (a DRONE across the canal: the parry), g5 THE COOLDOWN (the 15 s), g6 THE ARMOURY (the KESTREL back), g7 THE MIX, g8 THE REHEARSAL
// (a CHARGER), gr REST, then x9 THE BRAWL (B2's arena: 36 x 36, the lead's id and size; this file reserves the empty room).
// Run: node Tools/night/gen-n40-iceline.cjs   (then bash Tools/night/n40l2-regen.sh)
const fs = require("fs");
const path = require("path");
const { Level, r3 } = require("./n36-lib.cjs");

const ROOT = path.join(__dirname, "../..");
const OUT = "Design/levels/n40/iceline.plan.json";
const src = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/levels/n38/iceline.plan.json"), "utf8"));
const pulse = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/unity-config.json"), "utf8")).design.n27.pulse27;

// the room times of the beat sheet (s): the rank cuts below and each room's par
const TIMES = { g1: 12, g2: 30, g3: 25, g4: 30, g5: 30, g6: 8, g7: 50, g8: 30, gr: 4 };
const L = new Level("n40-iceline", src.name,
  "NIGHT40 builder L2 (design.n40.iceline40 swaps n37-iceline for this file): Tools/night/gen-n40-iceline.cjs from the beat sheet Design/levels/n40/iceline.beats.md: "
  + "the gauntlet's curriculum, one job per room: 2.1 TANK ALONE, 2.2 THE SHIELD (a WARDEN), 2.3 THE SWARM (the blast for the pack), 2.4 THE REACH (a DRONE across the canal), "
  + "2.5 THE COOLDOWN (the 15 s), 2.6 THE ARMOURY (the KESTREL returns: iceline40.awards), 2.7 THE MIX, 2.8 THE REHEARSAL (a CHARGER), 2.9 REST, THE BRAWL (B2's boss room x9). "
  + "GG 2026-10-13 asks 13-14 (Research/2026-10-14-wave40/GG-REQUEST.md).",
  { rank: RANK40(), n27: src.n27 });
const ICE = [];
function ice(r, lx0, lz0, lx1, lz1, minY, maxY, note) {
  ICE.push({ kind: "ice", rect: r.rect(lx0, lz0, lx1, lz1), minY, maxY, room: r.id, note });
}
const ARENA = (r) => { r.row.arena36 = true; };

// ---------------------------------------------------------------- 2.1 TANK ALONE (teach: the punch kills what guns barely scratch)
{
  const r = L.room({ id: "g1", name: "2.1 TANK ALONE", job: "teach", verb: "the punch: a tank dies to one swing of the gauntlet", w: 22, l: 26, h: 9, par: TIMES.g1, spawnLz: 2, exitFromN: 1.5 });
  ARENA(r);
  r.panel("E", -8, 5, 18, 0, 5, "ride panel W (5 m tall, 13 m, face east): the line round the tank if the swing is misjudged; room to circle a slow body");
  r.panel("W", 8, 5, 18, 0, 5, "ride panel E (face west), 16 m from W plane to plane: no chain, each its own line past the tank");
  r.nodeAt(0, 14);
  r.row.waves = [["tank"]];
  r.lesson({ kind: "husksDead", n: 1, prompt: "{fire}: ONE SWING FELLS A TANK" });
  r.ceil(0, 3, 3, 3, "white", 9, 1.0); r.ceil(0, 14, 4, 4, "amber", 10, 1.1);
  r.pool(0, 24.5, 2.5, "cyan", 7, 1.6);
  r.walkSE([[3, 0, 8], [3, 0, 20]]);
  r.frames([5, 0, 3], [0, 1, 14], [0, 0, 18], [0, 1, 26]);
}

// ---------------------------------------------------------------- 2.2 THE SHIELD (teach: the WARDEN; punch the shield or flank it)
{
  const r = L.room({ id: "g2", name: "2.2 THE SHIELD", job: "teach", verb: "the WARDEN: punch the shield (a 1 s stun) or get round it", w: 24, l: 28, h: 9, par: TIMES.g2, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  r.solid("pillar", -4, 10, 2, 4, 2, "cover", "the west PILLAR (2 x 2 x 4 m) 6 m short of the warden: step round it to its flank (the shield covers only its front 120 deg)");
  r.solid("pillar", 4, 10, 2, 4, 2, "cover", "the east PILLAR: the other way round");
  r.panel("E", -9, 6, 22, 0, 5, "ride panel W (face east, 16 m): ride past the warden to its back");
  r.panel("W", 9, 6, 22, 0, 5, "ride panel E (face west): the other flank by the wall");
  r.box("deck", -12, 23, -2, 28, 0, 1.5, "line", "the north deck's west half (1.5 m): wave 2's husks switch on here, in view, and come down its ramp; the exit runs between the halves at floor level");
  r.box("deck", 2, 23, 12, 28, 0, 1.5, "line", "the north deck's east half (1.5 m)");
  r.ramp(-6, 18, 23, 3, 1.5, "climb", "the west half's ramp: the husks' way down to you (1.5 m over 5 m) and yours up");
  r.ramp(6, 18, 23, 3, 1.5, "climb", "the east half's ramp");
  r.nodeAt(0, 16); r.nodeAt(0, 20);
  r.highNodeAt(-8, 25.5, 1.5); r.highNodeAt(8, 25.5, 1.5);
  r.row.waves = [["warden"], ["warden", "husk", "husk"]];
  r.lesson({ kind: "husksDead", n: 1, prompt: "{fire} THE SHIELD, OR FLANK IT" });
  r.pickup("healthPickup", -8, 25.5, 2.4, "the high ground pays health after the first warden");
  r.ceil(0, 3, 3, 3, "white", 9, 1.0); r.ceil(0, 16, 4, 4, "amber", 10, 1.1); r.ceil(-7, 25.5, 4, 3, "white", 9, 1.0); r.ceil(7, 25.5, 4, 3, "white", 9, 1.0);
  r.pool(0, 26, 2.5, "cyan", 7, 1.6);
  r.walkSE([[0, 0, 6], [0, 0, 20]]);
  r.walk([[-6, 0, 16], [-6, 1.5, 24], [-8, 1.5, 25.5]]);
  r.frames([6, 0, 3], [0, 1, 16], [0, 0, 20], [0, 1, 28]);
}

// ---------------------------------------------------------------- 2.3 THE SWARM (teach: five kills charge the glove; the blast is for the pack)
{
  const r = L.room({ id: "g3", name: "2.3 THE SWARM", job: "teach", verb: `five kills charge the glove (${pulse.pips} pips); the ${"{altFire}"} blast takes the pack in the gap`, w: 26, l: 28, h: 9, par: TIMES.g3, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  r.box("hull", -13, 18, -3, 20, 0, 3, "line", `the PEN's west wall (3 m): the swarm can only come through the 6 m gap, in a line: the blast's ${pulse.blastRange} m reach down the gap takes it (design.n27.pulse27.blastRange)`);
  r.box("hull", 3, 18, 13, 20, 0, 3, "line", "the PEN's east wall (3 m): the other side of the gap");
  r.panel("E", -11, 4, 16, 0, 5, "ride panel W (face east, 12 m): the dodge round the husks while the glove fills");
  r.panel("W", 11, 4, 16, 0, 5, "ride panel E (face west)");
  // the husks' spots at lz 15-16 (13 m from the door: spawn31's first-view rule, at least 12 m in the cone), the swarm's in the pen
  [[-3, 15], [0, 16], [3, 15], [-6, 23], [-2, 24], [2, 24], [6, 23], [-4, 26], [0, 26], [4, 26]].forEach(([x, z]) => r.nodeAt(x, z));
  r.row.waves = [["husk", "husk", "husk"], ["swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "swarmer"]];
  r.lesson({ kind: "husksDead", n: 3, prompt: "FIVE KILLS: {altFire} BLASTS THE PACK" });
  r.ceil(0, 3, 3, 3, "white", 9, 1.0); r.ceil(0, 13, 4, 3, "white", 9, 1.0); r.ceil(0, 19, 6, 2, "white", 9, 1.0); r.ceil(0, 24, 8, 4, "amber", 9, 1.1); // one accent per room: the pen
  r.pool(0, 26, 2.5, "cyan", 7, 1.6);
  r.walkSE([[0, 0, 10], [0, 0, 19], [0, 0, 24]]);
  r.frames([5, 0, 3], [0, 1, 19], [0, 0, 15], [0, 1, 28]);
}

// ---------------------------------------------------------------- 2.4 THE REACH (twist: the glove cannot reach a DRONE across the canal; the parry)
{
  const r = L.room({ id: "g4", name: "2.4 THE REACH", job: "twist", verb: "the glove cannot reach a flyer: punch its orb back at it", w: 24, l: 30, h: 10, par: TIMES.g4, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  // round 2: the channel 1 m deep (was 2: the walk bot wall-rode its sides and never climbed out, 5/5 seeds): a jump (jump.height 1.5) clears its lip
  // anywhere; the end steps stay for the walkers
  const C0 = 12, C1 = 18, FL = -1;
  r.pit(-12, C0, 12, C1, FL, `THE CANAL (6 m across, a dry channel ${-FL} m deep, its floor ice): the line between you and the drone; a fall costs a jump out, never health`);
  ice(r, -12, C0, 12, C1, FL - 0.5, FL + 0.5, "THE CANAL's frozen floor (the level's name; design.n30.glide): a fall skates");
  r.box("deck", -2, C0, 2, C1, FL, 0, "line", "the BRIDGE (4 m wide) over the canal: the one walk across, under the drone's orbs (the parry happens on it)");
  // a step out of the channel at both ends (0.5 m): the walkers' way up
  for (const [x0, x1, side] of [[-12, -9, "west"], [9, 12, "east"]])
    r.box("deck", x0, C0, x1, C0 + 3, FL, -0.5, "climb", `the canal's ${side} step (0.5 m under the floor): the walkers' way back up from a fall`);
  r.box("deck", -12, 24, -6, 30, 0, 1.5, "line", "the NW ledge (1.5 m): the far side's high ground under the drones (their orbs come from over it)");
  r.ramp(-9, 19, 24, 3, 1.5, "climb", "the NW ledge's ramp (walkers' route up, and yours)");
  r.nodeAt(0, 24); r.nodeAt(-3, 25); r.nodeAt(6, 25); r.nodeAt(3, 27); // clear of the NW ledge (lx -12..-6)
  r.row.waves = [["drone"], ["drone", "drone"]];
  r.lesson({ kind: "husksDead", n: 1, prompt: "{fire} AS THE YELLOW ORB ARRIVES" });
  r.ceil(0, 3, 3, 3, "white", 9, 1.0); r.ceil(0, 15, 4, 6, "white", 10, 1.1); r.ceil(0, 25, 8, 4, "amber", 10, 1.0);
  r.pool(0, 28, 2.5, "cyan", 7, 1.6);
  r.walkSE([[0, 0, 10], [0, 0, 20]]);
  r.walk([[-10.5, FL, 16.5], [-10.5, -0.5, 13.5], [-10.5, 0, 10]], { jump: true, maxSeconds: 8 });
  r.frames([4, 0, 3], [0, 4, 24], [0, 0, 20], [0, 1, 30]);
}

// ---------------------------------------------------------------- 2.5 THE COOLDOWN (twist: a blast spent early is not back for the second pack)
{
  const r = L.room({ id: "g5", name: "2.5 THE COOLDOWN", job: "twist", verb: `the blast's ${pulse.blastCooldownSeconds} s cooldown: punch the first pack, blast the second`, w: 26, l: 30, h: 9, par: TIMES.g5, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  r.box("deck", -13, 20, -8, 30, 0, 1.5, "line", "the WEST DECK (1.5 m): wave 2 pours off it at once, after the bait (the second pack the blast is for)");
  r.box("deck", 8, 20, 13, 30, 0, 1.5, "line", "the EAST DECK (1.5 m): wave 2's other side");
  r.ramp(-10.5, 15, 20, 3, 1.5, "climb", "the west deck's ramp: the second pack's way down to you (and yours up)");
  r.ramp(10.5, 15, 20, 3, 1.5, "climb", "the east deck's ramp");
  r.panel("E", -9, 4, 13, 0, 5, "ride panel W (face east, 9 m; round 3: moved from the middle, whose 5 m walls hid the decks from the door): the line from the door to the west deck's ramp");
  r.panel("W", 9, 4, 13, 0, 5, "ride panel E (face west): the line from the door to the east deck's ramp");
  [[-1, 14], [1, 14], [0, 15.5], [-1, 17], [1, 17]].forEach(([x, z]) => r.nodeAt(x, z));
  r.highNodeAt(-10.5, 25, 1.5); r.highNodeAt(-10.5, 28, 1.5); r.highNodeAt(10.5, 25, 1.5); r.highNodeAt(10.5, 28, 1.5);
  r.row.waves = [["swarmer", "swarmer", "swarmer", "swarmer", "swarmer"], ["swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "husk", "husk"]];
  r.lesson({ kind: "husksDead", n: 5, prompt: "SAVE {altFire} FOR THE SECOND PACK" });
  r.pickup("healthPickup", 10.5, 28, 2.4, "the east deck pays health after the second pack");
  r.ceil(0, 3, 3, 3, "white", 9, 1.0); r.ceil(0, 15, 4, 4, "amber", 9, 1.1); r.ceil(-10.5, 25, 4, 6, "white", 9, 1.0); r.ceil(10.5, 25, 4, 6, "white", 9, 1.0); // one accent: the bait
  r.pool(0, 28, 2.5, "cyan", 7, 1.6);
  r.walkSE([[0, 0, 8], [0, 0, 22]]);
  r.walk([[-10.5, 0, 13], [-10.5, 1.5, 21], [-10.5, 1.5, 26]]);
  r.frames([6, 0, 3], [0, 1, 15], [0, 0, 22], [0, 1, 30]);
}

// ---------------------------------------------------------------- 2.6 THE ARMOURY (reward: the KESTREL returns, iceline40.awards)
{
  const r = L.room({ id: "g6", name: "2.6 THE ARMOURY", job: "reward", verb: "the KESTREL returns: shoot the two husks", w: 14, l: 18, h: 6, par: TIMES.g6, spawnLz: 1.5, exitFromN: 1.5 });
  r.solid("plinth", 0, 5, 1.2, 0.6, 1.2, "reward", "the KESTREL's plinth, 3.5 m from the door on the axis: the gun is the first thing in the room (design.n40.iceline40.awards gives it on entry)");
  r.pickup("weaponPickup", 0, 5, 1.0, "the KESTREL on its plinth, in view from the door (the row's award gives it on entry anyway)");
  r.propsOut[r.propsOut.length - 1].weapon = "revolver";
  // the first KESTREL targets: two husks at the far end, 13 m from the door (spawn31's first-view rule: >= 12 m in the cone; levelgen: a lesson
  // needs enemies, so live husks, not stand-ins)
  r.nodeAt(-3, 14.5); r.nodeAt(3, 14.5);
  r.row.waves = [["husk", "husk"]];
  r.lesson({ kind: "husksDead", n: 2, prompt: "KESTREL BACK: {fire} THE FAR ONES" });
  r.ceil(0, 5, 3, 3, "cyan", 7, 1.2); r.ceil(0, 14, 6, 2, "white", 8, 1.0);
  r.pool(0, 16.5, 2, "cyan", 6, 1.4);
  r.walkSE([[2, 0, 5], [0, 0, 11]]);
  r.frames([4, 0, 2], [0, 1, 14], [0, 0, 10], [0, 1, 18]);
}

// ---------------------------------------------------------------- 2.7 THE MIX (combine: switching wins)
{
  const r = L.room({ id: "g7", name: "2.7 THE MIX", job: "combine", verb: "switch: the pistol for the far ones, the punch for the close and the big", w: 30, l: 32, h: 10, par: TIMES.g7, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  r.box("deck", -15, 24, -9, 32, 0, 2, "line", "the NW PERCH (2 m): the spitters' stand, 25 m from the door: the pistol's range, not the glove's");
  r.box("deck", 9, 24, 15, 32, 0, 2, "line", "the NE PERCH (2 m): the drone's side and wave 3's spitter");
  r.ramp(-12, 18, 24, 3, 2, "climb", "the NW perch's ramp: the spitters' way down and yours up (the punch reaches them only here)");
  r.ramp(12, 18, 24, 3, 2, "climb", "the NE perch's ramp");
  r.panel("E", -13, 4, 17, 0, 5, "ride panel W (face east, 13 m, by the west wall; round 3: moved from the middle, whose walls hid the perches from the door): ride north to the NW perch's ramp, out of the tank's reach");
  r.panel("W", 13, 4, 17, 0, 5, "ride panel E (face west, by the east wall): ride north to the NE perch's ramp");
  [[0, 14], [0, 18], [-2, 22], [2, 22], [-8, 12], [8, 12]].forEach(([x, z]) => r.nodeAt(x, z));
  r.highNodeAt(-12, 28, 2); r.highNodeAt(-12.5, 30.5, 2); r.highNodeAt(12, 28, 2); r.highNodeAt(12.5, 30.5, 2);
  // at most 2 kinds a wave (3 with a tank: the budget rule, design.n27.budget): the far + the big, the shield + the flyers, the big + the pack
  r.row.waves = [["tank", "spitter", "spitter"], ["warden", "drone", "drone"], ["tank", "swarmer", "swarmer", "swarmer", "swarmer", "swarmer", "swarmer"]];
  r.lesson({ kind: "husksDead", n: 3, prompt: "{quickSwap}: KESTREL FAR, GLOVE CLOSE" });
  r.pickup("healthPickup", 0, 20, 0.9, "the middle between the waves: a health pickup where the tanks walk");
  r.ceil(0, 3, 3, 3, "white", 10, 1.0); r.ceil(0, 14, 4, 4, "white", 10, 1.0); r.ceil(-12, 28, 5, 6, "amber", 10, 1.1); r.ceil(12, 28, 5, 6, "white", 10, 1.0); // one accent: the spitters' perch
  r.pool(0, 30, 2.5, "cyan", 8, 1.6);
  r.walkSE([[0, 0, 10], [0, 0, 24]]);
  r.walk([[-12, 0, 16], [-12, 2, 25], [-12, 2, 28]]);
  r.frames([8, 0, 3], [0, 1, 18], [0, 0, 22], [0, 1, 32]);
}

// ---------------------------------------------------------------- 2.8 THE REHEARSAL (rehearse: the CHARGER; the boss's rhythm in miniature)
{
  const r = L.room({ id: "g8", name: "2.8 THE REHEARSAL", job: "rehearse", verb: "dodge the charge into a pillar, punch its back (THE BRAWL's haymaker -> stuck -> punish)", w: 24, l: 30, h: 9, par: TIMES.g8, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  for (const [x, z, n] of [[-5, 10, "SW"], [5, 10, "SE"], [-5, 18, "NW"], [5, 18, "NE"]])
    r.solid("pillar", x, z, 2.5, 5, 2.5, "cover", `the ${n} PILLAR (2.5 x 2.5 x 5 m): step aside from the charge so it hits this and is stunned 1.5 s, its back open (Charger.cs)`);
  r.nodeAt(0, 22); r.nodeAt(-6, 25); r.nodeAt(6, 25);
  r.row.waves = [["charger"], ["charger", "husk", "husk"]];
  r.lesson({ kind: "husksDead", n: 1, prompt: "{dash} ASIDE, THEN {fire} ITS BACK" });
  r.ceil(0, 3, 3, 3, "white", 9, 1.0); r.ceil(0, 14, 4, 10, "white", 10, 1.0); r.ceil(0, 23, 6, 3, "amber", 9, 1.1);
  r.pool(0, 28, 2.5, "cyan", 7, 1.6);
  r.walkSE([[0, 0, 8], [0, 0, 22]]);
  r.frames([6, 0, 3], [0, 1, 22], [0, 0, 20], [0, 1, 30]);
}

// ---------------------------------------------------------------- 2.9 REST (a breath before THE BRAWL)
{
  const r = L.room({ id: "gr", name: "2.9 REST", kind: "link", job: "rest", verb: "none", w: 10, l: 10, h: 5, par: TIMES.gr, spawnLz: 1.5, exitFromN: 1.5 });
  r.pickup("healthPickup", 0, 5, 0.9, "the rest before THE BRAWL: full health");
  r.ceil(0, 5, 3, 3, "white", 8, 1.0);
  r.pool(0, 8.5, 2, "cyan", 6, 1.4);
  r.walkSE([]);
  r.frames([0, 0, 2], [0, 0.5, 6], [2, 0, 5], [0, 1, 10]);
}

// ---------------------------------------------------------------- THE BRAWL (x9: B2's boss room; reserved here empty)
{
  // B2's contract (Research/2026-10-14-wave40/B2.md): the sim builds the arena at brawl40.centre (a 36 x 36 floor, 1 m rim walls just outside
  // +-18 m, the entry gap x -2..2 on its +z wall); this room must leave the 38 x 38 m footprint free of solids and meet the gap with its door.
  // So the room is 38 x 38 (its own walls start outside the footprint), the centre 19 m in from the door: the door at centre.z + 19, the gap's
  // wall at +18..+19 (the 4 m door is the gap's 4 m), the spawn at centre.z + 16 (B2's), yaw 0.
  const W = 38, LEN = 38;
  const r = L.room({ id: "x9", name: "THE BRAWL", kind: "boss", job: "boss", verb: "the brawler: close in, punch, punish the stuck fist (B2's)", w: W, l: LEN, h: 14, par: 60, spawnLz: 3, exitFromN: LEN / 2 });
  r.row.boss = "brawl40";
  r.row.brawl40 = { centre: r.pt(0, 0, LEN / 2), size: [36, 36], _why: "NIGHT40 B2's arena (Research/2026-10-14-wave40/B2.md): the floor's centre (world); the sim builds THE BRAWL's 36 x 36 arena here (the lead's id x9); the room is 38 x 38 so nothing of its own is solid in the arena's footprint" };
  r.ceil(0, LEN / 2, 6, 6, "white", 16, 1.2);
  r.pool(0, LEN / 2, 3, "cyan", 8, 1.2);
  r.frames([0, 0, 3], [0, 1, LEN / 2], [10, 0, 10], [0, 1, LEN - 2]);
}

const plan = L.finish();
plan.wallHeight = src.wallHeight;
plan.surfaces = ICE;
plan.owns38 = ["jump", "double", "dash", "slide", "ride", "walljump"];
plan.owns38_why = "Builder E's gate checker (Design/levels/n38/GATES38.md), as n38-iceline: level 2 owns the movement verbs, the KESTREL and the PULSE gauntlet; the THUNDERCLAP boost is level 3's, the grapple level 4's. n40-iceline has no `gate` tag (no traversal gate: every crossing is a walk, a bridge or a ramp).";
fs.mkdirSync(path.join(ROOT, path.dirname(OUT)), { recursive: true });
fs.writeFileSync(path.join(ROOT, OUT), JSON.stringify(plan, null, 1) + "\n");
console.log("wrote", OUT, plan.rooms.length, "rooms [" + plan.rooms.map((q) => q.id).join(" ") + "],", plan.boxes.length, "boxes", plan.ramps.length, "ramps", plan.pits.length, "pits", plan.surfaces.length, "surfaces", plan.lights.length, "lights", plan.props.length, "props; floorSize", plan.floorSize);
const x9 = plan.rooms.find((q) => q.id === "x9");
console.log("THE BRAWL (x9): rect", JSON.stringify(x9.rect), "centre", JSON.stringify(x9.brawl40.centre), "spawn", JSON.stringify(x9.spawn.at));

/// The S/A/B/C rank cuts: the beat sheet's room times (TIMES, about 220 s) + THE BRAWL's bot band (B2's; 45 s, boss_rules 7's 30-60 s middle) x
/// [1.3, 2, 3, 4.5] as NIGHT36's / n38-iceline's; kills / style as n38-iceline's (NOT calibrated: no walk bot medians yet).
function RANK40() {
  const t = Object.values(TIMES).reduce((a, b) => a + b, 0) + 45;
  return { time: [1.3, 2, 3, 4.5].map((k) => Math.round(t * k / 5) * 5), kills: src.rank.kills, style: src.rank.style,
    _why: `Unity-only (builder L2, NIGHT40): S/A/B/C cuts: time = the beat sheet's room times (${t - 45} s) + THE BRAWL's bot band middle 45 s (boss_rules 7), x [1.3, 2, 3, 4.5] as NIGHT36's; kills and style as n38-iceline's (NOT calibrated: no walk bot medians yet).` };
}
