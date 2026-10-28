// NIGHT63 builder V (C2b, the lead's fix round after C2's gate FAIL, Logs/n63/verdict-C2.md section 6): rewrites ONLY two fields of builder V's
// view row design.n63.nearFade63, its `_why` and its `numbers` (the screen-door's: farM 0.9 so 1.0 m is whole, the floor, cellPx, rimNdv,
// inkGone; bossHideM kept), in Design/unity-config.json and its byte copy Assets/StreamingAssets/Design/unity-config.json, then asserts nothing
// else changed (the parsed file with the two fields put back equals the old one: keys, values and order; `enabled` and `enabled_why` untouched)
// and that the two copies are byte-identical (L-049, L-065). Text splices only (the rest of the file's bytes kept). Run once; a row already
// carrying cellPx is refused.
//   node Tools/night/n63-v-c2b-rows.cjs [--dry]
// The row is read with NearFade63.On(cfg) (NightRows.ViewOn) once a frame in EnemyLook.Frame; -skygrindOff63 (or older) and -skygrindNoNearFade63
// turn it off.
const fs = require("fs");
const path = require("path");

const dry = process.argv.includes("--dry");
const name = "nearFade63";
const root = path.join(__dirname, "..", "..");
const files = [path.join(root, "Design", "unity-config.json"), path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json")];

const WHY = "View row (builder V, NIGHT63 leftover 7, C2 'the enemy glow up close'; gg-proxy-start.md #9: 'enemies read as glow domes and flat beans', GG 2026-10-06 'effects ... still look a little cheesy' and 2026-10-16 'better-looking visors'): an enemy never fills the screen from inside. Measured red (Tools/night/n63-v-near.py on n62-freeze's verbs37): an enemy at the eye in 348 of 9000 frames, in runs up to 43 frames; vf/f0400 is 77 % enemy hue: the eye at husk 2's capsule (a screen-filling dark-red dome with an orange rim) and husk 3 behind it drawn as its x-ray silhouette. The rule: each live enemy part fades as the eye comes within numbers.farM of the part's world bounds, down to numbers.floor at numbers.nearM or nearer (inside the bounds: 0 m), so the room shows through it. C2b (Logs/n63/verdict-C2.md section 6: C2 drove the death's own 3D-cell dissolve, which the critic read as 'an exploding cloud of sheared flakes ... many of them BLACK', the ink outline's dark far side showing through every hole, and a kill's shatter on a live enemy): the fade is a SCREEN-DOOR of its own (Enemy.shader's _NearFade, never the death's _Dissolve): a 4 x 4 ordered dither in screen pixels, numbers.cellPx a cell at 1080p, so the near body is an even see-through screen; its rim stays solid (N.V under numbers.rimNdv), so you still read where it is; its ink outline is cut at the same pixel (no dark far side through a hole) and thins to nothing at numbers.inkGone. Read once a frame (EnemyLook.Frame, which sets the screen-door's globals), set per part (EnemyLook.Tint). Game layer only: the sim never reads it (no tape bit; rays and hitboxes untouched). Off: -skygrindOff63 (or older) or -skygrindNoNearFade63. The web has no such row: its enemies never pass this close in its reels.";

const NUMBERS = {
  _why: "The near fade's distances (m, from the eye to the part's world bounds: 0 inside), its level at the eye (the share of the screen-door's cells kept) and the screen-door's look. A husk is 1.8 m tall with a 0.4 m body radius; the eye is at 1.6 m; the punch reaches 2.0 m (verbs37 line 40), so the fade starts well inside that.",
  farM: 0.9,
  farM_why: "m: the part is whole at this distance from its bounds or more. C2's 1.0 m left a 1 % fade at the 1.0 m pose (63,402 px moved: the critic's 'the fade already shows at 1.0 m'); 0.9 m makes 1.0 m exactly whole, and the screen-door keeps every pixel down to a level of 31 / 32 (0.83 m from the bounds with these numbers).",
  nearM: 0.3,
  nearM_why: "m: the part is at its floor at this distance or nearer, and inside its bounds (vf/f0400's eye was about 0.45 m from husk 2's bounds).",
  floor: 0.2,
  floor_why: "The level at the eye (1 whole, 0 gone): 3 of each 16 screen-door cells stay (the thresholds 1 / 32, 3 / 32 and 5 / 32 are under 0.2), plus the solid rim: a sparse, even screen with the enemy's edge, never a wall of colour and never a vanished threat. C2's 0.1 of the death's 3D cells read as noise.",
  cellPx: 2,
  cellPx_why: "px at 1080p (scaled to the screen's height): the screen-door's cell. 2 px makes an 8 px ordered tile, small and even enough to read as a see-through surface, coarse enough to read as deliberate rather than as the death's flakes.",
  rimNdv: 0.25,
  rimNdv_why: "N.V (the surface's facing to the eye): a pixel whose N.V is under this is never cut, so the body's silhouette edge stays solid as a thin rim (the band where the surface turns away: about the outer 3 % of a sphere's radius) and you read where the enemy is.",
  inkGone: 0.45,
  inkGone_why: "The level the ink outline (NIGHT51 ink51, an inverted hull) is gone at: it thins from full at level 1 to nothing here (0.52 m from the bounds with these numbers), so the near body has no dark outline, and from inside the hull (the eye in the body) nothing dark draws. Over it the ink is cut at the same pixels as the body.",
  bossHideM: 0.6,
  bossHideM_why: "m: a boss part drawn with a material that has no dissolve (THE ICEBREAKER's Stone / URP Lit boxes) stops drawing while the eye is within this of its world bounds, and draws again once the eye is clear (NearFade63.HideNear). The lead's real-play bug: on b1-b4 boss63's iceline flinch / stagger frames were black but for the HUD (its leap lands on the player; its stuck pose leans the dark violet torso 50 deg toward the eye: the torso's centre about 0.9 m and the hump's about 0.46 m from the eye). 0.6 m hides the part the eye is in or against and keeps the rest of the body (the legs, the head, the stuck fist) and the room in view. Unchanged by C2b.",
};

/** The end (exclusive) of the JSON value starting at `open` in `raw`: an object (brace-matched, strings skipped) or a string. */
function valueEnd(raw, open) {
  if (raw[open] === '"') {
    for (let k = open + 1; k < raw.length; k++) {
      if (raw[k] === "\\") { k++; continue; }
      if (raw[k] === '"') return k + 1;
    }
    throw new Error("unterminated string at " + open);
  }
  if (raw[open] !== "{") throw new Error("not an object or a string at " + open);
  let depth = 0, inStr = false;
  for (let k = open; k < raw.length; k++) {
    const ch = raw[k];
    if (inStr) { if (ch === "\\") { k++; continue; } if (ch === '"') inStr = false; continue; }
    if (ch === '"') { inStr = true; continue; }
    if (ch === "{") depth++;
    else if (ch === "}") { depth--; if (depth === 0) return k + 1; }
  }
  throw new Error("unbalanced object at " + open);
}

const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const before = JSON.parse(raw);
const row0 = before.design.n63 && before.design.n63[name];
if (!row0) throw new Error("no design.n63." + name);
if (row0.numbers && row0.numbers.cellPx !== undefined) throw new Error("design.n63." + name + ".numbers already has cellPx (already run?)");
const rowAt = raw.indexOf('"' + name + '": {');
if (rowAt < 0 || raw.indexOf('"' + name + '": {', rowAt + 1) >= 0) throw new Error("design.n63." + name + " not found exactly once");
const rowEnd = valueEnd(raw, rowAt + name.length + 4);
const eol = raw.includes("\r\n") ? "\r\n" : "\n";

let out = raw;
// the numbers first (later in the row), then the _why (earlier), so the first splice never moves the second's offsets
const numKey = '"numbers": ';
const numAt = out.indexOf(numKey, rowAt);
if (numAt < 0 || numAt > rowEnd) throw new Error("its numbers not found in the row");
const numOpen = numAt + numKey.length;
const numEnd = valueEnd(out, numOpen);
const lineStart = out.lastIndexOf("\n", numAt) + 1;
const indent = out.slice(lineStart, numAt);
if (!/^ *$/.test(indent)) throw new Error("unexpected text before the numbers key on its line");
const blk = JSON.stringify(NUMBERS, null, 2).split("\n").map((l, n) => (n === 0 ? l : indent + l)).join(eol);
out = out.slice(0, numOpen) + blk + out.slice(numEnd);
const whyKey = '"_why": ';
const whyAt = out.indexOf(whyKey, rowAt);
if (whyAt < 0 || whyAt > numAt) throw new Error("the row's _why not found before its numbers");
const whyOpen = whyAt + whyKey.length;
out = out.slice(0, whyOpen) + JSON.stringify(WHY) + out.slice(valueEnd(out, whyOpen));

const after = JSON.parse(out);
const row1 = after.design.n63[name];
if (row1._why !== WHY || JSON.stringify(row1.numbers) !== JSON.stringify(NUMBERS)) throw new Error("the fields not written as the script says");
if (JSON.stringify(Object.keys(row1)) !== JSON.stringify(Object.keys(row0))) throw new Error("the row's keys or their order changed");
row1._why = row0._why;
row1.numbers = row0.numbers;
if (JSON.stringify(after) !== JSON.stringify(before)) throw new Error("something outside design.n63." + name + "._why / .numbers changed");
if (dry) {
  console.log("n63-v-c2b-rows --dry: design.n63." + name + "._why and .numbers would be written (" + out.length + " chars, was " + raw.length + ")");
  process.exit(0);
}
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
console.log("n63-v-c2b-rows: design.n63." + name + "._why and .numbers written (" + Object.keys(NUMBERS).filter((k) => !k.endsWith("_why")).map((k) => k + " " + NUMBERS[k]).join(", ") + "); enabled " + after.design.n63[name].enabled + "; both copies byte-identical; nothing else changed");
