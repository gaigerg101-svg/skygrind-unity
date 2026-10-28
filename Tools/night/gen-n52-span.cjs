// NIGHT52 builder K, round 2 (the room loop: span is K's from round 2): span's combat rooms rebuilt to THE ROOM CONTRACT (NIGHT52.md section 2)
// in Design/levels/n52/span.plan.json. The base is the lead's n52 plan (3c30cd38: n47's plan with the id n52-span and the loop's _why); this
// file rebuilds it from n47's plan every run (re-runnable), so the order of the rooms' slices does not matter.
//   p3 4.3 KNOT YARD (NIGHT46's THE TIE-OFF) as THE TIE-OFF II: 24 x 30 m; d3 moved 10 m south, p4 PULL LINE re-cut to meet it.
//   p5 4.5 SPOOL HALL as THE SPOOL HALL II: 26 x 28 m; d5 moved 8 m south, o1 THREAD LINE re-cut to meet it.
// The cards: docs/ROOMS.md, "NIGHT52 K identity cards (span)". Every other room, door and row of n52-span is n47's bit for bit (asserted); p1
// (job36 teach) and the links / the boss are exempt from the contract and untouched.
// Run (worktree root): node Tools/night/gen-n52-span.cjs [--rooms=p3,p5], then bash Tools/night/n52-loop-regen.sh span p3 p5
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN_IN = path.join(ROOT, "Design/levels/n47/span.plan.json");
const PLAN = path.join(ROOT, "Design/levels/n52/span.plan.json");
const text = fs.readFileSync(PLAN_IN, "utf8");
const plan = JSON.parse(text);
if (plan.id !== "n47-span") throw new Error("unexpected base plan " + plan.id);
// the lead's n52 base (3c30cd38): n47's plan, the id and the loop's _why
plan.id = "n52-span";
plan._why = "NIGHT52 the room loop (design.n52.span52 swaps n47-span for n52-span while the row is on; NIGHT52.md section 5): n47's plan with the loop's rebuilt rooms (THE ROOM CONTRACT, section 2). Before it: " + plan._why;
const before = JSON.parse(JSON.stringify(plan));
const ONLY = (process.argv.find((a) => a.startsWith("--rooms=")) || "--rooms=p3,p5").slice(8).split(",").filter(Boolean);
const eq = (a, b) => JSON.stringify(a) === JSON.stringify(b);
const room = (id) => plan.rooms.find((r) => r.id === id);
const door = (id) => plan.doors.find((d) => d.id === id);
const CHANGED = new Set();
const DOORS = new Set();
const CARD = "docs/ROOMS.md, NIGHT52 K identity cards (span)";
function swap(key, isMine, fresh) {
  const arr = plan[key] || [];
  const at = arr.findIndex(isMine);
  const rest = arr.filter((x) => !isMine(x));
  rest.splice(at < 0 ? rest.length : at, 0, ...fresh);
  plan[key] = rest;
}
function rebuild(Z, rows) {
  swap("boxes", (b) => b.zone === Z, rows.boxes);
  swap("ramps", (b) => b.zone === Z, rows.ramps);
  swap("pads", (b) => b.zone === Z, rows.pads);
  swap("pits", (b) => b.zone === Z, rows.pits);
  swap("props", (b) => b.room === Z, rows.props);
  swap("lights", (b) => b.room === Z || b.zone === Z, rows.lights);
  CHANGED.add(Z);
}
const pieces = (Z) => ({
  box: (kind, min, max, job, why, extra) => Object.assign({ zone: Z, kind, min, max, job, why, note: why }, extra || {}),
  ramp: (from, to, width, rise, why) => ({ zone: Z, from, to, width, rise, job: "climb", why, note: why }),
});
// a link's entry edge (its rect's z1) moved to meet a moved door: the rect grows, the spawn (and the entry frame when asked) follow the door, the
// noride liners that ran to the old edge run to the new one, one ceiling light over the new floor
function recutLinkEntry(Z, oldZ1, newZ1, wantLiners, frameToo, lightAt) {
  const r = room(Z);
  if (r.kind !== "link" || r.rect[3] !== oldZ1) throw new Error(Z + ": not the n47 link");
  const dz = newZ1 - oldZ1;
  r.rect = [r.rect[0], r.rect[1], r.rect[2], newZ1];
  r.spawn = Object.assign({}, r.spawn, { at: [r.spawn.at[0], r.spawn.at[1], r.spawn.at[2] + dz] });
  if (frameToo && r.frames36 && r.frames36.mid) r.frames36 = Object.assign({}, r.frames36, { mid: [r.frames36.mid[0], r.frames36.mid[1], r.frames36.mid[2] + dz, r.frames36.mid[3], r.frames36.mid[4]] });
  r.recut52_why = `NIGHT52 K: the entry edge moved ${dz} m (z ${oldZ1} -> ${newZ1}) to meet the moved door (the room before it shrank to THE ROOM CONTRACT's size); the spawn follows the door${frameToo ? " (and the entry frame)" : ""}; ${wantLiners} noride liner(s) run to the new edge; the gates, ledges, rings, voids, lessons and fall returns are n47's`;
  let n = 0;
  for (const b of plan.boxes) {
    if (b.zone !== Z || b.max[2] !== oldZ1) continue;
    if (!(b.noride && b.kind === "hull")) throw new Error(Z + ": a non-liner box reaches the old edge");
    b.max = [b.max[0], b.max[1], newZ1];
    b.why = b.why + ` (NIGHT52 K: re-cut to the moved door at z ${newZ1})`;
    b.note = b.why;
    n++;
  }
  if (n !== wantLiners) throw new Error(`${Z}: expected ${wantLiners} liners, found ${n}`);
  plan.lights.push({ id: Z + "-L52", room: Z, kind: "ceiling", at: lightAt, normal: [0, -1, 0], size: [3, 2, 0.1], color: "white", range: 14, intensity: 1, why: "NIGHT52 K: the re-cut entry floor's light" });
  CHANGED.add(Z);
}

// ================================================================ p3 4.3 KNOT YARD as THE TIE-OFF II (24 x 30 x 14 m: x -12..12, z 3.5..33.5)
function p3() {
  const Z = "p3", P = pieces(Z);
  const r = room(Z);
  if (!eq(r.rect, [-20, -6.5, 20, 33.5])) throw new Error("p3 is not n47's");
  r.rect = [-12, 3.5, 12, 33.5];
  r.exit = [0, 0, 5.5];
  r.spawn = { at: [0, 0, 29], yawDeg: 0 };
  r.spawn_why = "NIGHT52 K: 4.5 m in from the door on the entry deck (n46: 7.5 m at the cable's clamp, cut): the retry starts you facing the void, the cable and both anchorages";
  r.verb36 = "kills give pulls: swing or pull over the void (the rings over both lips and both stands, the KNOT floating over the middle), ride either cheek wall across it, walk the snapped cable down its middle; the husks come at you from both anchorages and over the cable, the spitters on the anchorages";
  r.grappleNodes = [[0, 9.5, 25.5], [0, 11, 18.5], [0, 9.5, 11.5], [-9.5, 9.5, 18.5], [9.5, 11, 15.5]];
  r.grapple = "FIVE rings (card: Molecule): RS over the entry deck's lip and RN over the exit deck's lip (9.5 m): a pull or a swing across the void from either deck; the KNOT (11 m) floating over the void's middle: one pull straight over from deck to deck (the shortcut; no column under it since the P-rank review: the S -> N pull flew into it and it hid RN from the door); RW over AW and RE over AE: a pull off the cable or a ride onto either stand over its parapet; a kill refills a pull (KILLS GIVE PULLS.)";
  // NIGHT52 K, the persona review (1): n52-r10's wave 1 stood in a row on the exit deck within 7.5 m of the exit wall (a shooting gallery). Now
  // wave 1 comes from both anchorages (2.5 / 4.2 m), the cable's north end (the void's middle) and one exit-deck corner: the first fight is across
  // and beside the void, from three heights; wave 3 spreads over the exit deck, the cable and AW.
  const w1 = [[-9.5, 2.5, 15], [9.5, 4.2, 15.5], [0, 0, 14], [5.5, 0, 8]]; // the exit-deck node east of the east tie-off post's line from the spawn (in view)
  const high = [[-9.5, 2.5, 21], [-9.5, 2.5, 15], [9, 4.2, 21], [9, 4.2, 15.5]];
  const w3 = [[-5, 0, 11], [-2.5, 0, 6], [4, 0, 8], [1, 0, 14.5], [-9.5, 2.5, 19]]; // round 4: the cable's node on its north stub (its middle is cut)
  const all = w1.concat(high, w3);
  const uniq = (l) => l.filter((v, i) => l.findIndex((u) => eq(u, v)) === i);
  r.nodes = uniq(all.filter((v) => v[1] === 0));
  r.highNodes = uniq(all.filter((v) => v[1] > 0));
  r.waveNodes43 = [w1, high, w3];
  r.waveNodes43_why = "NIGHT52 K (the persona review: no shooting gallery): wave 1 (four husks) from AW (2.5 m), AE (4.2 m), the cable's north end over the void and the exit deck's west corner: they come down the anchorages' ramps and over the cable at you from three heights; wave 2 on the anchorage tops (the spitters over their parapets, their husks come down the ramps); wave 3 from the exit deck, the cable and AW; every node in the entry's view cone at 12 m or more (spawn31)";
  r.fallReturn43 = {
    belowY: -1.5, to: [0, 0, 25], yawDeg: 0, damage: 5,
    by: [
      { rect: [-7, 18.5, 7, 23.5], to: [0, 0, 25], yawDeg: 0 },
      { rect: [-7, 13.5, 7, 18.5], to: [0, 0, 12], yawDeg: 180 },
    ],
    _why: "NIGHT46 A1's rule on the new void (A-4): feet under the fall line in the void -> back on the lip it was crossed from (the south half to the entry deck's, the north half to the exit deck's), 5 health",
  };
  r.proofs = [
    { kind: "walk", points: [[0, 0, 31], [-10.5, 0, 31], [-10.5, 2.5, 24], [-9.5, 2.5, 13], [-10.5, 2.5, 12.3], [-10.5, 0, 6], [3, 0, 6], [5, 0, 4.6], [10.5, 0, 4.9], [10.5, 4.2, 14], [10.5, 4.2, 22], [10.5, 0, 32.3], [0, 0, 31]] },
    { kind: "walk", points: [[-10.5, 0, 31], [-10.5, 2.5, 24], [-9.5, 2.5, 16]] },
    { kind: "walk", points: [[9.5, 4.2, 16], [10.5, 4.2, 13.5], [10.5, 0, 4.3]] },
  ];
  r.frames36 = { mid: [0, 0, 30.5, 0, 0.06], exit: [2, 0, 6, 180, -0.05] };
  r.frames52 = { _why: "NIGHT52 the room frames reel a452: the door just crossed (the void, the cable, both anchorages, the knot), the fight from the cable's south end, the east anchorage's top", entry: [0, 0, 31, 0, 0.08], mid: [-5, 0, 24.5, -20, 0.05], high: [9.5, 4.2, 21, 120, -0.2] };
  r.loop52_why = "NIGHT52 K (Tools/night/gen-n52-span.cjs; " + CARD + ": p3 THE TIE-OFF II): NIGHT46's THE TIE-OFF (40 x 40 m, 25 pieces 1.56 / 100 m2, 2 planes, no ride, no pickup) rebuilt to THE ROOM CONTRACT: 24 x 30 x 14 m, 18 pieces, three planes (0 / 2.5 / 4.2 m) on walkers' ramps, two cheek walls (rides across the void, 17 m each, parapets over the stands), the snapped cable down the void's middle, two tie-off posts, the winch spring (a pad onto AE), five rings, the cable's middle cut (the rings carry the gap); the waves, lesson and roles kept; the waist-high railings, coils, clamp and bollard cut (A11). The persona and P-rank reviews (n52-r10): the west ride moved off the floor into the void, the cable to the middle, the cheeks capped as parapets, the entry posts cut, the exit posts made the cable's tie-off posts, the knot's box cut (its ring floats), wave 1 off the exit deck.";
  // NIGHT52 K, the persona review (2)-(4): (2) the west ride ran on the west wall over AW and its ramp (solid floor: riding was optional): now the
  // WEST CHEEK stands in the void at its west edge, mirroring the east one, and the cable moves from AW's foot to the void's middle so both rides
  // cross the void (the lanes 0.7 m off the faces at x -5.7 / 5.7, 3.7 m from the cable). Both cheeks are parapets over their anchorage's top
  // (a 7 m cheek hid AE's spitter and husks from the whole room and made AE a 5 m trench to wait in, the P-rank review): the east one 1 m over
  // AE (5.2 m: the stand sees over it, a rider hops off onto it), the west one 1.5 m over AW (4 m: a ride wants 2 m of wall over the feet, and
  // AW is 2.5 m). (3) the four posts read random: the two by the entry are cut; the two by the exit are the cable's TIE-OFF POSTS at its north
  // end on the void's lip. (4) the KNOT box under the roof is cut (the P-rank review: on the rings' line the S -> N pull flew into it and from
  // the door it hid RN; the persona review's drum lost to that): the KNOT ring floats. Density back with pieces that serve a verb: RW and RE
  // (pulls onto the stands) and THE AW SPRING (a pad from the exit deck onto AW).
  const boxes = [
    P.box("deck", [-12, 0, 12], [-7, 2.5, 25], "line", "THE WEST ANCHORAGE (AW, 2.5 m, along the void's west side behind its cheek): a stand over the void (you and the spitters shoot over the 1.5 m parapet); its north end is a 2.5 m one-way drop onto the exit deck; the walkers come up its ramp (no safe ledge)"),
    P.box("wall", [-7, -14, 11], [-6.4, 4, 28], "line", "THE WEST CHEEK (ride, cyan, 17 m standing in the void at its west edge, AW's parapet: 1.5 m over its top): ride its east face across the void from the entry deck to the exit deck (the west crossing by a ride, mirroring the east cheek); wall-jump off it to the cable", { ride: true, ride36: { face: "E", planeX: -6.4, lz: [5.5, 22.5] } }),
    P.box("deck", [7, 0, 13], [12, 4.2, 23.5], "line", "THE EAST ANCHORAGE (AE, 4.2 m, the third plane, over the void's east side behind its cheek): the second spitter stand, the swings' high landing; two ramps from both decks (no dead end, no safe ledge); the winch spring (a pad) puts you on it"),
    P.box("wall", [6.4, -14, 11], [7, 5.2, 28], "line", "THE EAST CHEEK (ride, cyan, 17 m standing in the void at its east edge, AE's parapet: 1 m over its top): ride its west face across the void from the entry deck to the exit deck (the east crossing by a ride, beside the cable and a pull); hop off it over the parapet onto AE", { ride: true, ride36: { face: "W", planeX: 6.4, lz: [5.5, 22.5] } }),
    // NIGHT52 K, the midpoint review (round 4): the cable's middle 6 m is cut: the rings carry that gap (the KNOT's pull, a swing from RS / RN);
    // the walkers go round by the anchorages (AW's new north ramp, AE's two), the stubs are the crossing's two lips
    P.box("deck", [-2, -14, 21.5], [2, 0, 23.5], "line", "THE SNAPPED MAIN CABLE, south stub (2 m out from the entry deck at deck height; the hero prop): where the cable snapped: the jump-off into the KNOT's pull or a swing from RS across the 6 m gap"),
    P.box("deck", [-2, -14, 13.5], [2, 0, 15.5], "line", "THE SNAPPED MAIN CABLE, north stub (2 m out from the exit deck, its tie-off posts behind it): the far lip of the gap, the health on its tip (the riskiest spot: across the gap in both stands' fire); land a swing or a pull on it"),
    P.box("hull", [-3.5, 0, 11.5], [-2, 14, 13], "cover", "THE TIE-OFF POST, west (floor to roof, at the cable's north-west corner on the void's lip): the cable is tied off here; the cover on the exit deck when you come off the cable; the pivot of wave 3's file round the cable's end"),
    P.box("hull", [2, 0, 11.5], [3.5, 14, 13], "cover", "THE TIE-OFF POST, east (floor to roof, at the cable's north-east corner on the void's lip): the cable's other tie-off; it cuts the exit deck's line down the cable onto the entry deck (the crossing is covered at its end, not a gallery)"),
    P.box("hull", [11, 4.2, 17], [12, 14, 19.5], "cover", "AE's WINCH (from AE's top to the roof, on the east wall): the spitter's cover on the high stand: a pull or the ride puts you round it (no open firing deck)"),
  ];
  const ramps = [
    P.ramp([-10.5, 30.5], [-10.5, 25], 3, 2.5, "AW's walkers' ramp (2.5 m over 5.5 m) from the entry deck's west corner: the spitters' way down and yours up (no safe ledge, A12; GG: a route enemies can climb)"),
    P.ramp([10.5, 31.5], [10.5, 23.5], 3, 4.2, "AE's SOUTH RAMP (4.2 m over 8 m, its foot 2 m off the south wall: no slot) from the entry deck: the walkers' climb to the high stand; the ground loop's way up"),
    P.ramp([10.5, 5.5], [10.5, 13], 3, 4.2, "AE's NORTH RAMP (4.2 m over 7.5 m, its foot 2 m off the north wall: no slot) from the exit deck: the ground loop's way back up (the stand is never a dead end)"),
    P.ramp([-10.5, 6.5], [-10.5, 12], 3, 2.5, "AW's NORTH RAMP (2.5 m over 5.5 m, its foot 3 m off the north wall, the mirror of AE's; the midpoint review, round 4: it replaces the AW spring): the walkers' way onto the west stand from the exit deck without going round (the cable's middle is cut); the ground loop's west half"),
  ];
  const pads = [
    { zone: Z, at: [8, 0, 10], target: [9.5, 4.2, 16.5], apexAbove: 2, job: "line", why: "THE WINCH SPRING (a jump pad, the upward line, A5): from the exit deck between the cheek wall and AE's north ramp straight up over AE's north face onto its top (4.2 m; east of the cheek wall's line, so the arc never meets it): the fast way at the spitter after wave 1 is down", note: "the winch spring" },
  ];
  const pits = [
    { zone: Z, rect: [-7, 13.5, 7, 23.5], floorY: -14, job: "hazard", why: "THE TIE-OFF's VOID (14 x 10 m between the decks, the cable and the cheek wall standing in it; card: Hazard both ways): a fall is back on the lip it was crossed from (5 health, A-4); a walker punched, whipped or blasted off the cable is ended", note: "the void" },
  ];
  const props = [
    { id: "p3-P1", room: Z, kind: "healthPickup", at: [0, 0.9, 14.5], size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true, job: "reward", why: "the health on the snapped cable's north stub, its tip at the gap, over the void in both stands' fire (A20: the best at the riskiest spot)" },
  ];
  const lights = [
    { id: "p3-L1", room: Z, kind: "ceiling", at: [0, 13.95, 29], normal: [0, -1, 0], size: [4, 3, 0.1], color: "white", range: 11, intensity: 0.8 },
    { id: "p3-L2", room: Z, kind: "ceiling", at: [0, 13.95, 8], normal: [0, -1, 0], size: [4, 3, 0.1], color: "white", range: 11, intensity: 1 },
    { id: "p3-L3", room: Z, kind: "ceiling", at: [-9.5, 13.95, 15], normal: [0, -1, 0], size: [3, 3, 0.1], color: "white", range: 10, intensity: 0.9 },
    { id: "p3-L4", room: Z, kind: "ceiling", at: [9.5, 13.95, 15], normal: [0, -1, 0], size: [3, 3, 0.1], color: "white", range: 10, intensity: 0.9 },
    { id: "p3-L5", room: Z, kind: "ceiling", at: [0, 13.95, 18.5], normal: [0, -1, 0], size: [2, 2, 0.1], color: "amber", range: 14, intensity: 1.2 },
    { id: "p3-L6", room: Z, kind: "pool", at: [0, 0.025, 5.5], normal: [0, 1, 0], size: [2.5, 2.5, 0.05], color: "cyan", range: 8, intensity: 1.6 },
  ];
  rebuild(Z, { boxes, ramps, pads, pits, props, lights });
  const d3 = door("d3");
  if (!eq(d3.rect, [-2, -10.5, 2, -6.5])) throw new Error("d3 is not n47's");
  d3.rect = [-2, -0.5, 2, 3.5];
  DOORS.add("d3");
  recutLinkEntry("p4", -10.5, -0.5, 2, false, [0, 31.95, -5.5]);
}

// ================================================================ p5 4.5 SPOOL HALL as THE SPOOL HALL II (26 x 28 x 14 m: x -22..4, z -134.5..-106.5)
// n47's (NIGHT46's) room: 36 x 36 x 14 m, 22 pieces (1.70 per 100 m2), two planes (2.5 / 4 m in one cluster), waist-high railings and blocks.
// The card's identity stays (THE GREAT SPOOL alone in the winding pit, the whip's captain island; the gallery; the tail spool; the air loop gallery
// -> spool -> tail spool -> the south wall's ride; "{grapple} AT AN ENEMY: WHIP."); the entry door d4 stays; the exit d5 moves 8 m south to z
// -134.5 and o1 THREAD LINE is re-cut to meet it. Planes: 0; A 2.5 m (the spool, the tail spool); B 4.2 m (the gallery).
function p5() {
  const Z = "p5", P = pieces(Z);
  const r = room(Z);
  if (!eq(r.rect, [-28, -142.5, 8, -106.5])) throw new Error("p5 is not n47's");
  r.rect = [-22, -134.5, 4, -106.5];
  r.exit = [-17, 0, -132.5];
  r.spawn = { at: [-13.5, 0, -109], yawDeg: 0 };
  r.spawn_why = "NIGHT52 K: on the south walkway west of the gangway's posts, 14 m from the door d4, facing the spool and the winding frame across the pit (n46: 6.5 m at the winch block, cut; at 4 m the walk pilot's home was inside the dwell radius: n46flow dwell 2.3 -> 3.35 s; at (-4.5, -111.2), 4.6 m from the gangway's foot once the payout was cut (the reviews), every walker came off the gangway beside the home and pushed the pilot east into the door's corner: isolated dwell 2.75 -> 3.90 s)";
  r.verb36 = "the whip: the spitters stand on THE GREAT SPOOL across the winding pit: whip one to you, or pull onto the island; ride the winding frame over the pit; the capstan spring onto the gallery; the air loop gallery -> spool -> tail spool -> the south wall's ride";
  r.grappleNodes = [[-9, 10, -120.5], [-20, 8, -108.5], [1.5, 9.5, -113], [-15, 9, -126.5]];
  r.grapple = "FOUR rings (card: Molecule, flow plan 3b): R1 over THE GREAT SPOOL (10 m), R2 over the tail spool (8 m), R3 over the gallery's south end (9.5 m): gallery -> spool -> tail spool -> the south wall's ride -> gallery (the air loop); a pull onto the island from either walkway; R4 (9 m) over the pit's north-west corner (the midpoint review, round 4): a pull off the island or the frame's ride toward the exit";
  const w1 = [[-3, 0, -129.8], [-15, 0, -130], [-7, 2.5, -123.5], [-10.5, 2.5, -123.5]];
  const w2 = [[-6.5, 2.5, -123.5], [-11.5, 2.5, -123.5], [-19.5, 0, -122], [-19.5, 0, -128]];
  const w3 = [[1.5, 4.2, -123.5], [2.5, 4.2, -122], [-9, 2.5, -123.5], [-15, 0, -132.8], [-1, 0, -132]];
  const all = w1.concat(w2, w3);
  const uniq = (l) => l.filter((v, i) => l.findIndex((u) => eq(u, v)) === i);
  r.nodes = uniq(all.filter((v) => v[1] === 0));
  r.highNodes = uniq(all.filter((v) => v[1] > 0));
  r.waveNodes43 = [w1, w2, w3];
  r.waveNodes43_why = "NIGHT52 K (n46's flow plan 8 on the new floor): wave 1 on the north walkway and the spool (in view from the door across the pit); wave 2 on the spool (the captain's island: whip them off) and the west walkway; wave 3 on the gallery, the spool and the north walkway (ahead of a player going round to the exit); every node in the entry's view cone at 12 m or more (spawn31)";
  r.fallReturn43 = {
    belowY: -1.5, to: [-9, 0, -110.5], yawDeg: 0, damage: 5,
    by: [
      { rect: [-17, -120.5, -1, -112], to: [-9, 0, -110.5], yawDeg: 0 },
      { rect: [-17, -129, -1, -120.5], to: [-14.5, 0, -130.2], yawDeg: 180 },
    ],
    _why: "NIGHT46 A1's rule on the new pit (A-4): feet under the fall line in the winding pit -> back on the walkway lip it was crossed from (the south half to the south walkway, the north half to the north walkway), 5 health",
  };
  r.proofs = [
    { kind: "walk", points: [[-2, 0, -109.5], [-4, 0, -108], [-12, 0, -108], [-18, 0, -111.5], [-18, 0, -129], [-14.5, 0, -132], [-9, 0, -132.5], [-3, 0, -132.5]] },
    { kind: "walk", points: [[-4, 0, -109], [-9, 0, -109], [-9, 0, -111.5], [-9, 2.5, -117.5], [-9, 2.5, -123]] },
    { kind: "walk", points: [[2.5, 0, -133.8], [2.5, 4.2, -123.8], [1.5, 4.2, -114]] },
    { kind: "walk", points: [[-21, 0, -119.3], [-21, 2.5, -111], [-20, 2.5, -108.5]] },
    { kind: "walk", points: [[-10, 0, -132.5], [-10, 0, -129.5], [-10, 2.5, -124], [-10, 2.5, -121]] },
  ];
  r.frames36 = { mid: [-2, 0, -109.5, 20, 0.05], exit: [-14, 0, -131, 30, 0] };
  r.frames52 = { _why: "NIGHT52 the room frames reel a452: the door just crossed (the pit, the spool, the gallery), the fight from the south walkway's west end, the gallery's top", entry: [-2, 0, -109, 15, 0.04], mid: [-14, 0, -111, -25, 0.06], high: [2, 4.2, -114, 135, -0.2] };
  r.loop52_why = "NIGHT52 K (Tools/night/gen-n52-span.cjs; " + CARD + ": p5 THE SPOOL HALL II): NIGHT46's THE SPOOL HALL (36 x 36 m, 22 pieces 1.70 / 100 m2, 2 planes, no pad) rebuilt to THE ROOM CONTRACT: 26 x 28 x 14 m, 19 pieces, three planes (0 / 2.5 / 4.2 m) on walkers' ramps, the winding frame (a 17 m ride over the pit), the spool's two flanges (10 m rides across the pit), the south wall's ride, the capstan spring (a pad onto the gallery), four rings; the waves, lesson and roles kept; the waist-high railings, winch, bollard and cable blocks cut (A11). The persona review (n52-r10): the island is the spool (a barrel between two flanges with ride faces). The midpoint review (round 4): the payout bridge from the north rim onto the island (no camp behind a moat), the south posts cut, R4.";
  // NIGHT52 K, the persona review (5): p5 repeated p3's kit (an island on two ramps over a void). The island is now THE GREAT SPOOL itself: a
  // barrel (the 2.5 m top you stand on) lying east-west between two flanges that stand out of the pit to 5.5 m, 1.5 m past the barrel at both
  // ends; the flanges' outer faces are rides across the pit (the west one faces the winding frame 2.8 m away: ride, wall-jump across, ride back).
  // One flat-topped plate each: an octagon-ish flange (a lower plate under a higher one) left 1 m ledges at 4.5 m, and hold52 --perches found
  // the west one's south ledge a 0-hit camping spot. The payout (the north ramp) is cut: the walkers come up the gangway from the south walkway.
  const flange = (x0, x1, side) => [
    P.box("wall", [x0, -14, -125.5], [x1, 5.5, -115.5], "line", `THE SPOOL's ${side} FLANGE (ride, cyan, 10 m across the pit, 3 m over the barrel's top, 1.5 m past its ends: the spool's disc): ride its ${side === "WEST" ? "west face north or south over the pit (the winding frame 2.8 m behind you: wall-jump across and ride back)" : "east face over the pit toward the gallery (3.4 m across: jump off onto it)"}; the spitters' cover on the island`, { ride: true, ride36: { face: side === "WEST" ? "W" : "E", planeX: side === "WEST" ? x0 : x1 } }),
  ];
  const boxes = [
    P.box("deck", [-13, 0, -124], [-5, 2.5, -117], "line", "THE GREAT SPOOL's BARREL (2.5 m, 8 x 7 m, alone in the winding pit between its two flanges; the landmark and the hero prop): the captain's island where the spitters stand: whip one off it to you (the room's verb), or pull onto it (R1); the walkers come up its gangway (no safe ledge); jump off its north edge to the north walkway; the health on its top"),
    ...flange(-13.6, -13, "WEST"),
    ...flange(-5, -4.4, "EAST"),
    P.box("deck", [-1, 0, -124.5], [4, 4.2, -112], "line", "THE GALLERY (4.2 m, the third plane, along the east wall over the pit): the high stand over the island; its south end is a 4.2 m one-way drop onto the south walkway; the walkers come up its ramp from the north walkway (no safe ledge); the capstan spring or R3 put you on it"),
    P.box("hull", [3, 4.2, -120], [4, 14, -117.5], "cover", "THE GALLERY's WINCH (from the gallery's top to the roof, on the east wall): the spitter's cover on the high stand (no open firing deck)"),
    P.box("deck", [-22, 0, -110.5], [-18, 2.5, -106.5], "line", "THE TAIL SPOOL (2.5 m, the south-west corner): the air loop's west node (R2 over it) and the south wall's ride's start; the walkers come up its ramp"),
    P.box("wall", [-17, -14, -130], [-16.4, 6, -113], "line", "THE WINDING FRAME (ride, cyan, 17 m standing at the pit's west edge): ride its east face over the pit from the south walkway to the north one (the crossing by a ride beside the gangway and a pull); its west face is the west walkway's ride", { ride: true, ride36: { face: "E", planeX: -16.4, lz: [6.5, 23.5] } }),
    P.box("wall", [-18, 0, -107.1], [-3, 14, -106.5], "line", "THE SOUTH WALL's RIDE (cyan, 15 m on the south wall from the tail spool to the door, floor to roof: no top to camp on, hold52 found its 8 m top a 0-hit perch): the air loop's last leg: off the tail spool, ride east along the south walkway out of the pack's line, jump off into a pull at R3 onto the gallery", { ride: true, ride36: { face: "N", planeZ: -107.1 } }),
    // NIGHT52 K, the P-rank review (3): the posts on the pit's lip (one clean lane behind each). The midpoint review (round 4): the south pair
    // (the gangway's) served nothing and is cut; the north pair frames THE PAYOUT BRIDGE's foot (the 3 m gap between them) and the capstan spring
    P.box("hull", [-13, 0, -130.5], [-11.5, 14, -129], "cover", "THE NORTH-WEST POST (floor to roof, on the pit's lip): frames the payout bridge's foot (west); the cover on the north walkway by the exit where wave 1 stands; the lane behind it to the exit is clear"),
    P.box("hull", [-8.5, 0, -130.5], [-7, 14, -129], "cover", "THE CAPSTAN POST (floor to roof, on the pit's lip west of the capstan spring): frames the payout bridge's foot (east) and the spring's launch over the pit; cuts the gallery's line down the north walkway"),
  ];
  const ramps = [
    // NIGHT52 K, the midpoint review (round 4): the island, its health and one ramp behind a void moat was the camp: THE PAYOUT BRIDGE runs
    // from the north rim through the 3 m gap between the north posts onto the barrel's north edge (2.5 m over 5 m), so the pack comes at the
    // island from both walkways (the persona review's cut payout, back where the posts frame it)
    P.ramp([-10, -129], [-10, -124], 3, 2.5, "THE PAYOUT BRIDGE (2.5 m over 5 m over the pit, from the north rim through the gap between the north posts onto the barrel): the walkers' second way onto the island (no camp behind a moat); the exit side's way back onto the spool"),
    P.ramp([-9, -112], [-9, -117], 3, 2.5, "the spool's WINDING GANGWAY (2.5 m over 5 m over the pit, from the south walkway, between the flanges): the walkers' way onto the island from the door's side (no safe ledge); off its north edge the payout bridge down to the north walkway"),
    P.ramp([2.5, -132.5], [2.5, -124.5], 3, 4.2, "THE GALLERY RAMP (4.2 m over 8 m, its foot 2 m off the north wall: no slot, from the north walkway along the east wall): the walkers' climb to the high stand (no safe ledge, A12; GG: a route enemies can climb)"),
    P.ramp([-21, -118.5], [-21, -110.5], 2, 2.5, "the tail spool's RAMP (2.5 m over 8 m along the west wall, 2 m wide: the lane beside it to the winding frame is 3 m, not a 2 m slot, the P-rank review): the walkers' way onto the tail spool"),
  ];
  const pads = [
    { zone: Z, at: [-5.5, 0, -131], target: [1.5, 4.2, -121], apexAbove: 2, job: "line", why: "THE CAPSTAN SPRING (a jump pad, the upward line, A5): from the north walkway over the pit's north-east corner onto the gallery (4.2 m): the fast way at the gallery's spitter from the exit side", note: "the capstan spring" },
  ];
  const pits = [
    { zone: Z, rect: [-17, -129, -1, -112], floorY: -14, job: "hazard", why: "THE WINDING PIT (16 x 17 m round the spool; card: Hazard both ways, Spatial law): it makes the spool an island; a fall is back on the walkway lip it was crossed from (5 health, A-4); a spitter whipped or punched off the spool is ended", note: "the winding pit" },
  ];
  const props = [
    { id: "p5-P1", room: Z, kind: "healthPickup", at: [-7.5, 3.4, -120.5], size: [0.6, 0.6, 0.6], yawDeg: 0, collider: false, pickup: true, job: "reward", why: "the room's health on THE GREAT SPOOL's top (DECIDE46 N-9: bait): the exposed island in the gallery's line (A20: the best at the riskiest spot)" },
  ];
  const lights = [
    { id: "p5-L1", room: Z, kind: "ceiling", at: [-9, 13.95, -109.5], normal: [0, -1, 0], size: [4, 3, 0.1], color: "white", range: 11, intensity: 0.8 },
    { id: "p5-L2", room: Z, kind: "ceiling", at: [-9, 13.95, -132], normal: [0, -1, 0], size: [4, 3, 0.1], color: "white", range: 11, intensity: 1 },
    { id: "p5-L3", room: Z, kind: "ceiling", at: [1.5, 13.95, -122], normal: [0, -1, 0], size: [3, 3, 0.1], color: "white", range: 10, intensity: 0.9 },
    { id: "p5-L4", room: Z, kind: "ceiling", at: [-19.5, 13.95, -121], normal: [0, -1, 0], size: [3, 3, 0.1], color: "white", range: 10, intensity: 0.8 },
    { id: "p5-L5", room: Z, kind: "pool", at: [-7.5, 2.525, -120.5], normal: [0, 1, 0], size: [2.5, 2.5, 0.05], color: "amber", range: 10, intensity: 1.2 },
    { id: "p5-L6", room: Z, kind: "pool", at: [-17, 0.025, -132.8], normal: [0, 1, 0], size: [2.5, 2.5, 0.05], color: "cyan", range: 8, intensity: 1.6 },
  ];
  rebuild(Z, { boxes, ramps, pads, pits, props, lights });
  const d5 = door("d5");
  if (!eq(d5.rect, [-19, -146.5, -15, -142.5])) throw new Error("d5 is not n47's");
  d5.rect = [-19, -138.5, -15, -134.5];
  DOORS.add("d5");
  recutLinkEntry("o1", -146.5, -138.5, 0, true, [-17, 19.95, -142.5]);
}

const BUILD = { p3, p5 };
for (const id of ONLY) {
  if (!BUILD[id]) throw new Error("no rebuild for " + id);
  BUILD[id]();
}

// ---------------------------------------------------------------- the guard: every other room and row bit for bit (n47's)
for (let i = 0; i < plan.rooms.length; i++) if (!CHANGED.has(plan.rooms[i].id) && !eq(plan.rooms[i], before.rooms[i])) throw new Error("room changed: " + plan.rooms[i].id);
for (const k of ["boxes", "ramps", "pits", "props", "lights", "pads", "tunnels", "grapple", "surfaces"]) {
  const keep = (list) => (list || []).filter((b) => !CHANGED.has(b.zone) && !CHANGED.has(b.room));
  if (!eq(keep(before[k]), keep(plan[k]))) throw new Error(k + ": a row outside the rebuilt rooms changed");
}
for (const d of before.doors) if (!DOORS.has(d.id) && !eq(d, door(d.id))) throw new Error("door changed: " + d.id);
for (const k of Object.keys(before)) if (!["rooms", "doors", "boxes", "ramps", "pits", "props", "lights", "pads"].includes(k) && !eq(before[k], plan[k])) throw new Error(k + " changed");
fs.writeFileSync(PLAN, JSON.stringify(plan, null, 1) + "\n");
console.log(`gen-n52-span: wrote Design/levels/n52/span.plan.json (rebuilt: ${ONLY.join(", ")}; changed rooms: ${[...CHANGED].join(", ")}; doors moved: ${[...DOORS].join(", ")})`);
