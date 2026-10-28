// NIGHT44 builder V2 (design.n44.vis44, a view row): the Player scenario `look44`. Boot: reel.sh + -skygrindMap <level> + -skygrindReelOnly
// look44 (L-062; any level with an open floor: level 1 or 2). feel43 is forced ON in both passes; vis44 is forced OFF then ON (Vis44.Forced)
// at the same moments, from the same spot and view:
//   fusion +0.05 s  the FUSION's streaks (vis44: tapered curved streaks + the hot-pink / white glow at the hit before the pop)
//   torch  +1.0 s   the TORCH held on a husk at 4 m (vis44: a dense cone round the jet)
//   room   +0.4 s   a lesson line posted on the centre card, then the roomEnter handler (Ui.Vis44RoomEnter, called directly: this map has no
//                   rooms) 0.3 s later; the frame 0.4 s after it: vis44 off keeps the card (MinHold), vis44 on has no line (T-69)
// The pair: the mean |on - off| luma in the screen's centre (Pair43); GREEN when fusion and torch pair >= 0.02 each and the room frame with vis44
// on has no line in the slot (Hud36TeachNow empty and the slot's lesson dropped) while vis44 off still shows it.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN44V2 = { "look44" };

        static void N44Look(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = V2Look44Scenario(g, dir, res, offs, pre);

        const double V2PairMin = 0.02;

        static IEnumerator V2Look44Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            Feel43 feel = g.ImpactDirector != null ? g.ImpactDirector.feel43 : null;
            if (feel == null) { res[0] = "RED look44: no Feel43 (ImpactFrames not built)"; yield break; }
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)") + $" | map {g.MapId}";
            N43Blocks saved = sim.n43;
            N43Blocks? savedOverride = sim.n43Override;
            offs.Add(() =>
            {
                g.forcedCombat = IdleCombat;
                g.husks.frozen = false;
                sim.n43 = saved;
                sim.n43Override = savedOverride;
                feel.Force(N43View.RowOn(g.cfg, "feel43"));
                Vis44.Forced = null;
            });
            N43Blocks fused = saved;
            fused.fusion43 = true;
            sim.n43 = fused;
            sim.n43Override = fused;
            feel.Force(true); // feel43 ON in both passes: only vis44 differs
            g.forcedCombat = null;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            Pad27 pad = NewPad27(offs);
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = true;
            Start(g, "look44", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 300;

            int fusions = 0, flames = 0, gHits = 0;
            offs.Add(g.events.On<Fusion43Event>(N43Events.Fusion, e => { if (e.owner == "player") fusions++; }));
            offs.Add(g.events.On<FlameEvent>(G20Events.Flame, e => { if (e.owner == "player") flames++; }));
            offs.Add(g.events.On<HitEvent>("hit", e => { if (e.owner == "player" && e.weapon == "gauntlet") gHits++; }));

            var lines = new List<string>();
            var pair = new Dictionary<string, double>();
            var offGrids = new Dictionary<string, float[]>();
            var baseline = new float[Grid43W * Grid43H];
            var diff = new double[2];
            int framesSaved = 0, framesWanted = 0;
            Husk bag = null;
            Action aim = () => { God(g); if (bag != null && bag.alive) Look(g, bag.AimPoint); };
            string roomOff = "not reached", roomOn = "not reached";
            bool roomOk = false;

            IEnumerator Shot(string moment, bool on, double t0, double at)
            {
                framesWanted++;
                while (FeelClock.RealNow - t0 < at - 1e-6 && Time.realtimeSinceStartup < wallEnd) { aim(); yield return null; }
                var grid = new float[Grid43W * Grid43H];
                string tag = ((int)Math.Round(at * 1000)).ToString("000", ci);
                yield return Shot43(dir, $"V2-{(on ? "on" : "off")}-{moment}-{tag}", baseline, diff, grid);
                if (diff[0] >= 0) framesSaved++;
                if (!on) offGrids[moment] = grid;
                else if (offGrids.TryGetValue(moment, out float[] og)) pair[moment] = Pair43(grid, og);
            }

            foreach (bool on in new[] { false, true })
            {
                string pass = on ? "on" : "off";
                Vis44.Forced = on;
                Feel43Probe.Clear();

                // ---- a FUSION: carried at 16 m/s into a husk, the gauntlet swung within reach (as feel43) ----
                yield return Equip27(g, "gauntlet");
                g.forcedCombat = null;
                g.Respawn(spot, spotYaw * 180 / Math.PI);
                g.input.SetLook(spotYaw, 0);
                g.husks.KillAll();
                bag = PlaceAt(g, 6, 0, out double _);
                if (bag != null) bag.hp = bag.maxHp = 1e9;
                yield return Wait27(g, 2.2, wallEnd, aim);
                yield return Grab43(baseline);
                if (bag != null)
                {
                    int f0 = fusions, h0 = gHits;
                    bool pressed = false, synthetic = false;
                    double w0 = Time.realtimeSinceStartup;
                    while (fusions == f0 && Time.realtimeSinceStartup - w0 < 1.5 && Time.realtimeSinceStartup < wallEnd)
                    {
                        aim();
                        Vec3 to = bag.position - g.Feet;
                        to.y = 0;
                        double d = to.Length;
                        if (d > 0.3 && gHits == h0) g.mover.velocity = new Vec3(to.x / d * 16, g.mover.velocity.y, to.z / d * 16);
                        if (!pressed && d < 2.3) { pad.Send(0f, 1f); pressed = true; }
                        yield return null;
                    }
                    pad.Send(0f, 0f);
                    if (fusions == f0)
                    {
                        synthetic = true;
                        var fake = new Fusion43Event { owner = "player", position = bag.AimPoint, dir = new Vec3(0, 0, -1), speed = 16, weapon = "gauntlet", enemy = bag.id, kind = "husk", killed = false, damage = 0 };
                        g.events.Emit(N43Events.Fusion, fake);
                    }
                    double t0 = FeelClock.RealNow;
                    yield return Shot("fusion", on, t0, 0.05);
                    lines.Add($"V2-{pass}-fusion ({(synthetic ? "synthetic" : "real")})");
                    g.mover.velocity = new Vec3(0, g.mover.velocity.y, 0);
                }
                else lines.Add($"V2-{pass}-fusion: NO HUSK PLACED");
                yield return Wait27(g, 1.0, wallEnd, aim);

                // ---- the TORCH held on a husk at 4 m ----
                yield return Equip27(g, "torch");
                g.forcedCombat = null;
                if (g.loadout.Weapon != "torch") lines.Add($"V2-{pass}-torch: SKIPPED (not in hand: {g.loadout.Weapon})");
                else
                {
                    g.Respawn(spot, spotYaw * 180 / Math.PI);
                    g.input.SetLook(spotYaw, 0);
                    g.husks.KillAll();
                    bag = PlaceAt(g, 4, 0, out double _);
                    if (bag != null) bag.hp = bag.maxHp = 1e9;
                    yield return Wait27(g, 0.8, wallEnd, aim);
                    yield return Grab43(baseline);
                    int fl0 = flames;
                    pad.Send(0f, 1f);
                    double w0 = Time.realtimeSinceStartup;
                    while (flames == fl0 && Time.realtimeSinceStartup - w0 < 2 && Time.realtimeSinceStartup < wallEnd) { aim(); yield return null; }
                    if (flames == fl0) lines.Add($"V2-{pass}-torch: SKIPPED (no flame tick in 2 s)");
                    else
                    {
                        double t0 = FeelClock.RealNow;
                        yield return Shot("torch", on, t0, 1.0);
                        lines.Add($"V2-{pass}-torch: ticks {flames - fl0}, jet sprites {TorchFeel43.JetDrawn}");
                    }
                    pad.Send(0f, 0f);
                }
                g.husks.KillAll();
                bag = null;
                yield return Wait27(g, 1.2, wallEnd, aim);

                // ---- the room change: a lesson on the centre card, then roomEnter 0.3 s later ----
                Ui ui = g.UiView;
                if (ui == null) lines.Add($"V2-{pass}-room: NO UI");
                else
                {
                    int drops0 = Vis44.RoomDrops;
                    ui.Vis44PostLesson("LOOK44 LESSON: THE LAST ROOM'S LINE", 4.0);
                    yield return Wait27(g, 0.3, wallEnd, null);
                    string before = Ui.Hud36TeachNow;
                    ui.Vis44RoomEnter();
                    double t0 = FeelClock.RealNow;
                    yield return Shot("room", on, t0, 0.4);
                    string after = Ui.Hud36TeachNow, slot = ui.Vis44SlotText;
                    string state = $"before '{before}' after '{after}' slot '{slot ?? "none"}' card {Teach43Probe.Shown} drops {Vis44.RoomDrops - drops0}";
                    if (on) roomOn = state; else roomOff = state;
                    if (on) roomOk = before.Length > 0 && slot == null && Vis44.RoomDrops - drops0 == 1;
                    yield return Wait27(g, 4.2, wallEnd, null); // the posted line runs out before the next pass
                }
            }
            Vis44.Forced = null;
            g.husks.KillAll();
            yield return Wait27(g, 0.3, wallEnd, null);
            yield return Stop();

            bool fOk = pair.TryGetValue("fusion", out double pf) && pf >= V2PairMin;
            bool tOk = pair.TryGetValue("torch", out double pt) && pt >= V2PairMin;
            bool ok = fOk && tOk && roomOk && framesSaved == framesWanted;
            res[0] = $"{(ok ? "GREEN" : "RED")} look44 | vis44 off vs on (feel43 on): fusion +0.05 pair {(pair.ContainsKey("fusion") ? pf.ToString("0.0000", ci) : "none")} (>= {V2PairMin.ToString("0.00", ci)}) {fOk}, "
                + $"torch +1.0 pair {(pair.ContainsKey("torch") ? pt.ToString("0.0000", ci) : "none")} {tOk}, room +0.4 no stale line with vis44 on {roomOk} [off: {roomOff}] [on: {roomOn}], frames {framesSaved}/{framesWanted}"
                + $" || {string.Join(" || ", lines)} | {pc}";
        }
    }
}
