// NIGHT38 shared commit (the lead): adds design.n38 to Design/unity-config.json (every row off), then copies the file to
// Assets/StreamingAssets/Design/ (the Player's copy; it was byte-identical before this run).
// Run: node Tools/night/n38-config-block.cjs   (refuses to run twice). Builders patch ONLY their own row's text afterwards (L-049).
// The level rows' levelFrom values are the campaign order with NIGHT37's rows on (Lesson25Tests / N38SharedTests pin it).
const fs = require("fs");
const file = "C:/Users/gaige/dev/SKYGRIND-unity/Design/unity-config.json";
const copy = "C:/Users/gaige/dev/SKYGRIND-unity/Assets/StreamingAssets/Design/unity-config.json";
const raw = fs.readFileSync(file, "utf8");
const d = JSON.parse(raw);
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const tail = eol + "    }" + eol + "  }" + eol + "}";
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
if (d.design.n38) throw new Error("design.n38 already exists");
const LEVEL = "Flow only: a levelFrom -> levelTo swap in Curriculum25.N38Levels while on (the level file must exist first). ";
d.design.n38 = {
  _why: "NIGHT38, the flight night: every level rebuilt for flight (walls you stay on, gates that force their verb, open roofed arenas), doors you can see, a HUD that shows the charge (GG 2026-10-13 ~00:10, verbatim in Research/2026-10-13-wave38/GG-REQUEST.md; PLAN.md; DECIDE38.md D-1..D-6). Every row is off in the shared commit and turned on only when its check passes. Off (--off38 / -skygrindOff38, and every older off switch): NIGHT37's game bit for bit. Sim rows are behind N38Blocks; view rows (doors38, hud38, juice38) read their own `enabled` and are off with --off38 as well.",
  ride38: { _why: "Builder M (NIGHT38 PLAN.md section 1, decision D-3): a HELD wall ride keeps its height (no sink, a 60 s stuck guard, no 8 s cap); the look pitch beyond +-15 deg drifts the ride up or down at up to ~2.5 m/s, never above the wall's top; the timed (tap) ride keeps the 1.2 s rule. Builder M adds its numbers with their _why (in this row only).", enabled: false },
  foundry38: { _why: "Builder F (PLAN.md section 2 L1, D-4): level 1 keeps a1-a6 and adds an open roofed arena (KESTREL + ride walls, enemies on the route) and a flight room of wall-ride gates before THE WICK's rehearsal rooms. " + LEVEL, enabled: false, levelFrom: "n37-foundry", levelTo: "n38-foundry" },
  iceline38: { _why: "Builder S2 (PLAN.md section 2 L2): level 2's copied rooms i1a..i7 replaced by skating, GAUNTLET and ride-chain rooms, open roofed arenas, one flight room of skate-lane + ride gates; s1-s4 and the canal kept. " + LEVEL, enabled: false, levelFrom: "n37-iceline", levelTo: "n38-iceline" },
  kiln38: { _why: "Builder K3 (PLAN.md section 2 L3, the THUNDERCLAP and the shotgun boost): level 3 rebuilt whole: boost gates (ceilings under the double-jump apex, walls a ride cannot use, ledges only a mid-air boost reaches), open roofed arenas; THE KILN WYRM's room k5 and the Wyrm untouched (GG). " + LEVEL, enabled: false, levelFrom: "n35-kiln", levelTo: "n38-kiln" },
  span38: { _why: "Builder O2 (PLAN.md section 2 L4): level 4's copied rooms s1-s7 replaced by grapple gates over voids no jump or ride chain crosses, open roofed arenas with nodes, the STITCHER; o1-o3 kept. " + LEVEL, enabled: false, levelFrom: "n37-span", levelTo: "n38-span" },
  spire38: { _why: "Builder C2 (PLAN.md section 2 L5, D-5-style limit on the dash): level 5's copied rooms v1-v8 replaced by far dash-jump gates under ceilings with a limited dash meter at each gate (so the dash jump cannot be spammed gap to gap), open roofed arenas, the TORCH; v9-v11 kept." + LEVEL, enabled: false, levelFrom: "n37-spire", levelTo: "n38-spire" },
  finale38: { _why: "Builder F (PLAN.md section 2 L6): level 6 rebuilt whole with mixed gates (one per verb, in sequence: ride, boost, grapple, dash jump), two open arenas, its boss room as is (the old Stag until THE RIVAL, D-5). " + LEVEL, enabled: false, levelFrom: "n35-finale", levelTo: "n38-finale" },
  stag38: { _why: "Builder S2 (PLAN.md section 5): THE STAG's lane walls 5 m tall in runs of 30 m+ (a 2.6 m wall holds only while the feet are under ~2.25 m: Mover's contact band), the island walls rideable, a read pass (an off-screen arrow to THE STAG, brighter lane warnings). Owns gen-n37-iceline-boss.cjs and Stag37*.", enabled: false },
  doors38: { _why: "View row (builder M, PLAN.md section 3; D-2): every doorway is a holographic curtain visible from 40 m: CYAN = open (no collider, pass through), RED-ORANGE = locked (solid), the boss door PALE VIOLET-WHITE with its sign; the next room lit behind it. Off: today's dark gates.", enabled: false },
  hud38: { _why: "View row (builder H, PLAN.md section 4): under the crosshair a three-segment bar per gun's charge / cooldown (one colour charging, cyan when full, a white flash + event hud38Ready on the frame it fills); the crosshair ticks rotate through each reload and snap square when ready; the grapple's hook icon with two charge pips. Off: today's HUD.", enabled: false },
  juice38: { _why: "View row (builder H, PLAN.md section 4): a boss entrance = hit-stop + a shake (the bigger replaces, decays 3/s) + a full-screen flicker + the card; big boss moves shake and flicker; damage = a full-screen static / RGB-split burst for the i-frame time scaled by the hit. Wires feel.trauma.boss (read by nothing today). Off: today's feel.", enabled: false },
};
const block = JSON.stringify(d.design.n38, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
const head = raw.slice(0, raw.length - tail.length);
const out = head + eol + "    }," + eol + "    \"n38\": " + block + eol + "  }" + eol + "}";
fs.writeFileSync(file, out);
JSON.parse(fs.readFileSync(file, "utf8"));
fs.copyFileSync(file, copy);
console.log("design.n38 added: " + Object.keys(d.design.n38).filter((k) => k !== "_why").join(", "));
