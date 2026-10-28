// NIGHT32 builder A (arenas32): two lines in lead-owned files so the Lantern ring follows the RUN's arenas32 bit (a tape's replay, a
// harness override) instead of the design default (Lantern30ArenaParams.From(cfg) reads design.n32.arenas32.enabled and N32Blocks.ForceOff,
// which is right for every live run and harness; only a replay whose tape bits differ from the config needs this). Also rebuilds the cached
// ring when a later run in the same sim flips the bit. NOT applied by builder A. Run once (the lead): node Tools/night/n32-ring-wire.cjs
const fs = require("fs");
const root = process.argv[2] || "C:/Users/gaige/dev/SKYGRIND-unity/";
function patch(file, pairs) {
  let s = fs.readFileSync(root + file, "utf8");
  const crlf = s.includes("\r\n");
  if (crlf) s = s.replace(/\r\n/g, "\n");
  for (const [a, b] of pairs) {
    const n = s.split(a).length - 1;
    if (n !== 1) throw new Error(file + ": anchor found " + n + " times: " + a.slice(0, 80));
    s = s.replace(a, b);
  }
  if (crlf) s = s.replace(/\n/g, "\r\n");
  fs.writeFileSync(root + file, s);
  console.log("patched " + file);
}

patch("Assets/Scripts/Game/SurvivalSim.Boss30.cs", [
  ["        Lantern30Arena Lantern30ArenaEnsure() => lantern30Arena ?? (lantern30Arena = Lantern30Arena.Build(cfg));",
   "        Lantern30Arena Lantern30ArenaEnsure() => lantern30Arena != null && lantern30Arena.Round == (n32.arenas32 && cfg.Num(\"design.n32.arenas32.facetsPerPanel\") > 1) ? lantern30Arena : (lantern30Arena = Lantern30Arena.Build(cfg, n32.arenas32)); // NIGHT32 A: the run's ring (round while arenas32 is on)"],
]);

patch("Assets/Scripts/Game/SurvivalSim.Boss31.cs", [
  ["            Lantern30ArenaParams prm = Lantern30ArenaParams.From(cfg);",
   "            Lantern30ArenaParams prm = Lantern30ArenaParams.From(cfg, n32.arenas32); // NIGHT32 A: the run's ring (round arc platforms while arenas32 is on)"],
]);
