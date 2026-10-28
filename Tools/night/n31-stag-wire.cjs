// NIGHT31 (the lead): builder B's lines in lead-owned files: THE STAG takes level 2's boss room ("glacier") when design.n31.stag31 is on.
// Run once: node Tools/night/n31-stag-wire.cjs
const fs = require("fs");
const root = "C:/Users/gaige/dev/SKYGRIND-unity/";
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

patch("Assets/Scripts/Game/SurvivalSim.Boss25.cs", [
  ["            if (id == \"glacier\" && Glacier28On) // NIGHT28 B2:",
   "            if (id == \"glacier\" && n31.stag31) return Stag31Begin(out spawn, out yawDeg); // NIGHT31 B: THE STAG replaces the Glacier (SurvivalSim.Boss31.cs)\n            if (id == \"glacier\" && Glacier28On) // NIGHT28 B2:"],
  [": id == \"glacier\" ? (Glacier28On && glacier28 != null",
   ": id == \"glacier\" && n31.stag31 ? (IBossSystem)stag31 : id == \"glacier\" ? (Glacier28On && glacier28 != null"],
]);

patch("Assets/Scripts/Game/SurvivalGame.Boss26.cs", [
  ["            Boss30View.Current = new Boss30View(this); // NIGHT30 builder B: THE LANTERN RING and the Wyrm stagger view (view only)\n",
   "            Boss30View.Current = new Boss30View(this); // NIGHT30 builder B: THE LANTERN RING and the Wyrm stagger view (view only)\n            Boss31View.Current = new Boss31View(this); // NIGHT31 builder B: THE STAG and the NIGHT31 boss tells (view only)\n"],
  ["            Boss30View.Current?.Render(alpha); // NIGHT30 builder B\n",
   "            Boss30View.Current?.Render(alpha); // NIGHT30 builder B\n            Boss31View.Current?.Render(alpha); // NIGHT31 builder B\n"],
]);

patch("Assets/Scripts/Game/RinkView28.cs", [
  ["(game.sim?.glacier28 != null && game.sim.glacier28.Alive ? game.sim.glacier28Rink : null); // NIGHT28 B2:",
   "(game.sim?.glacier28 != null && game.sim.glacier28.Alive ? game.sim.glacier28Rink : null) ?? (game.sim?.stag31 != null && game.sim.stag31.Alive ? game.sim.stag31Rink : null); // NIGHT31 B: the Stag's rink too; NIGHT28 B2:"],
]);
