// NIGHT57 builder G: writes the numbers of G's rows (design.n57.press57.numbers; patches of NIGHT55-56 rows G owns: design.n56.tokens56,
// design.n55.kill55) into both config copies (Design/unity-config.json and Assets/StreamingAssets/Design/unity-config.json, byte-identical),
// replacing only those `numbers` objects (a PATCH replaces only the keys it names, in place). Idempotent. Asserts the two copies match before
// and after, that each row's line is unique, that the row is enabled: false (only the lead turns a row on), and that the parsed result differs
// from the input only inside the named rows' `numbers`. From Tools/night/n56-g-rows.cjs (NIGHT56 G's writer).
//   node Tools/night/n57-g-rows.cjs [row ...]      (no row: every row below)
const fs = require("fs");

const ROWS = {};

ROWS.press57 = {
  night: "n57",
  numbers: {
    _why: "Builder G (G2; Core/Press57.cs, SurvivalSim.N57.Press.cs; GG 2026-10-21 '2. yes': attack behaviour on EXISTING kinds may change). STANDING STILL LOSES, on the SPITTER's existing volley (three lobbed globs and their acid; count, speed, damage and acid unchanged): at its commit the spot under its target's feet is MARKED with the telegraph module's red-orange danger circle (E-4: 'avoid this area, it hits when full'), filling for the tell (spitter.tellTime; tells36's 1 s) + the globs' flight (spitter.globTimeMin..Max: 1.5-2.3 s in all), then the volley lands ON THAT SPOT: no lead, no deliberate miss. A player who stood still is hit and stands in the acid; one who moved is not. ULTRAKILL's Virtue (research/design.md lesson 2: its insignia 'follows and surrounds the player for about 3 seconds', then fires on that spot, so 'constant horizontal movement is encouraged': https://ultrakill.wiki.gg/wiki/Virtue). Ranged aim worse with the player's speed already ships (ai.aim, AimMath.HitChance) and a marked volley never leads: nothing added for (b). Campaign runs only (as tokens56). The web has no such number (its spitter leads, or aims at your spot only while the director hunts a camper).",
    markR: 3,
    markR_why: "m: the danger circle's radius on the marked spot, the area the walk pilot's answer leaves (n46flow, Harness.N57.Press.cs). = spitter.puddleRadius (3 m): the centre glob's acid covers exactly this circle, so the circle shows where standing still hurts; the side globs land spitter.spread (1.6 m) off the centre, inside it. A smaller circle (the globs' own reach, spread + globRadius + the capsule = 2.3 m) would leave a player who answers it standing in the centre's acid.",
  },
};

// NIGHT55's kill55 (G3 tonight): a new key (an ADD: appended to its numbers once, replaced in place after; every other key and the order stay)
ROWS.kill55 = {
  night: "n55",
  add: {
    runSpeed: 4,
    runSpeed_why: "m/s of the player's horizontal speed: a chain kill at or over it asks no extra hold (the chain still counts; the impact rules' own freeze, move54's 0.05 s, stands), so a RUNNING chain keeps move54's smooth camera. NIGHT57 G (G3): NIGHT56's a354 (a run with KESTREL kills at its own 0.483 s rate, the same spacing a355's chain needs) read jitter 0.230 with the ladder on vs 0.191 off: the ladder's longer holds are the held frames that raise it. 4 = half of ground.walkSpeed (8): a player walking on purpose; a355's chain stands (0 m/s) and keeps its ladder; the flow meter's stall line is 2 m/s. The web has no such number (no ladder).",
  },
};

// NIGHT56's tokens56 (G1's target tonight: per-enemy idle): a new key (an ADD), Core/HuskBrain.N56.cs reads it (absent or 0: NIGHT56's rule)
ROWS.tokens56 = {
  night: "n56",
  add: {
    nearestIfNoneInView: 1,
    nearestIfNoneInView_why: "1: an idle waiter (no token, no attack, standing idleS) with NO loop node in view walks to the NEAREST node (>= minM) instead of staying; 0: NIGHT56's rule (it stays until a node comes into view). NIGHT57 G (G1's target, E-5 'no enemy is idle > 3 s while the player is in reach'): with tokens56 on, kiln's idle drones were mostly WAITING (28845 of 34840 idle steps, n46flow --g57) and off their 24 m wait ring (844 on it): flying at a ring spot outside the room's walls, out of view, so ADVANCE found no node in view. Measured (bot, n46flow, n52 levels, seeds 1-10, tokens56 on): enemies idle over 20 % of their time foundry 9.3 -> 8.3, iceline 20.7 -> 18.4, kiln 19.9 -> 15.8, span 9.8 -> 9.8, spire 6.9 -> 7.3 %; attack starts in the cone 85.0 -> 84.7 % pooled; deaths 13 / 7 / 8 / 0 / 0 -> 13 / 6 / 8 / 0 / 0 (Research/2026-10-21-wave57/G.md). The web has no such number (its brain has no view rule).",
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
  if (ROWS[row].add) {
    // an add: the row's numbers as they are (after a patch), the named keys replaced in place when present, appended at the end when new
    const merged = Object.assign({}, ROWS[row].numbers || before.design[night][row].numbers);
    for (const k of Object.keys(ROWS[row].add)) merged[k] = ROWS[row].add[k];
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
console.log("n57-g-rows: wrote the numbers of " + want.join(", ") + " into both copies (identical)");
