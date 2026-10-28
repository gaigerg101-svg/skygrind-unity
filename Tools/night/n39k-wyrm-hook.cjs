// NIGHT39 builder K4 -> the lead (SurvivalSim.Wyrm24.cs is the lead's file: "the Wyrm24 arena hook" in NIGHT39.md's L0 / builders table). Two call
// sites so the worm loads the LEVEL's own arena (n38-kiln's k5 row `wyrmArena`: Design/maps/wyrm39-arena.json) through SurvivalSim.Kiln39.cs; a level
// without the field keeps design.n26.bosses.wyrm26.arena (n35-kiln, kiln38 off: NIGHT38's room bit for bit). The worm's paths, params and AI: untouched.
//   1. Wyrm24Ensure: the level-3 worm's arena path goes through Wyrm39Arena(the config's path).
//   2. Wyrm24BeginCore: a dead worm whose room was built from another arena file is rebuilt (as the NIGHT26 switch check does).
// Usage (repo root): node Tools/night/n39k-wyrm-hook.cjs          (idempotent: a patched file is left as it is; exit 1 if an anchor is missing)
const fs = require("fs");
const path = require("path");
const f = path.join(__dirname, "../../Assets/Scripts/Game/SurvivalSim.Wyrm24.cs");
let s = fs.readFileSync(f, "utf8");
if (s.includes("Wyrm39Arena(")) { console.log("already patched: " + f); process.exit(0); }
const a1 = 'arena = Path.Combine(Wyrm24Dir, cfg.Str(w26 ? "design.n26.bosses.wyrm26.arena" : pfx + ".arena"));';
const b1 = 'arena = Path.Combine(Wyrm24Dir, w26 ? Wyrm39Arena(cfg.Str("design.n26.bosses.wyrm26.arena")) : cfg.Str(pfx + ".arena")); // NIGHT39 K4: the level\'s own worm arena (SurvivalSim.Kiln39.cs)';
const a2 = "if (wyrm24 != null && !wyrmFinal && !wyrm24.Alive && wyrm24Is26 != Wyrm26On)";
const b2 = 'if (wyrm24 != null && !wyrmFinal && !wyrm24.Alive && (wyrm24Is26 != Wyrm26On || (wyrm24Is26 && Wyrm39ArenaStale(cfg.Str("design.n26.bosses.wyrm26.arena"))))) // NIGHT39 K4: or the level wants another arena file';
for (const [a, b] of [[a1, b1], [a2, b2]]) {
  const n = s.split(a).length - 1;
  if (n !== 1) { console.error(`anchor found ${n} times (want 1): ${a}`); process.exit(1); }
  s = s.replace(a, b);
}
fs.writeFileSync(f, s);
console.log("patched " + f + " (2 call sites)");
