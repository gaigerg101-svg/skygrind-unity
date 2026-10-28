// NIGHT40 builder S1: patches ONLY S1's own rows of design.n40 (start40, kiln40, cut40, rods40) in Design/unity-config.json, then copies
// the file to Assets/StreamingAssets/Design/unity-config.json (the two stay byte-identical). Each named row's text is replaced in place (the
// rest of the file keeps its bytes); the script checks that every other key of the config is unchanged after the patch (L-049).
// Run from the worktree root: node Tools/night/s1-n40-rows.cjs start40 [kiln40 ...]
const fs = require("fs");
const path = require("path");
const root = process.cwd();
const file = path.join(root, "Design", "unity-config.json");
const copy = path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json");

const off = "Off until the lead has looked at its Player frames once and iterated once (NIGHT40.md method 2).";

// The rows' new contents (keys in this order). A row keeps its shared-commit keys and adds S1's numbers with a _why each.
const rows = {
  start40: (old) => ({
    _why: old._why,
    enabled: old.enabled,
    enabled_why: old.enabled_why,
    moveMetres: 6,
    moveMetres_why: "S1: the hold ends when the player's feet are this far (3D) from where they stood when the wave was held (the level's spawn, or the respawn point). A hypothesis (NIGHT40.md S1; enemies-web.md section 5: Doom Eternal locks an arena when the player crosses in; no published grace number was found): about two strides of a sprint, so any deliberate move starts the fight.",
    holdSeconds: 4,
    holdSeconds_why: "S1: the hold ends this many seconds after it began even if the player stands still (never a soft-lock). A hypothesis (NIGHT40.md S1): time to read the room's first line and look around.",
    noAttackSeconds: 1.0,
    noAttackSeconds_why: "S1: the held wave's enemies cannot start an attack for this long after their spawn tell ends (design.n33.spawns33.tellSeconds when spawns33 is on); they move. A husk-family body (husk, tank, spitter, swarmer) gets it as its attack cooldown; any other kind is stunned for it. A hypothesis (NIGHT40.md S1).",
    afterRespawn: true,
    afterRespawn_why: "S1: a death's retry holds the room's first wave the same way (GG: the first wave comes too fast; a retry starts the room over).",
  }),
  kiln40: (old) => ({
    _why: old._why,
    enabled: old.enabled,
    enabled_why: old.enabled_why,
    levelFrom: old.levelFrom,
    levelTo: old.levelTo,
    startSet: old.startSet,
    startSet_why: old.startSet_why,
    awards: [{ room: "kb2b", weapon: "revolver" }, { room: "kb3", weapon: "gauntlet" }],
    awards_why: "S1 (DECIDE40 D-2: 'the pistol and gauntlet' unlocked later in the level): the first half (3.1 THE CHARGE, 3.1b THE FLUE, 3.2 POUR HALL) is the THUNDERCLAP's alone; the KESTREL comes on entering 3.2b THE STACK (before 3.3's spitters on the west gantry, a ranged job), the PULSE gauntlet on entering 3.3 CASTING FLOOR (the room whose verb is 'launch into the pour': the blast throws them in). Weapon ids, never display names.",
    chain: [
      { id: "jump", text: "BOOST 1/5 · PRESS {jump} TO JUMP", done: "jump" },
      { id: "look", text: "BOOST 2/5 · IN THE AIR, LOOK STRAIGHT DOWN", done: "lookDown" },
      { id: "blast", text: "BOOST 3/5 · PRESS {fire}: THE BLAST LIFTS YOU", done: "boost" },
      { id: "again", text: "BOOST 4/5 · AT THE TOP, PRESS {fire} AGAIN", done: "boostAgain" },
      { id: "double", text: "BOOST 5/5 · PRESS {jump} IN THE AIR: THE LAST METRES", done: "airJump" },
      { id: "summary", text: "THE BOOST: JUMP, LOOK DOWN, FIRE, FIRE, JUMP", done: "seen" },
      { id: "slug", text: "HOLD {fire} ON THE GROUND: CHARGE A SLUG", done: "seen" },
    ],
    chain_why: "S1 (NIGHT40.md S1, PRINCIPLES 2: ULTRAKILL's HudMessage, one step at a time, each shown when the last is done): the THUNDERCLAP CLIMB (K4's level: jump, shoot down, shoot again, double jump; envelopes38 'jump+double+boost x2' 13.36 m) as five numbered lines of at most 52 characters resolved (teach35.maxChars; BOOST n/5: the player sees it is a sequence), a one-time summary card (GG: 'more thorough'), then one on the charged slug (W1's charge40 rule: on the ground a hold charges). Round 3 (the lead's n40-r2 frames: 'JUMP SPACE.' read badly): one form for every line, all capitals, the bound button inside a verb phrase ('PRESS {jump} TO JUMP'). Words, not numbers: {jump} / {fire} resolve to the bound buttons (UiTeach35). done: jump | lookDown | boost | boostAgain | airJump | seen (SurvivalSim.N40.Kiln.cs).",
    lookDownDeg: 60,
    lookDownDeg_why: "S1: 'LOOK STRAIGHT DOWN' is done at a look pitch this far below level (a hypothesis: past 60 deg the blast's push is mostly up; the boost below accepts 45).",
    boostPitchDeg: 45,
    boostPitchDeg_why: "S1: a THUNDERCLAP shot in the air counts as a boost when the look pitch is at least this far below level (a hypothesis: Sandbox19 pushes along -aim, so at 45 deg the shot still lifts the player by 0.71 x jumpSpeed).",
    lineFallbackSeconds: 20,
    lineFallbackSeconds_why: "S1 (NIGHT40.md S1: 'each line held until done (a 20 s fallback)'): a step not done in this time moves the chain on, so the text never sticks.",
    minLineSeconds: 1.2,
    minLineSeconds_why: "S1: a chain line stands at least this long even when its step is already done (a fast climb does the shot, the double and the second shot inside a second; each line still reads). A hypothesis: about two reading fixations of a six-word line.",
    seenSeconds: 6,
    seenSeconds_why: "S1: the slug's line has nothing to do; it stands this long (teach35's showS is 5 s; one more for a longer line).",
  }),
  cut40: (old) => ({
    _why: old._why,
    enabled: old.enabled,
    enabled_why: old.enabled_why,
    cut: ["n35-finale", "n38-finale"],
    cut_why: "S1: level 6 under either id (n35-finale today; n38-finale while design.n38.finale38 swaps it in), so the cut holds whichever row is on. The files stay in the tree until GG's yes (D-4); --off40 still loads them. The level select's six slots (the sixth LOCKED, unnamed) and 5 playable are UnityTuning.Cut40.cs (view numbers).",
  }),
};

const raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ before the patch");
const before = JSON.parse(raw);
const names = process.argv.slice(2);
if (names.length === 0) throw new Error("name the rows to patch: " + Object.keys(rows).join(", "));
let text = raw;
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const n40At = text.indexOf('\n    "n40": {');
if (n40At < 0) throw new Error("design.n40 not found");
for (const name of names) {
  if (!rows[name]) throw new Error("not S1's row: " + name);
  const open = '\n      "' + name + '": {';
  const at = text.indexOf(open, n40At);
  if (at < 0) throw new Error("row not found: " + name);
  const close = text.indexOf("\n      }", at + open.length);
  if (close < 0) throw new Error("row end not found: " + name);
  const old = before.design.n40[name];
  const next = rows[name](old);
  const body = JSON.stringify(next, null, 2).split("\n").map((l, i) => (i === 0 ? l : "      " + l)).join(eol);
  text = text.slice(0, at) + '\n      "' + name + '": ' + body + text.slice(close + "\n      }".length);
}
const after = JSON.parse(text);
for (const name of names) {
  before.design.n40[name] = after.design.n40[name];
}
if (JSON.stringify(before) !== JSON.stringify(after)) throw new Error("a key outside S1's rows changed: refused");
fs.writeFileSync(file, text);
fs.copyFileSync(file, copy);
console.log("patched design.n40." + names.join(", design.n40.") + "; copies identical: " + (fs.readFileSync(file, "utf8") === fs.readFileSync(copy, "utf8")));
