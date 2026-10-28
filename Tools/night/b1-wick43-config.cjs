// NIGHT43 builder B1 (wick43): writes design.n43.wick43 (THE WICK readable: the bar's state and hints, the reveal, the legs' beats and
// variations, the shaft's air and panels) in Design/unity-config.json and copies the file to Assets/StreamingAssets/Design/unity-config.json
// (the two byte-identical before and after). Patches ONLY the wick43 row's text (L-049): the row from `"wick43": {` to its closing brace at
// the same indent is replaced; `enabled` stays false (the lead turns it on after the frames). Re-runnable. Run from the worktree root:
// node Tools/night/b1-wick43-config.cjs
const fs = require("fs");
const path = require("path");
const root = path.resolve(__dirname, "..", "..");
const file = path.join(root, "Design", "unity-config.json");
const copy = path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json");
let raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ before the patch");

const row = {
  _why: "Builder B1 (G13-G16, G10; GG 2026-10-16: 'the player needs clear feedback explaining whether the boss is taking damage, whether it is vulnerable, what phase it is in'; 'meaningful phases, evolving attacks, and readable transitions'; 'even more cinematic ... if it does not interrupt the game's momentum'; DECIDE43 D-7). THE WICK readable: the boss bar's state for the whole fight (SurvivalSim.N43.Wick.cs N43BossStateFill: valid from the reveal to the dying, hp, the leg's lock, OPEN, the stagger meter, the leg of 3, a hint), the reveal (the look eased toward the rising WICK while the player keeps control; the letterbox is the view's), each leg change a beat (Core/Enemies/Wick37.N43.cs: the recoil, the slag drop, bossLeg43, a gift crack) and one attack variation per later leg (not more HP). Notes and measurements: Research/2026-10-16-wave43/B1.md; harness: n43wick (Tools/SimHarness/Harness.N43.Wick.cs).",
  enabled: false,
  enabled_why: "Off until its check passes and the lead has looked at its before / after Player frames and iterated once (NIGHT43.md method 2).",
  hints: {
    _why: "The bar's hint (Boss43State.hint, U1's bossbar43 draws it): what hurts THE WICK now, in the HUD's words, {glyph} tokens for the bound buttons. One line per state, picked in this order: reveal, legChange, stall, window, crack, locked, ember, cap, idle (SurvivalSim.N43.Wick.cs Wick43Hint). The card's two lines are crack and idle (NIGHT43.md B1 G14).",
    reveal: "HOLD {jump}: RIDE. LET GO: LEAP UP",
    reveal_why: "The walk-in's reveal (no damage yet): the climb's verb first (BOSSES.md rule 2: movement is the only way). MEASURED (n43wick climb --off): the HELD ride's jump-off climbs THE STACK (10/10 hands, skilled and casual), a PRESSED wall jump never does (0/10, max feet 3.0 m: it peaks ~1.5 m, the panels rise 2.8 m, 90 degrees round), so the bar names the hold.",
    legChange: "LEG CLEARED: A PLATE CRACKS",
    legChange_why: "The brain's LEGCHANGE second (no attacks, no damage): says the gift crack is coming (legs.giftCrack).",
    stall: "STUCK: {dash} INTO IT, {punch}",
    stall_why: "Leg 3, the dive's stall at the wall: the launch (design.n37.wick37.boss.dive: a punch while dashing knocks it into the shaft).",
    window: "STAGGERED: EVERY HIT COUNTS",
    window_why: "The CYAN window (the stagger meter emptied): full damage from anything (Wick37.Filter: brain.Open).",
    crack: "RIDE, HOLD {fire}: SHOOT THE CRACK",
    crack_why: "A plate is cracked: a charged KESTREL shot from a ride through the gap lands in full (Wick37.Filter's pierce).",
    locked: "LOCKED: CLIMB TO THE NEXT LEDGE",
    locked_why: "The HP sits on the leg's lock (design.n37.wick37.boss.legLock 0.6 / 0.3): no hit moves the bar until the player stands on the next ledge (the legs are places). CODE-FACTS-B: 'The HP locks read as not taking damage'.",
    ember: "{punch} THE EMBER: CRACK A PLATE",
    ember_why: "EMBER's yellow tell or a glob in the air: the parry returns it into the cage and cracks the plate it hits (the fight's central verb, GG: 'preserve and build upon that').",
    cap: "DODGE THE DIVE, {punch} THE STALL",
    cap_why: "Leg 3 on the cap between attacks: the dive, then its stall is the opening.",
    idle: "CRACK A PLATE: RETURN AN EMBER",
    idle_why: "Anything else: the way in (NIGHT43.md B1 G14's first line)."
  },
  bosses: {
    _why: "Round 2 (the lead: DESIGN-RULES B-1, every boss): wick43 also fills the boss bar's state (N43BossState) for THE ICEBREAKER, THE KILN WYRM, THE LOOM and THE CROWN (SurvivalSim.N43.Bosses.cs): HP, the phase / leg, OPEN, the stagger meter and these hints, and emits bossOpen43 / bossLeg43 for them. The hint is picked by the boss's own state (its tell, its window, its stuck fists, its stagger, its open core).",
    window: "STAGGERED: EVERY HIT COUNTS",
    window_why: "Any boss's CYAN window (THE ICEBREAKER's, THE LOOM's, THE CROWN's): full damage from anything.",
    brawlStuck: "FISTS STUCK: HIT IT NOW",
    brawlStuck_why: "THE ICEBREAKER's fists in the ice after a haymaker / a rush / a leap: every hit x design.n40.brawl40 stuckMult.",
    brawlBlock: "{punch} THE BLOCK BACK",
    brawlBlock_why: "Its BLOCK's tell or a block in the air: the parry returns it and drains the stagger meter (the way to its window).",
    brawlHaymaker: "{dash} ASIDE, THEN HIT IT",
    brawlHaymaker_why: "The haymaker's tell: dash out of its lunge (n40-iceline's rehearsal line '{dash} ASIDE, THEN {fire} ITS BACK'); it sticks.",
    brawlSweep: "{jump} OVER THE SWEEP",
    brawlSweep_why: "NIGHT42's sweep tell (low arm, a ring on the ice): jump it.",
    brawlLeap: "{dash} OUT OF THE CIRCLE",
    brawlLeap_why: "NIGHT42's leap tell (a circle tracking you): leave it before it locks; it lands stuck.",
    brawlIdle: "PARRY THE BLOCK. PUNISH STUCK FISTS",
    brawlIdle_why: "Anything else: its two openings.",
    wyrmStagger: "STAGGERED: HOLD {fire}, SLUG IT",
    wyrmStagger_why: "THE KILN WYRM staggered by a head punch (design.n31.wyrm31): the THUNDERCLAP's slug lands in full.",
    wyrmCore: "ITS CORE IS OPEN: SHOOT IT",
    wyrmCore_why: "The core open (Wyrm.CoreOpen): its core multiplier.",
    wyrmCharge: "{punch} ITS HEAD AS IT COMES",
    wyrmCharge_why: "A charge / dive coming (Wyrm.Charging30): the head punch staggers it (design.n30.wyrm30).",
    wyrmIdle: "PUNCH THE HEAD TO STAGGER IT",
    wyrmIdle_why: "Anything else: guns land design.n31.wyrm31 gunScale until it is staggered."
  },
  reveal: {
    _why: "G15 / DECIDE43 D-7 (you keep control; letterbox + the camera eased toward the WICK for ~2.5 s the first time, ~1 s on a retry); research/bosses-web.md C.11 ('one in-engine camera move of <= 2 s that ends on the player's own view (or no camera takeover)') and B.8 (first meeting <= 6-8 s, every retry <= 1.5 s). The look ease is the sim's (it is the player's real aim: what you see is where you shoot); the letterbox and the shaft's lights are the view's (Wick43RevealView, UnityTuning.Wick43.cs).",
    firstSeconds: 2.5,
    firstSeconds_why: "DECIDE43 D-7: ~2.5 s the first time (THE WICK rises over design.n37.wick37.boss.revealSeconds 2 s: the ease frames the whole rise and lets go as the fight starts).",
    retrySeconds: 1.0,
    retrySeconds_why: "DECIDE43 D-7: ~1 s on a retry (bosses-web B.8: every retry <= 1.5 s).",
    lookRate: 4,
    lookRate_why: "Per second at the curve's full weight: the share of the look's angle left to the WICK closed each second (4 x 1/120 = 3.3 % a step: ~85 % of a 90 degree turn closed in the first second at full weight, no snap).",
    yieldDeg: 1.0,
    yieldDeg_why: "Degrees in one step (120 deg/s): a turn of the player's own bigger than this ends the ease for this reveal at once (D-7: the player keeps control; a mouse flick or a stick push wins).",
    pitchMaxDeg: 35,
    pitchMaxDeg_why: "The eased look never pitches past 35 degrees up or down (THE WICK rises from 3 m under the floor to 2 m over the player's head: a framing, not a stare at the floor)."
  },
  air: {
    _why: "G10 / G13 ('the player essentially runs into what feels like a brick wall'; CODE-FACTS-A B8 names the shaft's 12.4 m/s clamp a candidate): THE STACK's thick air (design.n37.wick37.air.maxSpeed: every leap of the climb flies the same arc) with wick43 on is never applied on a floor (the run in from the tunnel, a ledge, the floor keep their speed) and in the air bleeds the speed over 12.4 m/s at easeRate instead of cutting it in one step (SurvivalSim.N43.Wick.cs Wick43AirClamp).",
    easeRate: 30,
    easeRate_why: "Metres per second per second the airborne speed over the clamp bleeds (20 -> 12.4 m/s in 0.25 s: still well before the next panel ~66 degrees round; NIGHT42: in one step)."
  },
  legs: {
    _why: "G16 (research/bosses-web.md B.7: 'every transition: the layer breaks on the bar, ~1 s of boss invulnerability with a roar / pose, the arena change happens in view, then a free hit opportunity'; 'phase 2 adds one attack ... not the boss's HP'). The leg change is NIGHT37's (the player stands on the next ledge / the cap); these add the beat and one variation per later leg, built on the same language (Core/Enemies/Wick37.N43.cs).",
    volleyGap: 0.45,
    volleyGap_why: "Leg 2: EMBER's second volley this many seconds after the first (3 more yellow globs, aimed again): two parries in a rhythm, inside the attack's own 0.3 s active + 0.5 s recover (no new tell, no longer attack).",
    leg2Volleys: 2,
    leg2Volleys_why: "Leg 2's volleys per EMBER (leg 1: design.n37's one).",
    flareLeg3: true,
    flareLeg3_why: "Leg 3: the FLARE comes back on the cap (design.n37.wick37.attacks.flare.legs [1, 2]): the red half-wipe the player learned on the walls, now on the catwalk.",
    capFlareHalfDeg: 55,
    capFlareHalfDeg_why: "Leg 3's flare burns +-55 degrees round the player's angle (the walls' 90 would be half the 4 m catwalk: 34 m of running in a 1.4 s tell). 110 degrees at the catwalk's 11 m middle is 21 m of arc; out of it is ~11 m round (1.1 s at 10.5 m/s) or a dash across the hole.",
    capFlareWeight: 0.2,
    capFlareWeight_why: "Leg 3's flare's pick weight (design.n37's 1.0 on the walls; the DIVE 1.5, EMBER 1.4). MEASURED (n43wick band, 10 seeds, wick43 + chimney43 + ride43 + air.easeRate 30): weight 1.0: p50 54.0 / p90 69.7 s (the cap flare took the DIVE's turns: the stall is leg 3's only opening); 0.4: p50 54.0 / p90 63.6; 0.2: p50 49.3 / p90 63.6 (one seed); no cap flare: p50 50.5 / p90 52.8. The pity (+0.25 a turn) still brings it every few attacks.",
    capFlareEvery: 2,
    capFlareEvery_why: "Round 2 (the lead: the band SHORT at p90 63.6 s): after a cap flare's tell the flare leaves leg 3's pick until this many DIVE tells have come (Wick37.N43.cs SetFlareLeg3): the dive's stall, leg 3's only opening, comes at least twice between flares. Measured in Research/2026-10-16-wave43/B1.md (round 2).",
    slagDrop: 3,
    slagDrop_why: "Metres the slag falls at each leg change (clamped at its start): the arena changes in view and the climb behind you cools (the next leg starts with room under it).",
    recoilSeconds: 0.6,
    recoilSeconds_why: "The recoil's length (the view's cage shudder reads Wick37.recoil43): about half of the brain's 1.5 s LEGCHANGE.",
    recoilUp: 2.5,
    recoilUp_why: "Metres the cage is kicked up at the beat (its follow, 6 m/s, brings it back in ~0.4 s): it reads as being hurt.",
    recoilBack: 1.5,
    recoilBack_why: "Metres it is kicked away from the player at the beat (decays 5 % a step back toward the axis).",
    giftCrack: true,
    giftCrack_why: "When the LEGCHANGE ends, the plate facing the player cracks for design.n37.wick37.boss.crackSeconds (4 s): a free ride-shot to restart momentum (bosses-web B.7's gift window)."
  }
};

// find the row
const key = '"wick43": {';
const at = raw.indexOf(key);
if (at < 0) throw new Error("design.n43.wick43 not found");
const lineStart = raw.lastIndexOf("\n", at) + 1;
const indent = raw.slice(lineStart, at);
if (!/^ *$/.test(indent)) throw new Error("unexpected text before the row's key");
const eol = raw.slice(at + key.length, at + key.length + 2) === "\r\n" ? "\r\n" : "\n";
const closeRe = new RegExp("\\r?\\n" + indent + "\\}", "g");
closeRe.lastIndex = at;
const cm = closeRe.exec(raw);
if (!cm) throw new Error("the row's closing brace was not found");
const end = cm.index + cm[0].length;
const body = JSON.stringify(row, null, 2).split("\n").map((l, i) => (i === 0 ? l : indent + l)).join(eol);
const out = raw.slice(0, at) + '"wick43": ' + body + raw.slice(end);
JSON.parse(out); // still valid JSON
fs.writeFileSync(file, out);
fs.writeFileSync(copy, out);
console.log("design.n43.wick43 written (" + (end - at) + " -> " + ('"wick43": '.length + body.length) + " chars), copied to StreamingAssets");
