// NIGHT47 builder B: writes the numbers of B's own sim rows (design.n47.charger47 / brawl47 / wyrm47) into both config copies, byte-identical
// (Design/unity-config.json, Assets/StreamingAssets/Design/unity-config.json). Copied from Tools/night/n46-b1-rows.cjs (which this file does not
// run or edit): it replaces only each named row's "numbers" block (the row's last key) and touches nothing else. Idempotent.
// Run from the worktree root: node Tools/night/n47-b-rows.cjs   (then: cmp the two copies)
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const FILES = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];

const ROWS = {
  stampede47: {
    _why: "THE CHARGER's stampede as a SURVIVAL event (round 4, the lead's list; SurvivalSim.N47.R4.cs): a herd of chargers in together from one side, every few rounds (CoD Zombies' special rounds every few rounds: B.md R-B5, Reported).",
    firstRound: 6,
    firstRound_why: "the first SURVIVAL round with a stampede (design.director.rounds.introduce.charger 7: the herd comes after the player has met chargers alone)",
    every: 4,
    every_why: "rounds between stampedes (CoD Zombies' hellhound rounds come every 4-6 rounds: Reported)",
    count: 4,
    count_why: "chargers in a herd (THE STAMPEDE's wave 3 has 3; SURVIVAL's charger.maxCount 6 caps the herd plus any the director sends)",
    delayS: 3,
    delayS_why: "seconds after the round begins (the round's own spawns first, then the herd from the far side)",
  },
  trail47: {
    _why: "THE KILN WYRM's burrow trail burns the floor over its buried pieces (round 4; SurvivalSim.N47.R4.cs): the floor over the burrow is the worm's, the air is yours (G33 carried into the burrow; Diablos' burrow, B.md R-B2).",
    radius: 2.5,
    radius_why: "metres (horizontal) from a buried piece's centre that burn (the trail plates the view draws are 1.3 x a piece's footprint)",
    maxUp: 1.0,
    maxUp_why: "metres over the arena's floor under which the feet burn (a jump, a boost, a plate clear it)",
    dps: 12,
    dps_why: "damage a second while standing on the trail (the tail sweep's 20 in ~1.7 s): a floor to leave, not a kill",
    tickS: 0.5,
    tickS_why: "seconds between the burn's ticks",
  },
  brawl47: {
    _why: "THE ICEBREAKER's NIGHT47 fight (G28-G30; B-cards.md section 3): three phases, every tell over E-4's floor, phase 3's stampede, and the length for a human (the band 90-150 s median, n47human) from rests and the stagger meter, never HP. Read by SurvivalSim.N47.Icebreaker.cs onto brawl40.p on a run's first step, written back by a run with the row off; off = NIGHT46's fight.",
    phase2At: 0.66,
    phase2At_why: "phase 2 (the floes, the sweep and the leap) at this share of the bar (design.n40.brawl40.phase2At 0.5): the card's three phases at 66 / 33 %",
    phase3At: 0.33,
    phase3At_why: "phase 3 THE STAMPEDE at this share (Brawl40.phase3At47; NIGHT46 has none): a second transition beat, then the chargers",
    rushWindupS: 0.95,
    rushWindupS_why: "the PROW RUSH's wind-up (s; design.n40.brawl40.rush.windup 0.65, under E-4's 0.8 s for a big attack): the CHARGER's 0.95 s (charger47 tellS), the same language the level taught 4 rooms early",
    blockWindupS: 0.9,
    blockWindupS_why: "the yellow BLOCK throw's wind-up (s; block.windup 0.75): 0.9 s, inside E-4 and the card",
    rest1S: 1.8,
    rest1S_why: "seconds between attacks in phase 1 (rest.seconds 1.2): a length lever for the human band. Measured (n47human --boss brawl --rows brawl47, 40 seeds; the lead's baseline 50.9 s): phases + tells only 55.2 s (20 seeds); + rests 1.8 / 1.0 and meter 9 / 12: 52.6 s; + chip 0.6: 71.6 s (p90 88.8, min 52.8); rests 2.2 / 1.3 and meter 12 / 16 instead: 73.6 s (no real gain, so the gentler ones are kept). Still under the band's 90 s: round 2's lever is a phase, not HP",
    rest2S: 1.0,
    rest2S_why: "seconds between attacks in phases 2 and 3 (design.n42.brawl42.rest2 0.55, rest.seconds2): the length lever",
    meter1: 9,
    meter1_why: "the stagger meter in phase 1 (design.n40.brawl40.stagger [6, 8]): a gauntlet hit drains 3, a punch 1; empty = the WINDOW. The length lever (windows, not HP)",
    meter2: 12,
    meter2_why: "the stagger meter in phases 2 and 3",
    chip: 0.6,
    chip_why: "every hit outside STUCK and the WINDOW x this (Brawl40Params.chip47; NIGHT46 x1): 30 % of the WINDOW's x2, NIGHT47.md section 3's chip floor. The length lever that moves the human band: the rests (1.2 -> 1.8 s, 0.55 -> 1.0) and the meter (6 / 8 -> 9 / 12) alone took n47human's median 55.2 -> 52.6 s (40 seeds) because about half the damage lands outside windows",
    armorChip: 0.25,
    armorChip_why: "THE ICE ARMOUR (Brawl40Params.armorChip47; NIGHT46 none): at each phase's start (and the fight's, armorP1) every hit outside STUCK / the WINDOW lands x this until the phase's first WINDOW breaks it (the gauntlet, a punch, a phase-3 charger baited into it) or armorMaxS passes: the lead's round-2 lever ('a timed beat ... before its armor drops'; B-1 LOCKED: what to do to open it). Length from the windows, never HP",
    armorMaxS: 20,
    armorMaxS_why: "seconds the armour holds at most if no WINDOW breaks it (a human who never finds the gauntlet is not locked out)",
    armorP1: 1,
    armorP1_why: "1: phase 1 starts armoured too (the lock and its key are taught first: punch the BLOCK back, drain the meter)",
    armorP3Herd: 1,
    armorP3Herd_why: "1: phase 3's armour is THE HERD's (Brawl40Params.armorP3Herd47): a WINDOW does not break it; a charger run into the boss does, or the gates' chargers all dead, or armorMaxS: the lead's 'the herd must be cleared or baited into it before its armor drops'. With armour broken by any window (armorChip 0.25 / 0.15) n47human stayed at 85.8 / 85.3 s",
    crackWindowMult: 1.0,
    crackWindowMult_why: "what a WINDOW that opens on the armour lands (Brawl40Params.crackWindowMult47; the window's own x2): that window cracks the ice, the next one hurts (B-2: each phase starts with its lock). With the herd armour alone n47human stayed at 86.1 s",
    lineEntry: "Get close. {fire} hits it full.",
    lineEntry_why: "the fight's entry line while brawl47 is on (SurvivalSim.N47.Icebreaker.cs Brawl47Lines, in place of design.n40.brawl40.line.entry 'GET IN CLOSE: {fire} THE GAUNTLET HITS FULL'): sentence case, <= 6 words (F's COPY47 rules, the lead's copy note)",
    lineFar: "Too far. Shots barely scratch it.",
    lineFar_why: "in place of line.far 'TOO FAR: SHOTS BARELY SCRATCH IT'",
    lineStuck: "Its fist is stuck. Hit it!",
    lineStuck_why: "in place of line.stuck 'ITS FIST IS STUCK: HIT IT NOW'",
    lineBlock: "{punch} the yellow block back.",
    lineBlock_why: "in place of line.block 'YELLOW BLOCK: {punch} PUNCH IT BACK'",
    lineArmour: "{punch} it to crack the ice.",
    lineArmour_why: "said when THE ICE ARMOUR comes on at a phase's start (new with brawl47; the lead's copy note)",
    lineHerd: "{punch} a charger into it.",
    lineHerd_why: "said when phase 3's gates open (new with brawl47; the lead's copy note: 'Ram a charger into it' -> the glyph)",
    chargerDrain: 3,
    chargerDrain_why: "the meter a charger's charge into THE ICEBREAKER drains (as one gauntlet hit, stagger.gauntlet 3): bait a phase-3 charger into the boss and it opens (G27's lesson as the exam)",
    chargerHitPad: 0.5,
    chargerHitPad_why: "metres added to the boss's radius (1.2) and the charger's (0.7) for that hit",
    gateOffset: 13,
    gateOffset_why: "metres either side of the arena's centre where phase 3's chargers come in (the arena is 36 x 36 m: 5 m inside its walls)",
    p3Chargers: 2,
    p3Chargers_why: "chargers phase 3 sends in, once (the card's 'two CHARGERS enter once from the gates')",
  },
  charger47: {
    _why: "THE CHARGER's NIGHT47 rule (G26; Research/2026-10-18-wave47/B-cards.md section 1): a wind-up whose line follows you until its last moment, a charge that turns badly, bodies in its way burst or stop it, one faster wind-up after a partner dies. Read by SurvivalSim.N47.Brawl.cs into husks.chargerAttack at a run's start (Core/Enemies/Charger.cs's lock47 / turn47 / burst47 / heavyStun47 / enrageTell47); off = NIGHT46's charger.",
    tellS: 0.95,
    tellS_why: "the wind-up (s): design.n36.tells36.chargerTellS 1.2 -> 0.95, inside G26's 0.8-1.0 s; long enough to read (DESIGN-RULES E-4 >= 0.8 s for a big one) now that the line follows you (a still line made 1.2 s a free walk-away: n47b charger, early 0 / 40 hit)",
    lockS: 0.25,
    lockS_why: "the wind-up's last seconds in which the line no longer follows you (locks, flashes): a sidestep inside them dodges, one before is followed (Helldivers 2's 'dodge late, not early', ULTRAKILL Cerberus's last turn: B.md R-B1); 0.25 s = 30 steps, a human's reaction (n47human's 250 ms) plus the dash's start",
    turnDegS: 20,
    turnDegS_why: "the charge's turn rate toward you (deg/s; NIGHT46: 0, straight): at 22 m/s over the 0.5-0.6 s it takes to cross 10-14 m it bends ~10-12 deg (~2 m), so a late 0.4 s sidestep (~3.8 m) stays out of its 1.5 m reach while a still player is hit (Pinky's poor turning, Helldivers' widened arc: R-B1); tuned by n47b charger: 35 deg/s caught 8 of 20 late sidesteps (60 % dodged), 20 deg/s the line below",
    burst: 1,
    burst_why: "1: a charge through a light body (a husk, a swarmer, a spitter) bursts it (the launch36 damage path, burstDamage) and runs on; a heavy one (a tank, a warden, a charger, a burrower) stops it, stunned heavyStunS (G26 'a CHARGER that hits another enemy bursts it'; the stampede's tool: bait it into the pack)",
    burstDamage: 1000,
    burstDamage_why: "the damage a burst body takes (launch36's path, so the kill counts as the player's): far over any light body's health (husk 100, swarmer, spitter), so one charge bursts it whole",
    heavyStunS: 1.2,
    heavyStunS_why: "seconds stunned (back open) after running into a heavy body: NIGHT40's skid stun (foes40.charger.skidStunS 1.2), shorter than a wall's 2.0 s because a body gives",
    enrageTellS: 0.8,
    enrageTellS_why: "the one faster wind-up a charger gets when another charger within enrageRadius dies (ULTRAKILL Cerberus's enrage: B.md R-B1): 0.95 -> 0.8 s, never under E-4's 0.8 s floor; kill order becomes a choice in a room of three",
    gloveFront0: 1,
    gloveFront0_why: "1: the PULSE glove (the gauntlet, the punch) on a charger's prow lands nothing (Charger.WeaponMult47; NIGHT46: x0.3 front for every weapon): 'the armour shrugs' (the card's G27 lever, the lead's round-3 line): the glove is the stunned back's (x3) or the launch's, never the face's",
    gunBack: 3,
    gunBack_why: "the KESTREL into the back of a charger that is not stunned lands x this (in place of the front's x0.3; Charger.WeaponMult47): its pierce through two lined-up backs is THE STAMPEDE's trick shot (G27: a room that needs the gun). Measured before (n46flow g8, 20 seeds): gauntlet-only 39.1 s vs mixed 40.8 s",
    enrageRadius: 30,
    enrageRadius_why: "metres: a charger this near a charger that dies is enraged (one room: THE ROUNDHOUSE is 24 x 30 m)",
  },
  wyrm47: {
    _why: "THE KILN WYRM's NIGHT47 tells and burrow (G31 / G34; B-cards.md section 4.1; GG 2026-10-18: \"feels fine overall, but its attacks need clearer telegraphs ... and it should burrow like the previous version\"): each tell 0.8-1.5 s; the DIVE never cut by a charge and twice a cycle. Read by SurvivalSim.N47.Wyrm.cs onto the worm's params on the fight's first step, written back at the next run's start; off = NIGHT46's worm.",
    crackS: 1.2,
    crackS_why: "the floor crack where it dives (s; design.n26 wyrm26 tells.crack 0.6): G31's 0.8-1.5 s; the burrow's main tell, the one the level-3 beats teach",
    aggroS: 0.5,
    aggroS_why: "the head's proximity charge (s; tells.aggro 0.5, the shortest tell in the fight: 102 of them in 10 fights, n47b wyrm 23:12). KEPT at 0.5 (G31's one SHORT tell), measured: at 0.9 with the aim fixed at the tell's start it charged where the player had been (bot: 13 staggers in 10 fights, 3 / 10 killed); at 0.9 with aggroLockS 0.25 and punchWindowS 0.6 the human pilot killed 9 / 40 in 300 s (deaths 199, 2 windows median): the tracked head hits the human before his late punch",
    aggroLockS: 0,
    coilCut: 0,
    coilCut_why: "the COIL ends at this share of its ~297 m path in phases 1-2 (Wyrm.coilCut47; 0 = the whole coil, NIGHT46): G34's 2 burrows a minute by a shorter coil, not a skipped one (the lead's round-3 line). Measured at 0.5: bot 10 / 10, 93.1 s, 1.65 burrows a minute; n47human 40 seeds 280.8 s median, 21 / 40 killed, deaths 141 (off: 122.6, 38, 68): the coil is where the human-model pilot opens the head; every lever that shortens it (skip, cut) loses the fight. Left at 0: G34 SHORT at 1.52 a minute",
    spitDamage: 8,
    spitDamage_why: "a spit shot's damage (WyrmParams.spitDamage; design.n24.wyrm.spit.damage 8, so 8 = unchanged): the tail's killer by count: n47human --b47-tally counted shotsHit 163, contactsBody 189, contactsHead 87, contactsLunge 16 over the capped / slow fights (shots ~40 % of the damage, the head ~30 %). Cut to 5 with contactHead 12: 159.5 s median, 33 / 40 killed, deaths 79 (uncut 122.6, 38, 68): the cut made the fight longer, not shorter (not understood tonight): back at the design's",
    contactHead: 18,
    contactHead_why: "the head's contact damage (WyrmParams.contactHead; design.n24.wyrm.contact.head 18 = unchanged): the second killer (above); cut to 12 with the spit: worse (above)",
    aggroLeanS: 0,
    aggroLeanS_why: "THE LEAN (Wyrm.aggroLean47; 0 = none, NIGHT46): seconds the head rears and tracks you before the charge's own 0.5 s tell (the lead's round-3 idea: lean + tell = 0.9 s, the punch timing today's). Measured at 0.4: n47human 40 seeds 15 / 40 killed in 300 s (median 300, windows 2) with a dropped lean, and the bot (n47b wyrm, 10 seeds) 7 / 10 killed at 278 s with the lean always run into the tell (recoil30 7 in 10 fights against 13 without): the head's charge is punched far less often after a lean; why is not found tonight. Left at 0",
    punchWindowS: 0,
    punchWindowS_why: "the head-punch window, seconds before contact (SurvivalSim.Boss30.cs via Wyrm47PunchWindow; 0 = design.n30.wyrm30.windowSeconds 0.4): the lead's round-2 line (a 0.9 s tell with a matching punch width). 0.6 with the 0.5 s tell measured EXACTLY the same fight (n47human 40 seeds: 122.6 / 256.9 / 45.6): the window only gates the line and the head's colour, not the stagger: left at 0",
    aggroLockS_why: "the charge's aim follows the player through its tell until this many seconds are left, then locks (Wyrm.aggroLock47; the CHARGER's charger47 lockS: one language for 'it locks, now move')",
    spitS: 0.9,
    spitS_why: "the spit (s; tells.spit 0.6): 0.9 s for its lanes to read",
    coreS: 0.9,
    coreS_why: "the core's roar before it opens (s; tells.core 0.6): 0.9 s",
    lungeS: 1.0,
    lungeS_why: "the lunge's line (s; tells.lunge 0.8): 1.0 s, kept apart from the aggro's 0.9 so the two read differently in time",
    squeezeS: 1.0,
    squeezeS_why: "the phase-3 squeeze ring (s; tells.squeeze 0.8): 1.0 s",
    sweepS: 0.9,
    sweepS_why: "the feint's tail sweep (s; design.n31 wyrm31.sweepTellSeconds 0.6): 0.9 s",
    staggerS: 10,
    staggerS_why: "seconds STAGGERED after a head punch (Wyrm31Params.staggerSeconds; wyrm46 8): the length lever, as the card gate asked (\"if it grows, the lever is the longer windows, not removing tells\"): with the tells alone the human pilot's median grew 133.7 -> 157.3 s, p90 221.6 -> 281.2 s (n47human --boss wyrm, 40 seeds)",
    open: 3.0,
    open_why: "what every weapon lands while STAGGERED (Wyrm31.open32; wyrm46 2.5): the second window lever; the one-hit cap (15 % of the pool) unchanged",
    aggroCooldownS: 0,
    aggroCooldownS_why: "seconds between the head's charges (Wyrm.aggroCool31 from Wyrm31Params.aggroCooldown 5; 0 = left as it is). Tried 3.5 (more punch windows for the long tail; NIGHT46's wyrm46 trial T2): n47human --boss wyrm 40 seeds went 122.6 -> 245.5 s median, 38 -> 31 of 40 killed, deaths 68 -> 96 (more charges hurt the human more than they open it): left at 0",
    windowCap: 0,
    windowCap_why: "the most of the pool one STAGGER can take before it ends (Wyrm31.EndStagger47; NIGHT46: none): n47human --per-seed (40 seeds) showed the fastest fights over in 3 windows dealing 75-90 % (min 45.6 s, under the band's 60) while the slowest wait on deaths (3-7 a fight, 13-21 windows with few hits). Measured (n47human --boss wyrm --rows wyrm47, 40 seeds; median / p90 / min / killed): 0 (off) 122.6 / 256.9 / 45.6 / 38; 0.2: 214.2 / 300 / 69.3 / 28; 0.3: 156.9 / 277.6 / 64.6 / 37. A cap lifts the floor but the median follows it out of the band and the tail (deaths) stays: left at 0 tonight",
    coreBelowMult: 0,
    coreBelowMult_why: "G33 'a weak spot reachable from the air' (Wyrm.coreBelowMult47; NIGHT46: the open core x3 from anywhere): a core hit from a shot whose origin is under the core lands x this; from above (a perch, a slag island, a boost) it keeps x3 (design.n24.wyrm coreMult). 0 tonight (x3 from anywhere): the human-model pilot never leaves the floor (WyrmPilot31), so a value under 3 only lengthens its fight; it needs a boost policy to be measured (round 3)",
    coreAboveShare: 0,
    coreAboveShare_why: "how far over the core's centre (x its radius) the shot's origin must be to count as from above (0: the centre's height)",
    linePunch: "{punch} its head as it comes.",
    linePunch_why: "the head-punch line while wyrm47 is on (SurvivalSim.Boss30.cs via Wyrm47Line, in place of design.n30.wyrm30.linePunch 'PUNCH THE GLOWING HEAD.'): sentence case with the glyph (the lead's copy note)",
    glimpseY: 8,
    glimpseY_why: "G35 beat 1 THE GLIMPSE (SurvivalSim.N47.Kiln.cs, n47-kiln only): metres over kb1's spawn the feet must reach (the top of the boost steps: step 3 is 9.0 m, n47b reach) before the worm's back breaks out of the far wall once",
    glimpseS: 2.2,
    glimpseS_why: "seconds the glimpse's arc lasts (the view's; no collider, no damage)",
    passAfterS: 6,
    passAfterS_why: "G35 beat 2 THE PASS: seconds after entering kb2 POUR HALL before the burrow's trail runs across the hall under the player (no damage)",
    passS: 2.5,
    passS_why: "seconds the trail takes wall to wall (36 m: ~14 m/s, the dive's travel speed in the view)",
    cameoAfterS: 5,
    cameoAfterS_why: "G35 beat 3 THE CAMEO: seconds after entering kb3 CASTING FLOOR before the first breach's tell",
    cameoCount: 2,
    cameoCount_why: "breaches in the cameo (the card: once, then once more with the answer known)",
    cameoGapS: 4,
    cameoGapS_why: "seconds between the cameo's breaches",
    cameoTellS: 1.2,
    cameoTellS_why: "the breach's tell (s): the fight's crack tell (crackS 1.2), the same language taught first here (G31's 0.8-1.5 s)",
    cameoTrack: 0.6,
    cameoTrack_why: "the share of the tell the crack circle follows the player before it locks (the card: tracks 60 %, then locks)",
    cameoRadius: 2.6,
    cameoRadius_why: "metres: the breach circle (the fight's crack radius, Wyrm crackR 2.6)",
    cameoDamage: 20,
    cameoDamage_why: "damage to a player still in the circle at the breach (the fight's tail sweep: design.n31 wyrm31.sweepDamage 20): it teaches, it does not kill",
    cameoKnockUp: 7,
    cameoKnockUp_why: "m/s up on a hit (the ICEBREAKER's slam knock-up 7): the eruption throws you",
    cameoMaxUp: 1.0,
    cameoMaxUp_why: "metres over the room's floor under which the feet are hit: a jump (apex 1.58 m) pressed ~0.3 s before the breach, a boost or a ledge clears it (the answer: get up); at 1.5 m a jump 0.25 s before still hit 2 of 2 (n47b beats)",
    noAggroInDive: 1,
    noAggroInDive_why: "1: no proximity charge starts while the DIVE runs (Wyrm.noAggroInDive47): today 13 of 28 cracks were cut by a charge and the dive replayed from its start (Wyrm.cs:807), 3 of 10 fights never burrowed",
    advanceAfterAggro: 0,
    advanceAfterAggro_why: "1: after a proximity charge cuts the COIL the cycle goes on to the next pattern (the DIVE) instead of replaying the coil (Wyrm.advanceAfterAggro47). Measured with it on (round 2, the windows 10 s / x3): bot (n47b wyrm, 10 seeds) burrows 2.00 a minute, 10 / 10 fights burrow (G34's target) but 5 / 10 killed; n47human (40 seeds) 189.2 s median, 34 / 40 killed (off: 122.6, 38): the coil is where the pilot punches the head, the lunge / loop that follow kill it. Left at 0: G34 at 1.52 a minute (SHORT) rather than the band lost",
    diveAfterLunge: 0,
    diveAfterLunge_why: "1: the cycle gains a DIVE after each LUNGE (coil, dive, lunge, dive, loop, finale). 0 tonight: with advanceAfterAggro the burrow already runs 3.4-4.6 a minute (G34 asks >= 2); with both on the dive held 50 % of the fight and the bot killed 0 / 10 in 300 s",
  },
};

function block(name, nums, indent, eol) {
  const pad = " ".repeat(indent + 2), end = " ".repeat(indent);
  const lines = Object.entries(nums).map(([k, v]) => pad + JSON.stringify(k) + ": " + JSON.stringify(v));
  return "{" + eol + lines.join("," + eol) + eol + end + "}";
}

let out = null;
for (const rel of FILES) {
  const file = path.join(ROOT, rel);
  let text = fs.readFileSync(file, "utf8");
  const eol = text.includes("\r\n") ? "\r\n" : "\n";
  if (out === null) {
    for (const [name, nums] of Object.entries(ROWS)) {
      const head = '      "' + name + '": {' + eol;
      const at = text.indexOf(head);
      if (at < 0 || text.indexOf(head, at + 1) >= 0) throw new Error("row " + name + ": expected exactly one '" + head.trim() + "'");
      const close = text.indexOf(eol + "      }", at);
      const rowText = text.slice(at, close);
      const key = '        "numbers": ';
      const k = rowText.indexOf(key);
      if (k < 0) throw new Error("row " + name + ": no numbers key");
      const from = at + k + key.length;
      const to = at + rowText.length;
      const want = block(name, nums, 8, eol);
      const now = text.slice(from, to);
      if (now === want) { console.log("n47-b-rows: " + name + " already in"); continue; }
      text = text.slice(0, from) + want + text.slice(to);
      console.log("n47-b-rows: " + name + " numbers written (" + Object.keys(nums).filter((x) => !x.endsWith("_why")).join(", ") + ")");
    }
    JSON.parse(text);
    out = text;
  }
  fs.writeFileSync(file, out);
  console.log("n47-b-rows: wrote " + rel);
}
