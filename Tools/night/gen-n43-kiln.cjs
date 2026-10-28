// NIGHT43 builder K1 (NIGHT43.md card K1; GG 2026-10-16 G25-G28, verbatim in Research/2026-10-16-wave43/GG-REQUEST.md; DECIDE43): LEVEL 3 as
// n43-kiln. design.n43.kiln43 swaps n41-kiln for it (Curriculum25.N43Levels); n41-kiln / n40-kiln and their generators are untouched, so --off43
// loads n41-kiln bit for bit. Every older system knows this level as n40-kiln (SurvivalSim.N43BaseId -> n41-kiln -> N41BaseId -> n40-kiln: the
// THUNDERCLAP start set, the KESTREL / gauntlet awards, the walk pilot Walk38Kiln), and the written map's data.name stays "n40-kiln" as n41-kiln's.
//
// What changed against n40-kiln's generator (Tools/night/gen-n40-kiln.cjs, copied here and edited; its rooms keep K4's numbers unless named):
//   G25 / G26  kb1 is rebuilt whole as 3.1 THE STEPS: no ramp, no stand-in, no fight. From the door you see three wall-to-wall steps stacked
//              north, each one higher than the last (small, then big), each lip lit cyan where you land; each step is a boost gate the
//              checker proves (no other verb lands on it) and each line of the boost chain names the step it belongs to (design.n43.kiln43.chain,
//              SurvivalSim.N43.Kiln.cs). After the top step, a drop to the door of 3.1b THE FLUE (the climb, unchanged). n40's kb1 had a 13 m
//              wedge ("the rim's stair") that let you walk past the lesson, and 3 stand-ins + 2 husks; GG: "a strange ramp and other geometry
//              that feels placed simply because an object needed to be there".
//   G27        the rosters by ROLE (encounters-arenas-web.md A6: introduce / reinforce / combine / escalate; one new role per room; <= 3 roles at
//              once before level 4): DRONES (flyers, level 2's g4) come back in 3.2 POUR HALL with the CHARGER (level 2's g8, the mini-ICEBREAKER);
//              the BURROWER (A1's new kind, the level's one new role) is introduced alone in the new room 3.2c THE ASH BED and combined in 3.3
//              CASTING FLOOR. No "heavy" (a plain 40 HP husk today: CODE-FACTS-B). The table of roles per room: Research/2026-10-16-wave43/K1.md.
//   G28        THE RING (k5): see its block (the arena file the worm reads, ring43).
// The boost heights the steps are cut from are READ, not typed: Design/levels/n43/kiln-boost.json (`n43kiln heights --write`, the real sim with
// boost43 on and off), else the envelope table Design/levels/envelopes38.json (boost43 off). The steps must stand under BOTH (kiln43 may be on
// while boost43 is off).
// Run: bash Tools/night/n43k-regen.sh (this generator, n36-jobs, levelgen, the finish step `node Tools/night/gen-n43-kiln.cjs --finish`).
// Checks: n43kiln gates (the checker on this plan, boost43 off and forced on), n43kiln walk (the walk pilot, 10 seeds), n43kiln steps.
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const PLAN_OUT = "Design/levels/n43/kiln.plan.json";
const GEN_OUT = "Design/levels/n43/kiln.json";

// ---------------------------------------------------------------- the finish step (after levelgen): the game's copies
if (process.argv[2] === "--finish") {
  const src = path.join(ROOT, process.argv[3] || GEN_OUT);
  const text = fs.readFileSync(src, "utf8");
  const level = JSON.parse(text);
  if (level.name !== "n43-kiln") throw new Error(`${src}: name ${level.name}, expected n43-kiln`);
  // the older systems' name for this level (as n41-kiln's): data.name "n40-kiln" (MapModelJson reads the outer name first, so the map id stays n43-kiln)
  level.data.name = "n40-kiln";
  level.generatedBy = "Tools/night/gen-n43-kiln.cjs (plan " + PLAN_OUT + " -> levelgen -> --finish)";
  const eol = text.includes("\r\n") ? "\r\n" : "\n";
  const out = JSON.stringify(level) + (text.endsWith(eol) ? eol : "");
  const outs = [path.join(ROOT, "Design/levels/n43-kiln.json"), path.join(ROOT, "Assets/StreamingAssets/Design/levels/n43-kiln.json")];
  for (const f of outs) fs.writeFileSync(f, out);
  console.log(`n43-kiln: finished ${path.relative(ROOT, src)} -> ${outs.map((f) => path.relative(ROOT, f)).join(", ")} (${out.length} bytes; data.name ${level.data.name})`);
  process.exit(0);
}

const { Level, r3 } = require("./n36-lib.cjs");

// ---------------------------------------------------------------- the gate numbers (read, not typed: their sources)
const ENV = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/levels/envelopes38.json"), "utf8")).ground;
const NOBOOST = Object.entries(ENV).filter(([k]) => !k.includes("boost"));
const noBoostApex = Math.max(...NOBOOST.map(([, v]) => v.apex)); // 7.75: jump + the PULSE blast at pitch 70 (not owned in kb1: no gauntlet yet)
const climb = ENV["jump+double+boost x2"]; // 13.36: the THUNDERCLAP CLIMB (shot down, double at the apex, shot down)
const climb3 = ENV["jump+double+boost x3"]; // 16.26
const CORNER = 3.5; // the wall-jump corner cheese on the real Mover (NIGHT38 `n38kiln corner`: noride walls still give wall jumps)
const BLASTWALL = 11.31; // builder E's checker, b13c7802 (the PULSE blast + a wall jump: the highest feet)
const DEEP = 8; // K1 NIGHT43: the climb gates' tops in 3.1b and 3.3b 8 m deep (n40: GATE.depth 4.5): n43kiln gates --b43 found G1 / G3 at +10 % (the double's tries overshoot a 4.5 m top)
const GATE = { H: 12, depth: 4.5, gapFrom: 1.5, src: "Design/levels/envelopes38.json + E's checker b13c7802 + K4's round-2 sweep (gen-n40-kiln.cjs)" };
if (!(GATE.H > noBoostApex * 1.1 && GATE.H > CORNER * 1.1 && GATE.H > BLASTWALL && GATE.H < climb3.apex / 1.1 && GATE.H < climb.apex))
  throw new Error(`gate height ${GATE.H} outside its window: re-read envelopes38.json`);

// THE STEPS' heights (G26: small, then big), cut from a SCRIPTED PERSON's apexes on this level's floor (`n43kiln steps --write`: standing, the
// jump held through the flight, one tap straight down; the best over the shot's offsets), measured with boost43 OFF and ON (M1's combine:
// one boost WITH the jump 8.07 m on vs 7.26 off; a boost at the jump's apex 4.05 m on). A step must stand under both (kiln43 may be on while
// boost43 is off), so a LOWER bound takes the larger of the two and an UPPER bound the smaller; the person's share of each step, both ways,
// is `n43kiln steps`; the gate checker proves no other verb lands (n38 gates / n43kiln gates). Without the file: envelopes38.json's.
const B43_FILE = path.join(ROOT, "Design/levels/n43/kiln-boost.json");
const B43 = fs.existsSync(B43_FILE) ? JSON.parse(fs.readFileSync(B43_FILE, "utf8")) : null;
const both = (k, envKey) => {
  const v = [];
  for (const side of ["off", "on"]) if (B43 && B43[side] && typeof B43[side][k] === "number") v.push(B43[side][k]);
  if (!v.length) v.push(ENV[envKey].apex);
  return { lo: Math.min(...v), hi: Math.max(...v) };
};
const HT = {
  oneBoost: both("oneBoost", "jump+boost x1"), // the person's best single boost (envelopes38: 6.76 from a run-up)
  twoShots: both("twoShots", "jump+boost x2"), // the person's best two shots, no double jump (envelopes38: 9.61)
  boostDouble: both("boostDouble", "jump+double+boost x1"), // the person's best boost + the double jump at the apex (envelopes38: 7.54)
  climb: both("climb", "jump+double+boost x2"), // the person's best climb: shot, double jump, shot (envelopes38: 13.36)
  double: both("double", "jump+double"), // the double jump: a non-boost verb
  src: B43 ? "Design/levels/n43/kiln-boost.json (n43kiln steps --write, " + (B43.measured || "?") + ")" : "Design/levels/envelopes38.json (boost43 off; no kiln-boost.json yet)",
};
const cheat = Math.max(HT.double.hi, CORNER); // the best non-boost feet in kb1 (no gauntlet, so no PULSE blast: its owns38 below)
const round1 = (v) => Math.round(v * 10) / 10;
const STEP = {
  // S1: small: over every non-boost verb by 14 % (the corner wall jump 3.5 m) or 55 % of the smaller one boost, whichever is higher; M1's
  // "boost at the jump's apex" (4.05 m, boost43 on) just lands it: any boost before the apex does
  s1: round1(Math.max(cheat * 1.14, 0.55 * HT.oneBoost.lo)),
  // S2: bigger: 80 % of the smaller one boost: the shot must come with the jump ("TOGETHER": boost43 on, the shot -0.15..+0.15 s of the jump)
  s2: round1(0.80 * HT.oneBoost.lo),
  // S3: the biggest: over the larger one boost by 12 % (one shot never lands it) and 68 % of the smaller TWO SHOTS (the first with the jump,
  // the second at the top when the gun is ready), whichever is higher; over the boost + the double jump (a double pressed at the face is a
  // wall kick-off backwards: n43kiln trace); 3.1b THE FLUE then asks the whole climb (12 m)
  s3: round1(Math.max(1.12 * HT.oneBoost.hi, 0.68 * HT.twoShots.lo)),
};
if (!(STEP.s1 > cheat * 1.1 && STEP.s1 < STEP.s2 && STEP.s2 < STEP.s3 && STEP.s3 > HT.oneBoost.hi * 1.1 && STEP.s3 * 1.2 < HT.twoShots.lo && STEP.s3 < GATE.H))
  throw new Error(`the steps ${JSON.stringify(STEP)} are outside (cheat ${cheat} x 1.1; one boost ${JSON.stringify(HT.oneBoost)}, two shots ${JSON.stringify(HT.twoShots)}, G1 ${GATE.H})`);

const N35 = JSON.parse(fs.readFileSync(path.join(ROOT, "Design/levels/n35/kiln.plan.json"), "utf8"));
const ARENA39 = fs.existsSync(path.join(ROOT, "Design/maps/wyrm43-arena.json")) ? "Design/maps/wyrm43-arena.json" : "Design/maps/wyrm39-arena.json"; // K1 NIGHT43: THE RING's arena (Tools/night/gen-n43-wyrm-arena.cjs: wyrm39's boxes + the slag plate(s), the landmark, the loop proof)
const ARENA39_FILE = path.join(ROOT, ARENA39);

const OWNS = ["jump", "double", "dash", "slide", "boost", "ride", "walljump"];
const L = new Level("n43-kiln", "LEVEL 3 THE KILN",
  "NIGHT43 builder K1 (design.n43.kiln43 swaps n41-kiln for this file): Tools/night/gen-n43-kiln.cjs, n40-kiln's rooms (Tools/night/gen-n40-kiln.cjs, NIGHT39 K4's "
  + "level) with 3.1 rebuilt as THE STEPS (three boost steps " + STEP.s1 + " / " + STEP.s2 + " / " + STEP.s3 + " m, small then big, from " + HT.src + "), the rosters by role "
  + "(drones and the CHARGER in 3.2, the BURROWER introduced in the new 3.2c THE ASH BED and combined in 3.3), THE RING's arena. "
  + "Rooms: 3.1 THE STEPS, 3.1b THE FLUE (gate G1), 3.2 POUR HALL, 3.2b THE STACK (G2a, G2b), 3.2c THE ASH BED, 3.3 CASTING FLOOR, 3.3b THE SPOUT (G3), 3.4 REST, 3.5 THE RING.",
  {
    owns38: OWNS,
    rank: Object.assign({}, N35.rank, { _why: "Unity-only (builder K4, NIGHT39; kept by K1 NIGHT43): n35-kiln's S/A/B/C cuts (not calibrated on this level's walk)." }),
  });

const ARENA = (r) => { r.row.arena36 = true; };

/// A room before the PULSE gauntlet's award (design.n40.kiln40.awards: kb3; the run starts with the THUNDERCLAP alone, startSet): its own owns38
/// without the PULSE blast, so the gate checker tries only what the player holds there. (The checker's own per-room rule, N42PulseLockedUntil,
/// matches kiln40's levelTo n40-kiln, not n43-kiln: written here instead.)
function noPulseYet(r) {
  r.row.owns38 = OWNS;
  r.row.owns38_why = "K1 NIGHT43: the verbs held in " + r.id + " (the THUNDERCLAP only until the gauntlet's award at kb3: design.n40.kiln40.startSet / awards): no PULSE blast";
}


/// a light on a face pointing +z (south in web coordinates: the face of a cliff the player runs at)
function southStrip(r, lz, y, w, color, range, intensity) {
  r.light("wall", [r.X(0), r3(y), r3(r.Z(lz) + 0.05)], [0, 0, 1], [w, 0.2, 0.1], color, range, intensity);
}

/// noride liners along both side walls from lz0 to lz1 (from y0 to the roof): a ride along the room wall would jump off beside the cliff; the
/// liners take the ride away and show it (the noride look). Wall jumps off them stay the Mover's (the corner cheese: 3.50 m).
function liners(r, lz0, lz1, y0, why) {
  const a = r.box("hull", -r.w / 2, lz0, -r.w / 2 + 0.3, lz1, y0, r.h, "line", `noride liner, west wall (${why})`);
  const b = r.box("hull", r.w / 2 - 0.3, lz0, r.w / 2, lz1, y0, r.h, "line", `noride liner, east wall (${why})`);
  a.noride = b.noride = true;
}

/// one CLIMB GATE (gen-n40-kiln.cjs's): a cliff wall to wall from lz `face` to `face + depth`, from `base` up to `top`, faces noride, tagged for the
/// gate checker; the take-off point `gapFrom` m short of the face on the axis. o.accent false: its take-off lit white (one amber accent per room).
function cliff(r, o) {
  const W = r.w / 2, face = o.face, back = face + (o.depth || GATE.depth), base = o.base || 0, top = o.top, fromLz = face - (o.gapFrom || GATE.gapFrom);
  const c = r.box("deck", -W, face, W, back, o.y0 !== undefined ? o.y0 : base - 1, top, "line", `${o.id}'s CLIFF (top ${top} m, ${r3(top - base)} m over the take-off, wall to wall, faces noride): ${o.why}`);
  c.noride = true;
  c.gate = { id: o.id, verb: "boost", from: r.pt(0, base, fromLz), to: r.pt(0, top, (face + back) / 2), runUp: o.runUp || 6, why: o.why };
  r.row.routeVia = (r.row.routeVia || []).concat([[r.pt(0, base, fromLz), r.pt(0, top, face + 1.5)]]);
  r.light("ceiling", [r.X(0), r3(r.h - 0.05), r.Z(fromLz - 2)], [0, -1, 0], [3, 2, 0.1], o.accent === false ? "white" : "amber", Math.min(14, r.h), 1.0);
  southStrip(r, face, top - 0.15, r.w - 1, "cyan", 9, 1.5); // the lip: cyan where you land (player-usable)
  southStrip(r, face, base + (top - base) / 2, 3, "white", 8, 0.8); // the face: how tall it is, read from the take-off
  r.pool(0, (face + back) / 2, 2, "cyan", 6, 1.2, undefined, top); // the landing's pool: the next place to stand
  r.walk([[0, base, fromLz - 4], [0, base, fromLz]]);
  r.walk([[0, top, face + 0.8], [0, top, back - 0.6]]);
  return c;
}

// ---------------------------------------------------------------- 3.1 THE STEPS (teach, flight: the boost small, then big; G25 / G26)
// Read from the door: the floor ends at a 1st step; behind and above it a 2nd, behind and above that a 3rd, wall to wall, each lip a cyan strip.
// Nothing else stands in the room: every piece below is the lesson. A miss lands you on the step you jumped from (a 1-2 s retry, never a pit or
// lava: research movement-web 16 asks a consequence; here the consequence is the retry, the room has no other hazard). No enemy: one verb per
// room (ULTRAKILL 0-1; encounters-arenas-web A7: a teaching room under 30 s); the THUNDERCLAP's first fight is 3.2's first wave.
{
  const D1 = 8, D2 = 8, D3 = 10, RUN = 7, OUT = 7; // D3 10 (round 2: a person's two shots with boost43 flew past a 6 m deep top) // tier depths (a landing + the next run-up), the floor's run-up, the exit floor past the drop
  const f1 = RUN + 1.5, f2 = f1 + D1, f3 = f2 + D2, back3 = f3 + D3;
  const top1 = STEP.s1, top2 = r3(top1 + STEP.s2), top3 = r3(top2 + STEP.s3);
  const h = Math.ceil(top3 + 7); // room for the last step's boost + double over its top (+ the head)
  const r = L.room({ id: "kb1", name: "3.1 THE STEPS", kind: "link", job: "teach", verb: `the boost, small then big: three steps (${STEP.s1} / ${STEP.s2} / ${STEP.s3} m)`, w: 16, l: back3 + OUT, h, par: 25, spawnLz: 2, exitFromN: 2 });
  // the THUNDERCLAP is in hand from the start (design.n40.kiln40.startSet); the award stays (already owned: Campaign24 marks it given)
  r.row.award = "shotgun";
  noPulseYet(r);
  cliff(r, { id: "S1", face: f1, depth: D1, top: top1, runUp: 5, why: `STEP 1, the small one (${top1} m): one boost lands it (55 % of a person's one boost, ${HT.oneBoost.lo} / ${HT.oneBoost.hi} m); the double jump (${HT.double.hi} m) and the corner wall jump (${CORNER} m) fall short` });
  cliff(r, { id: "S2", accent: false, face: f2, depth: D2, base: top1, y0: -1, top: top2, runUp: 5, why: `STEP 2, bigger (${STEP.s2} m over step 1): 80 % of one boost (${HT.oneBoost.lo} / ${HT.oneBoost.hi} m): the shot WITH the jump` });
  cliff(r, { id: "S3", accent: false, face: f3, depth: D3, base: top2, y0: -1, top: top3, runUp: 5, why: `STEP 3, the biggest (${STEP.s3} m over step 2): one boost (${HT.oneBoost.lo} / ${HT.oneBoost.hi} m) and the boost + the double (${HT.boostDouble.lo} / ${HT.boostDouble.hi} m) fall short; TWO SHOTS (the second at the top: ${HT.twoShots.lo} / ${HT.twoShots.hi} m) land it; 3.1b then asks the whole climb` });
  liners(r, 0, back3, -1, "THE STEPS: a ride or a wall run along the room's wall would skip a step");
  // the lessons (Campaign24, in order; a taught link clears on its last): any step reached, then the exit floor past the drop. The boost chain
  // (design.n43.kiln43.chain, SurvivalSim.N43.Kiln.cs) names each step and tells HOW in the teach slot.
  r.lesson({ kind: "reached", rect: [-5, f1 + 1, 5, back3 - 1], minY: r3(top1 - 0.3), prompt: "THREE STEPS, EACH ONE HIGHER" }); // done on any step's top (the floor's best climb, 17.4 m, cannot top step 3: no way past step 1 without landing on a step)
  r.lesson({ kind: "reached", rect: [-6, back3 + 1, 6, r.l - 2], minY: -0.2, prompt: "UP ALL THREE, THEN DROP DOWN" }); // the room clears here; no lesson per step (a climb can overshoot a step: a lesson on it would hold the door), the chain names them
  r.ceil(0, 3, 3, 2, "white", 9, 1.0); // the door's light: the floor you start on
  r.pool(0, r.l - 2, 2.5, "cyan", 8, 1.6); // the exit door's pool, seen from the top step
  r.walk([[0, 0, back3 + 1], [0, 0, r.l - 2]]);
  r.frames([0, 0, 2], [0, top2, f2 + 2], [0, top3, back3 - 1], [0, 1, r.l]); // mid: from the door, the three steps; exit: from the top step down to the door
  // the steps' tops: the kiln43 chain's "top" lines read them (SurvivalSim.N43.Kiln.cs: a line is done when the feet stand on its top), and so
  // do n43kiln steps and the reel; written by this file so the words and the geometry never drift apart
  r.row.tops43 = { s1: top1, s2: top2, s3: top3, rises: [STEP.s1, STEP.s2, STEP.s3], take: [r.pt(0, 0, f1 - GATE.gapFrom), r.pt(0, top1, f2 - GATE.gapFrom), r.pt(0, top2, f3 - GATE.gapFrom)], src: HT.src,
    why: "K1 NIGHT43 (G26): the boost taught small then big; take = each step's take-off point (feet)" };
}

// ---------------------------------------------------------------- 3.1b THE FLUE (teach, flight: gate G1, the climb taught) -- n40's, unchanged
{
  const r = L.room({ id: "kb1b", name: "3.1b THE FLUE", kind: "link", job: "teach", verb: "the THUNDERCLAP CLIMB: jump, shoot down, double jump, shoot down", w: 16, l: 26, h: 32, par: 4, cx: L.prev.outX, spawnLz: 1.5, exitFromN: 2 });
  noPulseYet(r);
  const face = 11.5, back = face + DEEP; // DEEP (8 m, n40: 4.5): with boost43 on, the climb's tries with the double flew past a 4.5 m top (G1 +10 %)
  cliff(r, { id: "G1", face, depth: DEEP, top: GATE.H, why: `the CLIMB taught: one shot lifts ${HT.oneBoost.lo} / ${HT.oneBoost.hi} m, the climb (shot, double, shot) ${climb.apex} m; the PULSE blast tops at ${noBoostApex} m` });
  liners(r, 0, r.l, -1, "G1: the take-off, the cliff and the landing");
  r.lesson({ kind: "reached", rect: [-6, back + 2, 6, r.l - 2], minY: -0.2, prompt: "JUMP. SHOOT DOWN. JUMP. SHOOT DOWN." });
  r.ceil(0, back + 4, 3, 2, "white", 12, 1.0);
  r.pool(0, r.l - 2, 2.5, "cyan", 7, 1.6);
  r.walk([[0, 0, back + 1], [0, 0, r.l - 2]]);
  r.frames([0, 0, 2], [0, GATE.H, face + 0.5], [0, GATE.H, back - 0.5], [0, 1, r.l]);
  r.row.tops43 = { climb: GATE.H, take: [r.pt(0, 0, face - GATE.gapFrom)], why: "K1 NIGHT43: the climb's top (the kiln43 chain's climb line is done on it)" };
}

// ---------------------------------------------------------------- 3.2 POUR HALL (practise + combine: the boost through the pack; flyers; the CHARGER)
// The geometry is n40's (K4's): each piece's job in its why. The fight changes (G27): wave 1 the fodder (the THUNDERCLAP's first fight: up close,
// the boost through the pack onto the crucible), wave 2 REINFORCES the flyer (level 2's drones) with fodder: the drones hang over the pours, the
// boost puts you at their height; wave 3 COMBINES the flyer with the CHARGER (level 2's mini-ICEBREAKER): look up for the drones, dodge sideways for
// the charge, the crucible (6 m: the charger cannot jump) is the high ground out of its line.
{
  const r = L.room({ id: "kb2", name: "3.2 POUR HALL", job: "practise", verb: "the boost THROUGH the pack; drones over the pours; the charger and the drones together", w: 36, l: 44, h: 16, par: 50, cx: L.prev.outX, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  r.box("deck", -6, 16, 6, 26, 0, 6, "line", "THE CRUCIBLE (6 m) in the middle: over every non-boost apex but the blast; the boost from the pack at its foot lands on it; the charger cannot follow you up");
  r.ramp(3, 39, 26, 4, 6, "climb", "the crucible's walkers' ramp up its north face from the far floor (6 m over 13 m): the husks' way up and down (the charger's lane ends at its foot)");
  r.box("deck", -18, 8, -14, 34, 0, 3, "line", "the WEST GALLERY (3 m) over the west pour: a stand the drones dive past; the walkers' line along it to the pour's rim");
  r.box("deck", 14, 8, 18, 34, 0, 3, "line", "the EAST GALLERY (3 m) over the east pour: the ride wall's foot; a stand to shoot the drones level");
  r.ramp(-16, 1, 8, 3, 3, "climb", "the west gallery's ramp (walkers: 3 m over 7 m)");
  r.ramp(16, 1, 8, 3, 3, "climb", "the east gallery's ramp");
  r.lava(-13, 10, -9, 32, 0.8, "the WEST POUR (4 x 22 m): the walkers' line down the gallery runs along its rims; a blast or a punch launches them in; a charger that misses you near it goes in");
  r.lava(9, 10, 13, 32, 0.8, "the EAST POUR: as the west, under the ride wall");
  r.panel("W", 17.4, 10, 32, 3, 10, "the EAST RIDE (cyan): ride the room's east wall north from the gallery, over the pour, level with the drones (VERBS rows 9 / 11 on this map)");
  r.nodeAt(-4, 14.5); r.nodeAt(0, 15); r.nodeAt(4, 14.5); r.nodeAt(-2, 15.5); r.nodeAt(2, 15.5);
  r.nodeAt(-6, 33); r.nodeAt(6, 32); r.nodeAt(-1, 40); r.nodeAt(-7.5, 22); r.nodeAt(7.5, 22);
  r.highNodeAt(-16, 14, 3); r.highNodeAt(16, 14, 3); r.highNodeAt(-16, 28, 3); r.highNodeAt(16, 28, 3); r.highNodeAt(0, 22, 6);
  r.row.waves = [
    ["husk", "husk", "husk", "husk", "swarmer", "swarmer"], // FODDER (1 role): the THUNDERCLAP up close; the boost through the pack
    ["drone", "drone", "drone", "husk", "husk"], // REINFORCE the flyer (level 2's g4) + fodder (2 roles): the boost / the ride to their height
    ["charger", "drone", "drone"], // COMBINE flyer + CHARGER (2 roles): look up vs dodge sideways
  ];
  // (the validator's budget rule: at most 2 kinds a wave, LevelValidate29; the wave points: drone 2, charger 4, design.n40.iceline40.budgetCost)
  r.row.roles43 = ["fodder", "flyer + fodder", "charger + flyer"];
  r.row.roles43_why = "K1 NIGHT43 (G27; encounters-arenas-web A6): wave 1 the known fodder, wave 2 reinforces the flyer, wave 3 combines it with the charger (answers that conflict: look up, dodge sideways)";
  r.row.rims36 = { rects: [r.rect(-14, 8, -13, 34), r.rect(-9, 8, -8, 34), r.rect(8, 8, 9, 34), r.rect(13, 8, 14, 34)], why: "the floor strips beside the pours: the walkers' lines down from the galleries and along the floor" };
  r.lesson({ kind: "reached", rect: [-6, 8, 6, 12], minY: -0.2, prompt: "JUMP IN. SHOOT {fire} DOWN. FLY." });
  r.pickup("healthPickup", -3, 20, 6.9, "the crucible pays health: the boost through the pack lands on it");
  r.ceil(0, 8, 4, 4, "white", 13, 1.1); r.ceil(0, 21, 6, 4, "white", 13, 1.1); r.ceil(0, 34, 4, 4, "white", 13, 1.0);
  r.pool(0, 21, 3, "orange", 10, 1.3, { danger: true }, 6);
  r.pool(-16, 21, 2.5, "white", 7, 0.9, undefined, 3); r.pool(16, 21, 2.5, "white", 7, 0.9, undefined, 3);
  r.pool(0, 42, 2.5, "cyan", 8, 1.7);
  r.walkSE([[7.5, 0, 10], [7.5, 0, 40.5]]);
  r.walk([[5, 0, 41], [3, 6, 27], [0, 6, 22]]);
  r.walk([[-16, 0, 0.3], [-16, 3, 10], [-16, 3, 30]]);
  r.frames([3, 0, 4], [0, 6, 20], [0, 6, 25], [0, 1, 43]);
}

// ---------------------------------------------------------------- 3.2b THE STACK (test, flight: gates G2a and G2b) -- n40's, unchanged
{
  const r = L.room({ id: "kb2b", name: "3.2b THE STACK", kind: "link", job: "test", verb: "two climbs: 12 m, then 12 m more", w: 16, l: 36, h: 32, par: 8, cx: L.prev.outX, spawnLz: 1.5, exitFromN: 2 });
  noPulseYet(r);
  const f1 = 11.5, t1End = 23.5, f2 = t1End, b2 = f2 + 5;
  cliff(r, { id: "G2a", face: f1, depth: t1End - f1, top: GATE.H, why: "the first climb onto TIER 1 (12 m deep: a 6 m run-up on it for the second)" });
  cliff(r, { id: "G2b", accent: false, face: f2, depth: b2 - f2, base: GATE.H, y0: -1, top: 2 * GATE.H, why: "the second climb, from TIER 1 onto TIER 2: a miss drops you on tier 1 (or the floor: G2a again), never a pit" });
  liners(r, 0, r.l, -1, "G2a / G2b: the take-offs, the cliffs and the landing");
  r.lesson({ kind: "reached", rect: [-6, b2 + 2, 6, r.l], minY: -0.2, prompt: "CLIMB." });
  r.ceil(0, b2 + 4, 3, 2, "white", 14, 1.0);
  r.pool(0, r.l - 2, 2.5, "cyan", 7, 1.6);
  r.walk([[0, 0, b2 + 1], [0, 0, r.l - 2]]);
  r.frames([0, 0, 2], [0, GATE.H, f1 + 0.5], [0, 2 * GATE.H, b2 - 0.5], [0, 1, r.l]);
}

// ---------------------------------------------------------------- 3.2c THE ASH BED (introduce: the BURROWER, alone, then with fodder; G28)
// A new room (NIGHT43): the level's one new role. Its floor is the burrower's ground (A1's burrow43: it hunts the player's GROUND contact; in the
// air or on a metal plate you are safe; it bursts out under you after a tell and stays exposed ~0.5 s: a hit then is a COUNTER). So the room is
// an open floor (nothing to hide behind: the tell must be seen; encounters-arenas-web B8 "fight in the open") with five low METAL PLATES in a
// loose ring: the safe islands, a jump apart (a plain jump reaches ~8 m). Wave 1 is ONE burrower (INTRODUCE alone: never more than one tell on
// screen before level 4, B12); wave 2 adds fodder that walks you off the plates (REINFORCE). The plates stand 0.4 m (A1's safe-ground rule, and the nav's stepHeight: walkers step on;
// round 1 at 0.5 m the walk pilot stuck hopping at a plate's edge for 800 s while a lesson held the door; no ramp is owed).
{
  const r = L.room({ id: "kb2c", name: "3.2c THE ASH BED", job: "introduce", verb: "the BURROWER: leave the ground (a jump, a boost, a plate) or hit it as it surfaces", w: 28, l: 30, h: 12, par: 35, cx: L.prev.outX, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  const PLATE = 0.4; // A1: raised >= 0.4 m is burrower-safe (floorTolerance 0.35); the nav's stepHeight 0.4: walkers still step onto it
  const plates = [[-8, 9, "south-west"], [8, 9, "south-east"], [0, 15.5, "the middle"], [-8, 22, "north-west"], [8, 22, "north-east"]];
  for (const [lx, lz, where] of plates) {
    const b = r.box("deck", lx - 2, lz - 2, lx + 2, lz + 2, 0, PLATE, "cover", `a METAL PLATE (${where}, 4 x 4 m, ${PLATE} m): burrower-proof ground (it cannot surface through metal: A1's rule): the island you jump to when the floor ripples; a jump from the next plate reaches it`);
    b.burrowSafe43 = true;
    r.pool(lx, lz, 1.6, "cyan", 5, 1.0, undefined, PLATE); // cyan on the plate: safe, player-usable ground
  }
  // the spots: on the open ash between the plates, 12 m or more from the door (spawn31's first-view rule): the burrower dives at once there
  r.nodeAt(-12, 15.5); r.nodeAt(-4, 15.5); r.nodeAt(4, 15.5); r.nodeAt(12, 15.5); r.nodeAt(0, 22); r.nodeAt(-4, 27); r.nodeAt(4, 27);
  r.row.waves = [
    ["burrower"], // INTRODUCE (1 role): one burrower, alone, after its line
    ["burrower", "husk", "husk", "husk"], // REINFORCE (2 roles): the husks walk you off the plates onto its ground
  ];
  r.row.roles43 = ["burrower", "burrower + fodder"];
  r.row.roles43_why = "K1 NIGHT43 (G27 / G28; encounters-arenas-web A6, B12): introduce the BURROWER alone, then reinforce it with known fodder";
  // No Campaign24 lesson here: round 1's reached-lesson at the far end held the door after the fight in 1 of 10 walks (G5's bug class). The
  // room's two lines are the kiln43 chain's `at: "kb2c"` lines, shown on entry (design.n43.kiln43.chain); the burrower's own spawn tell
  // (spawn33) gives the line its second before the floor moves.
  r.ceil(0, 4, 4, 3, "white", 10, 1.0); r.ceil(0, 15.5, 6, 4, "white", 11, 1.1); r.ceil(0, 26, 4, 3, "white", 10, 1.0);
  r.pool(0, r.l - 2, 2.5, "cyan", 8, 1.6);
  r.walkSE([[12, 0, 8], [12, 0, 26]]);
  r.frames([0, 0, 3], [0, PLATE, 15.5], [0, PLATE, 22], [0, 1, r.l]);
}

// ---------------------------------------------------------------- 3.3 CASTING FLOOR (combine + escalate: the burrower with the spitters, then with the charger and the drones)
{
  const r = L.room({ id: "kb3", name: "3.3 CASTING FLOOR", job: "combine", verb: "ride over the pour, climb to the ladle; the burrower drives you up into the spitters' globs, then the charger and the drones", w: 40, l: 46, h: 18, par: 70, cx: L.prev.outX, spawnLz: 2, exitFromN: 2 });
  ARENA(r);
  r.lava(-8, 18, 8, 28, 0.8, "the POUR (16 x 10 m) in the middle of the floor: the walkers' lines from the gantry and the ladle pass along its rims; punch or blast them in");
  r.panel("W", -2.4, 18.2, 27.8, -0.8, 6, "the WEST POUR RIDE (cyan, standing in the pour): ride it north across the molten metal (off the burrower's ground)");
  r.panel("E", 2.4, 18.2, 27.8, -0.8, 6, "the EAST POUR RIDE (cyan, standing in the pour): the other side's ride; jump between the two");
  r.box("deck", 12, 12, 20, 28, 0, 8, "line", "THE LADLE (8 m, east): the high ground out of the charger's line and off the burrower's floor; one climb (shot, double, shot) or its long ramp");
  r.ramp(16, 44, 28, 4, 8, "climb", "the ladle's walkers' ramp from the north floor (8 m over 16 m): the husks follow you up");
  r.box("deck", -20, 12, -14, 30, 0, 4, "line", "the WEST GANTRY (4 m): the spitters' stand: parry their globs back from the floor or the ladle");
  r.ramp(-17, 2, 12, 3, 4, "climb", "the west gantry's ramp (walkers: 4 m over 10 m)");
  r.nodeAt(-4, 14.5); r.nodeAt(4, 14.5); r.nodeAt(0, 15.5); r.nodeAt(-8, 14.5); r.nodeAt(8, 14.5);
  r.nodeAt(-10, 23); r.nodeAt(10, 31); r.nodeAt(-5, 31); r.nodeAt(5, 32);
  // no high spawn spots here (n40 had four on the gantry and the ladle): a wave's kinds take the room's spots at random, and a BURROWER lives
  // on its spawn's floor height (A1): one spawned on the ladle hunted only the ladle's top (the stage3 walk, burrow43 on: 4 of 10 runs stuck
  // with 1 alive). The spitters walk up the gantry's ramp to their stand.
  r.row.waves = [
    ["spitter", "spitter", "husk", "husk", "husk"], // KNOWN (2 roles): parry the spitters' globs from the gantry, the fodder at your feet
    ["burrower", "spitter", "spitter"], // COMBINE (2 roles): the burrower drives you off the floor, into the globs: parry them in the air
    ["burrower", "drone", "drone", "drone"], // ESCALATE (2 roles): the burrower says leave the ground, and the air is the drones'
  ];
  r.row.roles43 = ["ranged + fodder", "burrower + ranged", "burrower + flyer"];
  r.row.roles43_why = "K1 NIGHT43 (G27): the burrower combined with a known role whose answer conflicts (leave the ground vs parry the globs), then escalated: the burrower says up, the drones own the air (at most 2 kinds a wave: LevelValidate29's budget)";
  r.row.rims36 = { rects: [r.rect(-10, 16, -8, 30), r.rect(8, 16, 10, 30), r.rect(-8, 28, 8, 30)], why: "the floor around the pour: the walkers' lines from the doors, the gantry and the ladle's ramp" };
  r.pickup("healthPickup", 16, 20, 8.9, "the ladle pays health: climb to it mid-fight");
  r.ceil(0, 8, 4, 4, "white", 14, 1.1); r.ceil(0, 23, 6, 4, "white", 15, 1.1); r.ceil(0, 38, 4, 4, "white", 14, 1.0);
  r.pool(0, 23, 4, "orange", 10, 1.4, { danger: true }, -0.8);
  r.pool(16, 20, 2.5, "white", 8, 0.9, undefined, 8); r.pool(-17, 21, 2.5, "white", 8, 0.9, undefined, 4);
  r.pool(0, 44, 2.5, "cyan", 8, 1.7);
  r.walkSE([[-11, 0, 8], [-11, 0, 34], [0, 0, 40]]);
  r.walk([[-17, 0, 1], [-17, 4, 14], [-17, 4, 28]]);
  r.walk([[16, 0, 45], [16, 8, 27], [16, 8, 14]]);
  r.frames([-6, 0, 3], [0, 3, 23], [16, 8, 26], [0, 1, 45]);
}

// ---------------------------------------------------------------- 3.3b THE SPOUT (test, flight: gate G3) -- n40's, unchanged
{
  const r = L.room({ id: "kb3b", name: "3.3b THE SPOUT", kind: "link", job: "test", verb: "the climb carried forward over a 4 m pit", w: 16, l: 30, h: 32, par: 5, cx: L.prev.outX, spawnLz: 1.5, exitFromN: 2 });
  const W = r.w / 2, lip = 9, face = 13, PIT = 3.9;
  r.pit(-W, lip - 3, W, face, -PIT, "G3's PIT: 4 m wall to wall in the open before the cliff, a dry pit 3.9 m deep; a miss costs the stair's climb in the notch, never health");
  r.box("deck", -W + 2.8, lip - 3, W, lip, -PIT - 1, 0, "line", "G3's take-off floor over the pit's strip: the run-up to the lip");
  r.box("deck", -W + 0.3, lip - 1.5, -W + 2.8, lip, -PIT - 1, -PIT + 1.3, "climb", "G3's recovery stair, step 1 (1.3 m over the pit floor, the west notch)");
  r.box("deck", -W + 0.3, lip - 3, -W + 2.8, lip - 1.5, -PIT - 1, -PIT + 2.6, "climb", "G3's recovery stair, step 2 (1.3 m under the take-off floor)");
  cliff(r, { id: "G3", face, depth: DEEP, top: GATE.H, gapFrom: face - lip + 0.5, y0: -PIT - 1, why: `the climb carried forward: take off at the pit's lip, ${face - lip} m short of the face` });
  liners(r, 0, r.l, -PIT - 1, "G3: the take-off, the pit, the cliff and the landing");
  r.lesson({ kind: "reached", rect: [-6, face + DEEP + 2, 6, r.l - 2], minY: -0.2, prompt: "CLIMB IT ON THE RUN." });
  r.ceil(0, face + DEEP + 4, 3, 2, "white", 12, 1.0);
  r.pool(0, r.l - 2, 2.5, "cyan", 7, 1.6);
  r.walk([[0, 0, face + DEEP + 1], [0, 0, r.l - 2]]);
  r.frames([0, 0, 2], [0, GATE.H, face + 0.5], [0, GATE.H, face + DEEP - 0.5], [0, 1, r.l]);
}

// ---------------------------------------------------------------- 3.4 REST -- n40's, unchanged
{
  const r = L.room({ id: "kb4", name: "3.4 REST", kind: "link", job: "rest", verb: "none", w: 10, l: 10, h: 5, par: 3, cx: L.prev.outX, spawnLz: 1.5, exitFromN: 1.5 });
  r.pickup("healthPickup", 0, 5, 0.9, "the last rest before THE KILN WYRM: health for the fight");
  r.ceil(0, 5, 3, 3, "white", 8, 1.0);
  r.pool(0, 8.5, 2, "cyan", 6, 1.4);
  r.walkSE([]);
  r.frames([0, 0, 2], [0, 0.5, 6], [2, 0, 5], [0, 1, 10]);
}

// ---------------------------------------------------------------- 3.5 THE RING (k5: THE KILN WYRM's room, copied from n35-kiln as n40's)
{
  const src = N35.rooms.find((q) => q.id === "k5");
  if (!src) throw new Error("n35-kiln has no k5");
  const [ox0, oz0, ox1, oz1] = src.rect;
  const w = ox1 - ox0, l = oz1 - oz0;
  const prev = L.prev;
  const ocx = (ox0 + ox1) / 2;
  const r = L.room({ id: "k5", name: src.name, kind: "boss", w, l, h: src.height, cx: prev.outX, outX: prev.outX, par: src.par, spawnLz: oz1 - src.spawn.at[2], exitFromN: src.exit[2] - oz0 });
  const dx = r.X(0) - ocx, dz = r.z1 - oz1;
  const mv = (p) => [r3(p[0] + dx), p[1], r3(p[2] + dz)];
  r.row.boss = src.boss;
  r.row.roof = src.roof;
  const cr = JSON.parse(JSON.stringify(src.crumble31));
  cr.draw.center = [r3(cr.draw.center[0] + dx), r3(cr.draw.center[1] + dz)];
  cr.draw.boxes.forEach((b) => { b.rect = [r3(b.rect[0] + dx), r3(b.rect[1] + dz), r3(b.rect[2] + dx), r3(b.rect[3] + dz)]; });
  if (fs.existsSync(ARENA39_FILE)) {
    const a = JSON.parse(fs.readFileSync(ARENA39_FILE, "utf8"));
    r.row.wyrmArena = ARENA39;
    r.row.wyrmArena_why = "NIGHT43 K1 (G28): " + a._why + " Before it, NIGHT39 K4 (GG: 'a new arena, and keep the worm AI the same'): the worm's room for this level: wyrm26-arena.json's shell (half 30, height 30) with a new inside (Tools/night/gen-n39-wyrm-arena.cjs); the paths file and the AI unchanged";
    if (a.crumble39) {
      cr.arena = ARENA39;
      cr.boxes = a.crumble39.boxes;
      cr.keep = a.crumble39.keep;
      cr._why = cr._why + " NIGHT39 K4: re-pointed at " + ARENA39 + " (" + a.crumble39._why + ").";
      cr.draw.boxes = a.crumble39.draw.map((b) => Object.assign({}, b, { rect: [r3(b.rect[0] + cr.draw.center[0]), r3(b.rect[1] + cr.draw.center[1]), r3(b.rect[2] + cr.draw.center[0]), r3(b.rect[3] + cr.draw.center[1])] }));
    }
  }
  r.row.crumble31 = cr;
  r.row.proofs = src.proofs.map((p) => Object.assign({}, p, { points: p.points.map(mv) }));
  r.row.spawn = { at: mv(src.spawn.at), yawDeg: src.spawn.yawDeg };
  r.row.exit = mv(src.exit);
  r.row.copied38 = "n35-kiln k5 (THE KILN WYRM's room) copied and moved by (" + r3(dx) + ", " + r3(dz) + ") to the level's end (GG: the worm's AI the same)";
  for (const li of N35.lights.filter((q) => q.room === "k5")) r.lightsOut.push(Object.assign({}, li, { at: mv(li.at) }));
  for (const pr of N35.props.filter((q) => q.room === "k5")) {
    if (pr.collider) throw new Error("k5 has a solid prop: tag it");
    r.propsOut.push(Object.assign({}, pr, { at: mv(pr.at) }));
  }
  for (const k of ["boxes", "ramps", "pits", "tunnels", "pads", "grapple"]) if ((N35[k] || []).some((q) => q.zone === "k5")) throw new Error("k5 has " + k + ": copy them");
  r.frames([0, 0, 4], [0, 3, 13], [0, 0, 22], [0, 1, 26]);
}

console.log(`n43-kiln: the steps ${STEP.s1} / ${STEP.s2} / ${STEP.s3} m (tops ${STEP.s1} / ${r3(STEP.s1 + STEP.s2)} / ${r3(STEP.s1 + STEP.s2 + STEP.s3)} m) from ${HT.src}: one boost ${HT.oneBoost.lo} / ${HT.oneBoost.hi}, the climb ${HT.climb.lo} / ${HT.climb.hi}, non-boost best ${cheat}`);
L.write(process.env.K43_OUT || PLAN_OUT);
