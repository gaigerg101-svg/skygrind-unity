// NIGHT40 builder B2: writes design.n40.brawl40's numbers (THE ICEBREAKER, Core/Enemies/Brawl40.cs; SurvivalSim.Brawl40.cs reads them) into
// Design/unity-config.json, then copies the file to Assets/StreamingAssets/Design/ (the two stay byte-identical). Patches ONLY the brawl40
// row's text (L-049); `enabled` is kept as it is (the lead turns the row on). Re-runnable: the row is rewritten whole each time.
// Run from the repo root (or a worktree): node Tools/night/n40-brawl40-config.cjs
const fs = require("fs");
const path = require("path");
const root = path.resolve(__dirname, "..", "..");
const file = path.join(root, "Design", "unity-config.json");
const copy = path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json");
const raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ before the patch");
const d = JSON.parse(raw);
const old = d.design.n40 && d.design.n40.brawl40;
if (!old) throw new Error("design.n40.brawl40 is missing (the NIGHT40 shared commit adds it)");

const row = {
  _why: old._why,
  enabled: old.enabled,
  enabled_why: old.enabled_why,
  name: "THE ICEBREAKER",
  name_why: "The working name (DECIDE40 D-3: GG may rename it). The boss bar and the name card read it.",
  arena: "Design/maps/brawl40-arena.json",
  arena_why: "The arena's boxes (Tools/night/gen-n40-brawl-arena.cjs): 36 x 36 m of ice, 8 m ride walls on the rim, four pillars to break line, the entry gap on +z. Built as world layer brawl40Arena at the boss room row's brawl40.centre (n40-iceline's x9 THE BRAWL), as THE STAG's canal was built at its row's centre.",
  salt: 40140,
  salt_why: "Mixed into the run's seed for the fight's own Mulberry32 (rule 6: per-run randomness).",
  pool: 6000,
  pool_why: "Hit points. Tuned for the bot band 30-60 s (boss_rules 7, DECIDE39 D-1): the PULSE's heavy swing lands 192 per 2.5 s (design.n27.pulse27 damageMult 6 x weapons.gauntlet.damage 32) and the KESTREL 25 per shot, both in full only up close. Measured (`n40brawl band`, the skilled mixed pilot, 10 seeds, round 1: the haymaker only): 2400 -> p50 19.3 s; 6000 -> p50 44.6 s, p90 46.3 s, 10/10 killed, damage gauntlet 69 % / KESTREL 28 % / punch 3 %. Research/2026-10-14-wave40/B2.md.",
  revealSeconds: 1.5,
  revealSeconds_why: "The walk-in: it stands and turns to face you, no damage, no attacks (walkin36's reveal; THE STAG used 1.5 s).",
  phase2At: 0.5,
  phase2At_why: "Phase 2 at half its bar (boss_rules 6: ULTRAKILL splits health about evenly, Minos Prime 65 + 65, Gabriel 50 + 50; research/boss-web.md section 1).",
  phaseSeconds: 1.5,
  phaseSeconds_why: "The phase change: no damage, no attacks, the kit's beat (a banner, a flash, a push away). boss_rules 6: a 1-2 s transition.",
  body: {
    height: 3.6,
    height_why: "A hunched giant, twice the player's 1.8 m: big fists, a small head (enemies-web.md section 4: squares for tanks). The view draws its model to these.",
    radius: 1.2,
    radius_why: "The torso's hit sphere and the body's footprint (the clamp, the pillars, the range rule measures from this surface).",
    fistRadius: 0.75,
    fistRadius_why: "Each fist's hit sphere (a shot at the glowing fist lands too).",
    margin: 0.6,
    margin_why: "Metres the body keeps from the rim walls beyond its radius (boss_rules 4: every position clamped to the arena rect minus a margin).",
  },
  move: {
    walkSpeed: 5.5,
    walkSpeed_why: "Its walk at the player in phase 1, metres per second: under the player's ground speed (the run's maxSpeed), so a player can always open distance; a brawler that comes to you (the husk walks 6).",
    walkSpeed2: 7.0,
    walkSpeed2_why: "Phase 2's walk: faster (boss_rules 6: phase 2 changes the attack set or the arena; V2's Brutal: 'movement and attacks 50 % faster', we take ~27 %).",
    turnRateDeg: 140,
    turnRateDeg_why: "Degrees per second it turns toward the player while walking (x1.5 in a wind-up's tracking share): a circle-strafe at speed gets behind it.",
    stopDist: 1.0,
    stopDist_why: "Metres off its body's surface where it stops walking (it does not push into the player).",
  },
  range: {
    close: 4,
    close_why: "Metres from its body's surface within which a gun lands x1 (NIGHT40.md B2: 'x1 at <= 4 m'; the Marauder's open band ~3-4.5 m, research/boss-web.md section 5).",
    far: 12,
    far_why: "Metres at and beyond which a gun lands farMult (NIGHT40.md B2: '~x0.2 beyond 12 m'); linear between close and far.",
    farMult: 0.2,
    farMult_why: "A gun's share from far away (NIGHT40.md B2 '~x0.2'). The pistol from range alone must take >= 3x a mixed fight (the card's check, `n40brawl range`).",
  },
  stagger: {
    meter: [6, 8],
    meter_why: "The STAGGER meter's full value per phase (Minos Prime's stamina pattern, 6 then 12; ours 6 then 8). Only the gauntlet and the punch drain it.",
    gauntlet: 3,
    gauntlet_why: "Meter points per PULSE swing that lands: two swings (or one swing on the stuck fist) open the window in phase 1.",
    punch: 1,
    punch_why: "Meter points per F punch (the gun in hand): the weaker melee.",
    stuckMult: 1.5,
    stuckMult_why: "While its fist is stuck after the haymaker: every hit x1.5 and melee stagger x1.5 (the punish window, Minos Prime's stuck fist).",
    windowSeconds: 2.5,
    windowSeconds_why: "The window when the meter empties (it reels): every hit x windowMult (design.n37.boss37.windowSeconds 2.5).",
    windowMult: 2,
    windowMult_why: "Every hit's multiplier in the window (after the range rule: a gun from far is still weak).",
  },
  rest: {
    seconds: 1.2,
    seconds_why: "Seconds between attacks in phase 1 (it walks at you meanwhile): under design.n37.boss37.restSeconds 1.5, a brawler keeps the pressure on.",
    seconds2: 0.8,
    seconds2_why: "Phase 2's rest: faster chains (NIGHT40.md B2).",
  },
  haymaker: {
    windup: 1.0,
    windup_why: "The heavy wind-up (boss_rules 1: heavy >= 0.9 s): the fist rises and glows red-orange; a RED tell36.",
    windup2: 0.9,
    windup2_why: "Phase 2's wind-up: still at the heavy floor 0.9 s.",
    trackShare: 0.7,
    trackShare_why: "It turns with you for the first 70 % of the wind-up, then commits (THE STAG's charge locks at 70 %): a late dash beats it.",
    startDist: 5,
    startDist_why: "Metres off its body within which it starts a haymaker (else it keeps walking): a step and a lunge reach you.",
    active: 0.2,
    active_why: "The swing's length: the lunge and the hit test.",
    lunge: 2.5,
    lunge_why: "Metres it lunges along its committed facing during the swing.",
    reach: 2.6,
    reach_why: "Metres past its body's surface the fist reaches (from the lunge's end).",
    coneDeg: 50,
    coneDeg_why: "Half-angle of the swing's cone: a dash sideways out of it (or a jump over its head) is the answer (boss_rules 2).",
    damage: 30,
    damage_why: "Health the player loses to a haymaker (a heavy: THE STAG's charge did 25-35).",
    knock: 12,
    knock_why: "Metres per second of horizontal knockback along its swing.",
    knockUp: 5,
    knockUp_why: "Metres per second up with the knockback.",
    stuckSeconds: 1.2,
    stuckSeconds_why: "Seconds its fist stays stuck in the ice after the swing, hit or miss: the punish window (Minos Prime's stuck fist 'about a second'; boss_rules 2 ~1 s).",
    recover: 0.4,
    recover_why: "Seconds it takes to pull the fist free before it walks again.",
  },
  camp: {
    seconds: 3,
    seconds_why: "Seconds grounded within camp.radius before its camp answer (the slam on your spot, round 2; design.n37.boss37.campSeconds 3).",
    radius: 4,
    radius_why: "Metres: staying within 4 m of one spot counts as camping (design.n37.boss37.campRadius).",
  },
  slam: {
    _why: "Both fists into the ice: a shockwave ring runs out along the floor (NIGHT40.md B2: 'a shockwave ring: jump or ride over it'). Its camp answer (boss_rules 3: standing still is punished by an attack you can see) and a mid-range pick.",
    windup: 0.9,
    windup_why: "The wind-up: both fists over its head, a RED tell36 and a ground ring decal (boss_rules 1: medium 0.65-0.9 s).",
    windup2: 0.8,
    windup2_why: "Phase 2's wind-up (still inside the medium band).",
    recover: 0.8,
    recover_why: "Seconds it stays bent over after the slam (a short punish: about one swing).",
    ringSpeed: 12,
    ringSpeed_why: "Metres per second the ring runs out: it crosses the 18 m arena half in 1.5 s, so a jump timed on sight clears it (the player's jump is in the air ~0.6 s).",
    ringRadius: 20,
    ringRadius_why: "Metres where the ring dies: the arena's half (18) plus the walls: it reaches every spot of the floor.",
    ringWidth: 0.6,
    ringWidth_why: "Metres of slack on each side of the ring's front for the hit test.",
    ringHeight: 0.5,
    ringHeight_why: "Metres above the floor the ring reaches: feet higher than this (a jump, a wall ride) pass over it.",
    damage: 20,
    damage_why: "Health a ring hit takes (a medium: under the haymaker's 30).",
    knockUp: 7,
    knockUp_why: "Metres per second up: the ring pops you off the ice.",
    midChance: 0.35,
    midChance_why: "At mid range (between haymaker.startDist and block.startDist) the chance it slams instead of walking on (never twice running).",
    chain2: 0.5,
    chain2_why: "Phase 2: the chance a haymaker up close is chained straight into a slam (NIGHT40.md B2: 'phase 2: faster chains').",
  },
  block: {
    _why: "A block of ice torn from the floor and thrown when you stay far (NIGHT40.md B2: 'a thrown ice block when the player stays far (YELLOW: parry it back for a stagger)'; ULTRAKILL: yellow = parryable).",
    windup: 0.75,
    windup_why: "The wind-up: the block raised in its left fist, a YELLOW tell36 (boss_rules 1: medium).",
    recover: 0.5,
    recover_why: "Seconds after the throw before it walks again.",
    startDist: 9,
    startDist_why: "Metres off its body past which it throws (it walks at you meanwhile): the range a gun lands at x0.5 or less.",
    speed: 20,
    speed_why: "Metres per second the block flies (the flight time is distance / speed, at least 0.35 s): readable, slower than the spitter's glob's parry window allows (16 m/s glob, design.n36 parry).",
    gravity: 12,
    gravity_why: "Metres per second squared on the block (a lob that still arrives at the aim point).",
    radius: 0.6,
    radius_why: "The block's hit radius against the player's capsule and the parry's reach measure.",
    damage: 15,
    damage_why: "Health an unparried block takes.",
    parryReach: 3.0,
    parryReach_why: "Metres from the eye a punch (or a gauntlet swing) reaches the block (THE STAG's chunk: design.n37.stag37.chunk.parryReach).",
    parryConeDeg: 60,
    parryConeDeg_why: "Half-angle of the punch's cone for the block.",
    returnSpeed: 32,
    returnSpeed_why: "Metres per second the parried block flies home.",
    parryDamage: 150,
    parryDamage_why: "Damage the returned block lands on it (in full: a parry hit), about one swing's worth less: the punch-back is the far answer, the gauntlet the close one.",
    stagger: 4,
    stagger_why: "Meter points the returned block drains (NIGHT40.md B2: 'parry it back for a stagger'): with one swing it opens the window in phase 1.",
    bufferSeconds: 0.12,
    bufferSeconds_why: "A punch this early still parries the block when it arrives in reach (design.n36 parry36's 0.12 s buffer).",
  },
  rush: {
    _why: "A shoulder charge along a locked line when the player is far (NIGHT40.md B2: 'a rush when the player is past ~15 m'); a wall or pillar stops it dazed (the CHARGER's rule, Charger.cs: stunned on a wall).",
    windup: 0.65,
    windup_why: "The wind-up: it lowers its head and paws, a RED tell36 and a ground line along its path (boss_rules 1: light 0.45-0.65 s); it turns with you for the first 60 %, then the line locks.",
    startDist: 15,
    startDist_why: "Metres off its body past which it may rush (NIGHT40.md B2 '~15 m').",
    speed: 16,
    speed_why: "Metres per second of the charge: under the CHARGER's 22 (Charger.cs) on a small arena; a dash sideways clears its 1.2 m half-width easily.",
    maxSeconds: 1.6,
    maxSeconds_why: "The longest charge (it crosses the arena's diagonal) before it stops on its own.",
    hitRadius: 0.5,
    hitRadius_why: "Metres past its body's radius the charge hits the player's capsule.",
    damage: 25,
    damage_why: "Health a rush takes.",
    knock: 14,
    knock_why: "Metres per second of knockback along the charge.",
    stuckSeconds: 1.2,
    stuckSeconds_why: "Seconds it is dazed after running into a wall or a pillar: the punish (as the haymaker's stuck fist; every hit x stagger.stuckMult).",
    recover: 0.4,
    recover_why: "Seconds after a charge (or a daze) before it walks again.",
    chance: 0.6,
    chance_why: "Past rush.startDist: the chance it rushes rather than throws (it never does the same far attack twice running).",
  },
  floes: {
    _why: "Phase 2's arena change (NIGHT40.md B2: 'Phase 2 at 50 %: faster chains, the ice cracking into floes'; boss_rules 6: phase 2 changes the arena): the floor as a grid of floes; one cracks where the player is heading (RED), then is open water that bites a grounded player. No geometry changes (the room's floor stays under it), so nothing falls: the arena changes what is safe to stand on.",
    grid: 6,
    grid_why: "Floes per side: 6 x 6 floes of 6 m on the 36 m arena (a floe is two strides across: one dash clears it).",
    every: 3,
    every_why: "Seconds between cracks in phase 2 (with floes.open 4 s at most two floes are water at once).",
    tell: 1.0,
    tell_why: "The RED crack before a floe opens (boss_rules 1: an area attack shows a ground decal; heavy >= 0.9 s since it denies ground).",
    open: 4,
    open_why: "Seconds a floe stays open water before it freezes over.",
    dps: 12,
    dps_why: "Health a second on open water (grounded): a nudge to move, never a trap (the haymaker is 30).",
    tick: 0.5,
    tick_why: "Seconds per bite (the damage lands in 0.5 s bites: 6 health each).",
    lead: 0.6,
    lead_why: "The floe that cracks is the one under where the player will be 0.6 s from now at his velocity (the anti-run line), else the one he stands on.",
  },
  line: {
    _why: "The boss line (B5's BossLine40; boss_rules 5: the damage rule taught on entry with the bound buttons, every hit answers). Resolved by the lesson widget: {fire} {punch} {dash} {jump}.",
    entry: "GET IN CLOSE: {fire} THE GAUNTLET HITS FULL",
    entry_why: "On entry (and once more after the first death): what hurts it, in one line. Short: the lesson box takes ~52 characters with the button names (n40-r3: the longer line was cut off).",
    far: "TOO FAR: SHOTS BARELY SCRATCH IT",
    far_why: "Every 4th gun hit landing at x0.5 or less (8+ m away), at most 3 times a fight: a hit that does little says why.",
    stuck: "ITS FIST IS STUCK: HIT IT NOW",
    stuck_why: "The first two times it is stuck (the punish window named).",
    block: "YELLOW BLOCK: {punch} PUNCH IT BACK",
    block_why: "The first two block wind-ups (the parry named; the gauntlet's swing parries it too).",
    seconds: 4,
    seconds_why: "Seconds a line stays up (the stuck and block lines half as long).",
  },
};

const key = '"brawl40": {';
const n40At = raw.indexOf('"n40": {');
const start = raw.indexOf(key, n40At);
if (n40At < 0 || start < 0) throw new Error("brawl40 row not found");
let i = start + key.length - 1, depth = 0, inStr = false;
for (; i < raw.length; i++) {
  const c = raw[i];
  if (inStr) { if (c === "\\") i++; else if (c === '"') inStr = false; continue; }
  if (c === '"') inStr = true;
  else if (c === "{") depth++;
  else if (c === "}") { depth--; if (depth === 0) break; }
}
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const lineStart = raw.lastIndexOf("\n", start) + 1;
const indent = raw.slice(lineStart, start);
const text = JSON.stringify(row, null, 2).split("\n").map((l, k) => (k === 0 ? l : indent + l)).join(eol);
const out = raw.slice(0, start) + '"brawl40": ' + text + raw.slice(i + 1);
const check = JSON.parse(out);
const a = JSON.parse(raw), b = JSON.parse(out);
delete a.design.n40.brawl40; delete b.design.n40.brawl40;
if (JSON.stringify(a) !== JSON.stringify(b)) throw new Error("the patch touched more than design.n40.brawl40");
if (JSON.stringify(check.design.n40.brawl40) !== JSON.stringify(row)) throw new Error("the row did not round-trip");
fs.writeFileSync(file, out);
fs.copyFileSync(file, copy);
console.log("design.n40.brawl40 written, enabled " + row.enabled + "; copied to StreamingAssets");
