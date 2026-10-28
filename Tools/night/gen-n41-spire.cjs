// NIGHT41 builder R5 (spire41; NIGHT41.md R5, INVENTORY A10): level 5 as n41-spire. It is n40-spire's plan (Design/levels/n40/spire.plan.json,
// written by Tools/night/gen-n40-spire.cjs, which this file does not run or edit) copied whole, with NIGHT41's changes applied on the copy, so
// `--off41` keeps n40-spire bit for bit:
//   1. the id `n41-spire` and its _why
//   2. GRAPPLE NODES in the two arenas of the descent, THE NAVE (v2) and THE CHOIR (v6): VERBS rows 17 / 19 / 22 / 23 were RED on level 5 since
//      NIGHT39 because n38 / n40-spire carry `"grappleNodes": []` (the grapple is owned from level 4, grapple35, but had nothing to hook here).
//      Where they go, and where they do NOT:
//      - THE NAVE and THE CHOIR are the descent's open roofed arenas (14 m and 16 m roofs, spitters on 2.4 m aisles / galleries): a ring over each
//        aisle / gallery pulls the player up level with the spitters (n40-span's p1 / p5 rule: a ring a few metres inside a deck's edge, the pull
//        lands you on it), and one ring over the middle (THE NAVE's crossing, THE CHOIR's dais) links one side to the other in a fight.
//      - NOT the gate rooms v3 / v4 / v5 / v7: their test is the jump-dash, one per pad (spire38's meter), and the grapple is off in them
//        (design.n38.spire38.meter.noHook); a node there would never be hookable.
//      - NOT v9 FALLING STONES / v10 THE RING: they teach the jump-dash the CROWN's fight asks for (the stones, the ring); a node would let a pull
//        skip the lesson. NOT v11 THE SPIRE: the CROWN's fight keeps its own nodes (crown32 / crown37 add theirs).
//      The first node of each room has an open floor spot 10 m due south of it (+z) in clear sight: the verbs reel's Node35 pick (rows 17 / 19)
//      takes the first node with such a spot, so rows 17 / 19 start on the nave's floor, not inside an aisle.
// Web coordinates (x east, y up, z south). Run from the repo (or worktree) root: bash Tools/night/n41-spire-regen.sh
const path = require("path");
const fs = require("fs");

const ROOT = path.join(__dirname, "../..");
const SRC = path.join(ROOT, "Design/levels/n40/spire.plan.json");
const OUT = path.join(ROOT, "Design/levels/n41/spire.plan.json");

const plan = JSON.parse(fs.readFileSync(SRC, "utf8"));
if (plan.id !== "n40-spire") throw new Error(`unexpected source id ${plan.id}`);
plan.id = "n41-spire";
plan._why = "NIGHT41 builder R5 (design.n41.spire41 swaps n40-spire for this file while the row is on): Tools/night/gen-n41-spire.cjs copies "
  + "n40-spire's plan whole and adds only: grapple nodes in THE NAVE (v2) and THE CHOIR (v6), the descent's two arenas (VERBS rows 17 / 19 / 22 / 23 "
  + "on level 5). n40-spire's _why: " + plan._why;

const room = (id) => {
  const r = plan.rooms.find((x) => x.id === id);
  if (!r) throw new Error(`room ${id} missing`);
  return r;
};
const box = (id, kind, test) => {
  const b = plan.boxes.find((x) => x.zone === id && x.kind === kind && test(x));
  if (!b) throw new Error(`room ${id}: no ${kind} box found`);
  return b;
};
const inRect = (r, p) => p[0] > r.rect[0] && p[0] < r.rect[2] && p[2] > r.rect[1] && p[2] < r.rect[3];

// ---- the numbers (each from the room it serves)
// Round 2 (the lead's n41-r2 verbs37 frames, f1405: after the pull to the nave's 10 m ring the camera sat at y 13.8, the whole view the roof's
// underside): the pull arrives at ~44 m/s and the release keeps the VERTICAL speed (`n41lv nodes --trace`: vy 29 -> 33 m/s at the release, the
// horizontal cut to design.n35.grapple35.arriveKeepMax 4 m/s), so a steep pull flies on up to the roof (every round-1 ring: eye 0.2 m under it).
// The rings are LOWER now, so a pull from the floor 10 m out (the verbs reel's spot) is shallow and its climb after the release ends with the
// eye >= 2.5 m under the roof (`n41lv nodes`: the clearance per ring, tap and hold). Round 1: decks + 5.6 m, the middle rings at 10 m.
const RING_OVER_DECK = 3; // m a deck ring stands over its deck's top (5.4 m over the floor): the pull's arrival drops onto the deck
const RING_IN = 3; // m inside the deck's inner edge (the room's middle side): the arrival's carry lands on the deck, not short of it
const CROSS_OVER = 4; // m: the middle rings' height over the room's floor (the nave's; the dais's 1.5 m top is under THE CHOIR's): a pull from the floor
// 10 m out rises 2.4 m (eye 1.6), shallow; measured at 5 m: the eye's peak 13.1 m under the nave's 14 m roof (clearance 0.9), 15.8 under the choir's 16
const NODE_SPOT = 10; // m: the verbs reel's Node35 spot distance (FeelReel.N35.Verbs.cs: 10 m from the node at the room's floor)

/// a ring over a deck: x RING_IN inside its inner edge toward the room's middle, at the deck's z middle (or zAt), RING_OVER_DECK over its top
const deckRing = (r, deck, zAt) => {
  const mid = (r.rect[0] + r.rect[2]) / 2;
  const west = deck.max[0] < mid;
  const x = west ? deck.max[0] - RING_IN : deck.min[0] + RING_IN;
  const z = zAt !== undefined ? zAt : (deck.min[2] + deck.max[2]) / 2;
  return [x, deck.max[1] + RING_OVER_DECK, z];
};

// ---- 5.2 THE NAVE (v2): the crossing ring first (its Node35 spot is the nave's open floor), then one over each aisle
{
  const r = room("v2");
  const westAisle = box("v2", "deck", (b) => b.max[0] <= -19);
  const eastAisle = box("v2", "deck", (b) => b.min[0] >= -1);
  const cx = (r.rect[0] + r.rect[2]) / 2;
  const cross = [cx, CROSS_OVER, 64]; // over the nave's floor, two thirds of the way to the exit (z 53): a pull from either aisle crosses the nave toward the exit
  const zA = 72; // the aisle rings: the aisles' middle-north half (spawn at z 87, the spitters' second wave on the aisles)
  r.grappleNodes = [cross, deckRing(r, westAisle, zA), deckRing(r, eastAisle, zA)];
  r.grapple = `THREE rings: one ${CROSS_OVER} m over the nave toward the exit (the crossing: aisle to aisle in a fight), one over each 2.4 m aisle `
    + `(${RING_OVER_DECK} m over it, ${RING_IN} m inside its edge): a pull from the floor lands you among the aisle's spitters`;
  r.grapple41_why = "R5 (spire41, INVENTORY A10): level 5 had no grapple node (VERBS rows 17 / 19 / 22 / 23 RED since NIGHT39); THE NAVE is the descent's first arena and its aisles are the spitters' stand";
}

// ---- 5.6 THE CHOIR (v6): the dais ring first, then one over each gallery
{
  const r = room("v6");
  const westGal = box("v6", "deck", (b) => b.max[0] <= -21);
  const eastGal = box("v6", "deck", (b) => b.min[0] >= 1);
  const dais = box("v6", "deck", (b) => b.min[0] > -21 && b.max[0] < 1);
  const daisZ = (dais.min[2] + dais.max[2]) / 2;
  // over the dais's north half (toward the exit, z -109), 3 m west of its middle: the Node35 spot 10 m south is then the floor beside the dais's south ramp
  const daisRing = [(dais.min[0] + dais.max[0]) / 2 - 3, CROSS_OVER, daisZ - 3];
  if (daisRing[1] < dais.max[1] + 2) throw new Error(`the dais ring ${daisRing} is under 2 m over the dais's ${dais.max[1]} m top`);
  r.grappleNodes = [daisRing, deckRing(r, westGal, daisZ), deckRing(r, eastGal, daisZ)];
  r.grapple = `THREE rings: one ${CROSS_OVER} m up over the dais (the high centre), one over each 2.4 m gallery (${RING_OVER_DECK} m over it, ${RING_IN} m inside `
    + "its edge): dais to gallery to dais, the three heights joined by pulls as well as jump-dashes";
  r.grapple41_why = "R5 (spire41, INVENTORY A10): THE CHOIR is the descent's second arena (combine: jump-dash between three heights); the rings add the pull between them";
  // NIGHT41 E6's hand-off (through the lead, 11:27): the level's DRONE room. Two drones join wave 2 beside the spitters (they spawn at the room's
  // nodes and fly); E6's why: right after the BELFRY descent links, the flyer uses the level's height; not the first fight (THE NAVE teaches the
  // ground kinds first); its answer is the parry, already owned.
  // The validator's wave budget (design.n27.budget: wave 2's 13 points, at most 3 kinds with a heavy; a drone costs 2, design.n40.iceline40.budgetCost):
  // n40-spire's wave 2 (spitter x2, husk x2, heavy = 12 points) + two drones is 16 points and 4 kinds (levelgen ERROR, measured). So the two husks and
  // one spitter make room: heavy + spitter + drone x2 = 12 points (wave 2's points unchanged), 3 kinds with the heavy.
  if (!Array.isArray(r.waves) || r.waves.length < 2) throw new Error("v6's waves are not n40-spire's three");
  if (JSON.stringify(r.waves[1]) !== JSON.stringify(["spitter", "spitter", "husk", "husk", "heavy"])) throw new Error("v6's wave 2 is not n40-spire's: " + JSON.stringify(r.waves[1]));
  r.waves[1] = ["heavy", "spitter", "drone", "drone"];
  r.drone41_why = "R5 for E6 (the level-5 DRONE room, E6's placement through the lead): wave 2 is heavy + spitter + two drones (was spitter x2, husk x2, heavy: the same 12 budget points); the drone's answer is the parry (owned)";
}

// ---- checks: every ring inside its room, under the roof by 2 m at least, and its first ring's Node35 spot on the open floor
for (const id of ["v2", "v6"]) {
  const r = room(id);
  for (const n of r.grappleNodes) {
    if (!inRect(r, n)) throw new Error(`${id}: ring ${n} outside the room ${r.rect}`);
    if (n[1] > r.height - 2) throw new Error(`${id}: ring ${n} within 2 m of the ${r.height} m roof`);
  }
  const f = r.grappleNodes[0];
  const spot = [f[0], 0, f[2] + NODE_SPOT];
  const onDeck = plan.boxes.some((b) => b.zone === id && spot[0] >= b.min[0] && spot[0] <= b.max[0] && spot[2] >= b.min[2] && spot[2] <= b.max[2])
    || plan.ramps.some((q) => q.zone === id && Math.abs(spot[0] - (q.from[0] + q.to[0]) / 2) <= q.width / 2 + (q.from[0] === q.to[0] ? 0 : Math.abs(q.from[0] - q.to[0]) / 2)
      && spot[2] >= Math.min(q.from[1], q.to[1]) && spot[2] <= Math.max(q.from[1], q.to[1]));
  if (onDeck || !inRect(r, spot)) throw new Error(`${id}: the first ring's Node35 spot ${spot} is not open floor`);
}

fs.mkdirSync(path.dirname(OUT), { recursive: true });
fs.writeFileSync(OUT, JSON.stringify(plan, null, 1) + "\n");
console.log("wrote", path.relative(ROOT, OUT), plan.rooms.length, "rooms; rings:",
  ["v2", "v6"].map((id) => id + " " + JSON.stringify(room(id).grappleNodes)).join("; "));
