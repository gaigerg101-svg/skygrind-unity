// NIGHT43 shared commit (the lead): adds design.n43 to Design/unity-config.json (every row off), registers n43-foundry, n43-iceline and
// n43-kiln in design.maps.list (campaignOnly, after n42-foundry), writes their placeholder files (byte copies of n42-foundry, n40-iceline
// and n41-kiln with only the "name" changed) so the list never points at a missing file before the builders' generators write the real
// ones, then copies the config to Assets/StreamingAssets/Design/ (byte-identical before this run). Refuses to run twice. Builders patch
// ONLY their own row's text afterwards (L-049), with a .cjs script.
// Run: node Tools/night/n43-config-block.cjs
const fs = require("fs");
const root = "C:/Users/gaige/dev/SKYGRIND-unity/";
const file = root + "Design/unity-config.json";
const copy = root + "Assets/StreamingAssets/Design/unity-config.json";
let raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ before the block");
const d = JSON.parse(raw);
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
if (d.design.n43) throw new Error("design.n43 already exists");

// 1. the maps.list rows
const levels = [
  ["n43-foundry", "LEVEL 1 THE FOUNDRY", "n42-foundry"],
  ["n43-iceline", "LEVEL 2 THE ICE LINE", "n40-iceline"],
  ["n43-kiln", "LEVEL 3 THE KILN", "n41-kiln"],
];
const anchorRe = /(\{\s*"id":\s*"n42-foundry",[^}]*\})/;
const m = raw.match(anchorRe);
if (!m) throw new Error("the n42-foundry row was not found");
if (raw.includes('"id": "n43-')) throw new Error("n43 rows already present");
const indent = raw.slice(raw.lastIndexOf(eol, m.index) + eol.length, m.index);
const add = levels.map(([id, label]) => "," + eol + indent + JSON.stringify({ id, label, file: "Design/levels/" + id + ".json", campaignOnly: true }).replace(/","/g, '", "').replace(/":/g, '": ').replace(/,"campaignOnly"/, ', "campaignOnly"')).join("");
raw = raw.replace(anchorRe, m[1] + add);

// 2. design.n43, every row off
const tail = eol + "    }" + eol + "  }" + eol + "}";
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
const off = "Off until its check passes and the lead has looked at its before / after Player frames and iterated once (NIGHT43.md method 2).";
const g = "Research/2026-10-16-wave43/GG-REQUEST.md";
const n43 = {
  _why: "NIGHT43, the cohesion night (GG 2026-10-16 after playing n42-r4 up to level 3; thirty asks verbatim in " + g + " as G1-G30; NIGHT43.md; DECIDE43.md; CODE-FACTS-A.md / CODE-FACTS-B.md; docs/DESIGN-RULES.md). Every row is off in the shared commit and turned on only when its check passes and its before / after frames were looked at. Sim rows (chimney43, ride43, boost43, foundry43, gauntlet43, parry43, fusion43, rooms43, iceline43, kiln43, burrow43, wick43) are behind N43Blocks; the view rows (hud43, counter43, teach43, screens43, bossbar43, feel43, models43, look43) are N43View rows (no bit). Off (--off43 / -skygrindOff43, and every older off switch): NIGHT42's game bit for bit. The level rows swap levelFrom for levelTo (Curriculum25.N43Levels); an n43 level is its levelFrom to every older system (SurvivalSim.N43BaseId through N42BaseId).",
  chimney43: { _why: "Builder M1 (G13 + G2, GG: 'after a certain number of jumps or transitions, the player simply cannot consistently make the movement work. Fix this at the movement-system level'): the wall-jump count (wallJump.maxPerAirtime 3, refilled only by a landing; CODE-FACTS-A A2) refills on a NEW wall (Titanfall / ULTRAKILL research), so a climb between facing walls never silently turns a press into the double jump. M1 adds its numbers with their _why.", enabled: false, enabled_why: off },
  ride43: { _why: "Builder M1 (G3, GG: 'clunky and buggy, sometimes snaps too aggressively, sometimes causes the player to fall off awkwardly, and can conflict with other inputs such as pressing B'): the ride's attach / hold / detach made forgiving and readable: a softer attach (wallgrip36's 1.5 m/s rise clamp), a longer lostGrace, a wider attach probe, no slam from B in the first moments after a ride (CODE-FACTS-A A3: B after a ride = a slam to (0,-48,0)), a buffered jump-off. M1 adds its numbers with their _why.", enabled: false, enabled_why: off },
  boost43: { _why: "Builder M1 (G26, GG: 'When I press jump and shoot downward at approximately the same time, I do not receive nearly enough height'): the THUNDERCLAP boost combines jump + shot inside a window (a ground press just before the jump fires in the air; a shot just after leaving the ground boosts in full), the jump cut never halves a boost (CODE-FACTS-B G26: releaseMult 0.5 halves it today), boost42's lift by pitch. M1 adds its numbers with their _why.", enabled: false, enabled_why: off },
  foundry43: { _why: "Builder L1 (G1, G2, G4, G10, G11): level 1 as n43-foundry (from n42-foundry; n42-foundry untouched): THE GANTRY over lava with dash gaps, THE FLUE a chimney that carries you up, PIERCE's lesson in order (ride, confirm, then the enemies arrive), the boss door's transition, no accidental backtrack.", enabled: false, enabled_why: off, levelFrom: "n42-foundry", levelTo: "n43-foundry" },
  gauntlet43: { _why: "Builder W1 (G17, GG: 'Experiment with an attack interval around 1.25 to 1.3 seconds'): the PULSE swing 1.8 s (gauntlet42) -> 1.25 s; the n27 / n42 rows untouched.", enabled: false, enabled_why: off, swingSeconds: 1.25, swingSeconds_why: "GG's 1.25-1.3 s (the low end; W1 measures DPS and THE ICEBREAKER's band)." },
  parry43: { _why: "Builder W1 (G18, GG: 'the player currently has to wait until an incoming projectile is extremely close'; CODE-FACTS-B G18: a ~0.11 s window from the eye point): the orb parry intercepts farther: parry42's surface-to-body reach and mercy, plus a swing that reaches along the orb's path and a buffered press. W1 adds its numbers with their _why.", enabled: false, enabled_why: off },
  fusion43: { _why: "Builder W1 (G19, GG: 'If the player performs a high-speed dash impact or gauntlet attack, create a powerful special feedback response'): a gauntlet hit or punch landed at high speed (a dash, a ride jump-off, a slide) is a FUSION: more damage, a knockback, a stagger, the event fusion43 (N43Events.Fusion) for F1's view. W1 adds its numbers with their _why.", enabled: false, enabled_why: off },
  rooms43: { _why: "Builder E1 (G5, G22, G4): a room's door counts only its own enemies (CODE-FACTS-A B9: EnemiesAlive is the whole level), a body out of its room or in a dry pit is ended, a verb lesson never blocks a clear forever, a stuck enemy on screen is moved, a wave can wait for a lesson (wave.afterLesson), 'heavy' is a real kind or a tank. E1 adds its numbers with their _why.", enabled: false, enabled_why: off },
  iceline43: { _why: "Builder E1 (G23, G19): level 2 as n43-iceline (from n40-iceline; untouched): more CHARGER rehearsals before THE ICEBREAKER, the gun-again room teaches dash + gauntlet (FUSION).", enabled: false, enabled_why: off, levelFrom: "n40-iceline", levelTo: "n43-iceline" },
  kiln43: { _why: "Builder K1 (G25, G26, G27, G28): level 3 as n43-kiln (from n41-kiln; untouched): a readable first room (no ramp without a reason), the boost taught small then big, drones and chargers in the compositions, burrowers placed, THE KILN WYRM's ring redesigned.", enabled: false, enabled_why: off, levelFrom: "n41-kiln", levelTo: "n43-kiln" },
  burrow43: { _why: "Builder A1 (G28, GG: 'worm-like enemies ... burrow underneath the player and emerge aggressively'): the BURROWER: tunnels under the floor (a readable trail), a tell, an emergence with a timing window (hit it in the window: a special reaction), a movement threat answered by a boost or a timed hit. A1 adds its numbers with their _why.", enabled: false, enabled_why: off },
  wick43: { _why: "Builder B1 (G13-G16): THE WICK readable and cinematic: the bar while the fight is live (not only while the body is), its state (LOCKED / OPEN / STAGGER) and leg, a beat at each leg change, a short reveal, the shaft's 12.4 m/s air clamp eased at the door. B1 adds its numbers with their _why.", enabled: false, enabled_why: off },
  hud43: { _why: "Builder U1 (G6): the HUD's visual language (DESIGN-RULES UI): the health bar redesigned, type, panels, motion; view only.", enabled: false, enabled_why: off },
  counter43: { _why: "Builder U1 (G5): a small enemies-left count for the locked room, in a corner, in the HUD's language; view only (reads SurvivalSim.N43RoomLeft).", enabled: false, enabled_why: off },
  teach43: { _why: "Builder U1 (G7, GG: 'Study how games such as ULTRAKILL present important tutorial text prominently in the center of the screen'): the centre teaching card (the bound glyph, big, a few seconds, out of the crosshair's way); view only.", enabled: false, enabled_why: off },
  screens43: { _why: "Builder U1 (G24): the shop and results screens over a blurred, darkened game, in panels with hierarchy and motion; view only.", enabled: false, enabled_why: off },
  bossbar43: { _why: "Builder U1 with B1 (G14): the boss bar shows HP, the state (LOCKED / OPEN / STAGGER), the leg and what hurts it (SurvivalSim.N43BossState); view only.", enabled: false, enabled_why: off },
  feel43: { _why: "Builder F1 (G19, G20): the FUSION burst (cyan + orange meeting as hot pink), gun hits and the charged slug, the TORCH's fire, rumble on fire / charge / dash / fusion; view only.", enabled: false, enabled_why: off },
  models43: { _why: "Builder A1 (G21, G23): the enemy models rebuilt in the pill language (visors, shields, armour, per-class shapes; orange / red / yellow); view only.", enabled: false, enabled_why: off },
  look43: { _why: "Builder A1 (G8, G29): the art-direction kit on the levels NIGHT43 touches: lighting that marks the way, hazard reads, fixtures that mean something (DESIGN-RULES art); view only.", enabled: false, enabled_why: off },
};
d.design.n43 = n43;
const block = JSON.stringify(n43, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
const head = raw.slice(0, raw.length - tail.length);
const out = head + eol + "    }," + eol + "    \"n43\": " + block + eol + "  }" + eol + "}";
const chk = JSON.parse(out);
const ids = chk.design.maps.list.map((x) => x.id);
for (const [id] of levels) if (!ids.includes(id)) throw new Error("missing " + id);

// 3. the placeholder level files (both copies; the builders' generators replace them)
for (const [id, , from] of levels) {
  for (const dir of ["Design/levels/", "Assets/StreamingAssets/Design/levels/"]) {
    const dst = root + dir + id + ".json";
    if (fs.existsSync(dst)) throw new Error(dst + " exists");
    let lv = fs.readFileSync(root + "Design/levels/" + from + ".json", "utf8");
    const key = '"name":"' + from + '"';
    const n = lv.split(key).length - 1;
    if (n < 1 || n > 2) throw new Error(from + ": the name key found " + n + " times");
    lv = lv.split(key).join('"name":"' + id + '"');
    const p = JSON.parse(lv);
    if (p.name !== id) throw new Error(dst + ": the name did not change"); // n41-kiln keeps data.name n40-kiln (kiln41 copied it): left as it is
    fs.writeFileSync(dst, lv);
  }
}

fs.writeFileSync(file, out);
fs.copyFileSync(file, copy);
console.log("design.n43 added: " + Object.keys(n43).filter((k) => k !== "_why").join(", "));
console.log("design.maps.list now " + ids.length + " rows; placeholders: " + levels.map((l) => l[0] + " (from " + l[2] + ")").join(", "));
