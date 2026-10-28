// NIGHT63 A1 (the lead; the shared commit, L-049 / L-065): writes design.n63 as design's last key into Design/unity-config.json and its byte
// copy Assets/StreamingAssets/Design/unity-config.json: the block's _why and the four VIEW rows the slate names (Logs/n63/slate.json:
// bossMotion63 B1 V, bossTell63 B2 G, bossColours63 B3 D, colours63 C1 D), each enabled false with empty numbers, so each builder patches
// only its own row's numbers (its own row script) and every pick merges by HEAD's copies plus that script. Then asserts nothing else changed
// (the parsed file minus design.n63 equals the old one: keys, values and order) and the two copies are byte-identical. Run once; a second run
// is refused (design.n63 already exists).
//   node Tools/night/n63-a1-rows.cjs [--dry]
// Each row is read with NightRows.ViewOn(cfg, "<row>"); -skygrindOff63 (or older) turns them all off, each builder adds its one-row switch.
const fs = require("fs");
const path = require("path");

const dry = process.argv.includes("--dry");
const root = path.join(__dirname, "..", "..");
const files = [path.join(root, "Design", "unity-config.json"), path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json")];

const offUntil = (id, what) =>
  "Off until slate " + id + " passes NIGHT63.md's rules in a Player (Logs/n63/slate.json " + id + "): " + what + "; then a skygrind-critic PASS whose SPOT line names its visible change.";

const BLOCK = {
  _why: "NIGHT63 (the boss night, NIGHT63.md; GG 2026-10-22 ~22:11: 'i would lead towards bosses, effect colores everything doenst feel alligned, boss animations are too rigid to start'): this night's design rows, all VIEW rows (no `sim`, no tape bit: the sim, the hitboxes and every fingerprint stay identical). Each is OFF until it passes NIGHT63.md section 2's visible rule in a Player (a same-run frame pair, a SPOT line naming its change, at least 41,472 px changed on its SPOT pose, and a skygrind-critic PASS). The lead's A1 writes the four rows the slate names with empty numbers; each builder fills only its own row's numbers with its own row script (L-049, L-065). The web has no such rows.",
  bossMotion63: {
    _why: "View row (builder V, NIGHT63 slate B1; GG 2026-10-22: 'boss animations are too rigid to start'; the boss views copy the sim rigidly: 0-9 spring / anticipation / flinch uses per view file, 0 in Boss37Kit.View.cs): BOSSES THAT MOVE LIKE THEY MEAN IT. One shared view-only layer (BossMotion63) over the five campaign bosses' views (THE WICK, THE ICEBREAKER, THE KILN WYRM, THE LOOM, THE CROWN): a damped second-order spring follow on each drawn part instead of a rigid copy of the sim, an anticipation (wind-up) pose before each attack driven by the sim's own attack state (BossAttackClock63, read only), follow-through and overshoot after it, a hit flinch on each damage event, a slump while staggered and idle breathing (muted during a wind-up); every view offset capped by its config limit so the drawn body never leaves its collider; no second hit-freeze (the sim has hitstop). The wyrm's segments that are hitboxes stay in the sim. Its numbers (spring f / z / r per part, the limits, the flinch and slump) are filled by builder V. Off: -skygrindOff63 (or older) or -skygrindNoBossMotion63. View only: no tape bit. The web has no such row: it has no campaign bosses.",
    enabled: false,
    enabled_why: offUntil("B1", "each part's rms offset from its rigid pose per second on > 0 on every boss and off = 0 (the boss63 reel), a wind-up starting >= 250 ms before every hit, a flinch within 2 frames of a damage event, a slump while staggered, breathing at idle; GUARD no part beyond its config limit, the hit frames and every fingerprint unchanged; the same-run SPOT pair at a flinch or the stagger slump naming the boss's pose or motion with >= 41,472 px changed; verbs37 x5 unchanged"),
    numbers: {},
  },
  bossTell63: {
    _why: "View row (builder G, NIGHT63 slate B2; GG 2026-10-12: bosses 'hard to understand what to do' and 'an understanding of parry mechanics'): BOSS ATTACKS READ IN TIME. Measured first on V's boss63 reel (every attack of the five bosses: its visible cue-to-hit lead in ms and its cue hue; the sim's own tells already run 0.55-1.5 s in config), then only the attacks whose cue lead is under 300 ms or whose cue is not in the danger hue get a danger-hue cue >= 300 ms before the damage frame; attacks already fine are untouched, and a parryable attack's parry tell is untouched whether this row is on or off. In its own file, attached by one hook line in the boss views. Off: -skygrindOff63 (or older) or -skygrindNoBossTell63. View only: no tape bit; no damage frame moves. The web has no such row: it has no campaign bosses.",
    enabled: false,
    enabled_why: offUntil("B2", "the boss63 reel lists every attack's cue lead and hue before and after; every attack under 300 ms or off the danger hue now shows a danger-hue cue >= 300 ms before its damage frame, the rest untouched; parry tells untouched on or off; GUARD no damage frame moves, fingerprints unchanged; the same-run SPOT pair inside a fixed cue (bossMotion63 off in that build) naming it with >= 41,472 px changed; verbs37 x5 unchanged"),
    numbers: {},
  },
  bossColours63: {
    _why: "View row (builder D, NIGHT63 slate B3; GG 2026-10-22: 'effect colores everything doenst feel alligned'; CLAUDE.md rule 8): BOSS COLOURS FOLLOW THE ROLE TABLE. Every boss's attack effects in the danger band (red-orange), its weak points in one colour on every boss, no boss effect in cyan (player-usable) or magenta (RIVAL); it reads C1's role table (colours63's numbers), so it is built after C1's table is picked. Off: -skygrindOff63 (or older) or -skygrindNoBossColours63. View only: no tape bit. The web has no such row: it has no campaign bosses.",
    enabled: false,
    enabled_why: offUntil("B3", "per boss frame the boss's attack effects in the danger band, its weak points one colour on every boss, no boss effect in cyan or magenta (the meter with planted cases); the same-run SPOT pair naming a colour change with >= 41,472 px changed; verbs37 x5 unchanged"),
    numbers: {},
  },
  colours63: {
    _why: "View row (builder D, NIGHT63 slate C1; GG 2026-10-19: 'The effects are inconsistent it all needs to be unified'; 2026-10-22: 'everything doenst feel alligned'; gg-proxy: lime floor stripes, a danger-red line on spire's exit, yellow edge lines): ONE COLOUR TABLE. An audit tool lists every effect colour in Assets/Scripts/Game not read from config's role colours (hex and Color literals, file:line, its role); the role table (CLAUDE.md rule 8: cyan player-usable, red-orange enemies and danger only, magenta RIVAL only, pale violet-white the boundary; yellow the parry tell only: DECIDE63 D-1) lives in this row's numbers, and the top offenders read it behind this row. Off: -skygrindOff63 (or older) or -skygrindNoColours63. View only: no tape bit. The web has no such row: its effects pick their colours one at a time.",
    enabled: false,
    enabled_why: offUntil("C1", "the audit tool lists every literal effect colour with file:line and its role, the top offenders (incl. the lime floor stripes, the danger-red line on spire's exit, the yellow edge lines) read the role table; the off-palette pixel share per level on verbs37 frames falls by >= 50 % on vs off in the same build (the meter with planted cases); the same-run SPOT pair with the biggest change naming a colour change with >= 41,472 px changed; verbs37 x5 unchanged"),
    numbers: {},
  },
};

const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const before = JSON.parse(raw);
if (before.design.n63) throw new Error("design.n63 already exists (already run?)");
const last = Object.keys(before.design).pop();
if (NightOf(last) >= 63) throw new Error("design's last key is " + last + " (expected an older night)");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const tail = eol + "    }" + eol + "  }" + eol + "}"; // the end of design's last night, of design, of the file
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
const block = JSON.stringify(BLOCK, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
const out = raw.slice(0, raw.length - tail.length) + eol + "    }," + eol + "    \"n63\": " + block + eol + "  }" + eol + "}";

const after = JSON.parse(out);
const dk = Object.keys(after.design);
if (dk[dk.length - 1] !== "n63") throw new Error("n63 is not design's last key");
if (JSON.stringify(after.design.n63) !== JSON.stringify(BLOCK)) throw new Error("design.n63 not written as the script says");
delete after.design.n63;
if (JSON.stringify(after) !== JSON.stringify(before)) throw new Error("something outside design.n63 changed (keys, values or order)");
if (dry) {
  console.log("n63-a1-rows --dry: design.n63 would be added after design." + last + " (" + out.length + " chars, was " + raw.length + ")");
  process.exit(0);
}
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
console.log("n63-a1-rows: design.n63 written after design." + last + " with " + Object.keys(BLOCK).filter((k) => k !== "_why").join(", ") + ": view rows, enabled false, numbers {} (both copies byte-identical; nothing else changed)");

function NightOf(key) {
  const m = /^n(\d+)$/.exec(key || "");
  return m ? Number(m[1]) : -1;
}
