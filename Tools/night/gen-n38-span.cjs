// NIGHT38 builder O2 (NIGHT38.md L4; Research/2026-10-13-wave38/PLAN.md section 2 L4): LEVEL 4 THE SPAN rebuilt for flight as n38-span, from its
// beat sheet (Design/levels/n38/span.beats.md, written first). n37-span's copied rooms s1-s7 are gone: five NEW rooms p1-p5 (open roofed arenas
// with grapple nodes, two flight rooms of grapple GATES over 56 m voids), then NIGHT37's o1 THREAD LINE, o2 SWING ANCHOR and o3 THE CANYON copied
// AS THEY ARE (same world coordinates, rows and solids) from Design/levels/n37/span.plan.json: THE LOOM's anchors / ledges / stations, the
// Loom37 pilot and the loom37 harness read them. The new rooms are laid so p5's north wall is s7's old one (o1's door unchanged).
// Every new solid carries job + why (n36-lib); every gate landing carries `gate {verb, from, to}` (builder E's checker, `n38 gates`); the side
// walls over a void carry a `noride` liner (builder M's flag: a wall the Mover must not ride).
// The reach check (real numbers, design.n32.grapple32.range): every route pull must be under GATE_MARGIN x the range, or the script FAILS.
// Run, from the repo (or worktree) root:  node Tools/night/gen-n38-span.cjs   then  bash Tools/night/n38o-regen.sh
const fs = require("fs");
const path = require("path");
const { Level, MOVE, r3 } = require("./n36-lib.cjs");

const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/levels/n37/span.plan.json");
const OUT = path.join(ROOT, "Design/levels/n38/span.plan.json");
const KEEP = ["o1", "o2", "o3"];

const cfg = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/unity-config.json"), "utf8"));
const G32 = cfg.design.n32.grapple32;
const REACH = G32.range;
const GATE_MARGIN = 0.9; // D-6: the intended verb crosses with >= 10 % margin of its envelope (the grapple's range)

const src = JSON.parse(fs.readFileSync(SRC, "utf8"));
const kept = (z) => KEEP.includes(z);
const o1 = src.rooms.find((r) => r.id === "o1");
const d8 = src.doors.find((d) => d.between[1] === "o1");
const NORTH_Z0 = d8.rect[3]; // s7's old north wall (z -142.5): p5 ends here, o1's door is unchanged
const OUT_X = (d8.rect[0] + d8.rect[2]) / 2; // o1's door centre (x -17)

// ---- the void gates (the beat sheet's numbers; builder E's envelope table, Design/levels/envelopes38.json, says why)
// Round 1 (n38 gates RED 0 / 3 on 56 m two-pull gates 4 m up): the checker proves the grapple by ONE hook from the take-off (line of sight, range), and
// the THUNDERCLAP chain (jump + double + boost x3) reaches 60.3 m at the take-off's height with an apex of 8.48 m. So a gate is ONE pull up a RISE the
// boost chain cannot climb: the landing 10 m above the take-off (8.48 x 1.1 = 9.3), its front face 30 m out (inside 0.9 x the 40 m range from the eye).
const VOID = 30; // m of void per gate (nothing to stand on)
const RISE = 10; // m the landing stands above the take-off (over the best non-grapple apex, envelopes38.json ground["jump+double+boost x3"].apex 8.48, + 10 %)
const PIT_Y = -14; // the void's floor (a fall is caught by respawnY first)
const RESPAWN_Y = -3; // feet below this in a void room: back at the room's start (Campaign24; design.n29.roomRules.fallCostFraction of health)
const RING_IN = 3, RING_UP = 5; // the landing ring: lz past the landing's lip, height over the landing
const links = []; // [label, from, to] every route pull (the reach check)
const near = (a, b) => Math.hypot(a[0] - b[0], a[1] - b[1], a[2] - b[2]);
const ENV = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/levels/envelopes38.json"), "utf8"));
// NIGHT39 E2: the gate rooms are LIMIT pistolOnly (FLIGHT below): no THUNDERCLAP boost, no PULSE blast, no shockwave there, so the rise is
// checked against the apex of the families those rooms keep (the boost chain's 16.26 m apex since E's NIGHT38 round-4 envelope is a LIMIT's job)
const APEX = Math.max(...Object.entries(ENV.ground).filter(([k]) => !/boost|pulse|shockwave/.test(k)).map(([, g]) => g.apex));
if (RISE < APEX * 1.1) throw new Error(`the rise ${RISE} m is under the best non-grapple apex ${APEX} m + 10 % (envelopes38.json)`);

const L = new Level("n38-span", "LEVEL 4 THE SPAN",
  "NIGHT38 builder O2 (design.n38.span38 swaps n37-span for this file): Tools/night/gen-n38-span.cjs from the beat sheet Design/levels/n38/span.beats.md. "
  + "Five new rooms (p1 STITCH YARD, p2 FIRST GAP, p3 KNOT YARD, p4 PULL LINE, p5 SPOOL HALL), then NIGHT37's o1 THREAD LINE, o2 SWING ANCHOR and o3 THE CANYON "
  + "(THE LOOM) copied as they are. Spacing from " + MOVE.src + " and design.n32.grapple32.",
  { n37: true, n38: true, n32: src.n32, wallHeight: src.wallHeight, killPlaneY: -40,
    killPlaneY_why: "under THE CANYON's void floor (-24) and the flight rooms' void floor (-14): a fall is caught by the room's respawnY / THE LOOM's ledge rule first",
    rank: Object.assign({}, src.rank, { _why: "Unity-only (builder O2, NIGHT38): NIGHT30's S/A/B/C cuts kept for level 4 (n37-span's); NOT calibrated on the n38 rooms (the walk bot's times come first)." }) });

const ARENA = (r) => { r.row.arena36 = true; }; // ULTRAKILL's arena flow (Game/SurvivalSim.Level36.cs): doors red on entry, spawn bubbles, the last-kill slow
const FLIGHT = (r) => {
  r.row.loomRoom37 = true; // THE CANYON's rules start here (Game/SurvivalSim.Loom37.cs on n38-span): a 0.8 s ride gives a charge back, a pull's arrival stops
  r.row.respawnY = RESPAWN_Y;
  r.row.respawnY_why = "a fall into the void is back at the room's start (Campaign24's respawnY, a share of health: design.n29.roomRules.fallCostFraction): 1 m of drop under the take-off is already a miss, so no move but the pull lives long enough over the void to cross it";
  r.row.noride38 = true; // the room's side walls over the void must not be ridden (builder M's flag; the liners below carry it per solid)
  // NIGHT39 E2 (the lead's round-2 card): Campaign24's room LIMIT pistolOnly, as n38-spire's gate rooms. It leaves the KESTREL alone (the owned list is
  // restored at the door), so the THUNDERCLAP's boost chain (jump + boost x3 + the apex double: 15-25 crossings of each 30 m / 10 m-rise gate on E's
  // checker since NIGHT38 round 4) and the PULSE blast cannot cross. The grapple is not a weapon and stays (measured: `n38 limits n38-span p2` / `p4`).
  r.row.limit = ["pistolOnly"];
  r.row.limitReason = "PULL ACROSS";
};
/// the side walls of a void room: a 0.2 m `noride` liner on each, floor of the void to the roof (builder M's flag; dull, no cyan trim)
const liners = (r, lz0, lz1) => {
  for (const [a, b, side] of [[-r.w / 2, -r.w / 2 + 0.2, "west"], [r.w / 2 - 0.2, r.w / 2, "east"]]) {
    const x = r.box("hull", a, lz0, b, lz1, PIT_Y, r.h, "line", `the ${side} wall's NORIDE liner over the void (dull, ridged, no cyan): a ride along the room's wall would skip every gate (D-6); only the grapple crosses`);
    x.noride = true;
  }
};
/// a grapple gate: void lz [lip, lip + VOID], the landing deck from the void's far edge to `deckTo`, RISE over the take-off, its faces `noride`; the ring over
/// it; the landing carries the gate tag (builder E's `n38 gates`) and the walk pilot's pull38 row
function gate(r, name, lip, fromY, deckTo, landW) {
  const toY = fromY + RISE;
  const far = lip + VOID;
  const ring = r.pt(0, toY + RING_UP, far + RING_IN);
  const deck = r.box("deck", -landW / 2, far, landW / 2, deckTo, PIT_Y, toY, "line",
    `${name}'s LANDING (${landW} m wide, ${RISE} m above the take-off, ${VOID} m of void before it; its faces noride): only the grapple's pull reaches it (the gate)`);
  deck.noride = true; // its front face is in reach of the THUNDERCLAP chain: a ride up it would skip the gate
  deck.gate = { verb: "grapple", from: r.pt(0, fromY, lip - 0.5), to: r.pt(0, toY, far + 2), id: `${r.id}-${name.replace(/ /g, "")}`, runUp: 6,
    why: `${VOID} m of void and ${RISE} m up: one pull (the ring over the landing, or its front face), under ${GATE_MARGIN} x the ${REACH} m range; the room is LIMIT pistolOnly (no THUNDERCLAP boost, no PULSE blast), the best apex left (${APEX} m) is under the rise, no ride wall in reach (builder E's envelopes)` };
  links.push([`${r.id} ${name}: lip -> the landing ring`, r.pt(0, fromY + 1.6, lip), ring]);
  // the walk pilot reads these (Assets/Scripts/Game/Bot/Walk38Span.cs): the level's own numbers, never the bot's
  (r.row.pull38 = r.row.pull38 || { _why: "the flight room's gates for the walk pilot (Bot/Walk38Span.cs): each gate's lip (the take-off edge), its ring and the landing point (world metres)", gates: [] }).gates.push({ name, lip: r.pt(0, fromY, lip), ring, land: r.pt(0, toY, far + 2) });
  return { ring, deck, far, toY };
}

// ---------------------------------------------------------------- 4.1 STITCH YARD (teach: the STITCHER, then the first pull up)
// pars: the walk bot's medians over 10 seeds (n38span walk, 2026-10-13: p1 19.6 s, p2 1.4, p3 35.1, p4 2.4, p5 36.0), rounded up (LEVEL-FORMAT: par = the bot median)
const SPEC = [
  { id: "p1", name: "4.1 STITCH YARD", job: "teach", verb: "the STITCHER (hold to stream) + the first pull", w: 32, l: 36, h: 12, par: 20, spawnLz: 2, exitFromN: 2 },
  { id: "p2", name: "4.2 FIRST GAP", kind: "link", job: "teach", verb: "look up and pull: one gate", w: 36, l: 8 + VOID + 12, h: 22, par: 3, spawnLz: 2, exitFromN: 2 },
  { id: "p3", name: "4.3 KNOT YARD", job: "practise", verb: "pulls between balconies in a fight; kills give pulls", w: 40, l: 40, h: 14, par: 35, spawnLz: 2, exitFromN: 2 },
  { id: "p4", name: "4.4 PULL LINE", kind: "link", job: "test", verb: "two rising gates, one charge each", w: 40, l: 8 + VOID + 12 + VOID + 12, h: 32, par: 5, spawnLz: 2, exitFromN: 2 },
  { id: "p5", name: "4.5 SPOOL HALL", job: "combine", verb: "the STITCHER, the whip, pulls", w: 36, l: 36, h: 14, par: 36, cx: -10, outX: OUT_X, spawnLz: 2, exitFromN: 2 },
];
// the line ends at s7's old north wall: the first room's south edge follows from the rooms' lengths and the 4 m doors between them
L.cursor = r3(NORTH_Z0 + SPEC.reduce((s, q) => s + q.l, 0) + 4 * (SPEC.length - 1));
const room = (i) => L.room(SPEC[i]);

{
  const r = room(0);
  ARENA(r);
  r.row.award = "nailgun";
  r.row.pickup = { weapon: "nailgun", at: r.pt(0, 1.1, 5) };
  r.pickup("weaponPickup", 0, 5, 1.1, "the STITCHER on the axis 3 m from the door: you run over it (the room's award is the fallback)");
  r.propsOut[r.propsOut.length - 1].weapon = "nailgun";
  r.box("deck", -16, 20, -6, 36, 0, 4, "line", "the WEST TERRACE (4 m): the second wave's spitters stand on it; the ring over its front pulls you up onto it");
  r.ramp(-11, 10, 20, 3, 4, "climb", "the terrace's ramp (4 m over 10 m): the walkers' way up and down, and the player's long way");
  r.box("hull", 4, 12, 6, 14, 0, 3, "cover", "a 2 x 2 x 3 m block on the husks' line: their rush bends round it; a stream from behind it");
  r.box("hull", 8, 22, 10, 24, 0, 3, "cover", "a second block on the swarmers' side: cover from the terrace's spitters on the way to the exit");
  const ring = r.pt(-8, 9, 24);
  r.row.grappleNodes = [ring];
  r.row.grapple = "ONE ring over the terrace's front edge at 9 m: a pull from the floor lands you on the terrace among the spitters";
  links.push(["p1 floor -> the terrace ring", r.pt(-8, 1.6, 12), ring]);
  [[0, 16], [4, 19], [-3, 20], [8, 30], [12, 32], [4, 33], [12, 26]].forEach(([x, z]) => r.nodeAt(x, z));
  r.highNodeAt(-12, 30, 4); r.highNodeAt(-9, 34, 4);
  r.row.waves = [["husk", "husk", "husk"], ["spitter", "spitter", "swarmer", "swarmer", "swarmer", "swarmer"]];
  r.lesson({ kind: "husksDead", n: 3, prompt: "STITCHER: HOLD TO STREAM." });
  r.lesson({ kind: "husksDead", n: 9, prompt: "{grapple} PULL TO THE RING." });
  r.ceil(0, 5, 3, 3, "white", 9, 1.0); r.ceil(0, 18, 4, 4, "white", 10, 1.0); r.ceil(-11, 30, 4, 4, "amber", 9, 1.0); r.ceil(10, 30, 4, 4, "white", 9, 1.0);
  r.pool(0, 5, 2, "white", 5, 1.2);
  r.pool(-8, 24, 2.5, "cyan", 6, 1.2, undefined, 4); // the ring's landing on the terrace
  r.pool(0, 34, 2.5, "cyan", 7, 1.6); // the exit
  r.walkSE([[0, 0, 8], [2, 0, 18], [2, 0, 30]]);
  r.walk([[-11, 0, 8], [-11, 4, 21], [-11, 4, 30]]);
  r.frames([0, 0, 6], [-8, 4, 26], [6, 0, 28], [0, 1, 36]); // mid: the yard, the terrace and its ring; exit: the far right to the door
}

// ---------------------------------------------------------------- 4.2 FIRST GAP (teach, flight: one gate)
{
  const r = room(1);
  FLIGHT(r);
  const LIP = 8, FAR = LIP + VOID, DECK_TO = r.l - 4;
  r.pit(-18, 0, -4, LIP, PIT_Y, "the void beside the entry pier (west): the pier is the only floor at the door");
  r.pit(4, 0, 18, LIP, PIT_Y, "the void beside the entry pier (east)");
  r.pit(-18, LIP, 18, FAR, PIT_Y, `THE GAP: ${VOID} m of void wall to wall, no floor (a fall is back at the door)`);
  r.pit(-18, FAR, -6, r.l, PIT_Y, "the void beside the landing (west): the landing is a pier");
  r.pit(6, FAR, 18, r.l, PIT_Y, "the void beside the landing (east)");
  const g = gate(r, "gate A", LIP, 0, DECK_TO, 12);
  liners(r, 0, r.l);
  r.row.grappleNodes = [g.ring];
  r.row.grapple = `ONE ring ${RING_UP} m over the landing, ${RING_IN} m past its lip: from the pier's lip, look up and PULL`;
  r.row.routeVia = [[r.pt(0, 0, LIP - 1), r.pt(0, RISE, FAR + 3)]];
  r.lesson({ kind: "reached", rect: [-6, FAR, 6, DECK_TO], minY: RISE - 0.5, prompt: "LOOK UP. {grapple} PULL." });
  r.ceil(0, 4, 3, 3, "white", 8, 1.0); r.ceil(0, FAR + 4, 4, 4, "white", 14, 1.0);
  r.pool(0, 6, 2, "cyan", 5, 1.0);
  r.pool(0, FAR + 4, 3, "cyan", 9, 1.8, undefined, RISE); // the goal light on the landing
  r.walk([[0, 0, 2], [0, 0, LIP - 1]]);
  r.walk([[0, RISE, FAR + 3], [0, 0, r.l - 2]], { maxSeconds: 6 });
  r.frames([0, 0, LIP - 1], [0, RISE + 2, FAR + 2], [0, RISE, FAR + 3], [0, 1, r.l]); // mid: from the lip up at the landing and its ring; exit: from the landing down to the door
}

// ---------------------------------------------------------------- 4.3 KNOT YARD (practise: pulls between balconies in a fight; kills feed pulls)
{
  const r = room(2);
  ARENA(r);
  r.box("deck", -20, 14, -12, 30, 0, 5, "line", "the WEST BALCONY (5 m): a spitter's stand; a ring pull lands on it");
  r.box("deck", 12, 14, 20, 30, 0, 5, "line", "the EAST BALCONY (5 m): a spitter's stand; a ring pull lands on it");
  r.ramp(-16, 2, 14, 3, 5, "climb", "the west balcony's ramp (5 m over 12 m): the walkers' and the player's way up");
  r.ramp(16, 2, 14, 3, 5, "climb", "the east balcony's ramp (5 m over 12 m)");
  r.box("hull", -2, 18, 2, 22, 0, 1.2, "cover", "a 1.2 m block in the middle: the husks' rush splits round it; a jump onto it, a stream over it");
  const ring = [r.pt(0, 10, 12), r.pt(-8, 10, 22), r.pt(8, 10, 22), r.pt(0, 10, 32)];
  r.row.grappleNodes = ring;
  r.row.grapple = "FOUR rings in a diamond at 10 m over the floor: ring to balcony to ring, while the swarmers chase (a kill gives a pull back)";
  links.push(["p3 south ring -> west ring", ring[0], ring[1]]);
  links.push(["p3 west ring -> east balcony", ring[1], r.pt(16, 6.6, 22)]);
  [[0, 15], [-5, 18], [5, 18], [0, 26], [-6, 34], [6, 34], [0, 30], [-8, 14], [8, 14]].forEach(([x, z]) => r.nodeAt(x, z));
  r.highNodeAt(-16, 24, 5); r.highNodeAt(16, 24, 5);
  r.row.waves = [["husk", "husk", "husk", "husk"], ["spitter", "spitter", "husk", "husk", "husk"], ["swarmer", "swarmer", "swarmer", "swarmer", "husk", "husk"]];
  r.lesson({ kind: "husksDead", n: 6, prompt: "KILLS GIVE PULLS." });
  r.ceil(0, 12, 3, 3, "white", 10, 1.0); r.ceil(0, 32, 3, 3, "white", 10, 1.0);
  r.ceil(-16, 22, 3, 6, "white", 9, 1.0); r.ceil(16, 22, 3, 6, "white", 9, 1.0); r.ceil(0, 20, 3, 3, "amber", 9, 1.0);
  r.pool(0, 37.5, 2.5, "cyan", 7, 1.6);
  r.walkSE([[6, 0, 10], [6, 0, 30]]);
  r.walk([[-16, 0, 0.8], [-16, 5, 16], [-16, 5, 28]]);
  r.walk([[16, 0, 0.8], [16, 5, 16], [16, 5, 28]]);
  r.frames([0, 0, 4], [0, 10, 22], [10, 0, 34], [0, 1, 40]);
}

// ---------------------------------------------------------------- 4.4 PULL LINE (test, flight: two gates, rising)
{
  const r = room(3);
  FLIGHT(r);
  const LIP = 8, B_FAR = LIP + VOID, B_LIP = B_FAR + 12, C_FAR = B_LIP + VOID, C_TO = r.l - 4;
  r.pit(-20, 0, -4, LIP, PIT_Y, "the void beside the entry pier (west)");
  r.pit(4, 0, 20, LIP, PIT_Y, "the void beside the entry pier (east)");
  r.pit(-20, LIP, 20, B_FAR, PIT_Y, `GAP A: ${VOID} m of void wall to wall`);
  r.pit(-20, B_FAR, -6, B_LIP, PIT_Y, "the void beside pier B (west)");
  r.pit(6, B_FAR, 20, B_LIP, PIT_Y, "the void beside pier B (east)");
  r.pit(-20, B_LIP, 20, C_FAR, PIT_Y, `GAP B: ${VOID} m of void wall to wall`);
  r.pit(-20, C_FAR, -6, r.l, PIT_Y, "the void beside pier C (west)");
  r.pit(6, C_FAR, 20, r.l, PIT_Y, "the void beside pier C (east)");
  const a = gate(r, "gate A", LIP, 0, B_LIP, 12);
  const b = gate(r, "gate B", B_LIP, RISE, C_TO, 12);
  liners(r, 0, r.l);
  r.row.grappleNodes = [a.ring, b.ring];
  r.row.grapple = "TWO rings, one over each landing: pull up to pier B, run its 12 m, pull up to pier C (one charge each: the second gate spends the last one)";
  r.row.routeVia = [[r.pt(0, 0, LIP - 1), r.pt(0, RISE, B_FAR + 3)], [r.pt(0, RISE, B_LIP - 1), r.pt(0, 2 * RISE, C_FAR + 3)]];
  r.lesson({ kind: "reached", rect: [-6, B_FAR, 6, B_LIP], minY: RISE - 0.5, prompt: "" });
  r.lesson({ kind: "reached", rect: [-6, C_FAR, 6, C_TO], minY: 2 * RISE - 0.5, prompt: "TWO CHARGES. TWO PULLS." });
  r.ceil(0, 4, 3, 3, "white", 8, 1.0); r.ceil(0, B_FAR + 6, 4, 4, "white", 14, 1.0); r.ceil(0, C_FAR + 4, 4, 4, "white", 14, 1.0);
  r.pool(0, 6, 2, "cyan", 5, 1.0);
  r.pool(0, B_FAR + 6, 3, "cyan", 9, 1.6, undefined, RISE); // pier B
  r.pool(0, C_FAR + 4, 3, "cyan", 9, 1.8, undefined, 2 * RISE); // pier C (the goal)
  r.walk([[0, 0, 2], [0, 0, LIP - 1]]);
  r.walk([[0, RISE, B_FAR + 2], [0, RISE, B_LIP - 1]]);
  r.walk([[0, 2 * RISE, C_FAR + 3], [0, 0, r.l - 2]], { maxSeconds: 8 });
  r.frames([0, 0, LIP - 1], [0, RISE + 2, B_FAR + 2], [0, RISE, B_LIP - 1], [0, 2 * RISE + 2, C_FAR + 2]);
}

// ---------------------------------------------------------------- 4.5 SPOOL HALL (combine: STITCHER + whip + pulls; the last fight before the rehearsals)
{
  const r = room(4);
  ARENA(r);
  // the spawn is at lx +10 (p4's door); the exit at lx -7 (o1's door, world x OUT_X): the line runs north-west across the hall, the gallery on its right
  r.box("deck", 0, 22, 18, 30, 0, 4, "line", "the EAST GALLERY (4 m): the spitters' stand over the line to the exit; a ring in front of it pulls you level with them");
  r.ramp(15, 12, 22, 3, 4, "climb", "the gallery's ramp (4 m over 10 m): the walkers' and the player's way up");
  r.box("hull", -10, 14, -2, 16, 0, 2, "cover", "a 2 m wall across the west lane: the husks' line splits round it, the spitters' globs break on it");
  const ring = [r.pt(6, 10, 16), r.pt(-6, 10, 22)];
  r.row.grappleNodes = ring;
  r.row.grapple = "TWO rings at 10 m: one in front of the gallery (pull up level with the spitters, whip one off), one over the west lane toward the exit";
  links.push(["p5 floor -> gallery ring", r.pt(6, 1.6, 4), ring[0]]);
  links.push(["p5 gallery ring -> west ring", ring[0], ring[1]]);
  [[2, 12], [4, 16], [-4, 18.5], [-8, 20], [-12, 26], [-14, 30], [-4, 30]].forEach(([x, z]) => r.nodeAt(x, z));
  r.highNodeAt(4, 26, 4); r.highNodeAt(10, 27, 4); r.highNodeAt(15, 26, 4);
  r.row.waves = [["husk", "husk", "husk", "husk", "swarmer", "swarmer"], ["spitter", "spitter", "husk", "husk", "husk"], ["swarmer", "swarmer", "swarmer", "swarmer", "spitter", "spitter"]];
  r.pickup("healthPickup", 9, 26, 4.9, "health on the gallery's middle: the last before the rehearsals and THE LOOM");
  r.lesson({ kind: "husksDead", n: 4, prompt: "{grapple} AT AN ENEMY: WHIP." });
  r.ceil(4, 8, 3, 3, "white", 9, 1.0); r.ceil(9, 26, 6, 2, "amber", 9, 1.0); r.ceil(-8, 24, 3, 3, "white", 9, 1.0);
  r.pool(OUT_X - r.cx, 34, 2.5, "cyan", 7, 1.6);
  r.walkSE([[4, 0, 10], [-7, 0, 20]]);
  r.walk([[15, 0, 8], [15, 4, 23], [9, 4, 27]]);
  r.frames([10, 0, 4], [9, 4, 26], [-6, 0, 24], [-7, 1, 36]); // mid: the line north-west, the gallery and its spitters on the right; exit: the west lane to o1's door
}

// ---------------------------------------------------------------- merge: the new rooms, then o1-o3 as NIGHT37 wrote them
const fresh = L.finish();
const p5 = fresh.rooms[fresh.rooms.length - 1];
if (Math.abs(p5.rect[1] - NORTH_Z0) > 1e-6) throw new Error(`p5's north edge ${p5.rect[1]} is not s7's old one ${NORTH_Z0}`);
if (Math.abs(p5.exit[0] - OUT_X) > 1e-6) throw new Error(`p5's exit x ${p5.exit[0]} is not o1's door ${OUT_X}`);
const oldDoors = src.doors.filter((d) => kept(d.between[1]));
const doors = [...fresh.doors];
for (const d of oldDoors) doors.push(Object.assign({}, d, { between: d.between[0] === "s7" ? ["p5", d.between[1]] : d.between }));
doors.forEach((d, i) => { d.id = `d${i + 1}`; });
const plan = {
  _why: fresh._why,
  id: "n38-span", name: "LEVEL 4 THE SPAN",
  owns38: ["jump", "double", "dash", "slide", "boost", "ride", "walljump", "grapple"],
  owns38_why: "the verbs level 4 owns (builder E's checker, Design/levels/n38/GATES38.md): the movement kit, the THUNDERCLAP's boost (level 3's weapon carries in), the grapple (design.n35.grapple35.fromLevel 4); the shockwave is not live (design.n25.shockwaveOff)",
  n29: true, n30: true, n36: true, n37: true, n38: true,
  floorSize: 0, killPlaneY: fresh.killPlaneY, killPlaneY_why: fresh.killPlaneY_why,
  wallHeight: fresh.wallHeight, rank: fresh.rank, n32: fresh.n32,
  rooms: [...fresh.rooms, ...src.rooms.filter((r) => kept(r.id))],
  doors,
  boxes: [...fresh.boxes, ...src.boxes.filter((b) => kept(b.zone))],
  ramps: [...fresh.ramps, ...src.ramps.filter((b) => kept(b.zone))],
  pads: [], grapple: [],
  pits: [...fresh.pits, ...src.pits.filter((b) => kept(b.zone))],
  tunnels: [...fresh.tunnels, ...(src.tunnels || []).filter((b) => kept(b.zone))],
  lights: [...fresh.lights, ...src.lights.filter((l) => kept(l.room))],
  props: [...fresh.props, ...src.props.filter((p) => kept(p.room))],
};
if (fresh.teach35) plan.teach35 = fresh.teach35;
let maxAbs = 0;
for (const r of plan.rooms) for (const v of r.rect) maxAbs = Math.max(maxAbs, Math.abs(v) + 4);
plan.floorSize = Math.ceil((maxAbs * 2 + 16) / 2) * 2;

// ---- the reach check (real numbers)
let bad = 0;
const lim = GATE_MARGIN * REACH;
console.log(`grapple reach ${REACH} m (design.n32.grapple32.range), a route pull must be under ${GATE_MARGIN} x = ${lim} m`);
for (const [label, a, b] of links) {
  const d = near(a, b);
  if (d > lim) bad++;
  console.log(`  ${d <= lim ? "ok  " : "FAIL"} ${d.toFixed(1).padStart(5)} m  ${label}`);
}
const gates = plan.boxes.filter((b) => b.gate);
console.log(`gates: ${gates.length} (${gates.map((g) => g.gate.id).join(", ")}), each ${VOID} m of void and ${RISE} m up (the best non-grapple apex ${APEX} m)`);
fs.mkdirSync(path.dirname(OUT), { recursive: true });
fs.writeFileSync(OUT, JSON.stringify(plan, null, 1) + "\n");
console.log("wrote", path.relative(ROOT, OUT), plan.rooms.length, "rooms", plan.boxes.length, "boxes", plan.pits.length, "pits", plan.lights.length, "lights", plan.props.length, "props",
  "| first room z1", L.cursor, "| floorSize", plan.floorSize);
if (bad) { console.log(`RED: ${bad} route pull(s) over ${lim} m`); process.exit(1); }
console.log("GREEN: every route pull under the margin");
