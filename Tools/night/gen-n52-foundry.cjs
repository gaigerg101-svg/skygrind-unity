// NIGHT52 builder F (level row foundry52: n48-foundry -> n52-foundry; NIGHT52.md section 2 THE ROOM CONTRACT; NIGHT52-BRIEF.md section 3).
// Level 1 as n52-foundry. Foundry's geometry is Design/levels/n46/foundry.plan.json (levelgen -> n46-foundry); n47-foundry and n48-foundry only
// add data to room a9 (gen-n47-foundry.cjs, gen-n48-foundry.cjs). So the chain is:
//   node Tools/night/gen-n52-foundry.cjs            the PLAN: n46's plan copied whole (Design/levels/n46/foundry.plan.json, untouched) to
//                                                   Design/levels/n52/foundry.plan.json, id n52-foundry, the rebuilt rooms' rows swapped in
//   dotnet run --project Tools/SimHarness -c Release -- levelgen Design/levels/n52/foundry.plan.json Design/levels/n52/foundry.json --walk
//   node Tools/night/gen-n52-foundry.cjs --finish   the LEVEL: a9's n47 and n48 keys re-applied exactly as gen-n47-foundry.cjs and
//                                                   gen-n48-foundry.cjs apply them, data.name "n46-foundry" (as n48-foundry has it), both copies
// The finish asserts: a9 bit-equal to n48-foundry's a9; every room not rebuilt bit-equal to n48-foundry's (its campaign row, its lights, props,
// pits, boxes); with no room rebuilt, the whole file equal to n48-foundry.json once its name and _why are n48's (the pipeline proof).
// n46 / n47 / n48 files and plans are never written (L-065 / L-077: the row off loads n48-foundry bit for bit).
// NIGHT53 K: a3's second pass is an in-place edit of the plan this file writes: after this file, run node Tools/night/n53-k-a3.cjs (it
// asserts a3 is rebuildA3's), then levelgen and --finish as above.
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN_IN = "Design/levels/n46/foundry.plan.json", PLAN_OUT = "Design/levels/n52/foundry.plan.json";
const GEN_OUT = "Design/levels/n52/foundry.json";
const N48 = "Design/levels/n48-foundry.json";
const OUTS = ["Design/levels/n52-foundry.json", "Assets/StreamingAssets/Design/levels/n52-foundry.json"];

// the rooms this file rebuilds (their plan rows come from ROOMS below); every other room is n46's plan / n48's level bit for bit
const REBUILT = ["a3", "a5", "b1"];
const ROOMS = { a3: rebuildA3, a5: rebuildA5, b1: rebuildB1 };

// ---- b1 1.6 THE YARD as THE SPILL, recast (card: docs/ROOMS.md "b1 1.6 THE YARD -> THE SPILL, recast"; NIGHT45's card kept: the tipped
// ladle, the slag river, two bridges, the crane rails beside them). Rect 36 x 40 -> 20 x 32 m (x -10..10, z -180..-148): the south side moves
// 8 m north (a5 takes it, see rebuildA5), d6 and b2 untouched. Room-local: lx = x, lz = metres north of the south (entry) wall z -148.
function rebuildB1(plan) {
  const Z = (lz) => -148 - lz;
  const room = plan.rooms.find((r) => r.id === "b1");
  if (!same(room.rect, [-18, -180, 18, -140]) || room.height !== 12) throw new Error("b1's rect / height moved upstream");
  const boxes = [
    piece({ zone: "b1", kind: "wall", min: [-10, 0, Z(17)], max: [-9.4, 12, Z(0)], job: "ride", ride: true, ride36: { face: "E", planeX: -9.4, lz: [0, 17] },
      why: "THE WEST CRANE RAIL (card: Verb it celebrates; NIGHT52 contract item 5, a ride wall >= 17 m; to the roof: at 7 m its 0.6 m top was a near-camp, 5 hits in hold52's 20 s, at 10 m a camp, 0 hits in K's round-4 sweep; nothing stands on it now): 17 m up the west wall from the entry corner over the slag's west end (round 4, D-16: no bridge there, the ride is that side's crossing; a 1 m curb under it is a single file a husk may take, right under you: pierce it, punch it into the slag); its jump-off east is the earned shortcut onto the ladle's rim" }),
    piece({ zone: "b1", kind: "wall", min: [9.4, 0, Z(17)], max: [10, 12, Z(0)], job: "ride", ride: true, ride36: { face: "W", planeX: 9.4, lz: [0, 17] },
      why: "THE EAST CRANE RAIL (card: Verb it celebrates): the east wall's 17 m rail over the east bridge, the same ride beside the other file (pierce it, punch it into the slag); its jump-off west reaches the rim" }),
    piece({ zone: "b1", kind: "deck", min: [6.4, -0.8, Z(17)], max: [9.4, 0, Z(11)], job: "lane",
      why: "THE EAST BRIDGE (card: Molecule node BE): the walkers' file comes over it in a line (pierce it from the east rail above), a punch off its west edge launches them into the slag; the health is on it" }),
    piece({ zone: "b1", kind: "deck", min: [-9.4, -0.8, Z(17)], max: [-6.4, 0, Z(11)], job: "lane",
      why: "THE WEST BRIDGE (card: Molecule node BW; round 6, the lead's midpoint review: with one bridge every walker filed through the east walkway): a 3 m crossing under the west rail (round 4's 1 m curb widened), so the files split over both bridges and each rail rides over one (pierce it, punch the front husk off its east edge into the slag)" }),
    piece({ zone: "b1", kind: "deck", min: [-3, 0, Z(22)], max: [3, 2.5, Z(20.5)], job: "perch",
      why: "THE LADLE's LIP (deck 2.5 m, card: Landmark, Hero prop; round 4, D-16: THE SPILL's own centrepiece): the tipped ladle's mouth over the river, the slag still pouring off it: the spitters' stand facing the entry across the slag, its front 3.5 m from the river; from the near side a rail's jump-off lands on it, from the strip under it a double jump" }),
    piece({ zone: "b1", kind: "hull", min: [-3.6, 0, Z(25)], max: [-3, 12, Z(24)], job: "cover",
      why: "THE LADLE's BAIL, west arm (card: Landmark): the ladle's lifting bail still standing up from its tipped body into the dark of the roof: the room's tall mark on the entry's line, framing the lip; in play a post beside the ladle's back to circle a husk round in the west lane (no crossbar: hold52 found its 9 m top a 0-hit perch a rail's jump-off might reach)" }),
    piece({ zone: "b1", kind: "hull", min: [3, 0, Z(25)], max: [3.6, 12, Z(24)], job: "cover",
      why: "THE LADLE's BAIL, east arm (card: Landmark): the bail's other arm to the roof, a post to circle in the east lane" }),
    piece({ zone: "b1", kind: "deck", min: [-10, 0, Z(32)], max: [-5.5, 4.5, Z(26.5)], job: "perch",
      why: "THE WEST CRANE CAB (deck 4.5 m, card: Molecule node CW): the crane's operator platform over the far bank, the high tier: a spitter on it covers both bridges; its pad and its ramp reach it, and it is where the rail rider climbs to next" }),
    piece({ zone: "b1", kind: "deck", min: [5.5, 0, Z(32)], max: [10, 4.5, Z(26.5)], job: "perch",
      why: "THE EAST CRANE CAB (deck 4.5 m, card: Molecule node CE): the other operator platform, the same high tier on the east (its pad, its ramp)" }),
    piece({ zone: "b1", kind: "hull", min: [-10, 4.5, Z(32)], max: [-8.5, 12, Z(30.5)], job: "cover",
      why: "the west cab's MAST (card: Place fantasy, the crane), cab to roof: a corner to circle on the cab (a pad landing is not a dead stand) and it breaks the cab's line along the north wall" }),
    piece({ zone: "b1", kind: "hull", min: [8.5, 4.5, Z(32)], max: [10, 12, Z(30.5)], job: "cover",
      why: "the east cab's MAST (card: Place fantasy), cab to roof: a corner to circle on the east cab" }),
    piece({ zone: "b1", kind: "hull", min: [4.4, 0, Z(8.1)], max: [5.6, 12, Z(6.9)], job: "cover",
      why: "the near bank's FURNACE POST (card: Every-solid rule; round 6, the lead's midpoint review: the west one cut, the mirror broken), floor to roof on the east side: breaks the rim spitters' line of sight onto the east rail's foot, so the east ride starts under cover (the west one starts in the open, over the new bridge); a post to circle a husk round when wave 3 comes from behind" }),
    piece({ zone: "b1", kind: "hull", min: [-6, 0, Z(2)], max: [-2, 12, Z(0)], job: "lane",
      why: "the yard gate's west CHEEK (card: Molecule node N; NIGHT52 contract item 4, the entrance gated), floor to roof, 2 m deep (round 4, D-16: at 4 m both rails ended in a pocket behind a cheek, each with a spawn node; now a south-bound rider jumps off into the arena): d5 opens into a short mouth, so the fight is never pulled back along the south wall to the door (round 2: the walk pilot kited the south wall through the door's band, dwell 3.7 s against P0's 2.6); you come out of the mouth onto the near bank's lane with the river and the ladle ahead" }),
    piece({ zone: "b1", kind: "hull", min: [2, 0, Z(2)], max: [6, 12, Z(0)], job: "lane",
      why: "the yard gate's east CHEEK (card: Molecule node N), floor to roof: the mouth's other side; the corners beside the cheeks are the rails' feet (the ride starts there) and wave 3's way in" }),
  ];
  const ramps = [
    piece({ zone: "b1", from: [0, Z(28)], to: [0, Z(22)], width: 6, rise: 2.5, job: "climb",
      why: "THE TIPPED LADLE's BODY (card: Hero prop; GG NIGHT20 'a route that enemies can climb up'): the ladle lying tipped on the far bank, its belly a 6 m slope from the ground up to its lip over the river: the walkers' way up to the lip (their file climbs it toward you: torch or pierce it) and your run down it" }),
    piece({ zone: "b1", from: [-8.5, Z(17.5)], to: [-8.5, Z(26.5)], width: 3, rise: 4.5, job: "climb",
      why: "the west cab's RAMP (card: Molecule; round 6, the lead's midpoint review: 3 m wide, was 1.5): 4.5 m over 9 m up the west wall from the bridge's north end, the walkers' way onto the cab (no safe ledge, RESEARCH A12) and the rail rider's walk up after the ride" }),
    piece({ zone: "b1", from: [8.5, Z(17.5)], to: [8.5, Z(26.5)], width: 3, rise: 4.5, job: "climb",
      why: "the east cab's RAMP (round 6: 3 m wide, was 1.5): the walkers' way onto the east cab" }),
  ];
  const pads = [
    piece({ zone: "b1", at: [-5.5, 0, Z(20)], target: [-7.75, 4.5, Z(29.5)], apexAbove: 1.5, job: "pad",
      why: "the west CAB PAD (card: Molecule; NIGHT52 contract item 5, the upward line): from the far bank's west lane up onto the west cab behind its spitter: the rail ride's next link (ride the bridge's file, land on the far bank, pad to the cab)" }),
    piece({ zone: "b1", at: [5.5, 0, Z(20)], target: [7.75, 4.5, Z(29.5)], apexAbove: 1.5, job: "pad",
      why: "the east CAB PAD (card: Molecule): the east lane's pad onto the east cab" }),
  ];
  const pits = [
    piece({ zone: "b1", rect: [-9.4, Z(17), 9.4, Z(11)], floorY: -0.8, lava: true, job: "hazard",
      why: "THE SPILL (card: Hazard both ways): the slag river across the yard rail to rail, 6 m, under the two 3 m bridges (round 6: the west bridge back, the walkers split over both) (a running jump falls short: a dash or a double jump crosses it; the walkers take the bridges); to you a burn and a knock-up out to the nearest bank; to a husk death (Lava30): punch or launch it off a bridge into it. Every edge is a bank or a bridge, never a wall (the walk pilot's way out of lava is the nearest edge)" }),
  ];
  const lights = [
    { room: "b1", normal: [1, 0, 0], id: "b1-RW", kind: "wall", at: [-9.35, 1.2, Z(8.5)], size: [17, 0.2, 0.1], color: "cyan", range: 6, intensity: 1.1 },
    { room: "b1", normal: [-1, 0, 0], id: "b1-RE", kind: "wall", at: [9.35, 1.2, Z(8.5)], size: [17, 0.2, 0.1], color: "cyan", range: 6, intensity: 1.1 },
    { room: "b1", normal: [0, -1, 0], id: "b1-L1", kind: "ceiling", at: [0, 11.95, Z(5)], size: [4, 3, 0.1], color: "white", range: 11, intensity: 0.8 },
    { room: "b1", normal: [0, -1, 0], id: "b1-lamp", kind: "ceiling", at: [0, 11.95, Z(19)], size: [3, 3, 0.1], color: "amber", range: 13, intensity: 1.5 },
    { room: "b1", normal: [0, -1, 0], id: "b1-L2", kind: "ceiling", at: [0, 11.95, Z(27)], size: [5, 3, 0.1], color: "white", range: 12, intensity: 1.25 },
    { room: "b1", normal: [0, 1, 0], id: "b1-L8", kind: "pool", at: [0, 0.025, Z(30)], size: [2.5, 2.5, 0.05], color: "cyan", range: 8, intensity: 1.6 },
  ];
  const props = [
    { id: "b1-P1", room: "b1", kind: "healthPickup", at: [7.9, 0.9, Z(14)], size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true, job: "reward",
      why: "the riskiest floor spot pays (A20; round 4, D-16: off the rim, a camp's top): the middle of the bridge over the slag, in the file's path and the rim's and the cabs' fire" },
    { id: "b1-hero", room: "b1", kind: "hero", at: [0, 1.25, Z(20.35)], size: [5, 2.2, 0.3], yawDeg: 0, collider: false, decor: true, job: "hero",
      why: "THE SPILL's hero prop (card: Hero prop): the tipped ladle's cast mouth over the river (round 4: on the lip's face), the one object that says what happened here (the ladle spilled the river)" },
    { id: "b1-lip", room: "b1", kind: "lavaChannel", at: [0, 2.35, Z(20.4)], size: [5.2, 0.6, 0.2], yawDeg: 0, collider: false, decor: true, job: "landmark",
      why: "the ladle's LIP still pouring slag-orange into the river (card: Landmark; round 4: at the tipped ladle's mouth over the river): the only hot glow at eye level from the entry, the river's source (danger colour: the slag)" },
  ];
  const mineZ = (o) => o.zone === "b1";
  const mineR = (o) => o.room === "b1";
  swapRows(plan.boxes, mineZ, boxes);
  swapRows(plan.ramps, mineZ, ramps);
  swapRows(plan.pads, mineZ, pads);
  swapRows(plan.pits, mineZ, pits);
  swapRows(plan.lights, mineR, lights);
  swapRows(plan.props, mineR, props);

  const F1 = [-7.2, 0, Z(19)], F2 = [7.2, 0, Z(19)], F3 = [-5, 0, Z(25)], F4 = [5, 0, Z(25)];
  const H1 = [-2, 2.5, Z(21.5)], H2 = [2, 2.5, Z(21.5)], H3 = [-7.5, 4.5, Z(28.5)], H4 = [7.5, 4.5, Z(28.5)];
  const N1 = [-8, 0, Z(3)], N2 = [8, 0, Z(3)], N3 = [-8, 0, Z(8.5)], N4 = [8, 0, Z(8.5)];
  const row = {
    id: "b1", name: "1.6 THE YARD", kind: "room", rect: [-10, Z(32), 10, Z(0)], height: room.height, roof: room.roof, par: room.par,
    job36: "combine",
    verb36: "the ride beside the bridge's file: ride a crane rail over a bridge and pierce the file walking at you, punch it into the slag; the rail's jump-off to the ladle's rim; the cab pads",
    // the respawn point 5.5 m in from d5, just past the gate's mouth (as a5's: n46flow's walk pilot returns to the room's spawn when no enemy is in sight)
    spawn: { at: [0, 0, Z(5.5)], yawDeg: 0 }, exit: [0, 0, Z(30)], arena36: room.arena36,
    waves: [["husk", "husk", "husk"], ["spitter", "spitter", "husk", "husk"], ["husk", "husk", "husk", "husk"]],
    proofs: [
      { kind: "walk", points: [[0, 0, Z(5.5)], [-7.9, 0, Z(9)], [-7.9, 0, Z(16.8)], [-6, 0, Z(18.5)], [-5.5, 0, Z(24)], [0, 0, Z(29)], [4.5, 0, Z(29)], [5.5, 0, Z(24)], [6, 0, Z(18.5)], [7.9, 0, Z(16.8)], [7.9, 0, Z(9)], [0, 0, Z(5.5)]] },
      { kind: "walk", points: [[-7.9, 0, Z(9)], [-7.9, 0, Z(10.5)], [-7.9, 0, Z(16.8)]] },
      { kind: "walk", points: [[0, 0, Z(30)], [0, 0, Z(28.4)], [0, 1.25, Z(25)], [0, 2.45, Z(22.1)], [0, 2.5, Z(21.2)]] },
      { kind: "walk", points: [[-8.5, 0, Z(17.2)], [-8.5, 0, Z(17.6)], [-8.5, 4.5, Z(26.8)], [-7.5, 4.5, Z(28.5)]] },
      { kind: "walk", points: [[0, 0, Z(29)], [0, 0, Z(30)]] },
    ],
    frames36: { mid: [-7.9, 0, Z(9), -20, 0.08], exit: [0, 0.5, Z(28), 0, -0.181] },
    frames52: {
      _why: "NIGHT52 F: reel a452's spots (feet x, y, z, yaw deg, pitch rad): entry = just inside d5 looking across the slag at the ladle, the rails both sides; mid = the west bank at the bridge's foot looking up the bridge and the rail at the rim and the west cab; high = on the east cab looking south-west over the river",
      entry: [0, 0, Z(1.5), 0, 0.1],
      mid: [-7.9, 0, Z(9), -20, 0.12],
      high: [7.75, 4.5, Z(29.5), 150, -0.25],
    },
    nodes: [F1, F2, F3, F4, N1, N2, N3, N4],
    highNodes: [H1, H2, H3, H4],
    waveNodes43: [[F1, F2, F3, F4], [H1, H2, H3, H4, F3, F4], [N1, N2, N3, N4]],
    waveNodes43_why: "NIGHT52 F (NIGHT45's card: The 30 s problem, State change): wave 1 three husks on the far bank at the bridges' north ends: they file over the bridges at you (ride beside a file: pierce, punch into the slag); wave 2 the spitters take the rim and the cabs with two husks from the far bank; wave 3 comes from the near bank's corners, the side you came in, so the bridges point the other way",
    bridges45: [[-9.4, Z(17), -6.4, Z(11)], [6.4, Z(17), 9.4, Z(11)]],
    bridges45_why: "NIGHT52 F: the two bridges (round 4 made the west one river, round 6 put it back as a 3 m bridge; read by Harness.N45.Yard.cs only)",
    card52_why: "NIGHT52 F round 2 (row foundry52; docs/ROOMS.md 'b1 1.6 THE YARD -> THE SPILL, recast'): rebuilt to THE ROOM CONTRACT at 20 x 32 m from NIGHT45's card, recast: the 12 m river -> 6 m, the 18 m crane rails onto the side walls over the bridges, the ladle as THE TIPPED LADLE (round 4: its body a 6 m ramp to its 2.5 m lip over the river, its bail's two arms to the roof), two crane cabs at 4.5 m (the high tier, each with a ramp and a pad), two furnace posts on the near bank, the gate's two cheeks round d5 (a 4 m mouth)",
  };
  plan.rooms[plan.rooms.indexOf(room)] = row;
}

// Replace every row of `arr` that `isMine` matches with `rows`, at the index of the first one removed (the plan keeps its room order).
function swapRows(arr, isMine, rows) {
  const at = arr.findIndex(isMine);
  const kept = arr.filter((o) => !isMine(o));
  kept.splice(at < 0 ? kept.length : at, 0, ...rows);
  arr.length = 0;
  arr.push(...kept);
}
const piece = (o) => Object.assign(o, { note: o.why }); // the plan's convention: note = why

// ---- a5 1.5 CRUCIBLE as THE POURING FLOOR (card: docs/ROOMS.md "NIGHT52 F: a5"). Room-local: lx = x (east), lz = metres north of the
// south (entry) wall z -112, so z = -112 - lz. The rect, the doors d4 / d5, the spawn and the exit are n46's (20 x 24 m, 10 m roof).
// Every piece's why names the card field that asks for it and the verb it serves (A-1, A-9; NIGHT52.md section 2 item 2).
function rebuildA5(plan) {
  const Z = (lz) => -112 - lz;
  const room = plan.rooms.find((r) => r.id === "a5");
  if (!same(room.rect, [-10, -136, 10, -112]) || room.height !== 10) throw new Error("a5's rect / height moved upstream");
  // ROUND 2 (b1 to THE ROOM CONTRACT's 32 m): b1 shrinks from its south side (z -140 -> -148) and a5 takes the 8 m (its north wall z -136 ->
  // -144; d5 moves with it, still 4 x 4). Why a5 and not b2 (the link north of b1): Walk43Foundry.cs walks b2's gates by room-relative lz from
  // b2's south edge, so moving that edge would move every gate under the pilot. a5's gantry and skim step run on north to the new wall as
  // galleries, each with a second walkers' ramp from the exit trench, and a pad joins them back across it.
  const NORTH = 32; // a5's new north wall, lz (z -144)
  room.rect = [-10, Z(NORTH), 10, -112];
  const d5 = plan.doors.find((d) => d.id === "d5");
  if (!same(d5.rect, [-2, -140, 2, -136])) throw new Error("d5 moved upstream");
  d5.rect = [-2, Z(NORTH + 4), 2, Z(NORTH)];
  const boxes = [
    piece({ zone: "a5", kind: "hull", min: [-1.5, -0.8, Z(11.5)], max: [-1, 10, Z(8.5)], job: "circuit",
      why: "THE CRUCIBLE's west jamb (card: Landmark, Molecule's centre): basin floor to roof, the mass the circuit loops round (A-2: run the loop, never stop); its 0.5 m jamb frames the 2 m TAP HOLE (round 4: the crucible 3 m wide, was 4, so wave 1 shows past it from the door; round 6: the hole a clear 3.2 m high pass-through on the door's line, no slide ceiling)" }),
    piece({ zone: "a5", kind: "hull", min: [1, -0.8, Z(11.5)], max: [1.5, 10, Z(8.5)], job: "circuit",
      why: "THE CRUCIBLE's east jamb (card: Landmark): the loop's centre mass from the east lane; it breaks the skim step's line of sight down the east lane, so a husk file chasing you round the basin comes round the corner at punch range" }),
    piece({ zone: "a5", kind: "hull", min: [-1, 3.2, Z(11.5)], max: [1, 10, Z(8.5)], job: "lane",
      why: "THE CRUCIBLE's body over the TAP HOLE (card: Hero prop, Landmark; round 6, the lead's midpoint review: the 1.19 m slide ceiling under it cut, so the tap hole is a clear 2 x 3.2 m pass-through and the tapping walk a straight shot north from the door through the crucible; the column stays the circuit's centre): the slab stack from 3.2 m to the roof over the hole, the cast face and the spout on it" }),
    piece({ zone: "a5", kind: "deck", min: [-1, -0.8, Z(13.5)], max: [1, 0, Z(6.5)], job: "lane",
      why: "THE TAPPING WALK (card: Molecule, the shortcut): a 2 m floor across the basin's lava from the apron through the tap hole to the north floor: the straight line north from the door through the crucible (round 6: a walk, was a slide; a husk file on it is a pierce line), and where you stand to punch a husk into the basin either side" }),
    piece({ zone: "a5", kind: "deck", min: [-10, 0, Z(NORTH)], max: [-4, 5, Z(16)], job: "perch",
      why: "THE CHARGING GANTRY (deck 5 m, card: Molecule node G, the 30 s problem): a 16 m gallery along the west wall, the spitters' perch over the floor; the pads land you on it behind them (punch them off: a 5 m drop); husks reach it up two ramps, one from each end (no safe ledge, RESEARCH A12)" }),
    piece({ zone: "a5", kind: "hull", min: [-10, 5, Z(24)], max: [-8, 10, Z(22)], job: "cover",
      why: "the crane's MAST on the gantry (card: Place fantasy, the overhead crane), gantry to roof: breaks the line of sight across the gantry, so a pad landing has a corner to circle and a spitter on the gantry's back must step out to lob" }),
    piece({ zone: "a5", kind: "deck", min: [4, 0, Z(NORTH)], max: [10, 2.5, Z(19)], job: "perch",
      why: "THE SKIM STEP (deck 2.5 m, card: Molecule node S): a 13 m gallery along the east wall, the RIDE's landing at the crane rail's north end (ride up, release onto it), a double jump from the exit trench, two short ramps for the walkers; the pad on it throws you across to the gantry" }),
    piece({ zone: "a5", kind: "hull", min: [4, 2.5, Z(24)], max: [5.5, 10, Z(22.5)], job: "cover",
      why: "THE LADLE hung over the skim step (card: Every-solid rule), step to roof: it breaks the gantry spitters' line of sight onto the step's pad, so a rider who lands has one beat of cover before the pad launches him" }),
    piece({ zone: "a5", kind: "wall", min: [9.4, 0, Z(19)], max: [10, 7, Z(1)], job: "ride", ride: true, ride36: { face: "W", planeX: 9.4, lz: [1, 19] },
      why: "THE CRANE RAIL (card: Verb it celebrates; NIGHT52 contract item 5, a ride wall >= 17 m): 18 m along the east wall from the entry corner to the skim step; you ride it straight at the gantry's spitters and punch their globs back from the wall (the parry from a ride), above the husks on the east lane, then pitch up and land on the step" }),
    piece({ zone: "a5", kind: "hull", min: [-6, 0, Z(2)], max: [-2, 10, Z(0)], job: "lane",
      why: "the hearth's west CHEEK (card: Molecule node E; NIGHT52 contract item 4, the entrance gated), floor to roof, 2 m deep (round 4, D-16: at 4 m they left a 2.5 m strip to the lava and the ride ended in a pocket behind the east cheek; measured, 2 m deep out to the side walls and 3 m deep did no better for the dwell): d4 opens into a short mouth facing the tap hole, so the fight is never pulled back along the south wall to the door (round 2: the walk pilot kited the south wall through the door's band, dwell 4.4 s against P0's 3.35); the corner beyond it is the gantry ramp's foot" }),
    piece({ zone: "a5", kind: "hull", min: [2, 0, Z(2)], max: [6, 10, Z(0)], job: "lane",
      why: "the hearth's east CHEEK (card: Molecule node E), floor to roof: the mouth's other side; the corner beyond it is the crane rail's foot, where the ride starts" }),
  ];
  const ramps = [
    piece({ zone: "a5", from: [-8.5, Z(6)], to: [-8.5, Z(16)], width: 3, rise: 5, job: "climb",
      why: "the GANTRY RAMP (card: Molecule, the walkers' way up; GG NIGHT20 'a route that enemies can climb up'): 5 m over 10 m along the west wall, the husks' file up to the gantry (a pierce line from the floor: charge the KESTREL down the ramp) and your walk up" }),
    piece({ zone: "a5", from: [8.15, Z(13)], to: [8.15, Z(19)], width: 2.5, rise: 2.5, job: "climb",
      why: "the SKIM RAMP (card: Molecule): 2.5 m over 6 m under the crane rail's north end, the walkers' way onto the skim step; a low ride lands on it and walks up" }),
    piece({ zone: "a5", from: [-3, Z(22)], to: [-3, Z(NORTH)], width: 2, rise: 5, job: "climb",
      why: "the GANTRY'S BACK RAMP (card: Molecule; round 2, the gallery's north end): 5 m over 10 m up the exit trench's west side, the second walkers' way onto the gantry, so its husks come at you from both ends of the gallery (no single choke to hold) and a file climbing it is a pierce line down the trench" }),
    piece({ zone: "a5", from: [3, Z(26)], to: [3, Z(NORTH)], width: 2, rise: 2.5, job: "climb",
      why: "the SKIM STEP'S BACK RAMP (card: Molecule; round 2): 2.5 m over 6 m up the exit trench's east side onto the step's north end, the second walkers' way onto the step (husks flank you on it from the trench) and your walk up from the exit side" }),
  ];
  const pads = [
    piece({ zone: "a5", at: [-1.5, 0, Z(15)], target: [-7, 5, Z(20.5)], apexAbove: 2, job: "pad",
      why: "the GANTRY PAD (card: Molecule; NIGHT52 contract item 5, the upward line: the game's first pad in a campaign room): on the north floor beside the tap hole's mouth, up onto the gantry behind its spitters (7.8 m, apex 2 m over the deck: over the gantry's east edge at 6.2 m); a slide out of the tap hole that veers left is thrown straight up to them" }),
    piece({ zone: "a5", at: [8, 2.5, Z(21.5)], target: [-6.5, 5, Z(20)], apexAbove: 1.5, job: "pad",
      why: "the SKIM PAD (card: the 30 s problem's last link): the ride lands you on the skim step, this pad throws you 14.5 m over the exit trench onto the gantry: ride, land, launch, punch the spitters off their perch" }),
    piece({ zone: "a5", at: [-7, 5, Z(29)], target: [7, 2.5, Z(29)], apexAbove: 1.5, job: "pad",
      why: "the RETURN PAD (card: Molecule; round 2): from the gantry gallery's north end 14 m back over the exit trench onto the skim step, so the high side is a loop (rail -> step -> skim pad -> gantry -> return pad -> step) and the gantry is never a dead end to hold" }),
  ];
  const pits = [
    piece({ zone: "a5", rect: [-4, Z(13.5), 4, Z(6.5)], floorY: -0.8, lava: true, job: "hazard",
      why: "THE POUR BASIN (card: Hazard both ways): the crucible's spill, a 2 m molten ring round it (8 x 7 m): to the player a burn and a knock-up out to the nearest rim (every edge of it is a walkable floor: no wall to be pinned on); to an enemy death (Lava30): the loop runs round its rim, so every husk chasing you round it is one punch (launch) from the basin" }),
  ];
  const tunnels = []; // round 6 (the lead's midpoint review): the tap hole's 1.19 m slide ceiling cut (a clear 3.2 m pass-through)
  const lights = [
    { id: "a5-L1", room: "a5", kind: "ceiling", at: [0, 9.95, Z(4)], normal: [0, -1, 0], size: [4, 3, 0.1], color: "white", range: 9, intensity: 0.9 },
    { id: "a5-L2", room: "a5", kind: "ceiling", at: [0, 9.95, Z(16)], normal: [0, -1, 0], size: [4, 3, 0.1], color: "white", range: 9, intensity: 1 },
    { id: "a5-L3", room: "a5", kind: "ceiling", at: [-6.5, 9.95, Z(19)], normal: [0, -1, 0], size: [3, 3, 0.1], color: "white", range: 10, intensity: 1.3 },
    { id: "a5-L4", room: "a5", kind: "pool", at: [-3, -0.775, Z(10)], normal: [0, 1, 0], size: [1.6, 6, 0.05], color: "orange", range: 9, intensity: 1.3, danger: true },
    { id: "a5-L5", room: "a5", kind: "pool", at: [0, 0.025, Z(NORTH - 1.5)], normal: [0, 1, 0], size: [2.5, 2.5, 0.05], color: "cyan", range: 7, intensity: 1.6 },
    { id: "a5-L6", room: "a5", kind: "ceiling", at: [0, 9.95, Z(26)], normal: [0, -1, 0], size: [3, 4, 0.1], color: "white", range: 9, intensity: 0.9 },
    { id: "a5-RW", room: "a5", kind: "wall", at: [9.35, 1.2, Z(10)], normal: [-1, 0, 0], size: [18, 0.2, 0.1], color: "cyan", range: 6, intensity: 1.1 },
    { id: "a5-TH", room: "a5", kind: "wall", at: [0, 3.35, Z(8.45)], normal: [0, 0, 1], size: [2, 0.15, 0.1], color: "cyan", range: 5, intensity: 1.2 },
  ];
  const props = [
    { id: "a5-P1", room: "a5", kind: "healthPickup", at: [0, 0.9, Z(7.4)], size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true, job: "reward",
      why: "the riskiest spot pays (A20; round 4, D-16: off the gantry's corner, a perch): the tapping walk's south bridge between the basin's lava, at the tap hole's mouth: you stand in both lanes' fire to take it (the north bridge measured worse: the walk pilot went for it through the slot and stood 30 s at the lintel)" },
    { id: "a5-hero", room: "a5", kind: "hero", at: [0, 6, Z(8.35)], size: [2.8, 3, 0.3], yawDeg: 0, collider: false, decor: true, job: "hero",
      why: "THE CRUCIBLE's cast face over the tap hole (card: Hero prop), seen from the door: says what this floor is for (the iron is tapped here)" },
    { id: "a5-lip", room: "a5", kind: "lavaChannel", at: [0, 4.6, Z(8.4)], size: [1.8, 0.6, 0.2], yawDeg: 0, collider: false, decor: true, job: "landmark",
      why: "the tap hole's glowing spout (card: Landmark): the one hot glow on the door's line (danger colour: the iron), lighting the pass-through under it" },
  ];
  const mineZ = (o) => o.zone === "a5";
  const mineR = (o) => o.room === "a5";
  swapRows(plan.boxes, mineZ, boxes);
  swapRows(plan.ramps, mineZ, ramps);
  swapRows(plan.pads, mineZ, pads);
  swapRows(plan.pits, mineZ, pits);
  swapRows(plan.tunnels, mineZ, tunnels);
  swapRows(plan.lights, mineR, lights);
  swapRows(plan.props, mineR, props);

  // the room row: the rect, doors, spawn, exit, job and arena36 kept; the encounter and the proofs rebuilt
  const F1 = [1.5, 0, Z(17)], F2 = [-3, 0, Z(17.5)], F3 = [5.5, 0, Z(16)], F4 = [-2.2, 0, Z(18.5)], F5 = [0, 0, Z(27)];
  // the respawn point 5 m in from the door (n46's: 2 m, z -114): past the 4 m dwell band. Why (logged, not hidden): n46flow's walk pilot
  // walks back to the ROOM'S SPAWN whenever no enemy is in its sight (Harness.N46.Flow.cs GoalProvider: a fight room's goal is its spawn), so
  // with the crucible hiding the north floor the meter's dwell counted the pilot standing at its own goal (round 1: 4.6-5.2 s with the spawn
  // at z -114, P0 3.3 s). A player walks in through d4 and respawns here after a death; the nodes keep spawn31's 12 m in the view cone from it.
  const SPAWN = { at: [0, 0, Z(5)], yawDeg: 0 };
  // the perches' nodes stand at their front edges: a spitter there is in line of sight from the floor (shoot it, or go up and punch it off);
  // round 1's first try had H2 at the gantry's back (-7, lz 21), hidden from the floor by the gantry's edge: the walk pilot died 8 times (P0 4)
  const H1 = [-6.5, 5, Z(16.8)], H2 = [-4.8, 5, Z(17.5)], H3 = [7, 2.5, Z(20)];
  // round 4 (D-16): wave 1 on the sight lines past the crucible from the spawn (each >= 12 m: spawn31): the gantry's front-west, the step's front, both lanes' north ends
  const W1 = [[-8, 5, Z(17.5)], [8.6, 2.5, Z(19.3)], [-6.2, 0, Z(15.4)], F3];
  const row = {
    id: "a5", name: "1.5 CRUCIBLE", kind: "room", rect: room.rect, height: room.height, roof: room.roof, par: room.par,
    job36: "combine",
    verb36: "the parry from a ride: ride the crane rail at the gantry's spitters and punch their globs back (+ the slide through the tap hole, the pads, the launch into the basin)",
    spawn: SPAWN, exit: [0, 0, Z(NORTH - 1.5)], arena36: room.arena36,
    waves: [["spitter", "husk", "husk"], ["husk", "husk"], ["spitter", "spitter", "husk"]],
    proofs: [
      { kind: "walk", points: [[0, 0, -114], [0, 0, Z(4.6)], [-5.5, 0, Z(5.2)], [-5.5, 0, Z(10)], [-5, 0, Z(15)], [2, 0, Z(16)], [5.5, 0, Z(14.5)], [6, 0, Z(10)], [5, 0, Z(5.2)], [0, 0, Z(4.6)], [0, 0, -114]] },
      { kind: "walk", points: [[0, 0, -114], [0, 0, Z(4.6)], [-8.5, 0, Z(5)], [-8.5, 2.5, Z(11)], [-8.5, 5, Z(16.5)], [-6, 5, Z(19)]] },
      { kind: "walk", points: [[5.5, 0, Z(12)], [8.15, 0, Z(12.5)], [8.15, 2.5, Z(19.5)], [7, 2.5, Z(22)]] },
      { kind: "walk", points: [[5.5, 0, Z(14)], [0, 0, Z(17.5)], [0, 0, Z(23)], [0, 0, Z(NORTH - 1.5)]] },
      { kind: "walk", points: [[0, 0, Z(20.5)], [-3, 0, Z(21.3)], [-3, 2.5, Z(27)], [-3, 4.8, Z(31.6)], [-5.5, 5, Z(31.2)]] },
      { kind: "walk", points: [[0, 0, Z(24.5)], [3, 0, Z(25.3)], [3, 2.45, Z(31.6)], [5.5, 2.5, Z(31.2)]] },
    ],
    frames36: { mid: [-9, 0, -113, -35.218, -0.019], exit: [3, 0, Z(17), 41.82, -0.086] },
    frames52: {
      _why: "NIGHT52 F: reel a452's spots (feet x, y, z, yaw deg, pitch rad): entry = just inside d4 looking at the crucible's tap hole, the gantry upper left, the pour wall right; mid = the east lane's south end looking up the pour wall at the gantry; high = on the gantry looking south-east over the floor",
      entry: [0, 0, -114.5, 0, 0.08],
      mid: [7.8, 0, Z(5), 22, 0.15], // round 2: out of the east cheek (x 2..6, lz 0..4), at the crane rail's foot
      high: [-7, 5, Z(19), -140, -0.25],
    },
    nodes: [F1, F2, F3, F4, F5, W1[2]],
    highNodes: [H1, H2, H3, W1[0], W1[1]],
    waveNodes43: [W1, [F3, F4, F5, F1], [H1, H2, H3]],
    waveNodes43_why: "NIGHT52 F (card: The 30 s problem, State change): wave 1 shows the problem at once (round 4, D-16: its four spots on the sight lines past the crucible from the door, so it is in view: the spitter on a perch half the time; its husks behind the crucible come round the basin both ways: the loop, punch them in); wave 2 (overlap46: as wave 1 thins) two husks from the crucible's north corners; wave 3 everything from the perches: both spitters on the gantry and the step, their husk down a ramp in a file (pierce it). Round 1 measured W1 = husks only behind the crucible: the pilot waited at its spawn (dwell 5.2 s)",
    card52_why: "NIGHT52 F (row foundry52; docs/ROOMS.md 'NIGHT52 F: a5 THE POURING FLOOR'): rebuilt to THE ROOM CONTRACT (NIGHT52.md section 2) from its identity card; n46's lava pool, its two 10.5 m panels, the 1.2 m deck and the rim ramps are cut (the card's Cut list)",
  };
  plan.rooms[plan.rooms.indexOf(room)] = row;
}

// ---- a3 1.3 RIDE as THE CASTING PIT (round 5, the room loop; card: docs/ROOMS.md "a3 1.3 RIDE -> THE CASTING PIT"). Room-local: lx = x
// (east), lz = metres north of the south (entry) wall z -59, so z = -59 - lz. The doors d2 / d3 (x -2..2) and the room's length (26 m) are
// n46's; the rect grows 4 m wider (x -8..8 -> -10..10: nothing stands there) and the roof 9 -> 10 m (THE ROOM CONTRACT's band). The lesson
// (the ride, "HOLD {jump} ON A WALL: RIDE", reached on the far floor) and the zoom teach (l1.zoom, riding the runner over the dry floor) are
// kept: the room is still where the ride is learnt, over a floor first and then over the lava, now on one 18 m wall under fire.
function rebuildA3(plan) {
  const Z = (lz) => -59 - lz;
  const room = plan.rooms.find((r) => r.id === "a3");
  if (!same(room.rect, [-8, -85, 8, -59]) || room.height !== 9) throw new Error("a3's rect / height moved upstream");
  room.rect = [-10, Z(26), 10, Z(0)];
  const zoom = plan.teach35.find((t) => t.id === "l1.zoom");
  if (!zoom || !same(zoom.rect, [-5, -71, -2, -62])) throw new Error("l1.zoom moved upstream");
  zoom.rect = [-3.6, Z(13), -1, Z(4)]; // both faces of the runner over the dry floor (it was P1's east face, x -5..-2)
  const boxes = [
    piece({ zone: "a3", kind: "hull", min: [-5, 0, Z(2)], max: [-2, 10, Z(0)], job: "lane",
      why: "the pit's west CHEEK (card: Molecule node E; NIGHT52 contract item 4, the entrance gated), floor to roof, 2 m deep, 3 m wide (a 2 m way past it to the shelf ramp's foot): d2 opens into a 4 m mouth facing the runner, so the fight is never pulled back along the south wall to the door; the corner beyond it is the runner's south end, where the ride starts" }),
    piece({ zone: "a3", kind: "hull", min: [2, 0, Z(2)], max: [5, 10, Z(0)], job: "lane",
      why: "the pit's east CHEEK (card: Molecule node E), floor to roof: the mouth's other side; the corner beyond it is the hoist pad's floor and the east lane's way north" }),
    piece({ zone: "a3", kind: "wall", min: [-2.6, -0.8, Z(22)], max: [-2, 10, Z(4)], job: "ride", ride: true, ride36: { face: "E", planeX: -2, lz: [4, 22] },
      why: "THE RUNNER (card: Verb it celebrates, Landmark; NIGHT52 contract item 5, a ride wall >= 17 m): the casting runner wall, 18 m from the entry floor across the pit to the far floor, floor to roof; ride either face (hold jump on a wall): the first 8 m over the dry floor (a fall costs nothing: the zoom teach is here), the next 9 over the lava, off onto the far floor where the husks are; its east face is on the door's west jamb line, so the door looks straight past it to the exit" }),
    piece({ zone: "a3", kind: "deck", min: [-10, 0, Z(26)], max: [-7, 2.5, Z(8)], job: "perch",
      why: "THE MOULD SHELF (deck 2.5 m, card: Molecule node G): an 18 m gallery along the west wall above the west lane, the swarmers' run along the pit; reached up its south ramp and its back ramp (no single choke), the return pad on its north end; a rider on the runner's west face is level with it" }),
    piece({ zone: "a3", kind: "hull", min: [-10, 2.5, Z(16.5)], max: [-9, 10, Z(15.5)], job: "cover",
      why: "a shelf STANCHION (card: Every-solid rule), shelf to roof: a corner on the gallery to circle (the swarmers' run along it breaks round it; the line of sight down the shelf is cut)" }),
    piece({ zone: "a3", kind: "hull", min: [-10, 2.5, Z(21.5)], max: [-9, 10, Z(20.5)], job: "cover",
      why: "the second shelf STANCHION (card: Every-solid rule): with the first it makes the shelf three short runs, so a body on it is never in a 20 m line from the far floor" }),
    piece({ zone: "a3", kind: "deck", min: [6.5, 0, Z(26)], max: [10, 5, Z(22.5)], job: "perch",
      why: "THE LADLE PERCH (deck 5 m, card: Molecule node L, the high tier): the crane's ladle cradle in the far east corner over the pit's north-east, where wave 1 shows and the husks come down; walkers reach it up the crane stair, you by the stair, the hoist pad over the pit or the return pad over the exit; a punch off it is a 5 m drop" }),
    piece({ zone: "a3", kind: "deck", min: [4, 0, Z(26)], max: [6.5, 2.5, Z(22.5)], job: "perch",
      why: "THE LADLE STEP (deck 2.5 m, card: Molecule node L): the step under the ladle perch on the far floor's east side, a double jump from the floor and another to the perch (two jumps up, or the stair), the husks' drop down off the perch" }),
    piece({ zone: "a3", kind: "hull", min: [4, 0, Z(7.2)], max: [5.2, 10, Z(6)], job: "cover",
      why: "THE INGOT STACK (card: Every-solid rule), floor to roof on the entry floor's east side: breaks the east lane's line of sight onto the entry floor (the swarmers round it at punch range) and screens the hoist pad from the far floor" }),
  ];
  const ramps = [
    piece({ zone: "a3", from: [-8.5, Z(2)], to: [-8.5, Z(8)], width: 3, rise: 2.5, job: "climb",
      why: "the SHELF RAMP (card: Molecule; GG NIGHT20 'a route that enemies can climb up'): 2.5 m over 6 m from the entry floor's west corner (its foot 2 m in from the south wall, reached past the west cheek) head-on onto the mould shelf: the walkers' way up and yours" }),
    piece({ zone: "a3", from: [8.75, Z(10.5)], to: [8.75, Z(22.5)], width: 2.5, rise: 5, job: "climb",
      why: "the CRANE STAIR (card: Molecule): 5 m over 12 m up the east wall beside the east lane onto the ladle perch: the husks' file down it at you (a pierce line from the entry floor) and the walk up" }),
    piece({ zone: "a3", from: [-6, Z(21.5)], to: [-6, Z(26)], width: 2, rise: 2.5, job: "climb",
      why: "the SHELF'S BACK RAMP (card: Molecule): 2.5 m over 4.5 m from the far floor up the north wall onto the shelf's north end: the second way on and off the shelf (swarmers come down it at the ride's landing)" }),
  ];
  const pads = [
    piece({ zone: "a3", at: [2.5, 0, Z(11)], target: [8.5, 5, Z(24)], apexAbove: 2, job: "pad",
      why: "the HOIST PAD (card: Molecule; NIGHT52 contract item 5, the upward line; round 9, D-30: 1 m west and 1 m north, off walk proof 1's line into the east lane, 2.5 m off the spawn's forward line): on the entry floor at the pit's south edge, it throws you 15 m over the pit's east half onto the ladle perch: the other way over the lava, through the air (the ride is the way over it on a wall)" }),
    piece({ zone: "a3", at: [-8.5, 2.5, Z(24)], target: [8.25, 5, Z(24)], apexAbove: 1.5, job: "pad",
      why: "the RETURN PAD (card: Molecule): from the mould shelf's north end 17 m over the exit apron onto the ladle perch, so the two high sides are a loop (shelf -> perch -> stair / step -> floor) and the shelf is never a dead end to hold" }),
  ];
  const pits = [
    piece({ zone: "a3", rect: [-5, Z(21), 5, Z(12)], floorY: -0.8, lava: true, job: "hazard",
      why: "THE CASTING PIT (card: Hazard both ways; n46's lesson: ride over a 12 x 9 m lava pit, now 10 x 9 m with the runner standing in it): to you a burn and a knock-up out to the nearest edge (every edge a walkable floor: the entry floor, the far floor, the 2 m west lane under the shelf and the 2.5 m east lane beside the stair); to a husk or a swarmer death (Lava30): punch it in from the lane, or ride past it" }),
  ];
  const lights = [
    { id: "a3-L1", room: "a3", kind: "ceiling", at: [1, 9.95, Z(5)], normal: [0, -1, 0], size: [3, 3, 0.1], color: "white", range: 9, intensity: 0.9 },
    { id: "a3-L2", room: "a3", kind: "ceiling", at: [0, 9.95, Z(23.5)], normal: [0, -1, 0], size: [4, 3, 0.1], color: "white", range: 9, intensity: 1 },
    { id: "a3-L3", room: "a3", kind: "pool", at: [0, -0.775, Z(16.5)], normal: [0, 1, 0], size: [8, 7, 0.05], color: "orange", range: 9, intensity: 1.3, danger: true },
    { id: "a3-L4", room: "a3", kind: "ceiling", at: [-8.5, 9.95, Z(16)], normal: [0, -1, 0], size: [2.5, 4, 0.1], color: "white", range: 9, intensity: 0.9 },
    { id: "a3-L5", room: "a3", kind: "pool", at: [0, 0.025, Z(24.5)], normal: [0, 1, 0], size: [2.5, 2.5, 0.05], color: "cyan", range: 7, intensity: 1.6 },
    { id: "a3-RE", room: "a3", kind: "wall", at: [-1.95, 1.2, Z(13)], normal: [1, 0, 0], size: [18, 0.2, 0.1], color: "cyan", range: 6, intensity: 1.1 },
    { id: "a3-RW", room: "a3", kind: "wall", at: [-2.65, 1.2, Z(13)], normal: [-1, 0, 0], size: [18, 0.2, 0.1], color: "cyan", range: 6, intensity: 1.1 },
  ];
  const props = [
    { id: "a3-P1", room: "a3", kind: "healthPickup", at: [-1, 0.9, Z(21.8)], size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true, job: "reward",
      why: "the riskiest spot pays (A20; card: The 30 s problem): on the pit's north edge where the runner's ride lands you among the husks, both lanes' and the shelf's fire on it; a ride that ends in the lava is paid back here (n46's pickup, moved from the far floor's west corner to the landing)" },
  ];
  const mineZ = (o) => o.zone === "a3";
  const mineR = (o) => o.room === "a3";
  swapRows(plan.boxes, mineZ, boxes);
  swapRows(plan.ramps, mineZ, ramps);
  swapRows(plan.pads, mineZ, pads);
  swapRows(plan.pits, mineZ, pits);
  swapRows(plan.lights, mineR, lights);
  swapRows(plan.props, mineR, props);

  // the nodes: wave 1 in view from the spawn past the runner's east face (spawn31: >= 12 m inside the view cone), wave 2 from the west
  // (the shelf, the west lane, behind the runner): the swarmers come at the ride from the side you cannot see from the door
  const F1 = [1.5, 0, Z(23.5)], F2 = [-3.8, 0, Z(24)], F3 = [6.25, 0, Z(17)], F4 = [-6, 0, Z(17)], F5 = [3, 0, Z(24.8)];
  const H1 = [-8.5, 2.5, Z(14)], H2 = [-8.5, 2.5, Z(18.5)], H3 = [8.25, 5, Z(24)], H4 = [5.25, 2.5, Z(24)];
  const row = {
    id: "a3", name: "1.3 RIDE", kind: "room", rect: room.rect, height: 10, roof: room.roof, par: room.par,
    job36: "practise",
    verb36: "the ride (hold jump on a wall): the runner's 18 m over the dry floor, then over the casting pit's lava, under fire (+ the zoom and slow on a wall); the hoist pad over the pit",
    spawn: { at: [0, 0, Z(5)], yawDeg: 0 }, exit: [0, 0, Z(24.5)], arena36: room.arena36,
    waves: [["husk", "husk", "swarmer", "swarmer"], ["swarmer", "swarmer", "swarmer", "husk"]],
    lessons: [{ promptPad: "HOLD {jump} ON A WALL: RIDE", kind: "reached", rect: [-10, Z(26), 10, Z(23)], minY: -0.2, prompt: "HOLD {jump} ON A WALL: RIDE" }],
    lessons31: [{ promptPad: "HOLD {jump} ON A WALL: RIDE", kind: "reached", rect: [-10, Z(26), 10, Z(23)], minY: -0.2, prompt: "HOLD {jump} ON A WALL: RIDE" }],
    proofs: [
      { kind: "walk", points: [[0, 0, -59.5], [0, 0, Z(5)], [1.5, 0, Z(9)], [6.25, 0, Z(11)], [6.25, 0, Z(20.5)], [5, 0, Z(21.7)], [0, 0, Z(22.5)], [0, 0, Z(24.5)]] },
      { kind: "walk", points: [[0, 0, Z(5)], [-1, 0, Z(3)], [-4.5, 0, Z(3)], [-6, 0, Z(11)], [-6, 0, Z(21)], [-3.5, 0, Z(22.5)], [0, 0, Z(24.5)]] },
      { kind: "walk", points: [[-4.5, 0, Z(3)], [-6, 0, Z(1)], [-8.5, 0, Z(1)], [-8.5, 0, Z(2.2)], [-8.5, 2.5, Z(8.5)], [-8, 2.5, Z(14)], [-8, 2.5, Z(24)]] },
      { kind: "walk", points: [[6.25, 0, Z(9.5)], [8.75, 0, Z(10.6)], [8.75, 5, Z(22.4)], [8.5, 5, Z(24)]] },
      { kind: "walk", points: [[-3.5, 0, Z(22.5)], [-6, 0, Z(21.6)], [-6, 2.5, Z(25.8)], [-8, 2.5, Z(25)]] },
    ],
    frames36: { mid: [6.5, 0, -60.5, 32.619, -0.007], exit: [-1, 0, Z(11), -8.13, -0.078] },
    frames52: {
      _why: "NIGHT52 F: reel a452's spots (feet x, y, z, yaw deg, pitch rad): entry = just inside d2 looking at the runner and the pit, the ladle perch upper right; mid = the east lane's south end looking north-west along the runner over the lava at the far floor and the shelf; high = on the ladle perch looking south-west over the pit at the door",
      entry: [0, 0, Z(1.5), 0, 0.08],
      mid: [6.25, 0, Z(9), 25, 0.1],
      high: [8.25, 5, Z(24.5), 150, -0.3],
    },
    nodes: [F1, F2, F3, F4, F5],
    highNodes: [H1, H2, H3, H4],
    waveNodes43: [[F3, F1, H4, F5], [H1, H2, F4, F2, H3]],
    waveNodes43_why: "NIGHT52 F (card: The 30 s problem): wave 1 (two husks, two swarmers) shows at once past the runner's east face: the east lane, the far floor, the ladle step (the husks file round the lanes, the swarmers rush: ride over them, kill them from the wall); wave 2 (overlap46: as wave 1 thins) from the side the door does not see: the mould shelf, the west lane and behind the runner's north end, so the ride's landing is contested",
    card52_why: "NIGHT52 F round 5 (row foundry52; docs/ROOMS.md 'a3 1.3 RIDE -> THE CASTING PIT'): rebuilt to THE ROOM CONTRACT from its identity card; n46's two 10.5 m panels P1 / P2 (the ride over a floor, then a jump across onto a second wall over the lava) become ONE 18 m runner (the floor, then the lava, on one held ride: b2 teaches the chain), n46's 2 m east bridge and dry strip become the two lanes round the pit",
  };
  plan.rooms[plan.rooms.indexOf(room)] = row;
}

const eolOf = (t) => (t.includes("\r\n") ? "\r\n" : "\n");
const same = (a, b) => JSON.stringify(a) === JSON.stringify(b);

if (process.argv[2] === "--finish") finish();
else writePlan();

function writePlan() {
  const text = fs.readFileSync(path.join(ROOT, PLAN_IN), "utf8");
  const plan = JSON.parse(text);
  const before = JSON.parse(text);
  if (plan.id !== "n46-foundry") throw new Error(`${PLAN_IN}: id ${plan.id}, expected n46-foundry`);
  plan.id = "n52-foundry";
  if (REBUILT.length) plan._why = "NIGHT52 builder F (row foundry52; Tools/night/gen-n52-foundry.cjs): n46-foundry's plan with the combat rooms " + REBUILT.join(", ") + " rebuilt to THE ROOM CONTRACT (NIGHT52.md section 2; their cards in docs/ROOMS.md). Before it: " + plan._why;
  for (const id of REBUILT) ROOMS[id](plan);
  // the guard: every room row, door and piece of a room not rebuilt is n46's bit for bit
  const mine = (o) => REBUILT.includes(o.zone) || REBUILT.includes(o.room);
  for (const r of before.rooms) if (!REBUILT.includes(r.id) && !same(r, plan.rooms.find((q) => q.id === r.id))) throw new Error("room changed: " + r.id);
  for (const k of ["boxes", "ramps", "pads", "grapple", "pits", "tunnels", "lights", "props"]) {
    const a = before[k].filter((o) => !mine(o)), b = plan[k].filter((o) => !mine(o));
    if (!same(a, b)) throw new Error(k + ": a row outside the rebuilt rooms changed");
  }
  for (const d of before.doors) {
    const e = plan.doors.find((q) => q.id === d.id);
    if (!e) throw new Error("door missing: " + d.id);
    if (!d.between.some((z) => REBUILT.includes(z)) && !same(d, e)) throw new Error("door changed: " + d.id);
  }
  // teach35 (the plan's tutorial rects): only a rebuilt room's entries may move (round 5: a3's l1.zoom onto the runner)
  if (!same(before.teach35.filter((t) => !REBUILT.includes(t.room)), plan.teach35.filter((t) => !REBUILT.includes(t.room))) || before.teach35.length !== plan.teach35.length) throw new Error("teach35: an entry outside the rebuilt rooms changed");
  for (const k of Object.keys(before)) if (!["_why", "id", "rooms", "doors", "boxes", "ramps", "pads", "grapple", "pits", "tunnels", "lights", "props", "teach35"].includes(k) && !same(before[k], plan[k])) throw new Error(k + " changed");
  // every rebuilt room's pieces carry a job and a why (A-1; the room check counts them)
  for (const k of ["boxes", "ramps", "pads", "pits", "tunnels"]) for (const o of plan[k]) if (mine(o) && !(String(o.job || "").trim() && String(o.why || "").trim())) throw new Error(k + ": a rebuilt piece without job / why: " + JSON.stringify(o));
  const eol = eolOf(text);
  fs.mkdirSync(path.join(ROOT, path.dirname(PLAN_OUT)), { recursive: true });
  fs.writeFileSync(path.join(ROOT, PLAN_OUT), JSON.stringify(plan, null, 1).replace(/\n/g, eol) + eol);
  console.log(`n52-foundry plan: ${PLAN_IN} -> ${PLAN_OUT}; rebuilt: ${REBUILT.join(", ") || "none (the pipeline proof)"}; every other room, door and piece n46's bit for bit`);
}

function finish() {
  const src = path.join(ROOT, process.argv[3] || GEN_OUT);
  const raw = fs.readFileSync(src, "utf8");
  const lvl = JSON.parse(raw);
  if (lvl.name !== "n52-foundry") throw new Error(`${src}: name ${lvl.name}, expected n52-foundry (levelgen of ${PLAN_OUT})`);
  lvl.data.name = "n46-foundry"; // the older systems' name for level 1, as n46 / n47 / n48-foundry carry it (MapModelJson reads the outer name first)

  // ---- gen-n47-foundry.cjs's a9 (copied: the same keys in the same order, so a9 comes out bit-equal to n47's) ----
  lvl.name = "n47-foundry";
  lvl._why = "NIGHT47 builder W (design.n47.foundry47 swaps n46-foundry for this file while the row is on; GG 2026-10-18: the WICK fight in three phases, the climb forgiving): Tools/night/gen-n47-foundry.cjs copies n46-foundry's level whole and changes the data of one room, 1.11 THE STACK (a9): its job, verb, par, the wick47 note and frames47. n46-foundry's _why: " + lvl._why;
  const rooms = lvl.campaign.rooms;
  const a9 = rooms.find((r) => r.id === "a9");
  if (!a9 || a9.boss !== "wick" || !a9.wick37) throw new Error("a9 (THE STACK, wick) missing");
  const c = a9.wick37.centre; // [0, 0, -382]
  a9.job36 = "boss";
  a9.verb36 = "P1 the parry on the floor, P2 the climb (ride, leap), P3 the punch at the open core (the exam)";
  a9.par = 150;
  a9.par_why = "NIGHT47 W: the human band's top (NIGHT47.md section 3: median 90-150 s), was 22 (the bot's).";
  a9.wick47 = {
    _why: "NIGHT47 W (rows wick47 / climb47 / wickTop47; W-cards.md section 1): THE WICK in three phases in this shaft: P1 THE FLOOR (the panels folded into the wall, 3 slag vents in the 9 m floor), P2 THE CLIMB (at 55 % the steps and the panels come out of the wall: 5 plain STEPS 1.4 m apart (a jump, no ride: round 3), then 8 ride panels from ledge 1 (the last step, 7 m) rising 2.5 / 2.8 m, ledge 2 at 16.2 m, the net), P3 THE CAP at 26.3 m (the glass slides over the shaft; the crack-open charge). The numbers are config, not this row: design.n47.wick47.numbers (the stack, the vents, the attacks) and design.n47.climb47.numbers; this row is the room's note for the reels and docs/ROOMS.md.",
    phases: ["P1 THE FLOOR", "P2 THE CLIMB", "P3 THE CAP"],
  };
  const at = (x, y, z, yaw, pitch) => [c[0] + x, c[1] + y, c[2] + z, yaw, pitch];
  a9.frames47 = {
    _why: "NIGHT47 W: reel a147's spots (the lead's frames): RED on n47-r1 with the rows off, GREEN with them on.",
    p1Entry: at(0, 0, 10.5, 0, 0.15),
    p1Floor: at(0, 0, 5, 0, 0.35),
    p2Climb: at(-7.5, 1.45, 0, 180, 0.3),
    capEdge: at(0, 26.3, 11, 0, 0.1),
    p3Cap: at(0, 26.3, 6, 0, 0.1),
  };

  // ---- gen-n48-foundry.cjs's a9 (copied) ----
  lvl.name = "n48-foundry";
  lvl._why = "NIGHT48 builder W (design.n48.climbTop48 swaps n47-foundry for this file while the row is on; NIGHT48.md G1: the climb's top third, GG 2026-10-18 'gradually becoming slightly harder near the top but always remaining fully possible and fluid'): Tools/night/gen-n48-foundry.cjs copies n47-foundry's level whole and adds one key to one room, 1.11 THE STACK (a9): climbTop48, the marker that gives THE STACK its top-third lips. n47-foundry's _why: " + lvl._why;
  a9.climbTop48 = {
    _why: "NIGHT48 W (level row climbTop48; G1): THE STACK's top third gets a LIP at the far end of each top-third panel (after panels 4, 5 and 6 of n47's stack): a catch shelf a little under the rider's feet at the panel's end, against the wall, so a release held past the panel's end lands on it and a short hop rides the next panel. The numbers are config, not this row: design.n48.climbTop48.numbers (lipAfter, lipOver, lipArcDeg, lipStartDeg, lipDepth); SurvivalSim.N48.Wick.cs reads this key's presence only.",
    lips: true,
  };

  // ---- the guards against n48-foundry.json (untouched) ----
  const n48raw = fs.readFileSync(path.join(ROOT, N48), "utf8");
  const n48 = JSON.parse(n48raw);
  const b9 = n48.campaign.rooms.find((r) => r.id === "a9");
  if (!same(a9, b9)) throw new Error("a9 is not n48-foundry's a9 bit for bit");
  if (rooms.length !== n48.campaign.rooms.length) throw new Error("room count changed");
  for (let i = 0; i < rooms.length; i++) {
    if (rooms[i].id !== n48.campaign.rooms[i].id) throw new Error("room order changed at " + i);
    if (!REBUILT.includes(rooms[i].id) && !same(rooms[i], n48.campaign.rooms[i])) throw new Error("room changed: " + rooms[i].id);
  }
  const mine = (o) => REBUILT.includes(o.zone) || REBUILT.includes(o.room);
  for (const k of ["lights", "props", "pits", "boxes"]) {
    const a = (lvl.campaign[k] || []).filter((o) => !mine(o)), b = (n48.campaign[k] || []).filter((o) => !mine(o));
    if (!same(a, b)) throw new Error("campaign." + k + ": a row outside the rebuilt rooms differs from n48-foundry's");
  }
  for (const d of n48.campaign.doors) {
    const e = lvl.campaign.doors.find((q) => q.id === d.id);
    if (!e || (!d.between.some((z) => REBUILT.includes(z)) && !same(d, e))) throw new Error("door changed: " + d.id);
  }
  const eol = eolOf(n48raw);
  const compact = !n48raw.trimEnd().includes("\n");
  const ser = (o) => (compact ? JSON.stringify(o) + (n48raw.endsWith("\n") ? eol : "") : JSON.stringify(o, null, 1).replace(/\n/g, eol) + eol);
  if (!REBUILT.length && ser(lvl) !== n48raw) throw new Error("the pipeline proof failed: the unchanged plan through levelgen + this finish is not n48-foundry.json byte for byte");

  // ---- n52-foundry ----
  lvl.name = "n52-foundry";
  lvl._why = "NIGHT52 builder F (design.n52.foundry52 swaps n48-foundry for this file while the row is on; NIGHT52.md section 2 THE ROOM CONTRACT; GG: \"every single level needs a complete arena overhaul\", \"a lot of the rooms to be a lot smaller and more action packed\"): Tools/night/gen-n52-foundry.cjs, levelgen of Design/levels/n52/foundry.plan.json (n46-foundry's plan; rebuilt: " + (REBUILT.join(", ") || "none") + "), then a9's n47 and n48 data re-applied bit-equal. n48-foundry's _why: " + lvl._why;
  lvl.generatedBy = "Tools/night/gen-n52-foundry.cjs (plan " + PLAN_OUT + " -> levelgen -> --finish); " + lvl.generatedBy;
  const text = ser(lvl);
  for (const o of OUTS) fs.writeFileSync(path.join(ROOT, o), text);
  if (fs.readFileSync(path.join(ROOT, OUTS[0]), "utf8") !== fs.readFileSync(path.join(ROOT, OUTS[1]), "utf8")) throw new Error("copies differ");
  console.log(`n52-foundry: finished ${path.relative(ROOT, src)} -> ${OUTS.join(", ")} (${text.length} chars, both copies identical); a9 bit-equal to n48-foundry's; rooms not rebuilt bit-equal; `
    + (REBUILT.length ? "rebuilt: " + REBUILT.join(", ") : "pipeline proof: with n48's name and _why the file IS n48-foundry.json byte for byte"));
}
