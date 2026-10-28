// NIGHT47 builder F: write the numbers of F's own config rows (design.n47.pulse47, later overheal47) into Design/unity-config.json and its
// byte copy Assets/StreamingAssets/Design/unity-config.json. Touches ONLY the "numbers" object of the named rows (a text edit inside each row's
// block; nothing else in the file moves). Re-running with the same numbers is a no-op. Modeled on Tools/night/n46-b1-rows.cjs.
// Run (from the repo or worktree root): node Tools/night/n47-f-rows.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const FILES = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];

// Each value with its _why (Research/2026-10-18-wave47/F-cards.md 1b; measured by Tools/SimHarness/Harness.N47.Feel.cs `n47feel blast`).
const ROWS = {
  pulse47: {
    launchSpeed: 18,
    launchSpeed_why: "m/s: the blast throws each husk-shaped body it hits through launch36's flight (design.n36.launch36: a launched body that meets another body or a wall bursts). launch36's moving punch throws at 16; the gauntlet's ultimate throws harder. NIGHT46's blast shoved 7 m/s for 0.2 s = 1.40 m (n47feel blast, 5 / 5 seeds): GG's 'weak gust of wind with no weight'.",
    lift: 6,
    lift_why: "m/s up at the launch (launch36's 5): a visible arc, off the floor about 0.6 s at the sim gravity.",
    flightS: 0.8,
    flightS_why: "s of flight (launch36's flightS 0.8): with 18 m/s the throw on open floor is ~14 m; a wall in reach bursts the body first.",
    range: 7,
    range_why: "m along the aim the blast reaches (design.n27.pulse27.blastRange 6).",
    radius: 2.2,
    radius_why: "m from the aim line within which a body is on the line (+ its radius + the boss margin; design.n27.pulse27.blastRadius 1.5): a blast, not a needle.",
    damageMult: 3.5,
    damageMult_why: "x weapons.gauntlet.damage (32 -> 112 per body on the line; design.n27.pulse27.blastDamageMult 2 = 64). Five kills earn it and it waits 15 s, so it is the gauntlet's big moment.",
    ringRadius: 3,
    ringRadius_why: "m round the player (flat, + the body's radius): the second ring catches what stands beside or behind you (Doom Eternal's Blood Punch: an inner hit and an outer shockwave; Research/2026-10-18-wave47/F.md R9).",
    ringDamageShare: 0.5,
    ringDamageShare_why: "share of the line's damage a body in the ring takes (56): the aim still matters.",
    hitstopS: 0.083,
    hitstopS_why: "s of the sim's hitstop on a blast that hits (10 steps at 120 Hz; DESIGN-RULES C-2: the gauntlet 12-20 steps, heavy feedback 70-90 ms, NIGHT47.md section 3), through SurvivalSim.Hitstop (loadout.hitstop41, cause 'pulse47').",
    staggerS: 0.6,
    staggerS_why: "s a body that ignores a launch (a boss, the bulwark, the burrower) is stunned by the blast instead (Enemy.Stun), as the FUSION's 0.6 s stagger (design.n43.fusion43).",
  },
  overheal47: {
    cap: 150,
    cap_why: "the most health with overheal (health.max 100 + 50): one more husk hit and a half of room (husk.meleeDamage 15), not a second bar; the HUD draws 100 -> 150 blue -> purple (F-cards 2c). DECIDE47 D-4: a kill spills health above 100 up to a cap.",
    holdSeconds: 2.0,
    holdSeconds_why: "s the overheal holds after its last gain before it drains (so a player who keeps killing up close keeps it; the gap between kills in a fight is measured by n46flow's kgap column: not checked for this number tonight).",
    decayPerSecond: 2.0,
    decayPerSecond_why: "health per s the overheal drains back toward 100 after the hold (50 drains in 25 s): decay, never regen below the max (CORE: no passive in-round regen, w19), so the rule holds.",
  },
  fx47: {
    corePunchSlow: { scale: 0.25, hold: 0.45, ease: 0.25 },
    corePunchSlow_why: "THE WICK's core punch (W's G7, NIGHT47.md: 'a short slow motion and a hard camera hit'): the game loop's slow-mo (ImpactRules, as design.impact.slowMo: the sim keeps its 120 Hz steps, fewer of them per real second) at scale 0.25 (stronger than the last kill's 0.3: the fight's biggest moment), held 0.45 s of real time, eased back over 0.25 s; design.impact.slowMo.minGap (3 s) still rules. Requested on the event wick47CorePunch with the fx47 row on (Game/ImpactFrames.cs).",
  },
  copy47: {
    map_why: "Every rewritten player-facing template: the OLD text exactly as the config or level file has it (its {action} placeholders kept, so the live glyph or key fills in) -> the new one (Game/Copy47.cs applies it at the teach card, the lesson widget and the boss lines; a line the map lacks shows as it is). The rules (NIGHT47.md section 3): second person, verb first, plain words, <= 10 words a line, the button shown, sentence case for lines, names in caps. The inventory: Research/2026-10-18-wave47/F-cards.md section 3. The lead's picks (DECIDE47 D-11): the gauntlet pickup = variant A, the game start = variant B. THE WICK's lines are builder W's (not here).",
    map: {
      // the two GG named (the lead's picks)
      "PRESS {fire} to PUNCH. {altFire} to PARRY.": "THE PULSE GAUNTLET. Punch with {fire}. {altFire} parries. Five kills: it blasts.",
      "MOVE. LOOK. {jump} JUMP.": "THE FOUNDRY. Stay fast. {jump} to jump, again in the air.",
      // teach35's cards (design.n35.teach35.lines)
      "PRESS {dash} to DASH. Works in the air.": "{dash} to dash. It works in the air too.",
      "HOLD {slide} to SLIDE. You keep your speed.": "Hold {slide} to slide. You keep all your speed.",
      "PRESS {fire} to FIRE the KESTREL.": "Shoot the KESTREL with {fire}.",
      "HOLD {fire} to CHARGE. Release to fire.": "Hold {fire} to charge, let go to fire. It pierces.",
      "HOLD {fire} to CHARGE. The shot pierces.": "Hold {fire} to charge, let go to fire. It pierces.",
      "HOLD {altFire} to ZOOM. Aim on a wall slows time.": "Hold {altFire} to zoom. On a wall, time slows.",
      "HOLD {jump} on a WALL to WALL RIDE. Let go to leap.": "Hold {jump} on a wall to ride. Release to leap.",
      "PRESS {jump} at a WALL to WALL RIDE.": "Hold {jump} on a wall to ride it.",
      "PRESS {jump} on a WALL to WALL JUMP. Chain them.": "{jump} off a wall to wall jump. Chain them up.",
      "HOLD {fire} to CHARGE a SLUG. Release to fire.": "Hold {fire} for a slug. It cracks armour.",
      "HOLD {fire} to CHARGE a SLUG. It cracks armour.": "Hold {fire} for a slug. It cracks armour.",
      "PRESS {grapple} to GRAPPLE. It pulls you to the hit.": "{grapple} to grapple. It pulls you where you aim.",
      "TAP {grapple} to PULL. HOLD to SWING.": "Tap {grapple} to pull. Hold it to swing.",
      "LATCH with {grapple} to BEND time. SWAP guns in it.": "Hook an enemy with {grapple}: time slows. Swap guns then.",
      "PRESS {dash} then {jump} to JUMP DASH.": "{dash}, then {jump}: a jump dash. Go far.",
      "AIM {grapple} at an enemy to WHIP. Then PUNCH.": "{grapple} an enemy to yank it close. Then punch it.",
      "THE THUNDERCLAP. PRESS {fire} to BLAST.": "THE THUNDERCLAP. {fire} to fire. Shoot down to fly.",
      "THE STITCHER. HOLD {fire} to STREAM nails.": "THE STITCHER. Hold {fire} to pour nails.",
      "THE TORCH. HOLD {fire} to BURN.": "THE TORCH. Hold {fire} to burn them.",
      // the kiln chain (design.n43.kiln43, through the teach card)
      "STEP 1 · PRESS {jump} TO JUMP": "{jump} to jump.",
      "STEP 1 · IN THE AIR, LOOK STRAIGHT DOWN": "In the air, look straight down.",
      "STEP 1 · PRESS {fire}: THE BLAST LIFTS YOU": "{fire}: the blast lifts you.",
      "STEP 2 · {jump} AND {fire} TOGETHER: HIGHER": "{jump} and {fire} together go higher.",
      "STEP 3 · AT THE TOP, PRESS {fire} AGAIN": "At the top, {fire} again.",
      "THE BOOST: JUMP, LOOK DOWN, FIRE, FIRE AGAIN": "THE BOOST: jump, look down, fire, fire again.",
      "THE FLUE · {fire}, {jump} AT THE TOP, {fire} AGAIN": "THE FLUE: {fire}, {jump} at the top, {fire} again.",
      "HOLD {fire} ON THE GROUND: CHARGE A SLUG": "On the ground, hold {fire} for a slug.",
      "IT HUNTS THE FLOOR: JUMP, BOOST, OR A PLATE": "Something hunts the floor. Get up high.",
      "HIT IT AS IT SURFACES: IT REELS": "Hit it as it surfaces.",
      // room lessons (the n46 level files; the n47 levels keep them unless their builder changes a line)
      "HOLD {slide}: SLIDE UNDER": "Hold {slide} to slide under.",
      "HOLD {jump} ON A WALL: RIDE": "Hold {jump} on a wall to ride it.",
      "PUNCH {punch} THE YELLOW GLOB BACK": "Yellow means punch it. {punch} sends it back.",
      "HOLD {jump} ON THE WALL": "Hold {jump} on the wall.",
      "{jump} WALL TO WALL. {dash} ACROSS": "{jump} wall to wall, then {dash} across.",
      "LOOK UP. {jump} WALL TO WALL": "Look up. {jump} from wall to wall.",
      "HOLD {altFire}: SLOW. {fire}: PIERCE": "Hold {altFire} to slow time. {fire} pierces.",
      "{fire}: ONE SWING FELLS A TANK": "{fire}: one gauntlet swing drops a tank.",
      "{fire} THE SHIELD, OR FLANK IT": "Punch the shield with {fire}, or get behind it.",
      "FIVE KILLS: {altFire} BLASTS THE PACK": "Five kills charge it. {altFire} blasts the pack.",
      "{fire} AS THE YELLOW ORB ARRIVES": "Punch the yellow orb back with {fire}.",
      "SAVE {altFire} FOR THE SECOND PACK": "Save {altFire} for the second pack.",
      "KESTREL BACK: {fire} THE FAR ONES": "KESTREL back: {fire} the far ones.",
      "{dash} INTO IT. {fire} THE GAUNTLET.": "{dash} in, then {fire} the gauntlet.",
      "{quickSwap}: KESTREL FAR, GLOVE CLOSE": "{quickSwap}: KESTREL far, GAUNTLET close.",
      "{dash} ASIDE: IT HITS THE WALL": "{dash} aside and it hits the wall.",
      "{dash} ASIDE, THEN {fire} ITS BACK": "{dash} aside, then {fire} its back.",
      "THREE STEPS, EACH ONE HIGHER": "Three steps, each one higher.",
      "UP ALL THREE, THEN DROP DOWN": "Up all three, then drop.",
      "JUMP. SHOOT DOWN. JUMP. SHOOT DOWN.": "Jump, shoot down, again.",
      "JUMP IN. SHOOT {fire} DOWN. FLY.": "Jump in, {fire} down, fly.",
      "CLIMB IT ON THE RUN.": "Climb it at a run.",
      "{grapple} PULL TO THE RING.": "{grapple} to pull to the ring.",
      "LOOK UP. {grapple} PULL.": "Look up. {grapple} to pull.",
      "KILLS GIVE PULLS.": "Kills give you more pulls.",
      "TWO CHARGES. TWO PULLS.": "Two charges: two pulls.",
      "{grapple} AT AN ENEMY: WHIP.": "{grapple} an enemy to yank it.",
      "{grapple} PULL. RIDE. PULL.": "{grapple}: pull, ride, pull.",
      "{grapple} HOOK KNEE. {grapple} PULL AWAY.": "{grapple} a lit knee, then pull away.",
      "THE TORCH: HOLD {fire} CLOSE": "THE TORCH: hold {fire} up close.",
      "{dash} DASH, THEN {jump}: FLY": "{dash}, then {jump}: fly.",
      "{dash} + {jump}: THROUGH THE RING": "{dash} + {jump} through the ring.",
      // boss lines through the lesson widget (BossLine30 and the bosses' own posts; THE WICK's are W's)
      "GET IN CLOSE: {fire} THE GAUNTLET HITS FULL": "Get close: {fire} hits hard.",
      "TOO FAR: SHOTS BARELY SCRATCH IT": "Too far. Your shots barely scratch.",
      "ITS FIST IS STUCK: HIT IT NOW": "Its fist is stuck. Hit it!",
      "YELLOW BLOCK: {punch} PUNCH IT BACK": "Yellow block. {punch} it back.",
      "PUNCH THE GLOWING HEAD.": "{punch} the glowing head.",
      "HOOK A LIT KNEE {grapple}. THEN {grapple} PULL AWAY.": "{grapple} a lit knee. Pull away.",
      "HOOKED. NOW {grapple} PULL AWAY TO A THREAD.": "Hooked! {grapple} away to tear it.",
      "OPEN: SHOOT THE CORE. EVERY GUN COUNTS.": "It's open. Shoot the core!",
      "FLY THROUGH THE RING: {dash} DASH + {jump} JUMP": "{dash} + {jump} through the ring.",
      "STAGGERED: EVERY HIT COUNTS": "Stunned! Every hit counts.",
      // the message slot's danger words (C# strings: UnityTuning.Spawns33 StillArmor33Line, UnityTuning.Hud33 Hud33DangerExposed), as fight lines under hud47
      "MOVE": "Keep moving!",
      "EXPOSED": "Standing still! Move.",
    },
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
      if (text.slice(from, to) === want) { console.log("n47-f-rows: " + name + " already in"); continue; }
      text = text.slice(0, from) + want + text.slice(to);
      console.log("n47-f-rows: " + name + " numbers written (" + Object.keys(nums).filter((x) => !x.endsWith("_why")).join(", ") + ")");
    }
    JSON.parse(text);
    out = text;
  }
  fs.writeFileSync(file, out);
  console.log("n47-f-rows: wrote " + rel);
}
