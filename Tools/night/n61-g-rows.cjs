// NIGHT61 builder G: writes ONLY builder G's own rows (each a VIEW row: no `sim`, no tape bit) as the last keys of the existing design.n61 block
// (the lead's A1 commit wrote it with a _why only; other builders' rows may already follow it) into Design/unity-config.json and its byte copy
// Assets/StreamingAssets/Design/unity-config.json, then asserts nothing else changed (the parsed file minus the new row equals the old one,
// keys, values and order) and the two copies are byte-identical (L-049). One row per run, named on the command line; a row already there is
// refused (run it once per row).
//   node Tools/night/n61-g-rows.cjs bossRule61 [--dry]   S3: every campaign boss says its rule when its fight starts (Game/BossRule61.cs)
//   node Tools/night/n61-g-rows.cjs text61 [--dry]       C1: one line of text at a time, no repeated instruction outside level 1 (Game/Text61.cs)
// Each row is read with NightRows.ViewOn(cfg, "<row>"); -skygrindOff61 (or older) and its one-row switch turn it off.
const fs = require("fs");
const path = require("path");

const dry = process.argv.includes("--dry");
const name = process.argv.slice(2).find((a) => !a.startsWith("--"));
const root = path.join(__dirname, "..", "..");
const files = [path.join(root, "Design", "unity-config.json"), path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json")];

const ROWS = {
  bossRule61: {
    _why: "View row (builder G, NIGHT61 S3; GG 2026-10-12: bosses 'you just teleport into these random rooms ... hard to understand what to do'; the critics since NIGHT55: 'no boss says what it wants'; docs/PRINCIPLES.md 2: 'A boss is the level's final exam of its verb'): when one of the five campaign bosses rises in a campaign run (its bossRise: wick, brawl40, wyrm, loom37, crown37), its rule line (numbers.lines.<boss id>: what its code rewards, at most 8 words, the bound button's glyph for the verb it names) comes in under its name card on hud47's one fight line (40 px em at 1080p) and holds that line for at most 5 s, once per fight start; the fight line's queue waits meanwhile, as it waits for the card (Game/BossRule61.cs, Game/UiBossRule61.cs; the Unity-only timing in UnityTuning.N61G.cs). Proof: the Player log's '[boss61] line: <boss>'. Game layer only: the sim never reads it (no tape bit). Off: -skygrindOff61 (or older) or -skygrindNoBossRule61. The web has no such row: it has no campaign bosses.",
    enabled: false,
    enabled_why: "Off until it passes NIGHT61.md section 2's visible rule in a Player (Logs/n61/slate.json S3): a reel frame per boss 1.0 s into its fight shows its line, lines bad 0 of 5 (at most 8 words, the glyph drawn, em >= 36 px at 1080p and >= 24 px at 720p, on screen <= 5 s, once per fight start); the SPOT on THE KILN WYRM's fight-start pair names the text; verbs37 x5 unchanged; then a skygrind-critic PASS.",
    numbers: {
      _why: "The rule lines, one per campaign boss keyed by its bossRise id, each written from that boss's damage rule as the code runs it with today's ON rows (each line's _why quotes the rule with file:line; Research/2026-10-22-wave61/G.md has the full reading). Text only: the sim never reads it. {action} tokens become the player's live buttons (the lesson widget's resolver, UiLesson25.cs). At most 8 words each, a token counting as one (BossRule61.Words; N61BossRuleRowTests and N61BossRuleTests pin it).",
      lines: {
        wick: "Parry {punch} the yellow embers, then shoot.",
        wick_why: "THE WICK (Wick37.cs with wick47 / wick54 / wick55 ON): a parried ember lands in full and cracks a plate (Wick37.cs:292, weapon parry31; TryParry Wick37.cs:395-431, brain.Parried(parryStagger)); wick55's stagger meter is 1 a leg (design.n55.wick55.numbers.stagger), so one parry opens the WINDOW (windowSeconds 4) in which every hit lands windowGain 2 x full (Wick37.cs:293 and Wick37.N55.cs:66-74); outside it a hit chips 0.3 (Wick37.N47.cs:186-187, wick55's chip). The parry is the punch button: the sim parries on the 'punch' MeleeEvent (SurvivalSim.Wick37.cs:43 and 188-194). 7 words.",
        brawl40: "Get close and swing the gauntlet {fire}.",
        brawl40_why: "THE ICEBREAKER (Brawl40.cs with brawl47 ON): a gun lands x1 within 4 m of its body and fades to x0.2 at 12 m (Brawl40.cs:7-8 and 395-400, design.n40.brawl40.range); the gauntlet's swing and the punch land x1 at any range (Brawl40.cs:410) and drain its STAGGER meter, the swing 3, the punch 1 (Brawl40.cs:443, design.n40.brawl40.stagger); the meter empty opens the WINDOW and breaks the ice armour it starts the fight in (Brawl40.cs:508-522; brawl47 armorP1 1, every hit x armorChip 0.25 until then: Brawl40.cs:413). The PULSE GAUNTLET is level 2's weapon; held, it swings on the fire button. 7 words.",
        wyrm: "Swing the gauntlet {fire} at its charging head.",
        wyrm_why: "THE KILN WYRM (Wyrm.cs with wyrm30 / wyrm31 / bosses32 / wyrm46 / wyrm47 / wyrm55 ON): a GAUNTLET hit (weapon 'gauntlet': the PULSE GAUNTLET's swing, the fire button with it in hand) on the head in the last 0.4 s of a charge at the player staggers it (SurvivalSim.Boss30.cs:282-310 and 337, design.n30.wyrm30.windowSeconds); staggered, every weapon lands open x the part (Wyrm31.cs:99-110; wyrm47 sets the window 10 s and open 3: SurvivalSim.N47.Wyrm.cs:131-155), else a gun lands gunScale x the part; the head is x2 (Wyrm.cs:1334). The F punch does NOT stagger it: it reports weapon 'punch' (Combat.cs:1406) and wyrmTell48, the punch's stagger, is OFF (SurvivalSim.N48.Brawl.cs:122-147); WyrmPilot31.cs:29-40 swings the gauntlet. So wyrm47's line '{punch} its head as it comes.' names the wrong button. 8 words.",
        loom37: "Hook a lit knee {grapple}, then grapple away.",
        loom37_why: "THE LOOM (Loom37.cs with loom40 / loom47 ON): the grapple's rope on a lit knee of the leg's pair LATCHES it (Loom37.N40.cs:155-176, Latch40); then, with a rope out, the player tearDistM (7 m) farther from it than his nearest since the latch TEARS it (Loom37.cs:526-536); a tear staggers it (Loom37.cs:406-414) and the torn knees open the CYAN window in which every hit lands in full, else hits chip loom47's 0.3 (Loom37.cs:345-365). A walk away does not tear: a rope must be out. 8 words.",
        crown37: "Jump {jump} and dash {dash} through the ring.",
        crown37_why: "THE CROWN (Crown37.cs with crown40 / crown47 ON): THE PASS: airborne at passSpeed or more (a jump-dash; a slam down through it counts too), the chest crossing the ring's plane within passRadius drains its STAGGER meter (Crown37.cs:430-443) and, with the TORCH burning, breaks a gem (crown47 torchPassGems 1); the meter empty opens the CYAN window in which every hit lands in full (Crown37.cs:270), else a gun chips 0.3 (Crown37.cs:271, crown47). 8 words.",
      },
    },
  },
  text61: {
    _why: "View row (builder G, NIGHT61 C1; gg-proxy 2026-10-22, Logs/n61/gg-proxy-start.md item 3; GG 2026-10-14: 'way too much instruction at the tsrat of the level' (OWNER.md:110); GG 2026-10-11: 'the first level will have text overlay explaining mechanics' (OWNER.md:101)): one line of text at a time, and no instruction repeated outside level 1. Outside the campaign's first level and outside a boss fight, the centre card's instruction (the teach card, UiTeach43) waits while hud33's message slot shows a room's title (at most UnityTuning.Text61WaitS, then it shows anyway) and shows at most once per run (a line already shown is withheld when it comes again). Everywhere, the previous room's ROOM CLEAR card (clear55) goes when the next room is entered. Level 1 keeps its overlay; a boss fight is hud47's fight line (the boss's rule line, bossRule61, is the one line a boss fight allows at its start: this row never touches it). The HUD's look is unchanged (GG 2026-10-20: 'the UI looks good as it is, so don't touch that'): only when a line shows. Game/Text61.cs, Game/UiText61.cs; proof: the Player log's '[text61] shown: <line>'. Game layer only: the sim never reads it (no tape bit). Off: -skygrindOff61 (or older) or -skygrindNoText61. The web has no such row: it has no campaign text.",
    enabled: false,
    enabled_why: "Off until it passes NIGHT61.md section 2's visible rule in a Player (Logs/n61/slate.json C1): on a157's n57-kiln entry frames (5 rooms) on the build that ships, frames with two or more centre text blocks <= 1 of 5 and the previous room's ROOM CLEAR 0 of 5; outside level 1 an instruction shows at most once per verb per run ('[text61] shown:' once per line, repeats 0); level 1's a357 entry frames keep their instruction; the HUD's look unchanged; the SPOT on the kb2c entry pair names less text or the room; verbs37 x5 unchanged; then a skygrind-critic PASS.",
    numbers: {},
  },
};

if (!name || !ROWS[name]) throw new Error("usage: node Tools/night/n61-g-rows.cjs <" + Object.keys(ROWS).join(" | ") + "> [--dry]");
const ROW = ROWS[name];
const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const before = JSON.parse(raw);
if (!before.design.n61) throw new Error("no design.n61 (the lead's A1 commit writes the block first)");
if (before.design.n61[name]) throw new Error("design.n61." + name + " already exists (already run?)");
if (Object.keys(before.design).pop() !== "n61") throw new Error("design.n61 is not design's last key");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const tail = eol + "    }" + eol + "  }" + eol + "}"; // the end of design.n61, of design, of the file
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
const block = JSON.stringify(ROW, null, 2).split("\n").map((l, i) => (i === 0 ? l : "      " + l)).join(eol);
const out = raw.slice(0, raw.length - tail.length) + "," + eol + "      \"" + name + "\": " + block + tail;

const after = JSON.parse(out);
const keys = Object.keys(after.design.n61);
if (keys[keys.length - 1] !== name) throw new Error(name + " is not design.n61's last key");
if (JSON.stringify(after.design.n61[name]) !== JSON.stringify(ROW)) throw new Error("design.n61." + name + " not written as the script says");
delete after.design.n61[name];
if (JSON.stringify(after) !== JSON.stringify(before)) throw new Error("something outside design.n61." + name + " changed (keys, values or order)");
if (dry) {
  console.log("n61-g-rows --dry: design.n61." + name + " would be added after design.n61." + keys[keys.length - 2] + " (" + out.length + " chars, was " + raw.length + ")");
  process.exit(0);
}
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
console.log("n61-g-rows: design.n61." + name + " written after design.n61." + keys[keys.length - 2] + ": a view row, enabled false (both copies byte-identical; nothing else changed)");
