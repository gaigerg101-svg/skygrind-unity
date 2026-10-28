// NIGHT39 builder B1: writes design.n39.boss39's numbers (the per-boss overrides; SurvivalSim.N39.Boss.cs reads them) into
// Design/unity-config.json, then copies the file to Assets/StreamingAssets/Design/ (the two stay byte-identical). Patches ONLY the boss39
// row's text (L-049); `enabled` is kept as it is (the lead turns the row on). Re-runnable: the row is rewritten whole each time.
// Run from the repo root (or a worktree): node Tools/night/n39-boss39-config.cjs
const fs = require("fs");
const path = require("path");
const root = path.resolve(__dirname, "..", "..");
const file = path.join(root, "Design", "unity-config.json");
const copy = path.join(root, "Assets", "StreamingAssets", "Design", "unity-config.json");
const raw = fs.readFileSync(file, "utf8");
if (raw !== fs.readFileSync(copy, "utf8")) throw new Error("the two config copies differ before the patch");
const d = JSON.parse(raw);
const old = d.design.n39 && d.design.n39.boss39;
if (!old) throw new Error("design.n39.boss39 is missing (the NIGHT39 shared commit adds it)");

const row = {
  _why: old._why,
  enabled: old.enabled,
  band_why: "Bot band 30-60 s (every seed killed, p50 30-60 s, p90 <= 70 s; Tools/SimHarness `boss39`): a person takes about twice the bot's time (CORE.md w31 / w32: the 15-22 s bot band was '~40 s person'; NIGHT32: tune the bot band to about half GG's target), so 30-60 s bot = GG's 'one to two minutes' (NIGHT39.md B1). Before / after per boss: Research/2026-10-14-wave39/B1.md. Each boss's keys override only that boss, only while the row is on; design.n37.* and the worm's rows are unchanged, so --off39 gives NIGHT38's fights.",
  wick: {
    _why: "THE WICK (level 1): before, total p50 34.3 s (from the cap 15.4 s). Its legs are places (the climb), so the pool is the lever that lengthens the fight at the cap; one more right move per window (stagger 3 -> 4) makes each window a cycle more. Measured: total p50 44.7 s, p90 44.7 s, from the cap 25.8 s, 10/10 killed.",
    hpMult: 2,
    hpMult_why: "The fight's pool x2 (Wick37.StartFight's pool x the run's scale). hpMult 2 alone: total p50 42.9 s; with stagger 4: 44.7 s.",
    stagger: [4, 4, 4],
    stagger_why: "The CYAN window's meter per leg (design.n37.wick37.kit.stagger [3, 3, 3]): one more crack / parry / pierce per window (D-1: the brain's lever first). Alone it changed nothing (34.3 s): the window count is not the Wick's bottleneck.",
  },
  stag: {
    _why: "THE STAG (level 2): before p50 20.9 s, p90 24.8 s. One window per leg ended each leg (a launch is 320 of a 300-per-leg pool), so a bigger pool makes each leg 2-3 stagger cycles, and the meter grows per leg (Minos Prime's stamina pattern, Boss37.cs). Measured: p50 37.9 s, p90 45.7 s, 10/10 killed.",
    hpMult: 4.5,
    hpMult_why: "The pool 900 -> 4050: legs of 1350 HP, so a leg takes 2-4 launches / window punches instead of one. Probed: x2 22.3 s, x3.5 + stagger 6 32.7 s, x4 + stagger 6 + window 2 s 35.8 s, x5 37.0 s (diminishing).",
    stagger: [6, 7, 8],
    stagger_why: "The meter per leg (design.n37.stag37.kit.stagger [4, 4, 4]): 3 / 4 / 4 hind-leg catches (catch.stagger 2) or chunk parries per window instead of 2.",
    windowSeconds: 2,
    windowSeconds_why: "The CYAN window 2.5 -> 2 s: a window is a burst, not a stand-and-shoot; the length comes from more windows.",
  },
  wyrm: {
    _why: "THE KILN WYRM (level 3; GG: 'the worm boss fight needs more health because it dies too quickly'): before p50 20.5 s on a pool of 1405 (design.n24.wyrm.hp.pool 7800 x 1.1 x design.n29 0.7 x design.n33 0.234). Its AI unchanged (GG); its phases are fractions of the pool (66 % / 33 %), so they move with it. Measured on level 3 itself (round 2: `boss39 kiln`, n38-kiln's k5 THE RING in K4's wyrm39-arena.json, NIGHT31's punch-and-slug pilot WyrmPilot31): before 11.8 s; after p50 43.0 s, p90 43.0 s, pool 5620, 10/10 killed (with P1's p39 on too: p50 50.4 s, p90 62.5 s). On the old `boss32 wyrm` rig (survival map, the old arena): p50 32.4 s, p90 43.8 s.",
    hpMult: 4,
    hpMult_why: "Each fight's pool x4 after NIGHT29 / 32 / 33's cuts (1405 -> 5620, still under NIGHT25's 8580). Round 1 picked x2.75 on the old rig (40.3 s); in the new arena x2.75 is 26.7 s. Probed on k5: x3.5 35.3 s, x4 43.0 s, x4.5 45.8 s, x5 66.6 s.",
  },
  loom: {
    _why: "THE LOOM (level 4): before p50 21.6 s, p90 21.8 s. Its legs are places and each leg ends when its one CYAN window closes, so its pool never gates the fight (hpMult x2: 21.7 s; nailsToTear 5: 21.9 s): only a longer road to each window does. Legs 2 and 3 ask for a parried spool on top of the two torn knees (the parry, GG's w35 'an understanding of parry mechanics'). Measured over 20 seeds: p50 38.8 s, p90 45.9 s, 20/20 killed.",
    stagger: [6, 7.5, 7.5],
    stagger_why: "The meter per leg (design.n37.loom37.kit.stagger [6, 6, 6]): two torn knees (tearStagger 3 each) open leg 1's window; legs 2 and 3 also need one parried spool (parryStagger 1.5). Spools are YELLOW in every leg (weight 1.2), so a person who misses one gets another. 7.5 in leg 1 too: 3/10 killed (the pilot dies to lashes waiting): leg 1 stays the teaching leg.",
    restSeconds: 2.5,
    restSeconds_why: "The breather between bursts 1.5 -> 2.5 s (Boss37 REST): the waits for a spool line up with the pilot's tears; 2.5 s measured 20/20 killed. 2 s measured 4/10 (the pilot died to lashes): the fight is sensitive here, so re-check after any loom change.",
  },
  crown: {
    _why: "THE CROWN (level 5): before p50 21.93 s, p90 23.02 s. Its health is its gems (a pass through the ring breaks passGems whole gems; the legs change at legGemsLeft), so neither the pool (x2: 21.9 s) nor the meter (stagger 6: 19.0 s) lengthens it: more gems = more passes, the fight's moving verb. The TORCH pass keeps its 2 gems (torchGems untouched: a verb's reward kept, PRINCIPLES 1). Measured: p50 36.5 s, p90 41.9 s, 10/10 killed.",
    gems: 20,
    gems_why: "design.n37.crown37.crown.gems 8 -> 20 (the pool is gems x gemHp 100, as Crown37.StartFight sets it; the view draws c.p.gems sockets). Probed: 12 27.7 s, 16 30.5 s, 20 36.5 s.",
    legGemsLeft: [13, 6],
    legGemsLeft_why: "Leg 2 at 13 gems left, leg 3 at 6 (design.n37.crown37 [5, 2] of 8): 7 / 7 / 6 gems per leg, leg 3 still ends on the finisher through the opening.",
  },
  stag31: {
    _why: "Level 6's old Stag (Stag31, plan id glacier on n35-finale): before 19.6 s (`boss32 stag`, NIGHT31's StagPilot31, the same time every seed). Pool only (its AI is NIGHT31's). Measured: 44.4 s, 10/10 killed; on n35-finale's own room x6 (round 2: `boss39 finale`, the game's config): before 19.4 s, after 37.5 s (with P1's p39 on too: 43.0 s), 10/10 killed.",
    hpMult: 3,
    hpMult_why: "Each fight's pool x3 after NIGHT32 / 33's cuts. Probed: x2 27.8 s, x2.5 37.4 s, x2.75 37.4 s, x3 44.4 s, x3.25 51.3 s.",
  },
};

// the row's text span inside n39 (a brace matcher that skips JSON strings)
const key = '"boss39": {';
const n39At = raw.indexOf('"n39": {');
const start = raw.indexOf(key, n39At);
if (n39At < 0 || start < 0) throw new Error("boss39 row not found");
let i = start + key.length - 1, depth = 0, inStr = false;
for (; i < raw.length; i++) {
  const c = raw[i];
  if (inStr) { if (c === "\\") i++; else if (c === '"') inStr = false; continue; }
  if (c === '"') inStr = true;
  else if (c === "{") depth++;
  else if (c === "}") { depth--; if (depth === 0) break; }
}
const eol = raw.includes("\r\n") ? "\r\n" : "\n";
const lineStart = raw.lastIndexOf("\n", start) + 1;
const indent = raw.slice(lineStart, start);
const text = JSON.stringify(row, null, 2).split("\n").map((l, k) => (k === 0 ? l : indent + l)).join(eol);
const out = raw.slice(0, start) + '"boss39": ' + text + raw.slice(i + 1);
const check = JSON.parse(out);
// nothing but the boss39 row changed
const a = JSON.parse(raw), b = JSON.parse(out);
delete a.design.n39.boss39; delete b.design.n39.boss39;
if (JSON.stringify(a) !== JSON.stringify(b)) throw new Error("the patch touched more than design.n39.boss39");
if (JSON.stringify(check.design.n39.boss39) !== JSON.stringify(row)) throw new Error("the row did not round-trip");
fs.writeFileSync(file, out);
fs.copyFileSync(file, copy);
console.log("design.n39.boss39 written (" + Object.keys(row).filter((k) => !k.endsWith("_why") && k !== "enabled").join(", ") + "), enabled " + row.enabled + "; copied to StreamingAssets");
