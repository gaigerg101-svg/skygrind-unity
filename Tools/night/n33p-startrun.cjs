// NIGHT33 P (builder P, card P, row design.n33.shop33): ONE line in the lead's SurvivalGame.Flow.cs: StartRun hands the run its shop inputs (the
// profile's wallet and upgrades for a campaign run with the row on, none otherwise) right before sim.StartRun, so the tape records them
// (SurvivalGame.Shop33.cs N33ShopBeforeRun). Needs SurvivalGame.Shop33.cs (builder P's commit) in the tree.
// Run once: node Tools/night/n33p-startrun.cjs [repoRoot]   (default: the main checkout; the replace asserts its anchor exists exactly once)
const fs = require("fs");
const root = (process.argv[2] || "C:/Users/gaige/dev/SKYGRIND-unity").replace(/\\/g, "/").replace(/\/?$/, "/");
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

patch("Assets/Scripts/Game/SurvivalGame.Flow.cs", [
  ["            sim.StartRun(seed, ResetViewsForRun, RaiseNewRun);\n",
   "            N33ShopBeforeRun(); // NIGHT33 P: a campaign run's wallet and upgrades (design.n33.shop33; none otherwise), before the tape records them (SurvivalGame.Shop33.cs)\n            sim.StartRun(seed, ResetViewsForRun, RaiseNewRun);\n"],
]);
