// NIGHT33 W round 4: whipboss33 aims at THE KILN WYRM's nearest exposed piece (not the head box, which is mostly buried or behind cover), presses
// again every 0.3 s until a stop, and measures the true gap toward the rope's anchor as well as the boss's centre. Anchored replaces, each exactly once.
const fs = require('fs');
const f = 'Tools/SimHarness/Harness.N33.Whip.cs';
let s = fs.readFileSync(f, 'utf8');
function rep(a, b) {
  const n = s.split(a).length - 1;
  if (n !== 1) throw new Error('anchor found ' + n + ' times: ' + a.slice(0, 80));
  s = s.replace(a, b);
}
rep(`            sim.Whip33Arm(on);
            sim.arcline21.Own(true);`,
`            sim.Whip33Arm(on);
            sim.arcline21.Own(true);
            Func<Vec3> aimFor = centre;
            if (boss == "wyrm31")
            {
                // THE KILN WYRM: the nearest piece that is not buried and that the eye sees (the head box is often under the floor or behind cover)
                Wyrm wy = sim.wyrm24;
                SurvivalSim sw = sim;
                aimFor = () =>
                {
                    Vec3 eye = sw.Eye, best = wy.boxes[0].c;
                    double bd = double.MaxValue, range = sw.cfg.movement.arcline.range;
                    for (int k = 0; k < wy.PieceCount; k++)
                    {
                        if (wy.buried[k]) continue;
                        Vec3 c = wy.boxes[k].c, dd = c - eye;
                        double l = dd.Length;
                        if (l < 2 || l > range) continue;
                        if (sw.world.RayIntersect(eye, dd / l, out RayHit wh) && wh.distance < l - 0.5) continue;
                        if (l < bd) { bd = l; best = c; }
                    }
                    return best;
                };
            }`);
rep(`                    G32Tick(sim, si, centre(), true, s == 0, false);
                    Rope rp = sim.mover.rope;`,
`                    bool idle = sim.mover.rope.phase == RopePhase.None && sim.mover.rope.cooldownSteps == 0;
                    bool press = s == 0 || (idle && sim.whip33Arrivals == arr0 && s % 36 == 0); // again every 0.3 s until a stop
                    if (press && s > 0 && sim.Grapple32On) sim.grappleCharges32 = sim.grapple32Rules.charges;
                    G32Tick(sim, si, aimFor(), true, press, false);
                    Rope rp = sim.mover.rope;`);
rep(`                    double trueGap = hit >= 0 ? hit - cr0 : (len < 0.5 ? -1 : double.NaN); // a miss right at the centre: inside`,
`                    double trueGap = hit >= 0 ? hit - cr0 : (len < 0.5 ? -1 : double.NaN); // a miss right at the centre: inside
                    if (rp.phase != RopePhase.None && rp.zip && sim.mover.zipAnchor != null)
                    {
                        // the rope's anchor too (a zip to a piece that is not the centre's)
                        Vec3 d2 = rp.anchor - chest;
                        double l2 = d2.Length;
                        double h2 = l2 > 1e-9 ? ray(chest, d2 / l2) : -1;
                        double g2 = h2 >= 0 ? h2 - cr0 : (l2 < 0.3 ? -1 : double.NaN);
                        if (!double.IsNaN(g2) && (double.IsNaN(trueGap) || g2 < trueGap)) trueGap = g2;
                    }`);
fs.writeFileSync(f, s);
console.log('patched ' + f);
