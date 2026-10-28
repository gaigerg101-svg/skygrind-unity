// NIGHT46 builder B1: write the numbers of B1's own config rows (design.n46.wyrm46, design.n46.walkin46) into Design/unity-config.json and its
// byte copy Assets/StreamingAssets/Design/unity-config.json. Touches ONLY the "numbers" object of the named rows (a text edit inside each row's
// block; nothing else in the file moves). Re-running with the same numbers is a no-op; a row whose numbers differ is rewritten.
// Run (from the repo or worktree root): node Tools/night/n46-b1-rows.cjs
const fs = require("fs");
const path = require("path");
const ROOT = path.join(__dirname, "../..");
const FILES = ["Design/unity-config.json", "Assets/StreamingAssets/Design/unity-config.json"];

// Each value with its _why (measured: Research/2026-10-17-wave46/B1.md; Tools/SimHarness/Harness.N46.Wyrm.cs `n46wyrm fight`, WyrmPilot31, 10 seeds, cap 300 s).
const ROWS = {
  wyrm46: {
    _why: "THE KILN WYRM in the bot band by its damage window first (DECIDE39 D-1: stagger before health; DECIDE46 N-1): the OPEN window after a head punch longer and richer, then the pool. Read by SurvivalSim.N46.Wyrm.cs into the NIGHT31 / 32 worm system (Wyrm31) while the row is on; a missing number leaves its lever as NIGHT45's. Tuned for the game it ships in: n46-kiln walked in (kiln46 + walkin46 + wyrm46 ON together). Measured (n46wyrm fight --walk, WyrmPilot31, 20 seeds): NIGHT45's fight on n45-kiln median 185.5 s over every seed (10 seeds: killed 7 / 10 in 300 s, deaths median 5, staggered share 58 %); walked in with walkin46 alone 191.3 s (10 seeds); these numbers walked in: median 38.3 s, 27.1-55.7 s, killed 20 / 20, deaths 0 on 20 / 20, damage taken > 0 on 20 / 20 (median 28), staggered share 63 %, the crumble fell 20 / 20. The fight is bimodal (a seed with 2+ deaths runs 120-300 s: each death puts the worm back at its phase start, NIGHT25's rule, kept).",
    staggerSeconds: 8,
    staggerSeconds_why: "seconds the worm stays STAGGERED (the bar's OPEN) after a head punch (design.n32.bosses32.wyrm.staggerSeconds 4): the fight's damage comes from this window. Trials (pool x0.5): 6 s + open 2.0 gave 118.7 s (share 64 %); 8 s + open 2.5 gave 47.4 s (share 82 %)",
    open: 2.5,
    open_why: "what every weapon lands while STAGGERED, x the part (design.n32.bosses32.wyrm.open 1.25); the one-hit cap (maxHitFrac 0.15 of the pool) is unchanged, so one stagger still cannot end the fight",
    poolScale: 0.9,
    poolScale_why: "x the fight's pool after NIGHT39's boss39 x4 (5620 -> 5058; the phases are fractions of the pool, so they move with it). Walked in on n46-kiln with 8 s / 2.5 (20 seeds): x1.0 38.9 s but 3 seeds spiral (140-300 s, 3-8 deaths); x0.9 38.3 s, every seed 27.1-55.7 s, 0 deaths; x0.8 34.0 s (one seed 118.5 s); x0.5 24.8 s (under the band; 5 seeds spiral). On n45-kiln (the teleport) the same window needed x0.5 (47.4 s; x1.0 114.3 s; x0.9 there: see Research/2026-10-17-wave46/B1.md). Why the walk-in shortens the bot's fight: not checked",
    retryKeep: 1,
    retryKeep_why: "a death in the fight keeps this share of the bar the player took off since the phase began (1: the bar as it was at the death; 0: NIGHT25's rule, back to the phase start, or in phase 1 a whole new fight at full health). DESIGN-RULES A-4: a failure costs time and a little health, not a restart. Walked in, 40 seeds, with aggroSpeedP3 1: the longest fight 300 -> 261.1 s alone (the tail was phase 3's punch, below)",
    aggroSpeedP3: 0.8,
    aggroSpeedP3_why: "the head's aggro charge in phase 3 x this (design.n26.bosses.wyrm26.aggro.speedMult 1; x0.8 = phase 2's speed, the phases' step design.n24.wyrm.speed.phaseMult 1.25): the charge IS the punch window (design.n30.wyrm30), and in phase 3 the bot met 15-23 charges with 0 staggers in its long seeds (n46wyrm fight per phase). Walked in, 40 seeds, retryKeep 1: x1.0 max 261.1 s / 3 over 60 s; x0.8 median 38.3 s, max 85.1 s, 4 / 40 over 60 s (57-85 s, at most 1 death); x0.65 48.5 s, max 106.5; x0.5 48.6 s, max 83.3",
  },
};

function block(name, nums, indent, eol) {
  const pad = " ".repeat(indent + 2), end = " ".repeat(indent);
  const lines = Object.entries(nums).map(([k, v]) => pad + JSON.stringify(k) + ": " + JSON.stringify(v));
  return "{" + eol + lines.join("," + eol) + eol + end + "}";
}

let out = null;
for (const rel of FILES) {
  const file = path.join(ROOT, rel);
  let text = fs.readFileSync(file, "utf8");
  const eol = text.includes("\r\n") ? "\r\n" : "\n"; // the working copy's own line ends (git autocrlf: CRLF on disk, LF in the index)
  if (out === null) {
    for (const [name, nums] of Object.entries(ROWS)) {
      const head = '      "' + name + '": {' + eol;
      const at = text.indexOf(head);
      if (at < 0 || text.indexOf(head, at + 1) >= 0) throw new Error("row " + name + ": expected exactly one '" + head.trim() + "'");
      const close = text.indexOf(eol + "      }", at);
      const rowText = text.slice(at, close);
      const key = '        "numbers": ';
      const k = rowText.indexOf(key);
      if (k < 0) throw new Error("row " + name + ": no numbers key");
      const from = at + k + key.length;
      const to = at + rowText.length;
      const want = block(name, nums, 8, eol);
      const now = text.slice(from, to);
      if (now === want) { console.log("n46-b1-rows: " + name + " already in"); continue; }
      text = text.slice(0, from) + want + text.slice(to);
      console.log("n46-b1-rows: " + name + " numbers written (" + Object.keys(nums).filter((x) => !x.endsWith("_why")).join(", ") + ")");
    }
    JSON.parse(text); // still JSON
    out = text;
  }
  fs.writeFileSync(file, out);
  console.log("n46-b1-rows: wrote " + rel);
}
