// NIGHT64 A1 (the lead; the shared commit, L-049 / L-065; modelled on Tools/night/n63-a1-rows.cjs): writes design.n64 as design's last key into
// Design/unity-config.json and its byte copy Assets/StreamingAssets/Design/unity-config.json: the block's _why and the three VIEW rows the slate
// and NIGHT64.md section 3 name (Logs/n64/slate.json: goal64 C4 D, order64 U1 G; fightLine64 leftover 2 G), each enabled false with empty
// numbers, so each builder patches only its own row's numbers (its own row script) and every pick merges by HEAD's copies plus that script.
// Then asserts nothing else changed (the parsed file minus design.n64 equals the old one: keys, values and order) and the two copies are
// byte-identical. Run once; a second run is refused (design.n64 already exists).
//   node Tools/night/n64-a1-rows.cjs [--dry]
// Each row is read with NightRows.ViewOn(cfg, "<row>") every frame (A1 lists the three in SameRun63Math.FrameRows); -skygrindOff64 (or older)
// turns them all off, each builder adds its one-row switch (NightRows.HasSwitch).
const fs = require("fs");
const path = require("path");

const dry = process.argv.includes("--dry");
const root = path.join(__dirname, "..", "..");
const files = [path.join(root, "Design", "unity-config.json"), path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json")];

const offUntil = (id, what) =>
  "Off until slate " + id + " passes NIGHT64.md's rules in a Player (Logs/n64/slate.json " + id + "): " + what + "; then a skygrind-critic PASS whose SPOT line names its visible change, flipped by Tools/night/n64-freeze-rows.cjs (the PASS's sha checked).";

const BLOCK = {
  _why: "NIGHT64 (the aligned night, NIGHT64.md; GG 2026-10-22 ~22:11: 'i would lead towards bosses, effect colores everything doenst feel alligned, boss animations are too rigid to start'; 2026-10-23: 'follow best methods and create next prompt that will go for 3 hours'): this night's design rows, all VIEW rows (no `sim`, no tape bit: the sim, the hitboxes and every fingerprint stay identical). Each is OFF until it passes NIGHT64.md section 2's visible rule in a Player (a same-run frame pair, a SPOT line naming its change and a skygrind-critic PASS bound to the sha it judged). The lead's A1 writes the three rows with empty numbers; each builder fills only its own row's numbers with its own row script (L-049, L-065). The web has no such rows.",
  goal64: {
    _why: "View row (builder D, NIGHT64 slate C4; gg-proxy on NIGHT63's shipped frames, Logs/n63/gg-proxy.md #4: 'a red-orange vertical line drops from a ceiling lamp down the cyan door' on 5 of 5 spire frames, named since NIGHT60; CLAUDE.md rule 8: red-orange is enemies and danger only): THE EXIT THAT READS DANGER. THE CROWN's goal column (Route47View.cs:113-135, its through-wall pass Column47.shader:98-106) stands at the centre of every spire door in the red-gold 0xff8a3a; with this row on it draws in the colour table's goal role, read with Colours63Math.From(cfg) every frame, never typed; the window shafts over the doors keep their red-gold (GG's call, DECIDE64). Off: -skygrindOff64 (or older) or -skygrindNoGoal64. View only: no tape bit. The web has no such row: its goal column was drawn in one colour.",
    enabled: false,
    enabled_why: offUntil("C4", "on the GATE build's same-run pairs (a157 n52-spire clear shots) the column's danger-band pixels inside every door box are 0 while the column is still drawn (its pixels in the boundary band > 0); the window shafts keep their red-gold; the SPOT pair at a spire exit names the line's colour change; verbs37 n52-spire x1 31 / 31, verbs37 x5 on rc1 unchanged, fingerprints EQUAL"),
    numbers: {},
  },
  order64: {
    _why: "View row (builder G, NIGHT64 slate U1; gg-proxy #3: 4 of 18 boss shots print the same order twice at once, small under the bar and large in the centre; GG 2026-10-14: 'way too much instruction'): ONE ORDER AT A TIME. While hud47's fight line holds an order, the boss bar's hint (UiBossBar43.cs:209-215, b43Hint) stays hidden, and shows it after the line fades; written after UiBossRule61's every-frame write (UiBossRule61.cs:120-150) so the rule line's release shows no one-frame duplicate. Off: -skygrindOff64 (or older) or -skygrindNoOrder64. View only: no tape bit; the HUD's look untouched (GG 2026-10-20: 'the UI looks good as it is'). The web has no such row: it has no campaign bosses.",
    enabled: false,
    enabled_why: offUntil("U1", "its counter prints per boss, in one same-run run, would-duplicate frames and duplicate frames shown; row on, on THE KILN WYRM and THE LOOM: duplicate frames shown 0 while would-duplicate > 0, and the count of orders shown the same on and off; bossRule61's line still holds the bar's hint (N61BossRule and N63BossRule tests GREEN); the SPOT pair at THE KILN WYRM's stagger names the duplicate gone; verbs37 n57-kiln x1 31 / 31, verbs37 x5 on rc1 unchanged, fingerprints EQUAL"),
    numbers: {},
  },
  fightLine64: {
    _why: "View row (builder G, NIGHT64.md section 3 leftover 2; gg-proxy on NIGHT63's shipped boss frames: the room's name, 'THE STACK' on bf/wick-windup and 'THE SPIRE' on bp/crown37-idle, stands as the fight line mid-fight where the boss's order or nothing belongs): THE FIGHT LINE IS THE FIGHT'S. While a boss fight runs, a room title does not take hud47's fight line; it shows before the fight or after it. Off: -skygrindOff64 (or older) or -skygrindNoFightLine64. View only: no tape bit; the HUD's look untouched. The web has no such row: it has no campaign bosses.",
    enabled: false,
    enabled_why: offUntil("L2 (leftover 2)", "on the boss63 reel's same-run pairs the fight line mid-fight holds no room title with the row on (a counter printing room-title frames during a fight, on and off), every room title still shown once outside the fight; the SPOT pair names the title gone; verbs37 x1 on the level it touches, fingerprints EQUAL"),
    numbers: {},
  },
};

const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const before = JSON.parse(raw);
if (before.design.n64) throw new Error("design.n64 already exists (already run?)");
const last = Object.keys(before.design).pop();
if (NightOf(last) >= 64) throw new Error("design's last key is " + last + " (expected an older night)");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const tail = eol + "    }" + eol + "  }" + eol + "}"; // the end of design's last night, of design, of the file
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
const block = JSON.stringify(BLOCK, null, 2).split("\n").map((l, i) => (i === 0 ? l : "    " + l)).join(eol);
const out = raw.slice(0, raw.length - tail.length) + eol + "    }," + eol + "    \"n64\": " + block + eol + "  }" + eol + "}";

const after = JSON.parse(out);
const dk = Object.keys(after.design);
if (dk[dk.length - 1] !== "n64") throw new Error("n64 is not design's last key");
if (JSON.stringify(after.design.n64) !== JSON.stringify(BLOCK)) throw new Error("design.n64 not written as the script says");
delete after.design.n64;
if (JSON.stringify(after) !== JSON.stringify(before)) throw new Error("something outside design.n64 changed (keys, values or order)");
if (dry) {
  console.log("n64-a1-rows --dry: design.n64 would be added after design." + last + " (" + out.length + " chars, was " + raw.length + ")");
  process.exit(0);
}
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
console.log("n64-a1-rows: design.n64 written after design." + last + " with " + Object.keys(BLOCK).filter((k) => k !== "_why").join(", ") + ": view rows, enabled false, numbers {} (both copies byte-identical; nothing else changed)");

function NightOf(key) {
  const m = /^n(\d+)$/.exec(key || "");
  return m ? Number(m[1]) : -1;
}
