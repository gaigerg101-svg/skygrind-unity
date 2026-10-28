// NIGHT63 builder G (text63: text61's critic points, DECIDE63 D-7): writes design.n63.text63 as design.n63's LAST key into
// Design/unity-config.json and its byte copy Assets/StreamingAssets/Design/unity-config.json (a VIEW row, enabled false, numbers.verbs: the
// technique table), then asserts nothing else changed (the parsed file minus design.n63.text63 equals the old one: keys, values and order),
// text63 is design.n63's last key, and the copies are byte-identical. Run once on a tree whose design.n63 has no text63; a second run is
// refused. On a pick onto a tree whose design.n63 gained other rows since, take HEAD's copies and run this script again (L-065).
//   node Tools/night/n63-g-text63-rows.cjs [--dry]
const fs = require("fs");
const path = require("path");

const dry = process.argv.includes("--dry");
const root = path.join(__dirname, "..", "..");
const files = [path.join(root, "Design", "unity-config.json"), path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json")];

const ROW = {
  _why: "View row (builder G, NIGHT63; text61's critic points, DECIDE63 D-7, Logs/n63/verdict-P3.md problems 3-5; GG 2026-10-14: 'way too much instruction at the tsrat of the level'): ONE LINE OF TEXT AT A TIME, EACH TECHNIQUE TAUGHT ONCE. text61 (ON) left two gaps in b3's verbs37 runs (grep '\\[text61\\]' Logs/player/n47-reel-n63-b3-v?.log): (a) after its 3 s cap an instruction showed with the room's title still up, 4 times (iceline, kiln, span, spire): with this row a new line waits up to UnityTuning.Text63WaitS (10 s) for the title to clear; (b) 'once a run' counted lines, not what they teach: kiln showed 10 lines, 7 of them the boost, several the same aim-down air shot in other words: with this row a line's withholding key is the technique it teaches (numbers.verbs: the template as brief55 words it -> its technique), so a restatement of a technique already taught this run is withheld, while the step tutorial keeps one line per step (PRINCIPLES 1; GG's G26 'smaller, easier jumps first'). A line the table does not name keeps text61's rule. Inside text61's gate (Game/Text63.cs, Text61Gate's keyed Pass, UiText61.cs); read every frame. Off: -skygrindOff63 (or older) or -skygrindNoText63 (text61 exactly as judged). View only: no tape bit. The web has no such row: it has no campaign tutorial.",
  enabled: false,
  enabled_why: "Off until it passes NIGHT63.md's rules in a Player: verbs37 x5 with the row on: no '(LATE ...' line under a title and kiln's boost restatements withheld ('[text63] withheld: ...'), the step lines still shown; the SPOT pair on an a157 n57-kiln frame where text61 alone shows a restated line naming less text; verbs37 x5 unchanged; then a skygrind-critic PASS.",
  numbers: {
    verbs: {
      "Jump {jump}, aim down, fire {fire}.": "boost",
      "Midair, aim down, then {fire}.": "boost",
      "Fire {fire} down to lift off.": "boost",
      "Press {jump} and {fire} together.": "boostHigher",
      "At the top, press {fire} again.": "boostAgain",
      "Boost: {jump}, aim down, {fire} twice.": "boostAgain",
      "Press {fire}, {jump}, then {fire} again.": "flue",
    },
    verbs_why: "n57-kiln's boost lessons as brief55's teachMap words them (design.n55.brief55.numbers.teachMap: STEP 1 jump / look / blast -> boost: the same aim-down air shot three ways; STEP 2 together -> boostHigher; STEP 3 again and THE BOOST summary -> boostAgain: the summary restates step 3's 'fire twice'; THE FLUE -> flue: it adds a jump between the shots). One technique, one line a run outside level 1; the steps stay apart, so the step tutorial still teaches each new move. b3's kiln run (Logs/player/n47-reel-n63-b3-vk.log): 10 lines -> 7 (withheld: 'Midair, aim down, then ...', 'Fire ... down to lift off.', 'Boost: ... twice.').",
  },
};

const raw = fs.readFileSync(files[0], "utf8");
if (raw !== fs.readFileSync(files[1], "utf8")) throw new Error("the two config copies differ");
const before = JSON.parse(raw);
if (!before.design || !before.design.n63) throw new Error("no design.n63 (NIGHT63's A1 first)");
if (before.design.n63.text63) throw new Error("design.n63.text63 already exists (already run?)");
const dk = Object.keys(before.design);
if (dk[dk.length - 1] !== "n63") throw new Error("design's last key is " + dk[dk.length - 1] + ", not n63");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const tail = eol + "    }" + eol + "  }" + eol + "}"; // the end of design.n63, of design, of the file
if (!raw.endsWith(tail)) throw new Error("unexpected config ending");
const block = JSON.stringify(ROW, null, 2).split("\n").map((l, i) => (i === 0 ? l : "      " + l)).join(eol);
const out = raw.slice(0, raw.length - tail.length) + "," + eol + "      \"text63\": " + block + tail;

const after = JSON.parse(out);
const nk = Object.keys(after.design.n63);
if (nk[nk.length - 1] !== "text63") throw new Error("text63 is not design.n63's last key");
if (JSON.stringify(after.design.n63.text63) !== JSON.stringify(ROW)) throw new Error("design.n63.text63 not written as the script says");
delete after.design.n63.text63;
if (JSON.stringify(after) !== JSON.stringify(before)) throw new Error("something outside design.n63.text63 changed (keys, values or order)");
if (dry) {
  console.log("n63-g-text63-rows --dry: design.n63.text63 would be added after design.n63." + Object.keys(before.design.n63).pop() + " (" + out.length + " chars, was " + raw.length + ")");
  process.exit(0);
}
for (const f of files) fs.writeFileSync(f, out);
if (fs.readFileSync(files[0], "utf8") !== fs.readFileSync(files[1], "utf8")) throw new Error("the copies differ after the write");
console.log("n63-g-text63-rows: design.n63.text63 written as design.n63's last key (a view row, enabled false, numbers.verbs; both copies byte-identical; nothing else changed)");
