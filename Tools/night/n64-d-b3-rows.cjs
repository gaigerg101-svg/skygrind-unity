// NIGHT64 builder D (slate B3; NIGHT63's view row design.n63.bossColours63, OFF): writes design.n63.bossColours63.numbers.weakAim into BOTH
// config copies (Design/unity-config.json and Assets/StreamingAssets/Design/unity-config.json, byte-identical), and nothing else: the key is
// added after `parts` (or, with --replace, its value replaced: a tuning round), every other key, value and byte of the file kept (checked by
// parsing both and comparing). The row's `enabled` is never touched (only a freeze turns it on). Run with --dry first.
//   node Tools/night/n64-d-b3-rows.cjs [--dry] [--replace]
// The numbers (CLAUDE.md rule 5: a view colour the web never had, in the row's own config block; the web has no campaign bosses):
//   weakAim.<boss id> = { aim: [r, g, b] (sRGB, brightest channel 1), scale } : with the row on, in that boss's graded fight (BossGrade54.RimBoss),
//   a weak part's routed cyan is drawn as aim x (its own brightest channel x scale) instead of RimAt's rim (WeakAim64Math.N64D.cs).
// Round 1 (the first candidate, from the red: n63-b11's head, RGB (8, 238, 157), hue 158.5; the head #0854FF x6.8 with its shell #0854FF x3.6):
//   THE KILN WYRM's grade multiplies blue by about 0.33 (linear) AFTER the tonemap's clip, so the screen's blue tops out near 157 (the red's own
//   blue) while an HDR green clips at 1 first and stays over it. RimAt's input already had hue 222; what made it green is its brightness. So the
//   aim keeps green under the clip: green / blue 0.30 (hue 222, #004DFF) at 0.8 of the site's brightness (the lit head x2.2 -> x1.76, its shell
//   x its glow). Builder D's model of URP's LDR grade in this fight (post exposure, Neutral tonemap and clip, LogC contrast, the filter, split
//   toning in gamma, saturation; scratch, not in the repo) reads the red's three measured heads at hue 86 / 102 / 151 against 88 / 94 / 158.5
//   (error -8..+7 deg, its blue 25-50 levels low), and this aim at hue 185-189 over the shell's pulse (head + shell (0, 110-125, 129-136),
//   V about 0.5: a darker cyan than the wick's, like the Loom's V 0.61, because the grade's blue cut caps blue near 157). A model, not a
//   measurement: b2's --hue reads it, and the fix round moves these two numbers by the measured gap (the model: +0.2 scale ~ -12 deg).
// Round 2 (B3's GATE FAIL on b2, Logs/n64/verdict-B3.md; run with --replace): b2 measured (n63-b3-roles.py --hue-all) the bright cube
//   (wyrm.open, #004CFF x1.76) at 192.5 and the translucent open-window box (wyrm.mark, #0048F0 x0.94 at glow 1.2) at 220.3, navy, V 0.45, most
//   of the head's area. One aim per boss cannot put both in 175-200, so weakAim gains `parts` (a part's own aim; WeakAim64Math Aim.For). The
//   model read b2's cube at 188.3 (measured 192.5) and its box at 207.5 (measured 220.3: the model is ~13-15 deg too green on the dim box). The
//   box: green / blue 0.42 at scale 1.0 (x1.18 at glow 1.2): the model reads it at 178, so ~191-193 by b2's box offset, and brighter. The cube:
//   scale 0.8 -> 0.6 (x1.32), so the brighter box over it does not push it green: the model reads head + box at 179.5 (b2's cube offset +4).
const fs = require("fs");
const path = require("path");

const dry = process.argv.includes("--dry");
const replace = process.argv.includes("--replace");
const root = path.join(__dirname, "..", "..");
const files = [path.join(root, "Design", "unity-config.json"), path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json")];

const WEAK_AIM = {
  _why: "NIGHT64 B3 (builder D; Logs/n63/verdict-B3.md: THE KILN WYRM's OPEN head read spring green, median hue 158.5, RGB (8, 238, 157), against the wick's and crown's 180). Per graded boss (its design.n54.grade54 id), the colour a weak part is DRAWN in during that boss's graded fight, chosen so that after the boss's grade the screen reads the player's cyan: aim x (the part's routed brightest channel x scale), in place of BossGrade54.RimAt (which pre-divides the grade's rim by its colour filter but cannot undo the tonemap's clip of an HDR green that the filter then leaves over blue). Read by Game/WeakAim64Math.N64D.cs, applied in Game/BossColours63.N63D.cs (PartRim, ShellRim) under RimAt's own gate (BossGrade54.RimBoss). A boss with no entry keeps RimAt. grade54's own numbers are untouched (GG's NIGHT54 design). A tuning loop measured in a Player (Tools/night/n63-b3-roles.py --hue on boss63's same-run b3k stagger pair; the slate's band 175-200). View only: no tape bit. The web has no such numbers: it has no campaign bosses.",
  wyrm: {
    aim: [0, 0.3, 1],
    aim_why: "Round 1 (NIGHT64, builder D's first candidate): #004DFF, hue 222, green / blue 0.30, so the grade's sulphur highlights (which lift green) and its blue cut leave green near blue on screen (target hue ~187, the slate's band 175-200). RimAt's own input was #0854FF (hue 222 too) at x6.8: it read 158.5 because its green clipped before the filter.",
    scale: 0.6,
    scale_why: "Round 2 (round 1: 0.8, the cube read 192.5 on b2): the drawn colour's brightest channel is 0.6 x the part's own (the lit head x2.2 -> x1.32), so green stays under the tonemap's clip once the brighter box (parts.wyrm.mark) adds over the cube. Builder D's model: +0.2 here moves the cube about -12 deg (greener).",
    parts: {
      _why: "Round 2 (Logs/n64/verdict-B3.md: on b2 the cube read 192.5 and the translucent box 220.3, navy, most of the head's area): a part's own aim (WeakAim64Math Aim.For); a weak part not listed takes the boss's aim above.",
      "wyrm.mark": {
        aim: [0, 0.42, 1],
        aim_why: "BossMarks32's open-window box over the head (a translucent shell, 1.45 x the head): green / blue 0.42 (hue 214.8), greener than the cube's 0.30 because the dim box gets almost none of the grade's sulphur lift (it read 220.3 at 0.30).",
        scale: 1.0,
        scale_why: "x1.0 of the shell's own brightness (x its glow, 1.1-1.6: x1.18 at the shot's 1.2; round 1 drew it at x0.94 and it read V 0.45), so it is brighter and its green reaches the screen: builder D's model reads it at 178, about 191-193 by b2's measured box offset.",
      },
    },
  },
};

const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const before = JSON.parse(raw);
const row = before.design && before.design.n63 && before.design.n63.bossColours63;
if (!row || typeof row.numbers !== "object" || row.numbers === null) throw new Error("no design.n63.bossColours63.numbers");
const has = Object.prototype.hasOwnProperty.call(row.numbers, "weakAim");
if (has && !replace) throw new Error("weakAim is already there (use --replace for a tuning round)");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";

function closeOf(s, open) {
  let depth = 0, inStr = false;
  for (let k = open; k < s.length; k++) {
    const ch = s[k];
    if (inStr) { if (ch === "\\") { k++; continue; } if (ch === '"') inStr = false; continue; }
    if (ch === '"') { inStr = true; continue; }
    if (ch === "{" || ch === "[") depth++;
    else if (ch === "}" || ch === "]") { depth--; if (depth === 0) return k; }
  }
  return -1;
}

const rowKey = '"bossColours63": {';
const rowAt = raw.indexOf(rowKey);
if (rowAt < 0 || raw.indexOf(rowKey, rowAt + 1) >= 0) throw new Error("design.n63.bossColours63 not found exactly once");
const numKey = '"numbers": ';
const numAt = raw.indexOf(numKey, rowAt);
const open = numAt + numKey.length;
if (numAt < 0 || raw[open] !== "{") throw new Error("bossColours63's numbers not found");
const close = closeOf(raw, open);
if (close < 0) throw new Error("numbers' closing brace not found");
const lineStart = raw.lastIndexOf("\n", numAt) + 1;
const indent = raw.slice(lineStart, numAt);
if (!/^ *$/.test(indent)) throw new Error("unexpected text before the numbers key on its line");
const inner = indent + "  ";
const block = JSON.stringify(WEAK_AIM, null, 2).split("\n").map((l, n) => (n === 0 ? l : inner + l)).join(eol);

let out;
if (!has) {
  let last = close - 1;
  while (/\s/.test(raw[last])) last--;
  out = raw.slice(0, last + 1) + "," + eol + inner + '"weakAim": ' + block + raw.slice(last + 1);
} else {
  const wKey = '"weakAim": ';
  const wAt = raw.indexOf(wKey, open);
  if (wAt < 0 || wAt > close) throw new Error("weakAim not found inside the numbers");
  const wOpen = wAt + wKey.length;
  const wClose = closeOf(raw, wOpen);
  out = raw.slice(0, wOpen) + block + raw.slice(wClose + 1);
}

const after = JSON.parse(out);
if (JSON.stringify(after.design.n63.bossColours63.numbers.weakAim) !== JSON.stringify(WEAK_AIM)) throw new Error("weakAim not written as the script says");
if (has) after.design.n63.bossColours63.numbers.weakAim = row.numbers.weakAim;
else delete after.design.n63.bossColours63.numbers.weakAim;
if (JSON.stringify(after) !== JSON.stringify(before)) throw new Error("something outside design.n63.bossColours63.numbers.weakAim changed");
if (dry) {
  console.log("n64-d-b3-rows --dry: weakAim would be " + (has ? "replaced" : "added") + " (" + out.length + " chars, was " + raw.length + ")");
  process.exit(0);
}
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
console.log("n64-d-b3-rows: design.n63.bossColours63.numbers.weakAim " + (has ? "replaced" : "added") + " (wyrm aim " + JSON.stringify(WEAK_AIM.wyrm.aim) + ", scale " + WEAK_AIM.wyrm.scale + "); bossColours63.enabled " + after.design.n63.bossColours63.enabled + "; both copies byte-identical; nothing else changed");
