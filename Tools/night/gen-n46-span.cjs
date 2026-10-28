// NIGHT46 builder A1 (span46; NIGHT46.md A1 "level 4's arenas"; GG 2026-10-17: "need to feel and flow better ... study real game deisgn and areana
// layouts"; DESIGN-RULES A-9 + arena-flow-web.md Part D: the identity card AND the flow plan first, every solid's reason from them).
// Level 4 as n46-span: n40-span's plan (Design/levels/n40/span.plan.json, written by Tools/night/gen-n40-span.cjs, which this file does not run or
// edit) copied whole, with THREE rooms rebuilt on the copy from their cards (docs/ROOMS.md "NIGHT46 identity cards"):
//   p1 4.1 HOOK YARD -> THE HOOK YARD (walled: the yard round the CABLE DRUM, the terrace, the HOIST WELL and its ride, the GREAT HOOK ring)
//   p3 4.3 KNOT YARD -> THE TIE-OFF (bridge: two decks, an 18 m void band, the snapped cable, the two anchorages, the KNOT)
//   p5 4.5 SPOOL HALL -> THE SPOOL HALL (islands: the GREAT SPOOL alone in the winding pit, the gallery and its drop, the tail spool, the south ride)
// and ONE data change on four links (FLOW-AUDIT row 3): p2 / p4 / o1 / o2 get `fallReturn43` lip returns (10 health, back on the gap's own lip)
// in place of the respawnY rule's "back at the link's start for 25 % of max health" (Campaign24.cs:706 asks StepFallReturn43 first; rooms43 is on).
// Every other room, door, light and prop is n40-span's, bit for bit (the script asserts it).
// Every piece carries a `why` that names its card field / flow-plan line (A-1; n36-jobs.cjs checks a job and a why on every solid).
// Coordinates in the comments: lx = metres east of the room's centre line, lz = metres north of its entry (south) edge (n36-lib's Room.pt).
// Probes: N46S_<KNOB> env vars override the knobs below; a probe must write to N46S_OUT (never the shipped plan).
// Run from the repo (or worktree) root: bash Tools/night/n46-span-regen.sh
const path = require("path");
const fs = require("fs");

const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/levels/n40/span.plan.json");
const OUT = path.join(ROOT, "Design/levels/n46/span.plan.json");
const env = (k, d) => (process.env["N46S_" + k] !== undefined ? Number(process.env["N46S_" + k]) : d);
const K = {
  VOID_Y: env("VOID_Y", -14), // the void's floor (n40's flight rooms' PIT_Y): a fall is caught by the lip return first
  // feet under this in a fight room's void -> the lip return. -1.5 (was -3): the walk (r1) rode the hoist well's wall at y -2.1, never caught;
  // nothing walkable in these rooms is under 0
  FALL_Y: env("FALL_Y", -1.5),
  // the RAILINGS on every void edge the plan does not ask you to cross (1 m: a jump clears it, a walk or a strafe does not). The walk (r1, 10 seeds,
  // the rails absent) stepped off the new edges 216 / 141 / 483 times in p1 / p3 / p5 (lip returns; p5 32 deaths): a backward strafe into a void
  // is a stop the flow plan never asked for (FLOW-AUDIT row 3's cost moved into the fight)
  RAIL_Y: env("RAIL_Y", 1.0),
  FALL_HP: env("FALL_HP", 10), // the link gaps' lip return cost (A-4; DECIDE43 D-5, THE GANTRY's number)
  // the fight rooms' lip return cost (A-4 "a little health"): 5. n46flow r2 at 10: the meter's pilot fell 5-17 times a room (it hops the rails),
  // 10 health each was a death in p3 / p5 on 4 of 10 seeds, and a death redoes the room from wave 1 (p3 dead 5.2 -> 10.4 s, clear +40 s on
  // those seeds). The fall still costs the fade, the speed and the place in the fight
  FALL_HP_FIGHT: env("FALL_HP_FIGHT", 5),
  // p3 THE TIE-OFF
  P3_VOID_S: env("P3_VOID_S", 11), P3_VOID_N: env("P3_VOID_N", 29), // the void band (lz): 11 m past the door, 18 m across
  P3_CABLE_X0: env("P3_CABLE_X0", -9), P3_CABLE_X1: env("P3_CABLE_X1", -5), // the snapped cable (lx), 4 m wide on the west third
  // the anchorages' inner edge 11 (was the card's 12): `n46span reach` round 1 measured the swing's carry from a standstill at 11.6 m past the release, down
  // at lx 9.7: 2.3 m short of a 12 m edge (the lead's gate note 2: shorten the gap, never a link only a perfect pull makes)
  P3_ANCH_IN: env("P3_ANCH_IN", 11), P3_ANCH_OUT: env("P3_ANCH_OUT", 18), P3_ANCH_S: env("P3_ANCH_S", 9), P3_ANCH_N: env("P3_ANCH_N", 31),
  // the anchorages 2.5 m (a double jump; was 4) with no rail on their inner edge: F1's 20-seed flow table (84b5f8c2) capped seed 17 in p3 on two
  // wave-2 spitters on AE's 4 m top the pilot never ended (its line from the deck blocked by the 4 m edge + the 1 m rail); a perch the floor answers
  P3_ANCH_Y: env("P3_ANCH_Y", 2.5),
  // RS / RN hang OVER the anchorage each pull lands on (lx +-14, 3 m inside its inner edge): `n46span reach` (the trace) measured a pull's arrival
  // clamping the speed along the ground to ~5 m/s (design.n31.grapplePull.arriveKeepMax) plus the upward pop, so a pull carries only ~5-6 m past its
  // ring on air control: a ring on the centre line (round 1) or 4 m off it (round 2) dropped the player into the void. The KNOT 3 m before the exit
  // deck's lip (round 2 at lz 23: down 0.1 m inside the lip, no margin) and 12 m up: round 3 at 10 m sat inside the grapple's 15 deg aim cone
  // (PortExport arcline.coneDeg) on the AW -> RN line, and the hook took the KNOT instead of RN; at 12 m it is ~18 deg off that line. lz 26: round 4
  // at lz 24 / 12 m came down 0.4 m inside the exit deck's lip
  P3_RING_Y: env("P3_RING_Y", 9), P3_RING_X: env("P3_RING_X", 14), P3_KNOT_Y: env("P3_KNOT_Y", 12), P3_KNOT_Z: env("P3_KNOT_Z", 26),
  // p1 THE HOOK YARD
  P1_TERR_X: env("P1_TERR_X", -8), P1_TERR_S: env("P1_TERR_S", 18), P1_TERR_Y: env("P1_TERR_Y", 4),
  // the drum stands on its flange, 5.5 m (out of a double jump's 2.85 m): the meter (n46flow r1) found no loop round a 2.5 m drum with a walkers'
  // ramp (a walkable top is no hole) and the stall up 16.8 -> 26.3 %; 7 x 8 m so the hole is over the meter's 50 m2
  P1_DRUM_HALF: env("P1_DRUM_HALF", 3.5), P1_DRUM_S: env("P1_DRUM_S", 15), P1_DRUM_N: env("P1_DRUM_N", 23), P1_DRUM_Y: env("P1_DRUM_Y", 5.5),
  P1_WELL_X: env("P1_WELL_X", 8), P1_WELL_S: env("P1_WELL_S", 12), P1_WELL_N: env("P1_WELL_N", 26),
  P1_HOOK_Y: env("P1_HOOK_Y", 9), P1_HOIST_Y: env("P1_HOIST_Y", 8),
  // p5 THE SPOOL HALL
  // the pit starts 10 m past the door (was 7) and the spool stands on two gangways (r2): the meter (n46flow r1) measured p5's dwell 3.6 -> 15.2 s,
  // dead 5.2 -> 11.4 s, 11 deaths, 152 falls, and the walk's falls began on the south walkway 6-8 m in (the pilot dodging by the door): the walkers
  // had only the west walkway round to the player
  P5_PIT_X0: env("P5_PIT_X0", -11), P5_PIT_X1: env("P5_PIT_X1", 13), P5_PIT_S: env("P5_PIT_S", 10), P5_PIT_N: env("P5_PIT_N", 29),
  P5_SPOOL_HALF: env("P5_SPOOL_HALF", 5), P5_SPOOL_S: env("P5_SPOOL_S", 15), P5_SPOOL_N: env("P5_SPOOL_N", 23), P5_SPOOL_Y: env("P5_SPOOL_Y", 2.5),
  P5_GAL_X: env("P5_GAL_X", 13), P5_GAL_S: env("P5_GAL_S", 12), P5_GAL_Y: env("P5_GAL_Y", 4),
  P5_TAIL_X: env("P5_TAIL_X", -14), P5_TAIL_N: env("P5_TAIL_N", 4), P5_TAIL_Y: env("P5_TAIL_Y", 2.5),
};
const OUT_FILE = process.env.N46S_OUT ? path.resolve(ROOT, process.env.N46S_OUT) : OUT;
const probe = Object.keys(process.env).some((k) => k.startsWith("N46S_") && k !== "N46S_OUT");
if (probe && OUT_FILE === OUT) throw new Error("a probe (N46S_* knobs) must write to N46S_OUT, not the shipped plan");

const r2 = (v) => +(+v).toFixed(3);
const srcText = fs.readFileSync(SRC, "utf8");
const plan = JSON.parse(srcText);
if (plan.id !== "n40-span") throw new Error(`unexpected source id ${plan.id}`);
plan.id = "n46-span";
plan._why = "NIGHT46 builder A1 (design.n46.span46 swaps n40-span for this file while the row is on; GG 2026-10-17 'need to feel and flow better'; DESIGN-RULES A-9; "
  + "arena-flow-web.md Part D): Tools/night/gen-n46-span.cjs copies n40-span's plan whole and rebuilds p1 / p3 / p5 from their identity cards and flow plans "
  + "(THE HOOK YARD, THE TIE-OFF, THE SPOOL HALL; docs/ROOMS.md), and gives the link gaps p2 / p4 / o1 / o2 lip returns (FLOW-AUDIT row 3). n40-span's _why: " + plan._why;

const REBUILT = ["p1", "p3", "p5"];
const room = (id) => { const r = plan.rooms.find((q) => q.id === id); if (!r) throw new Error(`room ${id} missing`); return r; };

// ---- the cut (each card's Cut list): every n40 piece of p1 / p3 / p5 goes; p5's health pickup moves (the card's pickup line)
const cut = {};
for (const z of REBUILT) cut[z] = { boxes: plan.boxes.filter((b) => b.zone === z).length, ramps: plan.ramps.filter((b) => b.zone === z).length, lights: plan.lights.filter((l) => l.room === z).length };
plan.boxes = plan.boxes.filter((b) => !REBUILT.includes(b.zone));
plan.ramps = plan.ramps.filter((b) => !REBUILT.includes(b.zone));
plan.pits = plan.pits.filter((b) => !REBUILT.includes(b.zone));
plan.lights = plan.lights.filter((l) => !REBUILT.includes(l.room));
const p5Health = plan.props.find((p) => p.room === "p5" && p.kind === "healthPickup");
if (!p5Health) throw new Error("p5's health pickup missing");
if (plan.props.some((p) => (p.room === "p1" || p.room === "p3"))) throw new Error("p1 / p3 carry props in n40: read them before cutting");

/// a room frame: local (lx, lz) -> world, and the emitters for its pieces (world rows, as n40's plan holds them)
function Frame(id) {
  const r = room(id);
  const [x0, z0, x1, z1] = r.rect;
  const cx = (x0 + x1) / 2;
  const f = {
    r, id, cx, zS: z1, h: r.height,
    X: (lx) => r2(cx + lx), Z: (lz) => r2(z1 - lz),
    pt: (lx, y, lz) => [f.X(lx), r2(y), f.Z(lz)],
    box(kind, lx0, lz0, lx1, lz1, y0, y1, job, why, extra) {
      const b = Object.assign({ zone: id, kind, min: [f.X(Math.min(lx0, lx1)), r2(y0), f.Z(Math.max(lz0, lz1))], max: [f.X(Math.max(lx0, lx1)), r2(y1), f.Z(Math.min(lz0, lz1))], job, why, note: why }, extra || {});
      if (!job || !why) throw new Error(`${id}: a box needs a job and a why`);
      plan.boxes.push(b);
      return b;
    },
    /// a ramp from (lx, lzLow) at the floor up `rise` to (lx, lzHigh)
    ramp(lx, lzLow, lzHigh, width, rise, why) {
      const b = { zone: id, from: [f.X(lx), f.Z(lzLow)], to: [f.X(lx), f.Z(lzHigh)], width, rise, job: "climb", why, note: why };
      plan.ramps.push(b);
      return b;
    },
    voidRect(lx0, lz0, lx1, lz1) { return [f.X(Math.min(lx0, lx1)), f.Z(Math.max(lz0, lz1)), f.X(Math.max(lx0, lx1)), f.Z(Math.min(lz0, lz1))]; },
    void(lx0, lz0, lx1, lz1, why) {
      const p = { zone: id, rect: f.voidRect(lx0, lz0, lx1, lz1), floorY: K.VOID_Y, job: "hazard", why, note: why };
      plan.pits.push(p);
      return p;
    },
    light(o) { plan.lights.push(Object.assign({ room: id, normal: [0, 1, 0] }, o)); },
    /// a railing (0.3 m thick, RAIL_Y high, standing on y0) along a void edge: [lx0, lz0, lx1, lz1] is the rail's own footprint
    rail(lx0, lz0, lx1, lz1, y0, what) {
      return f.box("hull", lx0, lz0, lx1, lz1, y0, r2(y0 + K.RAIL_Y), "cover", `a ${K.RAIL_Y} m RAILING along ${what} (the flow plan's hazard line: the void is crossed on purpose, by a jump or a pull, never by a backward strafe; the walk r1 fell off this edge without it); it is also half cover`);
    },
    ceil(n, lx, lz, w, d, color, range, intensity) { f.light({ id: `${id}-${n}`, kind: "ceiling", at: [f.X(lx), r2(f.h - 0.05), f.Z(lz)], normal: [0, -1, 0], size: [w, d, 0.1], color, range, intensity }); },
    pool(n, lx, lz, size, color, range, intensity, topY = 0) { f.light({ id: `${id}-${n}`, kind: "pool", at: [f.X(lx), r2(topY + 0.025), f.Z(lz)], size: [size, size, 0.05], color, range, intensity }); },
    yawTo(lx, lz, tlx, tlz) { return r2(-Math.atan2(tlx - lx, tlz - lz) * 180 / Math.PI); }, // yaw 0 looks north (+lz = world -z); + yaw turns left (west)
  };
  return f;
}
const both = (what) => `to the player a lip return (${K.FALL_HP_FIGHT} health, back on the lip it was crossed from: A-4); to an enemy ${what}: ended by rooms43 after 2 s under the floor, so the door counts it but it gives NO pull back (no kill event: a known limit tonight, the lead's card gate)`;
/// the room's lip returns (fallReturn43 `by`): each entry [lx0, lz0, lx1, lz1, toLx, toY, toLz]
function lipReturns(f, list, why) {
  const by = list.map(([a, b, c, d, tx, ty, tz]) => ({ rect: f.voidRect(a, b, c, d), to: f.pt(tx, ty, tz), yawDeg: 0 }));
  f.r.fallReturn43 = { belowY: K.FALL_Y, to: by[0].to, yawDeg: 0, damage: K.FALL_HP_FIGHT, by, _why: why };
}

// ================================================================ p3 THE TIE-OFF (bridge)
{
  const f = Frame("p3");
  const VS = K.P3_VOID_S, VN = K.P3_VOID_N, C0 = K.P3_CABLE_X0, C1 = K.P3_CABLE_X1, AI = K.P3_ANCH_IN, AO = K.P3_ANCH_OUT, AS = K.P3_ANCH_S, AN = K.P3_ANCH_N, AY = K.P3_ANCH_Y;
  const W = f.r.rect[2] - f.cx, L = f.r.rect[3] - f.r.rect[1]; // 20, 40
  // the void band (card: Spatial law, Hazard both ways; flow plan 6): wall to wall between the anchorages, except the cable
  const T = 0.3;
  f.rail(-AI, VS - T, C0, VS, 0, "the entry deck's lip (west of the cable)");
  f.rail(C1, VS - T, AI, VS, 0, "the entry deck's lip (east of the cable)");
  f.rail(-AI, VN, C0, VN + T, 0, "the exit deck's lip (west of the cable)");
  f.rail(C1, VN, AI, VN + T, 0, "the exit deck's lip (east of the cable)");
  f.rail(C0, VS, C0 + T, VN, 0, "the cable's west side (the file stays on it)");
  f.rail(C1 - T, VS, C1, VN, 0, "the cable's east side");
  // no rail on the anchorages' inner edges (round 3): the spitters there must be in the strafe deck's line of fire
  f.void(-AI, VS, C0, VN, `THE TIE-OFF's VOID (west of the cable, lz ${VS}..${VN}; card: Hazard both ways): the floor that fell between the anchorages; ${both("knocked off the cable")}`);
  f.void(C1, VS, AI, VN, `THE TIE-OFF's VOID (east of the cable, lz ${VS}..${VN}; card: Spatial law, flow plan 6): it cuts the room's straight line: the cable, the east anchorage or a pull; ${both("punched, whipped or knocked in")}`);
  // the anchorages (card: Every-solid rule; flow plan 3a / 3b): the spitters' stands and the swings' landings; AE the walkers' second crossing
  f.box("deck", -AO, AS, -AI, AN, 0, AY, "line", `the WEST ANCHORAGE (AW, ${AY} m, lx ${-AO}..${-AI}, lz ${AS}..${AN}; card: Molecule's AW): a spitter's stand in the open, the south swing's landing; its north-east edge is the one-way drop onto the exit deck (flow plan 3c)`);
  f.box("deck", AI, AS, AO, AN, 0, AY, "line", `the EAST ANCHORAGE (AE, ${AY} m, lx ${AI}..${AO}, lz ${AS}..${AN}; card: Molecule's AE): a spitter's stand, the north swing's landing, and with its two ramps the walkers' second crossing (the ground loop, flow plan 3a)`);
  const RW = 3, RX = AI + RW / 2, RUN = 8;
  f.ramp(-RX, AS - RUN, AS, RW, AY, `AW's SOUTH RAMP (${AY} m over ${RUN} m; card: Every-solid rule): the walkers' way up from the entry deck (GG NIGHT20) and the player's; AW has no other ramp`);
  f.ramp(RX, AS - RUN, AS, RW, AY, `AE's SOUTH RAMP (${AY} m over ${RUN} m; flow plan 3a): the ground loop's way back to the entry deck, the walkers' route`);
  f.ramp(RX, AN + RUN, AN, RW, AY, `AE's NORTH RAMP (${AY} m over ${RUN} m; flow plan 3a): the ground loop's way up from the exit deck (the swarmers' long way round)`);
  // the masonry (card: Every-solid rule): closes the room to ~30 m and frames the door; no pocket behind the anchorages
  f.box("hull", -W, 0, -AO, L, 0, f.h, "line", "the WEST ANCHORAGE MASONRY (roof-high, behind AW; card: Every-solid rule): the room ~30 m wide, no strip behind the stand");
  f.box("hull", AO, 0, W, L, 0, f.h, "line", "the EAST ANCHORAGE MASONRY (roof-high, behind AE): the room ~30 m wide");
  f.box("hull", -AO, 0, -(AI + RW), AS, 0, f.h, "line", "the masonry beside AW's south ramp (roof-high): the entry deck's west corner closed (the door frames the room)");
  f.box("hull", AI + RW, 0, AO, AS, 0, f.h, "line", "the masonry beside AE's south ramp (roof-high): the entry deck's east corner closed");
  f.box("hull", -AO, AN, -AI, L, 0, f.h, "line", "the masonry north of AW (roof-high): AW's only ways off are its ramp, the drop and the pulls (flow plan 3c / 3d)");
  f.box("hull", AI + RW, AN, AO, L, 0, f.h, "line", "the masonry beside AE's north ramp (roof-high): the exit deck's east corner closed");
  // cover (flow plan 1 foothold, 5): 1.2 m blocks, one jump
  // the clamp off the door's axis, in front of the spawn (r2 had it lx -3..3, 5 m in front of the door: the lead's a146 frame from the door showed
  // "a big block" filling the middle and no enemy, no landmark: Part D 1, the lip shows the arena and wave 1)
  // (round 3 b: in front of the spawn, lx 3..7 lz 9..10, p3's dwell rose 3.4 -> 5.4 s: the pilot's way out of its anchor was blocked; now west)
  f.box("hull", -7.5, 7, -3.5, 8, 0, 1.2, "cover", "the cable's CLAMP (1.2 m; flow plan 1 first foothold, 7 m in at the cable's south end, OFF the door's line of sight and the spawn's way in): cover from the anchorages' spitters, one jump");
  // the KNOT's mass (card: Landmark): the main cable's tied-off end wrapped round the KNOT ring's hanger, a solid hung from the roof over the ring
  // (0.6 m above it: the pull's line of sight ends at the ring), under the amber lamp, dead ahead of the door 26 m in: the thing you read on entry
  f.box("hull", -1.75, K.P3_KNOT_Z - 1.75, 1.75, K.P3_KNOT_Z + 1.75, K.P3_KNOT_Y + 0.6, f.h, "marker", "the great KNOT (card: Landmark): the main cable's tied-off end round the KNOT ring's hanger, hung from the roof over the ring (the shortcut's pull), dead ahead from the door under the room's one amber lamp; out of reach (no one stands on it)");
  f.box("hull", -11, 8, -9, 9, 0, 1.2, "cover", "a BOLLARD at the cable's south end (1.2 m; flow plan 5, the south-west quadrant): the file's last cover before the deck");
  f.box("hull", -8, 33, -4, 34, 0, 1.2, "cover", "a COIL of slack cable (1.2 m; flow plan 5, the north-west quadrant; card: the swarmers' nest)");
  f.box("hull", 3, 33, 7, 34, 0, 1.2, "cover", "a second COIL (1.2 m; flow plan 5, the north-east quadrant)");
  // the rings (card: Molecule RS / RN / K; flow plan 3b): the swings between the anchorages and the KNOT's straight crossing
  const RS = f.pt(-K.P3_RING_X, K.P3_RING_Y, VS), RN = f.pt(K.P3_RING_X, K.P3_RING_Y, VN), KN = f.pt(0, K.P3_KNOT_Y, K.P3_KNOT_Z);
  f.r.grappleNodes = [RS, KN, RN];
  f.r.grapple = `THREE rings (card: Molecule): RN over the exit deck's lip ${K.P3_RING_X} m east (AW -> AE) and RS over the entry deck's lip ${K.P3_RING_X} m west (AE -> AW), ${K.P3_RING_Y} m: the swings anchorage to anchorage, one way round (the air loop, two pulls a lap); the KNOT dead ahead of the door (${K.P3_KNOT_Y} m, lz ${K.P3_KNOT_Z}): one pull over the void onto the exit deck (the shortcut). Placed by \`n46span reach\` (the carry measured)`;
  // the fight (card: The 30 s problem, State change; flow plan 8)
  const W1 = [f.pt(-7, 0, 31), f.pt(-3, 0, 32), f.pt(6, 0, 31), f.pt(10, 0, 32)];
  const W2 = [f.pt(-13, AY, 22), f.pt(-13, AY, 27), f.pt(13, AY, 22), f.pt(13, AY, 27), f.pt(13, AY, 17)]; // round 3: at the inner edge (in the deck's line of fire)
  const W3 = [f.pt(-7, 0, 30), f.pt(-5, 0, 36), f.pt(0, 0, 37), f.pt(5, 0, 36), f.pt(-7, 0, 27), f.pt(8, 0, 35)];
  f.r.nodes = [...W1, ...W3];
  f.r.highNodes = W2;
  f.r.waveNodes43 = [W1, W2, W3];
  f.r.waveNodes43_why = "NIGHT46 A1 (THE TIE-OFF's flow plan 8): wave 1 on the exit deck at the cable's and AE's north ends (in view from the door, 31-33 m); wave 2 on the anchorage tops "
    + "(the spitters in the open; its husks come down the ramps); wave 3 from the coils and the cable's north end (the card's State change: the crossing becomes their file)";
  lipReturns(f, [[-AI, VS, AI, (VS + VN) / 2, 0, 0, VS - 1.5], [-AI, (VS + VN) / 2, AI, VN, 0, 0, VN + 1.5]],
    "NIGHT46 A1 (THE TIE-OFF's Hazard both ways; A-4): feet under the fall line in the void band -> back on the deck lip it was crossed from (the south half to the entry deck's lip, the north half to the exit deck's), 5 health");
  // the light story (card: Light / colour story)
  f.ceil("knot", 0, K.P3_KNOT_Z + 2.5, 2, 2, "amber", 13, 1.5); // the KNOT's lamp: the brightest point (beside the knot's mass, which reaches the roof)
  f.ceil("aw", -15, 24, 3, 4, "white", 8, 0.8); f.ceil("ae", 15, 24, 3, 4, "white", 8, 0.8); // the spitters' stands
  f.ceil("exit", 0, 35, 4, 3, "white", 10, 1.0);
  f.pool("pool", 0, 37.5, 2.5, "cyan", 7, 1.6); // the exit
  f.r.proofs = [
    { kind: "walk", points: [f.pt(0, 0, 2), f.pt(-1.5, 0, 9.3), f.pt(-7, 0, VS - 1), f.pt(-7, 0, VN + 1), f.pt(0, 0, 38)] }, // round 3: past the clamp's east end
    { kind: "walk", points: [f.pt(RX, 0, AS - RUN + 0.5), f.pt(RX, AY, AS + 1), f.pt(15, AY, AN - 1), f.pt(RX, AY, AN + 0.3), f.pt(RX, 0, AN + RUN - 0.3)] },
    { kind: "walk", points: [f.pt(-RX, 0, AS - RUN + 0.5), f.pt(-RX, AY, AS + 1), f.pt(-15, AY, AN - 1)] },
  ];
  f.r.frames36 = { mid: [f.X(0), 0, f.Z(3), f.yawTo(0, 3, 0, 20), 0.06], exit: [f.X(8), 0, f.Z(34), f.yawTo(8, 34, 0, 40), -0.05] };
  f.r.verb36 = "kills give pulls: swing deck lip to anchorage over the void (two pulls a lap); the KNOT straight over";
  // the room's spawn (the retry point and the walk pilot's anchor in a fight) at the first foothold, beside the clamp, 7 m in (was lz 2, 2 m from
  // the door): F17 (commit, don't doorway-camp; Part D 1). r1's meter: dwell 2.2 -> 5.4 s, the walkers coming to the pilot at the door
  if (f.r.spawn.at[2] !== f.Z(2)) throw new Error(`p3's spawn moved in n40: ${f.r.spawn.at}`);
  f.r.spawn = { at: f.pt(5, 0, 7.5), yawDeg: 0 };
  f.r.spawn_why = "NIGHT46 A1 (F17, Part D 1): the retry point at the first foothold (the clamp), 7.5 m inside the door; the door itself carries your speed in";
  // the lesson "KILLS GIVE PULLS." on 4 husk kills (was 6 of the room's 9): a husk knocked into the void is ended, not killed, so 6 kills were often
  // never reached and rooms43's lessonCapS held the door 8 s after the last body (n46flow r2: p3's dead time 5.2 -> 10.4 s; FLOW-AUDIT row 11)
  for (const k of ["lessons", "lessons31"]) for (const l of f.r[k] || []) if (l.kind === "husksDead") { if (l.n !== 6) throw new Error(`p3's ${k} husksDead n is ${l.n}, not n40's 6`); l.n = 4; }
  f.r.span46_why = `A1 (span46, THE TIE-OFF): n40's p3 cut (${cut.p3.boxes} boxes, ${cut.p3.ramps} ramps, ${cut.p3.lights} lights); void lz ${VS}..${VN}; cable lx ${C0}..${C1}; anchorages lx +-${AI}..${AO} lz ${AS}..${AN} ${AY} m; rings ${K.P3_RING_Y} / knot ${K.P3_KNOT_Y} m`;
}

// ================================================================ p1 THE HOOK YARD (walled)
{
  const f = Frame("p1");
  const W = 16, L = 36;
  const TX = K.P1_TERR_X, TS = K.P1_TERR_S, TY = K.P1_TERR_Y, DH = K.P1_DRUM_HALF, DS = K.P1_DRUM_S, DN = K.P1_DRUM_N, DY = K.P1_DRUM_Y, WX = K.P1_WELL_X, WS = K.P1_WELL_S, WN = K.P1_WELL_N;
  // the hoist houses 3 m deep and the passage 12 m wide (r1: 5 m deep, 8 m wide): n46flow r2 measured p1's dwell 1.0 -> 3.8 s and the stall
  // 16.8 -> 22.2 %: the walkers met the pilot inside the passage (the dwell's breakdown: husks / swarmers at 5-10 m, the pilot in the air at its spawn)
  f.box("hull", -W, 0, -6, 3, 0, f.h, "line", "the WEST HOIST HOUSE (roof-high, lz 0..3; card: Every-solid rule, Light story): the short dark entry passage's wall and the yard's south wall (the frame round the first view)");
  f.box("hull", 6, 0, W, 3, 0, f.h, "line", "the EAST HOIST HOUSE (roof-high): the entry passage 12 m wide and 3 m deep; it opens on the whole yard at lz 3 (flow plan 1)");
  f.box("deck", -W, TS, TX, L, 0, TY, "line", `the LOADING TERRACE (${TY} m, lx ${-W}..${TX}, lz ${TS}..${L}; card: Molecule's T): the spitters' stand and the GREAT HOOK pull's landing; its front edge is the one-way drop (flow plan 3c)`);
  f.ramp(-12, TS - 10, TS, 3, TY, `the terrace's RAMP (${TY} m over 10 m; card: Every-solid rule): the walkers' way up (GG NIGHT20) and the slow way under the spitters' fire`);
  f.box("hull", -DH, DS, DH, DN, 0, DY, "cover", `the CABLE DRUM standing on its flange (${DY} m, ${2 * DH} x ${DN - DS} m, out of a double jump; card: Hero prop): the ground loop's centre (flow plan 3a: the strafe round it), full cover from the terrace's spitters on one side at a time`);
  f.rail(WX - 0.3, WS, WX, WN, 0, "the hoist well's west lip (the east lane's side)");
  f.rail(WX, WS - 0.3, W - 0.6, WS, 0, "the hoist well's south lip");
  f.rail(WX, WN, W - 0.6, WN + 0.3, 0, "the hoist well's north lip");
  f.void(WX, WS, W, WN, `the HOIST WELL (lx ${WX}..${W}, lz ${WS}..${WN}; card: Hazard both ways, flow plan 6): it cuts the yard's east side so the ride over it is the faster way north; ${both("launched or whipped in")}`);
  f.box("wall", W - 0.6, WS - 2, W, WN + 2, K.VOID_Y, f.h, "line", "the HOIST WELL's RIDE PANEL (the east wall over the well, floor to roof, cyan trim; card: Every-solid rule, flow plan 3b): the air loop's ride north between the two pulls (roof-high: `n46span reach` round 2 landed the hoist pull's pop ON a 7 m panel's top)",
    { ride: true, ride36: { face: "W", planeX: f.X(W - 0.6), lz: [WS - 2, WN + 2] } });
  f.box("hull", 10, 28, W, L, 0, f.h, "line", "the ENGINE HOUSE (roof-high, the north-east corner; card: Every-solid rule): no pocket behind the well, the exit framed");
  f.box("hull", -6, 8, -2, 9, 0, 1.2, "cover", "a cable CRATE (1.2 m; flow plan 1 first foothold, 7 m from the spawn; 5: the south-west quadrant)");
  f.box("hull", 3, 9, 6, 10, 0, 1.2, "cover", "a cable crate (1.2 m; flow plan 5: the south-east quadrant)");
  f.box("hull", -6, 29, -3, 30, 0, 1.2, "cover", "a cable crate (1.2 m; flow plan 5: the north-west quadrant, under the terrace's spitters)");
  f.box("hull", 3, 29, 6, 30, 0, 1.2, "cover", "a cable crate (1.2 m; flow plan 5: the north-east quadrant)");
  const HOOK = f.pt(TX, K.P1_HOOK_Y, 24), HOIST = f.pt(13.5, K.P1_HOIST_Y, (WS + WN) / 2); // 1.9 m off the ride face: the arrival leaves you at the wall
  f.r.grappleNodes = [HOOK, HOIST];
  f.r.grapple = `TWO rings (card: Landmark, Molecule): the GREAT HOOK's eye over the terrace's front edge (${K.P1_HOOK_Y} m; the lesson's ring: a pull from the floor lands you on the terrace), and the HOIST BLOCK over the well (${K.P1_HOIST_Y} m): drum -> hoist -> the well's ride -> the hook (the air loop)`;
  const W1 = [f.pt(-6, 0, 17), f.pt(6, 0, 19), f.pt(-6.5, 0, 23)]; // in the two lanes beside the drum: in view past its sides from the spawn
  const W2 = [f.pt(-12, TY, 32), f.pt(-10, TY, 34), f.pt(-2, 0, 31), f.pt(2, 0, 33), f.pt(6, 0, 32), f.pt(-5, 0, 33)];
  f.r.nodes = [...W1, ...W2.filter((p) => p[1] === 0)];
  f.r.highNodes = W2.filter((p) => p[1] > 0);
  f.r.waveNodes43 = [W1, W2];
  f.r.waveNodes43_why = "NIGHT46 A1 (THE HOOK YARD's flow plan 8): wave 1 round the drum's north side (in view from the spawn, 21-23 m); wave 2 on the terrace's north end and the north apron (ahead of a player who cleared wave 1)";
  lipReturns(f, [[WX, WS, W, WN, 11, 0, WS - 1.5]], "NIGHT46 A1 (THE HOOK YARD's Hazard both ways; A-4): feet under the fall line in the hoist well -> back on its south lip, 5 health");
  f.ceil("yard", 0, 18, 4, 4, "white", 10, 1.0);
  f.ceil("hook", TX, 24, 3, 3, "amber", 12, 1.5); // the GREAT HOOK's lamp: the brightest point
  f.pool("land", TX - 2.5, 26, 2.5, "cyan", 6, 1.2, TY); // the hook's landing on the terrace
  f.pool("pool", 0, 34, 2.5, "cyan", 7, 1.6); // the exit
  f.r.proofs = [
    { kind: "walk", points: [f.pt(0, 0, 2), f.pt(0, 0, 12), f.pt(5.5, 0, 19), f.pt(0, 0, 34)] },
    { kind: "walk", points: [f.pt(0, 0, 2), f.pt(0, 0, 12), f.pt(-5.5, 0, 19), f.pt(-5.5, 0, 27), f.pt(0, 0, 34)] },
    { kind: "walk", points: [f.pt(-12, 0, TS - 10.5), f.pt(-12, TY, TS + 1), f.pt(-12, TY, 30)] },
  ];
  f.r.frames36 = { mid: [f.X(0), 0, f.Z(5), f.yawTo(0, 5, -3, 24), 0.1], exit: [f.X(4), 0, f.Z(26), f.yawTo(4, 26, 0, 36), 0] };
  f.r.verb36 = "the first pull: the GREAT HOOK's eye from the floor onto the terrace; the air loop drum -> hoist ring -> the well's ride -> the hook";
  f.r.span46_why = `A1 (span46, THE HOOK YARD): n40's p1 cut (${cut.p1.boxes} boxes, ${cut.p1.ramps} ramps, ${cut.p1.lights} lights); terrace lx -16..${TX} lz ${TS}..36 ${TY} m; drum +-${DH} lz ${DS}..${DN} ${DY} m; well lx ${WX}..16 lz ${WS}..${WN}`;
}

// ================================================================ p5 THE SPOOL HALL (islands)
{
  const f = Frame("p5");
  const W = 18, L = 36;
  const PX0 = K.P5_PIT_X0, PX1 = K.P5_PIT_X1, PS = K.P5_PIT_S, PN = K.P5_PIT_N, SH = K.P5_SPOOL_HALF, SS = K.P5_SPOOL_S, SN = K.P5_SPOOL_N, SY = K.P5_SPOOL_Y;
  const GX = K.P5_GAL_X, GS = K.P5_GAL_S, GY = K.P5_GAL_Y, TX = K.P5_TAIL_X, TN = K.P5_TAIL_N, TY = K.P5_TAIL_Y;
  const PAY = 1.5; // the payout ramp's half width (3 m)
  const pit = `the WINDING PIT (card: Hazard both ways, Spatial law; flow plan 6): it makes the spool an island from the entry side and cuts the hall's middle so the walkway round it is the strafe loop; ${both("whipped short or knocked off a walkway")}`;
  f.rail(PX0, PS - 0.3, -PAY, PS, 0, "the winding pit's south lip (the entry walkway, west of the gangway)");
  f.rail(PAY, PS - 0.3, PX1, PS, 0, "the winding pit's south lip (east of the gangway)");
  f.rail(PX0, PN, -PAY, PN + 0.3, 0, "the winding pit's north lip (west of the payout)");
  f.rail(PAY, PN, PX1, PN + 0.3, 0, "the winding pit's north lip (east of the payout)");
  f.rail(PX0 - 0.3, PS, PX0, PN, 0, "the winding pit's west lip (the west walkway)");
  f.rail(PX1, PS, PX1 + 0.3, GS, 0, "the winding pit's east lip beside the gallery's ramp");
  f.rail(GX, GS, GX + 0.3, PN, GY, "the gallery's inner edge over the pit (its north end stays open: the one-way drop)");
  f.void(PX0, PS, -PAY, SS, pit + " (south-west, beside the gangway)");
  f.void(PAY, PS, PX1, SS, pit + " (south-east, beside the gangway)");
  f.void(PX0, SS, -SH, SN, pit + " (west of the spool)");
  f.void(SH, SS, PX1, SN, pit + " (east of the spool)");
  f.void(PX0, SN, -PAY, PN, pit + " (north-west, beside the payout)");
  f.void(PAY, SN, PX1, PN, pit + " (north-east, beside the payout)");
  f.box("deck", -SH, SS, SH, SN, 0, SY, "line", `the GREAT SPOOL (${SY} m, ${2 * SH} x ${SN - SS} m, alone in the pit; card: Landmark, Hero prop): the island the wave-2 spitters and the health stand on (the whip's and the pull's target), the air loop's centre node`);
  f.ramp(0, PS, SS, 2 * PAY, SY, `the spool's WINDING GANGWAY (${SY} m over ${SS - PS} m, from the south walkway; card: Molecule, the r2 change): the walkers' straight way from the spool to the door side (r1's meter: with only the west walkway round, p5's dwell 15.2 s and dead 11.4 s), the strafe loop's centre crossing`);
  f.ramp(0, PN, SN, 2 * PAY, SY, `the spool's CABLE PAYOUT (${SY} m over ${PN - SN} m, from the north walkway; card: Every-solid rule): the walkers' way onto the island (GG NIGHT20: no floor the player stands on that they cannot reach) and the strafe loop's centre crossing north`);
  f.box("deck", GX, GS, W, PN, 0, GY, "line", `the GALLERY (${GY} m, lx ${GX}..${W}, lz ${GS}..${PN}; card: Molecule's E): the spitters' high stand; its north end is the one-way drop that breaks the circle (flow plan 3c)`);
  f.ramp(16, 2, GS, 3, GY, `the gallery's RAMP (${GY} m over ${GS - 2} m; card: Every-solid rule): the walkers' way up and the ground loop's east leg (flow plan 3a)`);
  f.box("deck", -W, 0, TX, TN, 0, TY, "line", `the TAIL SPOOL (${TY} m, the south-west corner; card: Molecule's T): the air loop's west node and the south wall ride's take-off (flow plan 3b); a double jump or its ramp up`);
  f.ramp(-16.5, 12, TN, 3, TY, `the tail spool's RAMP (${TY} m over ${12 - TN} m, along the west wall; card: Every-solid rule): the walkers' way up, the player's way down to the west walkway`);
  f.box("wall", -13, 0, 6, 0.6, 0, f.h, "line", "the SOUTH WALL's RIDE PANEL (lx -13..6, floor to roof: no top ledge to land on, cyan trim; card: Every-solid rule, flow plan 3b): the air loop's ride east from the tail spool to the gallery's ring (the door is at lx 8..12)",
    { ride: true });
  f.box("hull", 3, 6.5, 7, 7.5, 0, 1.2, "cover", "a WINCH block (1.2 m; flow plan 1 first foothold, 6 m from the door; 5: the south-east quadrant)");
  f.box("hull", -10, 3, -8, 4, 0, 1.2, "cover", "a BOLLARD (1.2 m; flow plan 5: the south-west quadrant)");
  f.box("hull", -12, 32, -9, 33, 0, 1.2, "cover", "a cable block (1.2 m; flow plan 5: the north-west quadrant, by the exit)");
  f.box("hull", 4, 32, 7, 33, 0, 1.2, "cover", "a cable block (1.2 m; flow plan 5: the north-east quadrant)");
  const R1 = f.pt(0, 10, (SS + SN) / 2), R2 = f.pt(-16, 8, 2), R3 = f.pt(15.5, 9, GS + 2);
  f.r.grappleNodes = [R1, R2, R3];
  f.r.grapple = "THREE rings (card: Molecule, flow plan 3b): R1 over the GREAT SPOOL (10 m), R2 over the tail spool (8 m), R3 over the gallery's south end (9 m): gallery -> spool -> tail spool -> the south wall's ride -> gallery";
  p5Health.at = [f.X(0), r2(SY + 0.9), f.Z((SS + SN) / 2)];
  p5Health.why = "the room's health on the GREAT SPOOL's top (card: the pickup; DECIDE46 N-9: bait): the exposed island, reached by a pull, the payout or the 6 m jump, under the spitters";
  // r2: wave 1 on the spool (in view over the 1 m rail from the door: the body is above the eye) and the north walkway east of the payout;
  // they come down the gangway at you
  const W1 = [f.pt(-3, SY, 17), f.pt(3, SY, 17), f.pt(0, SY, 21), f.pt(4, 0, 31), f.pt(8, 0, 31), f.pt(-4, 0, 31)];
  const W2 = [f.pt(-2, SY, 18), f.pt(2, SY, 21), f.pt(-14, 0, 15), f.pt(-14, 0, 21), f.pt(-13, 0, 26)];
  const W3 = [f.pt(15.5, GY, 27), f.pt(0, SY, 18), f.pt(-6, 0, 32), f.pt(-12, 0, 34), f.pt(0, 0, 34), f.pt(8, 0, 34)];
  const all = [...W1, ...W2, ...W3];
  f.r.nodes = all.filter((p) => p[1] === 0);
  f.r.highNodes = all.filter((p) => p[1] > 0);
  f.r.waveNodes43 = [W1, W2, W3];
  f.r.waveNodes43_why = "NIGHT46 A1 (THE SPOOL HALL's flow plan 8): wave 1 on the north walkway's east half and the gallery's north end (in view from the door, east of the spool); "
    + "wave 2 on the GREAT SPOOL (the captain's island) and the west walkway; wave 3 on the gallery, the spool and the north walkway (ahead of a player going round to the exit). "
    + "A wave's kinds take its nodes at random (Campaign24.SpawnWave): a husk on the spool comes off by the payout";
  const mid = (SS + SN) / 2;
  lipReturns(f, [[PX0, PS, PX1, mid, -3, 0, PS - 1.5], [PX0, mid, PX1, PN, -3, 0, PN + 1.5]],
    "NIGHT46 A1 (THE SPOOL HALL's Hazard both ways; A-4): feet under the fall line in the winding pit -> back on the walkway lip it was crossed from (the south half to the south walkway, the north half to the north walkway), 5 health");
  f.ceil("spool", 0, mid, 3, 3, "amber", 13, 1.5); // the GREAT SPOOL's lamp: the brightest point
  f.ceil("north", -4, 33, 4, 3, "white", 10, 0.9);
  f.pool("pool", -7, 34, 2.5, "cyan", 7, 1.6); // the exit (o1's door)
  f.r.proofs = [
    { kind: "walk", points: [f.pt(10, 0, 2), f.pt(0, 0, 4), f.pt(-12.5, 0, 6), f.pt(-13, 0, 18), f.pt(-13, 0, 31), f.pt(-7, 0, 34)] },
    { kind: "walk", points: [f.pt(16, 0, 1.5), f.pt(16, GY, GS + 1), f.pt(16, GY, PN - 1)] },
    { kind: "walk", points: [f.pt(0, 0, 32), f.pt(0, SY, SN - 1), f.pt(0, SY, SS + 1)] },
  ];
  f.r.frames36 = { mid: [f.X(10), 0, f.Z(3), f.yawTo(10, 3, 0, 18), 0.05], exit: [f.X(-2), 0, f.Z(31), f.yawTo(-2, 31, -7, 36), 0] };
  f.r.verb36 = "the whip: the spitters stand on the GREAT SPOOL across the void: whip one to you, or pull onto the island; the air loop gallery -> spool -> tail spool -> ride -> gallery";
  // the spawn at the first foothold (the winch), 6.5 m in (was lz 2 at the door): F17; r1's meter: p5's dwell 3.6 -> 7.1 s
  if (f.r.spawn.at[2] !== f.Z(2)) throw new Error(`p5's spawn moved in n40: ${f.r.spawn.at}`);
  f.r.spawn = { at: f.pt(9, 0, 6.5), yawDeg: 0 };
  f.r.spawn_why = "NIGHT46 A1 (F17, Part D 1): the retry point at the first foothold (the winch), 6.5 m inside the door";
  f.r.span46_why = `A1 (span46, THE SPOOL HALL): n40's p5 cut (${cut.p5.boxes} boxes, ${cut.p5.ramps} ramps, ${cut.p5.lights} lights); pit lx ${PX0}..${PX1} lz ${PS}..${PN}; spool +-${SH} lz ${SS}..${SN} ${SY} m; gallery lx ${GX}..18 lz ${GS}..${PN} ${GY} m; tail spool ${TY} m`;
}

// ================================================================ the link gaps (FLOW-AUDIT row 3): lip returns, data only, geometry untouched
{
  const lip = (id, list, why) => {
    const r = room(id);
    const by = list.map(([rect, to]) => ({ rect, to, yawDeg: 0 }));
    r.fallReturn43 = { belowY: r.respawnY, to: by[0].to, yawDeg: 0, damage: K.FALL_HP, by, _why: why };
  };
  const g = (id, i) => room(id).pull38.gates[i];
  const [p2x0, p2z0, p2x1, p2z1] = room("p2").rect;
  const p2a = g("p2", 0);
  lip("p2", [[[p2x0, p2a.land[2] + 2, p2x1, p2z1], [0, 0, p2a.lip[2] + 1.5]], [[p2x0, p2z0, p2x1, p2a.land[2] + 2], [0, p2a.land[1], p2a.land[2] - 1]]],
    "NIGHT46 A1 (FLOW-AUDIT row 3; A-4): a fall into THE GAP -> back on the pier's lip 1.5 m before it (was the room's start for 25 % of max health); a fall off the landing's far side -> back on the landing; 10 health");
  const [p4x0, p4z0, p4x1, p4z1] = room("p4").rect;
  const a = g("p4", 0), b = g("p4", 1);
  lip("p4", [[[p4x0, a.land[2] + 2, p4x1, p4z1], [0, 0, a.lip[2] + 1.5]], [[p4x0, b.land[2] + 2, p4x1, a.land[2] + 2], [0, b.lip[1], b.lip[2] + 1.5]], [[p4x0, p4z0, p4x1, b.land[2] + 2], [0, b.land[1], b.land[2] - 1]]],
    "NIGHT46 A1 (FLOW-AUDIT row 3; A-4): a fall into GAP A -> back on the entry pier's lip; into GAP B -> back on pier B's lip (was the room's start for 25 % of max health); off pier C -> pier C; 10 health");
  for (const id of ["o1", "o2"]) {
    const r = room(id);
    const p = plan.pits.find((q) => q.zone === id);
    if (!p) throw new Error(`${id}: no void pit`);
    lip(id, [[r.rect, [r.spawn.at[0], 0, r2(p.rect[3] + 1.5)]]],
      `NIGHT46 A1 (FLOW-AUDIT row 3; A-4): a fall into ${id}'s void -> back on its entry lip 1.5 m before the void (was the room's start for 25 % of max health), 10 health. THE LOOM's own rules in this room are not changed`);
  }
}

// ---- every other room bit for bit (asserted): the copy's rows outside p1 / p3 / p5 equal the source's, except the four links' fallReturn43
{
  const src = JSON.parse(srcText);
  const keep = (k, rowZone) => {
    const A = (src[k] || []).filter((r) => !REBUILT.includes(rowZone(r))).map((r) => JSON.stringify(r));
    const B = (plan[k] || []).filter((r) => !REBUILT.includes(rowZone(r))).map((r) => JSON.stringify(r));
    if (A.length !== B.length || A.some((s, i) => s !== B[i])) throw new Error(`${k}: a row outside p1 / p3 / p5 changed`);
  };
  for (const k of ["boxes", "ramps", "pits", "tunnels", "pads", "grapple"]) keep(k, (r) => r.zone);
  keep("lights", (r) => r.room);
  const pa = src.props.filter((p) => p.room !== "p5").map((p) => JSON.stringify(p)), pb = plan.props.filter((p) => p.room !== "p5").map((p) => JSON.stringify(p));
  if (pa.length !== pb.length || pa.some((s, i) => s !== pb[i])) throw new Error("props: a prop outside p5 changed");
  const LINKS = ["p2", "p4", "o1", "o2"];
  for (const r of src.rooms) {
    if (REBUILT.includes(r.id)) continue;
    const q = Object.assign({}, plan.rooms.find((x) => x.id === r.id));
    if (LINKS.includes(r.id)) delete q.fallReturn43;
    if (JSON.stringify(q) !== JSON.stringify(r)) throw new Error(`room ${r.id} changed`);
  }
  for (const k of Object.keys(src)) if (!["_why", "id", "rooms", "boxes", "ramps", "pits", "tunnels", "pads", "grapple", "lights", "props"].includes(k) && JSON.stringify(src[k]) !== JSON.stringify(plan[k])) throw new Error(`${k} changed`);
}

// ---- the reach check (the lead's card gate note 2): every air-loop pull under 0.9 x the grapple's range (the harness measures the carry)
const cfg = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/unity-config.json"), "utf8"));
const REACH = cfg.design.n32.grapple32.range, LIM = 0.9 * REACH;
const dist = (a, b) => Math.hypot(a[0] - b[0], a[1] - b[1], a[2] - b[2]);
let bad = 0;
const eye = (p) => [p[0], p[1] + 1.6, p[2]];
const F = { p1: Frame("p1"), p3: Frame("p3"), p5: Frame("p5") };
const links = [
  ["p1 terrace front -> HOIST ring", eye(F.p1.pt(-9.5, K.P1_TERR_Y, 21)), room("p1").grappleNodes[1]],
  ["p1 the well's ride (north end) -> GREAT HOOK", F.p1.pt(15, 3, K.P1_WELL_N), room("p1").grappleNodes[0]],
  ["p1 floor -> GREAT HOOK (the lesson)", eye(F.p1.pt(-4, 0, 14)), room("p1").grappleNodes[0]],
  ["p3 AW north end -> RN", eye(F.p3.pt(-15, K.P3_ANCH_Y, K.P3_VOID_N - 1)), room("p3").grappleNodes[2]],
  ["p3 AE south end -> RS", eye(F.p3.pt(15, K.P3_ANCH_Y, K.P3_VOID_S + 1)), room("p3").grappleNodes[0]],
  ["p3 entry deck -> KNOT", eye(F.p3.pt(0, 0, 8)), room("p3").grappleNodes[1]],
  ["p5 gallery -> R1 (spool)", eye(F.p5.pt(15.5, K.P5_GAL_Y, 20)), room("p5").grappleNodes[0]],
  ["p5 spool -> R2 (tail spool)", eye(F.p5.pt(-3, K.P5_SPOOL_Y, 17)), room("p5").grappleNodes[1]],
  ["p5 the south ride's east end -> R3 (gallery)", F.p5.pt(6, 3, 1), room("p5").grappleNodes[2]],
];
console.log(`grapple reach ${REACH} m (design.n32.grapple32.range); a route pull must be under 0.9 x = ${LIM} m`);
for (const [label, a, b] of links) { const d = dist(a, b); if (d > LIM) bad++; console.log(`  ${d <= LIM ? "ok  " : "FAIL"} ${d.toFixed(1).padStart(5)} m  ${label}`); }

fs.mkdirSync(path.dirname(OUT_FILE), { recursive: true });
fs.writeFileSync(OUT_FILE, JSON.stringify(plan, null, 1) + "\n");
for (const z of REBUILT) console.log(`  ${z}: ${room(z).span46_why}`);
console.log("wrote", path.relative(ROOT, OUT_FILE), plan.rooms.length, "rooms", plan.boxes.length, "boxes", plan.pits.length, "pits", plan.lights.length, "lights");
if (bad) { console.log(`RED: ${bad} pull(s) over ${LIM} m`); process.exit(1); }
console.log("GREEN: every air-loop pull under the margin");
