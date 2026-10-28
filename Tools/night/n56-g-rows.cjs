// NIGHT56 builder G: writes the numbers of G's rows (design.n56.rank56 / tokens56 .numbers, and NIGHT55's design.n55.wyrm55.numbers, G4's row)
// into both config copies (Design/unity-config.json and Assets/StreamingAssets/Design/unity-config.json, byte-identical), replacing only those
// `numbers` objects. Idempotent: run it again after editing the objects below. Asserts the two copies match before and after, that each row's
// line is unique, and that the parsed result differs from the input only inside the named rows' `numbers`. The rows stay `enabled: false` (the
// lead flips them in a build's copy).
//   node Tools/night/n56-g-rows.cjs [row ...]      (no row: every row below)
const fs = require("fs");

const ROWS = {};

ROWS.rank56 = {
  night: "n56",
  numbers: {
    _why: "Builder G (G1; Core/Rank56.cs, SurvivalSim.N56.Rank.cs). The style rank also multiplies heal36's close-kill heal (design.n36.heal36.amount 10 HP) and RIDE KILL's boost refund (design.n53.rideKill53.numbers.charges x the boost's cost), read from the same rank index as points.rankMult (StyleMeter.rank, the rank shown when the kill lands). gameplay.md P1 (ULTRAKILL's Hard Damage recovers x2-x3 at the top ranks: https://ultrakill.wiki.gg/wiki/Hard_Damage). The web has no such numbers (no heal36, no ride kill). Ships OFF for GG to try.",
    mult: [1, 1.2, 1.4, 1.6, 1.8, 2],
    mult_why: "per rank DULL, COOL, BOLD, AIRBORNE, SAVAGE, SKYGRIND: x1.0 at DULL (NIGHT55's payoffs exactly) to x2.0 at SKYGRIND (the brief's ends), in equal steps of 0.2 so every rank-up pays the same: the 10 HP close-kill heal reads 10 / 12 / 14 / 16 / 18 / 20 HP (whole numbers on the view's rising number) and one ride-kill boost (0.5 of the bar) 0.5 / 0.6 / 0.7 / 0.8 / 0.9 / 1.0 (a full bar at SKYGRIND, capped at the bar's max). points.rankMult's own shape (1, 1.1, 1.25, 1.5, 1.75, 2) was the alternative: its first step (+1 HP) is too small to see.",
  },
};

ROWS.tokens56 = {
  night: "n56",
  numbers: {
    _why: "Builder G (G3; Core/HuskBrain.N56.cs, SurvivalSim.N56.Tokens.cs). On top of tokens41's two-wind-up cap: ON SCREEN FIRST (an enemy out of the SIM cone, camera.fovDeg / 2 = 37 deg round the sim's look, waits before it may take a token) and ADVANCE (an idle waiter walks to a loop node in view). DOOM 2016's attack tokens (enemies.md M1: https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-). Ranged accuracy falling with speed (M2) already ships (ai.aim: hit 0.7 standing -> 0.25 at 24 m/s): nothing added. The web has none of these numbers (its brain has no view rule).",
    waitS: 4,
    waitS_why: "s of thinks an enemy OUT of the sim cone that wants a token waits first (it keeps its ring and gives back an uncommitted token, so the enemies in view take the tokens); after it, it may attack from where it is (looking away is never a shield). Measured on the n52 levels (n46flow seeds 1-10, the walk pilot, bot): row off 46.5 % of attack starts inside the cone; waitS 3: 83.0 % pooled, deaths 13 / 1 / 5 / 0 / 0 (off 15 / 2 / 8 / 1 / 0); waitS 4: 84.7 % pooled (foundry 86.9, iceline 81.6, kiln 91.2, span 77.2, spire 89.5), deaths 14 / 1 / 8 / 1 / 0, inside the off run's seed-to-seed band (seeds 11-20 off: 20 / 3 / 12 / 1 / 0). 4 s is about three of a husk's attack cycles: the enemy behind you comes, but late.",
    idleS: 1,
    idleS_why: "s a waiter with no token and no attack stands (horizontal speed under idleSpeed) before it advances to a node in view (E-5: 'no enemy is idle > 3 s while the player is in reach'; the NIGHT43 still nudge only flips its strafe).",
    idleSpeed: 0.5,
    idleSpeed_why: "m/s: under this the waiter counts as standing (the n46flow IDLE column's own threshold; a husk walks 3+ m/s, the ring's hold speed is a share of that).",
    minM: 3,
    minM_why: "m: the node it walks to is at least this far (a step that reads as a move, not a shuffle).",
    arriveM: 1.5,
    arriveM_why: "m: at this distance from the node the advance is over (the ring again).",
    advanceMaxS: 4,
    advanceMaxS_why: "s: an advance ends after this whatever (a node it cannot reach does not hold it).",
  },
};

// NIGHT55's wyrm55 (G4 tonight; the lead: the row exists, G fills its numbers)
ROWS.wyrm55 = {
  night: "n55",
  numbers: {
    _why: "Builder G (G4, NIGHT56; Core/Enemies/Wyrm55.cs, SurvivalSim.N56.Wyrm.cs; measured with n47human --boss wyrm --level n52-kiln --seeds 40 --rows wyrm55, the human-model bot, Tools/night/n56-g-wyrm.sh). THE ENCOUNTER only (wick54's method): every attack keeps its tell, speed, damage, push, radius and count; the barrage's PACING (s between two barrages) is the encounter's, as wick54's rest per leg. Baseline (row off): 38/40 killed, median 122.6 s, p90 256.9 s, 68 deaths; of the hurt (9 fights), body contacts 117 of 270 hits, shots (the spit barrage) 97, the head 50. These numbers: 40/40, median 86.8 s, p90 178.3 s, 32 deaths (Logs/n56/g4-p2.txt). The web has no worm (NIGHT24).",
    poolScale: 0.6,
    poolScale_why: "x the fight's pool (after boss39 and wyrm46's 0.9): with the rest below, the median 122.6 -> 86.8 s (GG 2026-10-13: 'one to two minutes, maybe three if you're slow'); probe 1 (0.7, no pacing change) gave 99.4 s and 62 deaths",
    contactCooldown: 1.6,
    contactCooldown_why: "s between two contact hurts (contact.cooldown 0.8; contact.body / head / lunge damage unchanged): probe 1 measured body contacts 117 -> 106 per 9 fights (separate touches, not one long one): kept as the softer touch rule",
    phase3At: 0.2,
    phase3At_why: "the bar share where P3 (the squeeze) starts (hp.phase3At 0.33): a shorter P3, where the coil closes on the player",
    spitEvery2: 6.4,
    spitEvery2_why: "s between two spit barrages in P2 (spit.every2 3.2, x2; the barrage's count, speed, damage, life and aim unchanged): the barrage was 97 of 270 hurt hits; with every3 below deaths 62 -> 32. x3 (9.6 / 6.6, probe 3) gave 33 deaths and a cap (p90 195.9 s): no better",
    spitEvery3: 4.4,
    spitEvery3_why: "s between two spit barrages in P3 (spit.every3 2.2, x2), as every2",
    open: 4,
    open_why: "x what every weapon lands while staggered (wyrm47's 3): the punch window pays more (dealt in windows 70 -> 72 %)",
  },
};

// NIGHT55's kill55 (G owns it tonight: G2): a PATCH of its existing numbers (only the keys named here change; every other key and the order stay)
ROWS.kill55 = {
  night: "n55",
  patch: {
    chainS: 0.09,
    chainS_why: "s: the 3rd and later kills of a chain hold this, under the impact director's kill cap (move54's killCapS 0.12). NIGHT56 G (G2 attempt 2): 0.08 -> 0.09: n56-r2's a355 showed kills 3-6 held 4 frames = 0.067 s at 60 fps (0.08 s is 4.8 frames: the 5th frame is cut); 0.09 holds 5 whole frames (0.083 s), the critic's 'the 3rd and later at least 0.08 s on screen'. A chain at the KESTREL's live 0.483 s rate holds 0.05 / 0.065 / 0.09 / 0.09 ...: its densest real second (two kills) 0.18 s, under perSecondS 0.25.",
    chainGapS: 0.55,
    chainGapS_why: "s of PLAY time (sim seconds, freezes excluded): a kill within this of the one before continues a chain. NIGHT56 G (G2): 0.45 -> 0.55. The KESTREL's LIVE interval is 0.483 s (weapons.revolver.fireInterval 0.4 x design.n39.p39.intervalMult 1.2 = 58 steps at 120 Hz; `n56 kestrel --scheme alt`), so NIGHT55's 0.45 (set for a 0.4 s interval) never chained a KESTREL kill: n56-r1's a355 read chain 1 on every kill, its taps 0.508-0.516 s apart. 0.55 chains the KESTREL at its own rate and a tap a frame or two late (0.517 s: `--scheme tap`), and stays under a355's spaced pass (40 frames, ~0.62 s of play). a354's spacing (0.483-0.492 s) IS the KESTREL's own rate, so any gap that chains the KESTREL chains it too: a354 measures move54 alone with -skygrindOff55. ULTRAKILL counts a multikill within 0.5 s (web.md section 3.3, the wiki's Style page).",
  },
};

const files = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];
const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const before = JSON.parse(raw);
const want = process.argv.slice(2).length ? process.argv.slice(2) : Object.keys(ROWS);

function fmt(v, ind) {
  const pad = " ".repeat(ind);
  if (Array.isArray(v)) {
    if (v.every((x) => typeof x !== "object" || x === null)) return "[" + v.map((x) => JSON.stringify(x)).join(", ") + "]";
    return "[" + eol + v.map((x) => pad + "  " + fmt(x, ind + 2)).join("," + eol) + eol + pad + "]";
  }
  if (v && typeof v === "object") {
    const keys = Object.keys(v);
    if (!keys.length) return "{}";
    return "{" + eol + keys.map((k) => pad + "  " + JSON.stringify(k) + ": " + fmt(v[k], ind + 2)).join("," + eol) + eol + pad + "}";
  }
  return JSON.stringify(v);
}

let lines = raw.split(eol);
for (const row of want) {
  if (!ROWS[row]) throw new Error("not one of G's rows: " + row);
  const night = ROWS[row].night;
  if (!before.design[night] || !before.design[night][row]) throw new Error("no design." + night + "." + row);
  if (ROWS[row].patch) {
    // a patch: the row's numbers as they are, the named keys replaced in place (a key the row lacks is refused: a patch never adds one)
    const old = before.design[night][row].numbers;
    const merged = {};
    for (const k of Object.keys(old)) merged[k] = Object.prototype.hasOwnProperty.call(ROWS[row].patch, k) ? ROWS[row].patch[k] : old[k];
    for (const k of Object.keys(ROWS[row].patch)) if (!(k in old)) throw new Error(row + ": the patch names " + k + ", which its numbers lack");
    ROWS[row].numbers = merged;
  }
  if (before.design[night][row].enabled !== false) throw new Error(row + " is not enabled: false (only the lead turns a row on)");
  const hits = lines.filter((l) => l.trim() === '"' + row + '": {').length;
  if (hits !== 1) throw new Error(row + ": its row line appears " + hits + " times (want 1)");
  const open = lines.findIndex((l) => l.trim() === '"' + row + '": {');
  const ind = lines[open].match(/^ */)[0].length;
  const end = lines.findIndex((l, k) => k > open && l.match(/^ */)[0].length === ind && l.trim().startsWith("}"));
  const n = lines.findIndex((l, k) => k > open && k < end && l.match(/^ */)[0].length === ind + 2 && l.trim().startsWith('"numbers": '));
  if (n < 0) throw new Error(row + ": its numbers key not found");
  // the numbers object runs from line n to its closing brace at the same indent (or is `{}` on one line)
  let last = n;
  if (!/^"numbers": \{\},?$/.test(lines[n].trim())) {
    last = lines.findIndex((l, k) => k > n && l.match(/^ */)[0].length === ind + 2 && l.trim().startsWith("}"));
    if (last < 0) throw new Error(row + ": the numbers object's end not found");
  }
  const comma = lines[last].trim().endsWith(",") ? "," : "";
  const text = (" ".repeat(ind + 2) + '"numbers": ' + fmt(ROWS[row].numbers, ind + 2) + comma).split(eol);
  lines = lines.slice(0, n).concat(text, lines.slice(last + 1));
}
const out = lines.join(eol);
const after = JSON.parse(out);
for (const row of want) {
  const night = ROWS[row].night;
  if (JSON.stringify(after.design[night][row].numbers) !== JSON.stringify(ROWS[row].numbers)) throw new Error(row + ": written numbers differ from the script's");
  after.design[night][row].numbers = before.design[night][row].numbers;
}
const norm = (o) => JSON.stringify(o, (k, v) => (v && typeof v === "object" && !Array.isArray(v) ? Object.fromEntries(Object.entries(v).sort()) : v));
if (norm(after) !== norm(before)) throw new Error("something outside the named rows' numbers changed");
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
console.log("n56-g-rows: wrote the numbers of " + want.join(", ") + " into both copies (identical)");
