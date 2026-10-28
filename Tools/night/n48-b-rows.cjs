// NIGHT48 builder B: the numbers of B's three NIGHT48 sim rows (wyrmTell48, wyrmDive48, kilnBoost48) in design.n48.<row>.numbers, in BOTH
// config copies (Design/unity-config.json and Assets/StreamingAssets/Design/unity-config.json, byte-identical). Touches ONLY those three
// "numbers" objects (the shared commit pre-placed each as {}); every other byte is checked unchanged by a parse round-trip. Re-runnable: the
// whole numbers object of each row is replaced by the one below. Run from the worktree root: node Tools/night/n48-b-rows.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const FILES = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];

const NUMBERS = {
  wyrmTell48: {
    _why: "G31 (NIGHT48.md: the third approach, the lead's): change the COUNTER, not the timing. The head-charge tell grows to tellS with the aim tracking until lockS are left (so the charge comes where you are), and the head is punchable (Wyrm30Punchable: the view's yellow) for the WHOLE tell and the whole charge: a gauntlet hit on the worm with the head's box in reach, or a fist punch / parry (parry 1) with the head in reach, staggers it. The NIGHT47 window (the last 0.4 s before contact, by the contact time) no longer gates it, so a longer tell no longer makes the punch harder. Read by SurvivalSim.N48.Brawl.cs.",
    tellS: 0.8,
    tellS_why: "the head-charge tell (s; design.n47.wyrm47.numbers.aggroS 0.5, design.n26 tells.aggro 0.5): G31's every tell >= 0.8 s. Measured (n47human --boss wyrm --level n47-kiln --b47-boost, 20 seeds, lockS 0.25): 1.0 s 157.5 / 203.8 s median / p90, 19 / 20 killed, deaths 42; 0.8 s 135.0 / 163.0 / 47.9 min, 20 / 20, staggers 39 vs 30 (the off row: 97.1 / 146.7 / 46.8); 1.0 s with a fixed aim (lockS 0) 146.8 / 228.6",
    lockS: 0.25,
    lockS_why: "the charge's aim follows the player through its tell until this many seconds are left, then locks (Wyrm.aggroLock47; the CHARGER's charger47 lockS 0.25: one language for 'it locks, now move'); NIGHT47 measured a 0.9 s tell with a fixed aim charging where the player had been",
    wholeCharge: 1,
    wholeCharge_why: "1: the head stays punchable for the whole charge pass (any contact time), not only design.n30.wyrm30.windowSeconds (0.4 s) before contact",
    parry: 1,
    parry_why: "1: a fist punch (the melee button, the parry verb, VERBS 40) pressed while the head is punchable and its box within weapons.gauntlet.range + design.n30.wyrm30.headReachMargin staggers it as a gauntlet hit would (no bonus damage: the fist's own hit lands as it does today)",
  },
  wyrmDive48: {
    _why: "G34 (NIGHT48.md: the third approach, the lead's): the burrow as the punish. When a STAGGER ends (Wyrm31's STAGGERED window, or wyrm47's windowCap spending it) the worm dives at once (Wyrm.Dive48: the DIVE pattern from where the head is: the crack tell, the dive, the travel under the floor, the eruption), so the burrows rise with the player's success. Read by SurvivalSim.N48.Brawl.cs.",
    tryS: 3.0,
    tryS_why: "seconds after the stagger ends the sim keeps asking Wyrm.Dive48 (it refuses while an aggro charge, its tell, a recoil or the finale runs); past this the dive is dropped (event wyrm48DiveMissed)",
    when: 2,
    when_why: "G34's second attempt (round 2, the lead's line: the dive as the punish AT the stagger): 0 the dive when the STAGGERED window ends (round 1); 1 at the stagger: after the punch's recoil the worm flees under the floor and the STAGGERED window opens whole when it erupts (Wyrm31.OnStagger at the dive's end), so the punish is not lost under the floor; 2 both. Measured (n47human --boss wyrm --level n47-kiln --b47-boost, 40 seeds; dives a minute median, median / p90 / min s): 0 + windowShare 0.25: 1.78, 113.1 / 156.9 / 62.3; 1: 1.94, 78.9 / 113.9 / 38.1; 2: 2.28, 73.3 / 129.8 / 38.1; 1 + 0.25: 2.01, 96.0 / 120.4 / 52.4; 2 + 0.2: 2.25, 117.4 / 164.2 / 66.8; 2 + 0.25: 2.38, 103.7 / 131.2 / 67.7 (the band IN, 40 / 40 fights dive): 2",
    beatExtraS: 0,
    beatExtraS_why: "the min's second lever (round 3; Wyrm.beatExtra48): seconds added to each phase beat (66 % / 33 %, no damage lands): the same length added to every fight. Measured with wyrmTell48 + wyrmDive48 (when 2, windowShare 0.25), 40 seeds: +8 s 138.3 / 179.0 / 85.7 s median / p90 / min (p90 on the band's edge): 0 tonight (windowShare 0.15 holds the min with p90 174.9)",
    windowShare: 0.15,
    windowShare_why: "the worm's min (NIGHT48.md: 'a cap per window on the share of the pool one stagger can take'): once one STAGGER has cost it this share of its pool it flees: the window ends (Wyrm31.EndStagger47) and the dive follows. 0 = no cap. Measured with wyrmDive48 alone (n47human --boss wyrm --level n47-kiln --b47-boost, 20 seeds): 0 76.6 / 120.7 / 43.0 s median / p90 / min, 1.87 dives a minute; 0.35 88.6 / 119.3 / 47.4, 2.13; 0.25 107.1 / 134.0 / 62.3 (the band IN), 1.81, 20 / 20 fights dive (NIGHT47's wyrm47 windowCap without the dive: 0.2 214.2, 0.3 156.9). Round 3, when 2, 40 seeds, with wyrmTell48 on too (G31's min): 0.25 131.0 / 157.6 / 44.4; 0.2 130.4 / 158.4 / 59.3; 0.15 135.2 / 174.9 / 64.0 (IN, 2.09 dives a minute); all three B rows at 0.15 132.5 / 174.9 / 64.0 (2.17); wyrmDive48 alone at 0.15 116.6 / 164.2 / 68.5 (2.35): 0.15",
  },
  kilnBoost48: {
    _why: "G33 (NIGHT48.md: 'make the islands pay more'): the open core's x3 (design.n24.wyrm hp.coreMult) lands only from an island (feet on a slag island) or mid-boost (airborne after a THUNDERCLAP shot fired airborne, until the feet land); from the floor it lands x floorMult. And a health pickup on each island (healAmount, back after healEveryS). The islands are gen-n47-wyrm-arena.cjs's boxes 35-37 (ISLAND r 12, bearings 90 / 210 / 330 deg, half 2, top 6.0 m), arena-local to the worm's arena centre. Read by SurvivalSim.N48.Brawl.cs.",
    floorMult: 1.0,
    floorMult_why: "what an open-core hit from the floor lands (x; the body's x1): the x3 is the islands' and the boost's pay",
    highY: 5.5,
    highY_why: "metres over the arena's floor at or over which grounded feet count as up (the islands' 6.0 m top, the 6 m perch 23 and the 9.5 / 13 m rim perches: every surface only the boost reaches)",
    islandR: 12,
    islandR_why: "the islands' ring radius (m, arena-local; gen-n47-wyrm-arena.cjs ISLAND.r)",
    islandFirstDeg: 90,
    islandFirstDeg_why: "the first island's bearing from the arena's +x axis (deg; ISLAND.bearings[0])",
    islandStepDeg: 120,
    islandStepDeg_why: "the bearing step between the three islands (deg; 90 / 210 / 330)",
    islandCount: 3,
    islandCount_why: "islands (gen-n47-wyrm-arena.cjs boxes 35-37)",
    islandHalf: 2.3,
    islandHalf_why: "half the island's side (m; ISLAND.half 2) plus 0.3 m of the player's capsule over its edge",
    islandTop: 6.0,
    islandTop_why: "the islands' top over the arena floor (m; ISLAND.top)",
    healAmount: 25,
    healAmount_why: "HP the island's pickup gives (design.n33's pickup scale: a quarter bar of 100): standing on an island with hp under max takes it",
    healEveryS: 20,
    healEveryS_why: "seconds before an island's pickup is back (one per island; the fight's spit cadence is 2.2-3.2 s, a minute-long fight sees ~3 per island)",
  },
};

function closeOf(text, open) {
  // the index of the brace closing the object that opens at `open` (string-aware)
  let depth = 0, inStr = false;
  for (let i = open; i < text.length; i++) {
    const c = text[i];
    if (inStr) {
      if (c === "\\") i++;
      else if (c === '"') inStr = false;
      continue;
    }
    if (c === '"') inStr = true;
    else if (c === "{") depth++;
    else if (c === "}") { depth--; if (depth === 0) return i; }
  }
  throw new Error("unclosed object at " + open);
}

let outText = null;
for (const f of FILES) {
  const raw = fs.readFileSync(path.join(ROOT, f), "utf8");
  const eol = raw.includes("\r\n") ? "\r\n" : "\n";
  const before = JSON.parse(raw);
  let text = raw;
  const n48At = text.indexOf('"n48": {');
  if (n48At < 0) throw new Error(f + ": no design.n48");
  for (const [row, nums] of Object.entries(NUMBERS)) {
    const rowAt = text.indexOf('"' + row + '": {', n48At);
    if (rowAt < 0) throw new Error(f + ": no design.n48." + row);
    const rowEnd = closeOf(text, text.indexOf("{", rowAt));
    const numAt = text.indexOf('"numbers": {', rowAt);
    if (numAt < 0 || numAt > rowEnd) throw new Error(f + ": " + row + " has no numbers");
    const open = text.indexOf("{", numAt);
    const close = closeOf(text, open);
    const indent = text.slice(text.lastIndexOf(eol, numAt) + eol.length, numAt);
    const block = JSON.stringify(nums, null, 2).split("\n").map((l, i) => (i === 0 ? l : indent + l)).join(eol);
    text = text.slice(0, open) + block + text.slice(close + 1);
  }
  const after = JSON.parse(text);
  for (const row of Object.keys(NUMBERS)) {
    if (JSON.stringify(after.design.n48[row].numbers) !== JSON.stringify(NUMBERS[row])) throw new Error(row + " did not round-trip");
    after.design.n48[row].numbers = before.design.n48[row].numbers;
  }
  if (JSON.stringify(after) !== JSON.stringify(before)) throw new Error(f + ": something besides B's three numbers objects changed");
  if (outText !== null && outText !== text) throw new Error("the two copies differ");
  outText = text;
  fs.writeFileSync(path.join(ROOT, f), text);
  console.log("n48-b-rows: " + f + ": " + Object.keys(NUMBERS).join(", ") + " numbers written");
}
