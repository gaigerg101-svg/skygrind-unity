// NIGHT34 (the lead): the split bend's render. Inside a split bend the world steps 1 in N (SurvivalSim.Run.cs `if (!n34HoldWorld)`), and every
// enemy / projectile keeps its previous position only on a world step, so the world's views interpolate with B's BendWorldAlpha(alpha) and
// animate on the world's pace (Bend34WorldAnimMul: 1 - depth x (1 - 1/N); 1 outside a split bend). Outside a split bend both equal the old
// values (BendWorldAlpha(alpha) == alpha, the mul 1), so NIGHT33's render. The player's own views (the camera, the rope, the ghost) keep alpha.
// Run once: node Tools/night/n34-lead-render.cjs   (every replace asserts its anchor exists exactly once)
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
const E = "simulating && hitstopLeft <= 0 ? frameDt * WorldScale : 0";
const EW = "simulating && hitstopLeft <= 0 ? frameDt * WorldScale * wMul : 0";
const P = "hitstopLeft > 0 ? 0 : frameDt * WorldScale";
const PW = "hitstopLeft > 0 ? 0 : frameDt * WorldScale * wMul";
patch("Assets/Scripts/Game/SurvivalGame.Render.cs", [
  ["            using (rWorld.Auto())\n            {\n                coreMesh.Render(alpha);\n",
   "            double wAlpha = sim.BendWorldAlpha(alpha), wMul = sim.Bend34WorldAnimMul; // NIGHT34 B (split bend): the world's interpolation and pace (alpha and 1 outside a split bend)\n            using (rWorld.Auto())\n            {\n                coreMesh.Render(wAlpha);\n"],
  ["huskView.Render(alpha, " + E + ");", "huskView.Render(wAlpha, " + EW + ");"],
  ["swarmerView.Render(alpha, " + E + ");", "swarmerView.Render(wAlpha, " + EW + ");"],
  ["spitterView.Render(alpha, " + E + ");", "spitterView.Render(wAlpha, " + EW + ");"],
  ["N19EnemyViewsRender(alpha, " + E + ");", "N19EnemyViewsRender(wAlpha, " + EW + ");"],
  ["N20EnemyViewsRender(alpha, " + E + ");", "N20EnemyViewsRender(wAlpha, " + EW + ");"],
  ["projectileViews.Render(alpha, " + P + ");", "projectileViews.Render(wAlpha, " + PW + ");"],
  ["N19GunFxRender(alpha, " + P + ");", "N19GunFxRender(wAlpha, " + PW + ");"],
  ["N20EffectsRender(alpha, " + P + ");", "N20EffectsRender(wAlpha, " + PW + ");"],
  ["                N22RenderViews(worldDt, alpha);", "                N22RenderViews(worldDt * wMul, wAlpha);"],
  ["                N26RenderViews(worldDt, alpha);", "                N26RenderViews(worldDt * wMul, wAlpha);"],
  ["                N25RenderViews(worldDt, alpha);", "                N25RenderViews(worldDt * wMul, wAlpha);"],
  ["                N24RenderViews(worldDt, alpha);", "                N24RenderViews(worldDt * wMul, wAlpha);"],
]);
patch("Assets/Scripts/Game/SurvivalSim.N34.cs", [
  ["        void N34Init() => n34Design = N34Blocks.From(cfg);\n",
   "        /// <summary>NIGHT34 (the lead's render line): the world views' animation pace inside a split bend, 1 - depth x (1 - 1/N); 1 outside one\n" +
   "        /// (and on the global path, where WorldScale already slows everything).</summary>\n" +
   "        public double Bend34WorldAnimMul => n34.bendSplit34 && Bend34Depth01 > 0 && bend34 != null && bend34.WorldN > 1 ? 1 - Bend34Depth01 * (1 - 1.0 / bend34.WorldN) : 1;\n\n" +
   "        void N34Init() => n34Design = N34Blocks.From(cfg);\n"],
]);
