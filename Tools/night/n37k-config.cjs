// NIGHT37 builder K: writes design.n37.boss37 (the Boss37 brain's shared numbers, Core/Boss37.cs Boss37Params) into Design/unity-config.json
// (this repo or worktree: the path is relative to this script). L-049: only the text of the row's object inside `"n37": {` is replaced
// (brace matched, strings skipped); every other byte of the file is kept. Re-runnable. `enabled` is kept as the file has it (the lead's).
// A boss's own row may override any key under design.n37.<row>.kit (Boss37Params.From(cfg, "<row>")).
// Run: node Tools/night/n37k-config.cjs   then: cp Design/unity-config.json Assets/StreamingAssets/Design/unity-config.json && cmp both
const fs = require("fs");
const path = require("path");
const file = path.join(__dirname, "..", "..", "Design", "unity-config.json");
let raw = fs.readFileSync(file, "utf8");
const eol = raw.includes("\r\n") ? "\r\n" : "\n";

const rows = {};

rows.boss37 = {
  _why:
    "Builder K (NIGHT37.md K1; BOSSES.md shared rules 1-8): the Boss37 brain the four bosses share (Core/Boss37.cs): legs with checkpoints, the stagger meter + the CYAN window (ULTRAKILL Minos Prime's stamina), the attack budget with a forced rest and the parry refund (MinosPrime.cs:342-353, :1125), bursts then a rest (Gabriel.cs:132-142), the weighted pick with pity (Gabriel.cs:74-128), the habit reader (Minotaur.cs:294-296), the camp clock (Boss36), the anti-hide timer (GabrielBase.cs:355-363); research/uk-bosses.md. A boss's row overrides any key under its own `kit` object. Harness: `boss37 kit` prints each rule alone. Off: no Boss37 boss is built (the n36 bosses stay).",
  enabled: false,
  restSeconds: 1.5,
  restSeconds_why:
    "Seconds of rest after a burst, after the CYAN window and after a leg change: between Gabriel's 3 s (Standard) and 1 s (Brutal) burst cooldown (Gabriel.cs:132-142), near Boss36's restP1 1.6 s (design.n36.boss36). The player is always moving in a Boss37 fight, so the rest is a breather, not a wait.",
  burst: 2,
  burst_why: "Attacks chained back to back before a rest: Gabriel's burst of 2 on Standard (Gabriel.cs:132-142; 3 on Violent+). BOSSES.md rule 4.",
  budgetCap: [6, 6, 10],
  budgetCap_why:
    "Budget points per leg (leg 1, 2, 3) before the forced rest: Minos Prime's cap 6 unenraged on Standard (MinosPrime.cs:342-353); leg 3 is the enraged leg: Sisyphus Prime's 10 enraged on Standard (SisyphusPrime.cs:365-376). Attack costs 1-3 are each boss's (MinosPrime.cs:520-740).",
  budgetRestSeconds: 2,
  budgetRestSeconds_why: "Seconds of the forced rest at the cap: 2 s on Standard and up (MinosPrime.cs:342-353). BOSSES.md rule 4: 'the breather where you reposition'.",
  parryRefund: 5,
  parryRefund_why:
    "Budget points a parry takes back: attackAmount -= 5 (MinosPrime.cs:1125; SisyphusPrime.cs:1181). Delta: the budget is clamped at 0 here (Minos lets it go negative), so two parries never buy a boss 10 extra attacks before its rest.",
  pityStep: 0.25,
  pityStep_why: "The pity bonus every unpicked attack gains per pick; the picked attack's resets to 0; never the previous attack (Gabriel.cs:74-128). BOSSES.md rule 5.",
  stagger: [6, 9, 12],
  stagger_why:
    "The stagger meter's full value per leg (leg 1, 2, 3): Minos Prime's stamina 6 in phase 1 and 12 in phase 2 (BOSSES.md rule 3; DECIDE37 D-5); leg 2 between them. A boss's right move calls Stagger(amount) with its own amounts (a parry 2-3, a pierce 2 ...).",
  windowSeconds: 2.5,
  windowSeconds_why: "The CYAN window's length (the core takes full damage): BOSSES.md rule 3 '2-3 s'; THE WICK's and THE STAG's cards say 2.5 s.",
  chipScale: 0.1,
  chipScale_why: "The damage share outside the window and outside the boss's right move: BOSSES.md 'anything from a ledge does 10 %' (THE WICK), 'Guns do 10 %' (THE STAG). Boss37Brain.Scale(rightMove).",
  autoLegs: true,
  autoLegs_why: "The legs follow the bar (legAt). A boss whose legs are places (a lap, a tower pair) sets kit.autoLegs false and calls NextLeg() itself.",
  legAt: [0.667, 0.333],
  legAt_why: "The bar's fraction at which leg 1 and leg 2 end: three equal legs (BOSSES.md rule 1: 'split into 3 legs'; ULTRAKILL Leviathan: the arena changes at each third).",
  legChangeSeconds: 1.5,
  legChangeSeconds_why: "Seconds of the leg change (the arena changes, no attacks, no damage): as long as the longest wind-up (BOSSES.md rule 7: 1.0-1.5 s) so the change reads as one beat.",
  restartRestSeconds: 1.5,
  restartRestSeconds_why: "Seconds the boss rests after RestartLeg (a fall / a death puts the player at the leg's checkpoint, DECIDE37 D-6): one rest (restSeconds) to get moving before the next tell.",
  habitSeconds: 8,
  habitSeconds_why: "Seconds for the habit bias to cross from 1 (air / walls) to 0 (ground) or back: BOSSES.md rule 5 '~8 s' (Minotaur.cs:294-296 drifts over 20 s; ours is a shorter fight).",
  habitGain: 2,
  habitGain_why: "A ground-tagged attack weighs 1 + habitGain x (1 - airBias), an air-tagged one 1 + habitGain x airBias: at full habit the denying attack is 3x as likely as an untagged one of the same weight. Ours (Minotaur picks by a roll against the bias, Minotaur.cs:399-410).",
  habitSnap: true,
  habitSnap_why: "After a habit attack the bias snaps to the other extreme (Minotaur.cs:399-410), so the boss doesn't deny the same space twice running.",
  campSeconds: 3,
  campSeconds_why: "Seconds grounded inside campRadius before the next pick is the boss's camp attack: THE WICK's plate slam 'on a ledge > 3 s', THE CROWN's 'one platform > 3 s' (BOSSES.md). Boss36 used 6 s (V2.cs:708-731); a moving fight answers sooner.",
  campRadius: 4,
  campRadius_why: "Metres: staying within 4 m of one spot counts as camping (design.n36.boss36.campRadius); a wall ride or leaving the circle restarts the clock.",
  hideSeconds: 3,
  hideSeconds_why: "Seconds on the hide timer before the boss repositions onto the player (boss37Hide): GabrielBase.cs:355-363 teleports at 3 s. BOSSES.md rule 6.",
  hideFill: 1,
  hideFill_why: "The hide timer fills 1 per second hidden (GabrielBase.cs:355-363).",
  hideDrain: 2,
  hideDrain_why: "The hide timer drains 2 per second seen (GabrielBase.cs:355-363).",
  hideReach: 20,
  hideReach_why: "Metres: farther than this from the boss counts as hiding (GabrielBase.cs:355-363: more than 20 m away).",
  hideBelow: 15,
  hideBelow_why: "Metres: more than this below the boss counts as hiding (GabrielBase.cs:355-363: more than 15 m below).",
};

/** The text span [start, end) of the JSON object that starts at the `{` at `open` (strings skipped). */
function objectEnd(text, open) {
  let depth = 0;
  let inStr = false;
  for (let i = open; i < text.length; i++) {
    const ch = text[i];
    if (inStr) {
      if (ch === "\\") i++;
      else if (ch === '"') inStr = false;
      continue;
    }
    if (ch === '"') inStr = true;
    else if (ch === "{") depth++;
    else if (ch === "}") {
      depth--;
      if (depth === 0) return i + 1;
    }
  }
  throw new Error("unbalanced object at " + open);
}

function serialize(obj, indent) {
  const s = JSON.stringify(obj, null, 2);
  return s.split("\n").map((l, i) => (i === 0 ? l : indent + l)).join(eol);
}

const nAt = raw.indexOf('"n37": {');
if (nAt < 0) throw new Error("design.n37 not found (the shared commit adds it)");
const nOpen = raw.indexOf("{", nAt);
let nEnd = objectEnd(raw, nOpen);
for (const name of Object.keys(rows)) {
  const key = '"' + name + '": {';
  const at = raw.indexOf(key, nOpen);
  if (at < 0 || at > nEnd) throw new Error("design.n37." + name + " not found inside design.n37 (the shared commit adds the rows)");
  const lineStart = raw.lastIndexOf("\n", at) + 1;
  const indent = raw.slice(lineStart, at);
  const open = raw.indexOf("{", at);
  const end = objectEnd(raw, open);
  const cur = JSON.parse(raw.slice(open, end));
  if (typeof cur.enabled === "boolean") rows[name].enabled = cur.enabled; // the row's switch is the lead's
  raw = raw.slice(0, open) + serialize(rows[name], indent) + raw.slice(end);
  nEnd = objectEnd(raw, nOpen);
  console.log("design.n37." + name + " written");
}
JSON.parse(raw); // still valid JSON
fs.writeFileSync(file, raw);
