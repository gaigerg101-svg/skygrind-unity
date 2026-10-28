// NIGHT37 builder C (crown37): writes ONLY builder C's rows in Design/unity-config.json (text edits, the file's layout kept), then copies the file
// to Assets/StreamingAssets/Design/unity-config.json:
//   1. design.n37.crown37: its numbers (each with a _why), keeping `enabled`, `levelFrom`, `levelTo` and any key the lead added;
//   2. design.maps.list: an n37-spire line right after n33-spire's;
//   3. design.n31.unlocks.levels.rows: an n37-spire row right after n33-spire's (n 5, earns the TORCH: as n33-spire).
// Run: node Tools/night/n37c-config.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const CFG = path.join(ROOT, "Design/unity-config.json");
const SA = path.join(ROOT, "Assets/StreamingAssets/Design/unity-config.json");

const ROW = {
  _why: "Builder C (NIGHT37.md C1; BOSSES.md 'L5 THE CROWN'): THE CROWN from scratch as a descent of a falling spire (level 5's n37-spire: Tools/night/gen-n37-spire-boss.cjs). "
    + "The fight: the summit shatters, you drop onto orbiting ring platforms (leg 1), down the gallery's broken spiral (leg 2) to the plaza (leg 3); the Crown (a ring of 8 gems) "
    + "is hurt only by a jump-dash THROUGH its ring (a pass: gems break) or in the CYAN window its stagger break opens; leg 3 ends with a THUNDERCLAP slug or a KESTREL charge "
    + "through its centre. Core/Enemies/Crown37.cs on K's Boss37 brain, Core/Orbit37.cs (the pieces), Game/SurvivalSim.Crown37.cs. Off: n33-spire and THE CROWN of NIGHT32.",
  name: "THE CROWN",
  name_why: "D-3: THE CROWN keeps its name with a new fight.",
  pieces: {
    orbitSpeed: 3.5,
    orbitSpeed_why: "m/s along the ring (a 13 m ring turns in 23 s): Devices33's platform moves 3 m/s (design.n33.devices33.platform.speed); a little faster so the rings read as moving at 30 m. The carry is exact at any speed (the harness prints the carry error).",
    rideMargin: 0.3,
    rideMargin_why: "as design.n33.devices33.platform.rideMargin (the capsule's overhang past the edge still rides).",
    rideHeight: 0.15,
    rideHeight_why: "as design.n33.devices33.platform.rideHeight (grounded feet sit on the top within the Mover's skin).",
    fallDelay: 2,
    fallDelay_why: "NIGHT37.md / builder C's brief: the rehearsal stones drop 2 s after you land on them.",
    fallReturn: 3,
    fallReturn_why: "s a dropped rehearsal stone stays down (a retry after the stair's climb finds it back). Ours.",
    galleryReturn: 6,
    galleryReturn_why: "s a gallery slab the beam cut stays down: the spiral is never cut off for long (BOSSES.md: 'pick another'). Ours.",
    orbitReturn: 5,
    orbitReturn_why: "s an orbit platform dropped by the habit rule stays down. Ours.",
    dropSeconds: 1.2,
    dropSeconds_why: "the view: a dropped piece falls away this long before it hides (no collider meanwhile). Render-side, kept here so the event carries it. Ours.",
    shatterTell: 1.0,
    shatterTell_why: "s the summit shakes RED before it shatters (BOSSES.md rule 7: a wind-up of 1.0-1.5 s).",
  },
  crown: {
    gems: 8,
    gems_why: "BOSSES.md: a ring of 8 gem-set blades; 8 gems.",
    gemHp: 100,
    gemHp_why: "hp per gem: the bar is gems x gemHp. A pass breaks whole gems; in the CYAN window every hit counts toward the next gem. Ours.",
    legGemsLeft: [5, 2],
    legGemsLeft_why: "gems left when leg 2 (the gallery) and leg 3 (the plaza) start: BOSSES.md 'Leg 3 (the plaza) ... last 2 gems'; 3 + 3 + 2.",
    ringRadius: 3.0,
    ringRadius_why: "BOSSES.md: the ring ~6 m across.",
    passRadius: 2.4,
    passRadius_why: "m from the ring's centre a body point may cross its plane and count as a pass (inside the blades' 3 m ring). Ours.",
    lockRadius: 8,
    lockRadius_why: "m: within this the ring's facing locks (it stops turning to face you), so a pass is a straight line through a still plane. Ours.",
    spinRate: 1.6,
    spinRate_why: "rad/s the blade ring spins (the view); 0 in the CYAN window (BOSSES.md: 'the ring stops spinning CYAN').",
    moveSpeed: 11,
    moveSpeed_why: "m/s the Crown moves to its hover point (it keeps up with a running player, 8 m/s walk, never with a dash). Ours.",
    hoverDistance: 5,
    hoverDistance_why: "m from the player (flat) the Crown hovers on the line toward the piece it offers (or along the spire's turn when none): the jump-dash covers it in 0.2 s (24 m/s: config dash.speed), straight through. Ours.",
    passAbove: 1.0,
    passAbove_why: "m over the player's chest it hovers: the jump-dash's rise 0.2 s after take-off (v0 x dashJump.v0Mult 6.67 m/s under half gravity: ~1.1 m; the real sim's apex 1.85 m, Harness crown37 jumps). Ours.",
    targetMin: 8,
    targetMin_why: "m (flat) the nearest piece the Crown offers as the landing past it: a jump-dash flies 21 m on the flat (Harness crown37 jumps); air steering (air.accel) shortens it to about this. Ours.",
    targetMax: 22,
    targetMax_why: "m (flat) the farthest piece it offers (the jump-dash's flat reach 21 m plus the drop's airtime). Ours.",
    launchLead: 0.25,
    launchLead_why: "s from the moment the Crown sets its line to the jump-dash leaving the piece (the dash, then the jump): the line leads a moving piece by its flight (flat metres / config dash.speed) plus this. Ours.",
    openers: { leg1: ["bladeRing", "beamCut"], leg2: ["bladeRing", "beamCut"], leg3: ["bladeRing", "plazaDrop"] },
    openers_why: "each leg opens with these attacks, forced in order (Boss37Brain.Force) from its first rest; the Crown offers no line (no landing past its ring) until the last one's wind-up starts (the cut shows which piece is gone): every leg meets a cut of the piece you head for (a re-route) and a blade ring to dodge (the lead, round 3: the bot met almost no attack; pressure, never health: GG w31-w32). Ours.",
    passSpeed: 12,
    passSpeed_why: "m/s of flat speed a pass needs (a jump-dash flies at 24, a run jump at 8: config dash.speed, ground.walkSpeed): the pass is the jump-dash's (BOSSES.md 'jump-dash THROUGH the ring'). Ours.",
    passGems: 1,
    passGems_why: "gems a pass breaks without the TORCH. BOSSES.md: the gems take damage only at close range.",
    torchGems: 2,
    torchGems_why: "gems a pass breaks with the TORCH burning (fire held with the TORCH in hand within torchWindow of the pass): BOSSES.md '2 per pass at most'.",
    torchWindow: 0.4,
    torchWindow_why: "s around the pass the TORCH counts as burning the gems passed. Ours.",
    grantTorch: true,
    grantTorch_why: "level 5 earns the TORCH at its END (design.n31.unlocks n33-spire earns torch); the Crown's damage rule needs it in the fight (BOSSES.md: 'the TORCH earned here burns at close range'): the spire's walk-in hands it over (with design.adds.b5 on). Ours.",
    passStagger: 2,
    passStagger_why: "stagger a pass drains (the right move); stagger [4, 4, 4] (kit) = two passes open the CYAN window.",
    parryStagger: 2,
    parryStagger_why: "stagger a parried gem shard drains, and its socket shatters (a gem): BOSSES.md 'parry it back to shatter a gem socket = Stagger'.",
    finisherMin: { charge: 60, slug: 300 },
    finisherMin_why: "leg 3's finisher: a KESTREL charge (revolver >= 60) or a THUNDERCLAP slug (shotgun >= 300) through the ring's centre, NIGHT32's thresholds (design.n32.boss5.kit.chargeMin / slugMin).",
    fallDamage: 20,
    fallDamage_why: "BOSSES.md L5: 'A fall = back to the leg's checkpoint platform (-20 HP)'; D-6.",
    revealSeconds: 2.0,
    revealSeconds_why: "the walk-in reveal: the Crown rises over the summit (walkin36's shape: a few seconds of no damage). Ours.",
    habitStandSeconds: 3,
    habitStandSeconds_why: "BOSSES.md: 'if you stay on one platform > 3 s, the platform drops' (never a checkpoint).",
    habitTellSeconds: 1.0,
    habitTellSeconds_why: "s the stood-on platform tells RED before it drops (BOSSES.md rule 7).",
    airborneSeconds: 6,
    airborneSeconds_why: "BOSSES.md: 'if you stay airborne > 6 s, it fires low' (the lowFire attack is forced).",
    landY: 1.6,
    landY_why: "m over the plaza the Crown hovers in leg 3 (it 'lands': its ring at chest height, a jump-dash pass on the flat). Ours.",
  },
  attacks: {
    bladeRing: { kind: "red", windup: 1.2, active: 0.6, recover: 0.3, cost: 2, weight: 1, minDist: 0, maxDist: 40, legs: [1, 2, 3], habit: "any",
      damage: 25, halfThick: 0.6, reach: 30,
      _why: "BOSSES.md: 'Blade ring (RED, 1.2 s): it spins out to cut a full ring around the spire at your height: drop or jump-dash over it'. A band halfThick m thick at the tell's height sweeps out from the core to reach m during active; damage hp (ours)." },
    gemShard: { kind: "yellow", windup: 1.0, active: 1.6, recover: 0.4, cost: 1, weight: 1.2, minDist: 3, maxDist: 40, legs: [1, 2, 3], habit: "any",
      speed: 22, damage: 15, radius: 0.5, parryReach: 3.0,
      _why: "BOSSES.md: 'Gem shard (YELLOW, 1.0 s): it flings a gem shard at you: parry it back to shatter a gem socket'. speed m/s, damage hp, radius m, parryReach m from the eye (ours; the punch's parry reach)." },
    beamCut: { kind: "red", windup: 1.4, active: 0.3, recover: 0.5, cost: 2, weight: 1, minDist: 0, maxDist: 40, legs: [1, 2], habit: "ground",
      _why: "BOSSES.md: 'Beam cut (RED, 1.4 s): a beam cuts the next platform you're heading to (shown RED first): pick another'. The tell marks the piece; it drops when the tell ends (a gallery slab comes back after pieces.galleryReturn)." },
    lowFire: { kind: "red", windup: 1.0, active: 1.2, recover: 0.4, cost: 1, weight: 0.6, minDist: 0, maxDist: 40, legs: [1, 2, 3], habit: "air",
      speed: 26, damage: 15, radius: 0.6, drop: 1.2,
      _why: "BOSSES.md: 'if you stay airborne > 6 s, it fires low': a RED shard aimed drop m under your feet's path (it denies the air under you; forced after crown.airborneSeconds airborne)." },
    plazaDrop: { kind: "red", windup: 1.2, active: 0.3, recover: 0.6, cost: 2, weight: 1.2, minDist: 0, maxDist: 40, legs: [3], habit: "ground",
      _why: "BOSSES.md: 'the plaza: a round floor whose outer sectors drop into the void one by one': the sector nearest you tells RED, then drops (for the leg)." },
  },
  kit: {
    restSeconds: 0.5,
    restSeconds_why: "the Crown's breather between bursts (design.n37.boss37.restSeconds 1.5 for the kit): shorter so each leg's openers land inside the 15-22 s band (the lead, round 3: raise the attack rate, never the health). Ours.",
    legChangeSeconds: 0.8,
    legChangeSeconds_why: "the dive between legs (kit 1.5 s): the Crown's legs are a fall down the spire, not a rest. Ours.",
    stagger: [4, 4, 4],
    stagger_why: "two passes (passStagger 2) or a pass and a parry open the CYAN window in every leg: the descent is short (BOSSES.md band: bot 15-22 s).",
    autoLegs: false,
    autoLegs_why: "the legs are places: the Crown's gems left (crown.legGemsLeft) move the fight down the spire (NextLeg).",
  },
};

function eolOf(t) { return t.includes("\r\n") ? "\r\n" : "\n"; }
function closeOf(t, open, o = "{", c = "}") {
  let depth = 0, inStr = false;
  for (let i = open; i < t.length; i++) {
    const ch = t[i];
    if (inStr) { if (ch === "\\") i++; else if (ch === "\"") inStr = false; continue; }
    if (ch === "\"") inStr = true;
    else if (ch === o) depth++;
    else if (ch === c && --depth === 0) return i;
  }
  throw new Error("unbalanced");
}

let t = fs.readFileSync(CFG, "utf8");
const eol = eolOf(t);
{
  const n37 = t.indexOf("\"n37\": {");
  if (n37 < 0) throw new Error("no design.n37 block: merge port/stage3 first");
  const n37Close = closeOf(t, t.indexOf("{", n37));
  const key = t.indexOf("\"crown37\": {", n37);
  if (key < 0 || key > n37Close) throw new Error("no crown37 row inside design.n37");
  const open = t.indexOf("{", key), close = closeOf(t, open);
  const old = JSON.parse(t.slice(open, close + 1));
  const indent = t.slice(t.lastIndexOf("\n", key) + 1, key);
  const row = { _why: ROW._why, enabled: old.enabled === true, levelFrom: old.levelFrom, levelTo: old.levelTo };
  for (const [k, v] of Object.entries(ROW)) if (!(k in row)) row[k] = v;
  for (const [k, v] of Object.entries(old)) if (!(k in row) && !(k in ROW)) row[k] = v;
  const body = JSON.stringify(row, null, 2).split("\n").map((l, i) => (i === 0 ? l : indent + l)).join(eol);
  t = t.slice(0, open) + body + t.slice(close + 1);
}
if (!t.includes("{ \"id\": \"n37-spire\"")) {
  const a = "{ \"id\": \"n33-spire\", \"label\": \"LEVEL 5 THE SPIRE\", \"file\": \"Design/levels/n33-spire.json\", \"campaignOnly\": true },";
  const at = t.indexOf(a);
  if (at < 0 || t.indexOf(a, at + 1) >= 0) throw new Error("maps.list: n33-spire's line not found exactly once");
  const indent = t.slice(t.lastIndexOf("\n", at) + 1, at);
  const line = "{ \"id\": \"n37-spire\", \"label\": \"LEVEL 5 THE SPIRE\", \"file\": \"Design/levels/n37-spire.json\", \"campaignOnly\": true },";
  t = t.slice(0, at + a.length) + eol + indent + line + t.slice(at + a.length);
}
if (!/"id": "n37-spire",\s*"earns"/.test(t)) {
  const idAt = t.search(/"n": 5,\s*"id": "n33-spire",\s*"earns"/);
  if (idAt < 0) throw new Error("unlocks: n33-spire's row not found");
  const open = t.lastIndexOf("{", idAt), close = closeOf(t, open);
  const objIndent = t.slice(t.lastIndexOf("\n", open) + 1, open);
  const inner = objIndent + "  ";
  const row = ["{", `${inner}"n": 5,`, `${inner}"id": "n37-spire",`, `${inner}"earns": "torch",`,
    `${inner}"note": "LEVEL 5 THE SPIRE with THE CROWN's descent (NIGHT37 C, design.n37.crown37 swaps n33-spire for it): n33-spire's rooms v1-v8 as they were, two rehearsal rooms (FALLING STONES, THE RING) and THE SPIRE. Starts with all four weapons; earns the TORCH at its end (the spire's walk-in hands it over early: crown.grantTorch)."`,
    `${objIndent}}`].join(eol);
  t = t.slice(0, close + 1) + "," + eol + objIndent + row + t.slice(close + 1);
}
JSON.parse(t);
fs.writeFileSync(CFG, t);
fs.writeFileSync(SA, t);
const c = JSON.parse(t);
console.log("crown37 keys:", Object.keys(c.design.n37.crown37).filter((k) => !k.endsWith("_why")).join(" "), "enabled", c.design.n37.crown37.enabled);
console.log("maps.list n37-spire:", c.design.maps.list.some((m) => m.id === "n37-spire"));
console.log("unlocks row n37-spire:", c.design.n31.unlocks.levels.rows.some((r) => r.id === "n37-spire"));
