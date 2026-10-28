// NIGHT46 builder V1 (design.n46.feel46; Research/2026-10-17-wave46/V1.md G1-G5): reel phase a546, the feel46 off / on pairs in ONE Player
// (L-001; DECIDE46 N-4: the lead and a reviewer pick the stronger frame blind). Every moment is played twice, pass "off" (Feel46View.Force
// false: NIGHT45's picture) then pass "on" (Force true), from the same spot, the same look, the same placed husk, framed at the same frame-
// clock offsets after its event (FeelClock.RealNow). Not run by its author (no Player in the worktree).
// Boot: the SURVIVAL arena is enough (reel.sh + -skygrindReelOnly a546, no -skygrindMap; n46-r1 ran it with -skygrindMap n46-foundry, which
// also boots SURVIVAL on that map: L-062). It needs no campaign room: every moment is placed at SafeSpot, each husk at the first clear
// distance of 8 / 6 / 5 / 4 / 3 m (n46-r1: 8 m found no clear floor). God; the guns are
// equipped by Equip27 (added when the level lacks them); the trigger is a virtual pad's RT (Pad27). The husks are frozen: each moment's
// event is real where a shot can make it (hit, kill, pump, pumpkill) and raised by the reel where it needs an enemy's timing (the wind-up
// behind: a 1.2 s huskMelee tell on a frozen husk through the sim's Tells36; the parry and the hurt: a synthetic "parry" / "hurt" on the
// bus, labelled "synthetic", so the view's answer is framed without a fight).
// Frames (dir/shot-N46-<name>.png), name V46-<pass>-<moment>-<ms after the event>:
//   V46-<pass>-behind-050 / -600   a husk 3 m behind the right shoulder winding up (G1: the edge chevron on "on")
//   V46-<pass>-hit-000 / -050      a KESTREL tap into a 1e9-hp husk at 8 m (G2: the hit's kick)
//   V46-<pass>-kill-000 / -050 / -150  a KESTREL tap killing a 1-hp husk at 8 m (G3: the body kill's kick + FOV punch-in)
//   V46-<pass>-pump-050            a THUNDERCLAP tap at nothing (G4: the shot's shake, cut on "on")
//   V46-<pass>-pumpkill-050        a THUNDERCLAP tap killing a 1-hp husk at 6 m (G4: the kill's shake, the same in both)
//   V46-<pass>-parry-020           a synthetic parry (G5: the white flash in the edge band + ring on "on", the whole HUD on "off")
//   V46-<pass>-hurt-100            a synthetic 30-hp hurt from a husk ahead (G5: juice38's static in the edge band on "on")
//   V46-<pass>-aim41-000 / -800    aim41's pad pair (pass off: AimAssist.weakPoint41 false; on: true + design.n41.aim41's reach / acquire): a
//                                  synthetic weak point on a frozen husk at 8 m, the look started 4 deg right of its centre; the line gives the
//                                  look's angle off the weak point and off the centre after 0.8 s, and the lock
//   S41-off-select / S41-on-select the level select with design.n41.select41 off, then on (the run's config value flipped, put back after)
// Measured per frame: the camera's pitch (deg, + up) and vertical FOV, the screen change against the frame before the moment (mean |delta
// luma| on a 160 x 90 grid), and on the "on" pass the pair against "off": all / the centre half / the edge band (outside the central 72 %).
// Line: GREEN when every moment happened, every frame was saved, and the on pass's probe counted each fix (markers shown, a hit kick, a kill
// kick, an FOV punch, a heavy shot cut, the hurt static in the band). The parry flash is reported, not required: with hud33 on its layer is
// hidden (alpha 0), so today's game draws no parry flash at all (n46-r1's V46-off-parry-020 shows none). The verdict is the lead's frames.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N46Feel(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = Shift46View.ReelFlag ? Shift46Scenario(g, dir, res, offs, pre) : Feel46Scenario(g, dir, res, offs, pre); // -skygrindShift46: the leftover row shift46's frames (FeelReel.N46.Shift.cs)

        const int Grid46W = 160, Grid46H = 90;

        /// <summary>The reel's raised wind-up's attack id: NOT a scan id (SurvivalSim.IsScanTell compares by reference), so the husk scan does not end
        /// it on the next step (n46-r1: a raised "huskMelee" on a frozen husk ended at once and no marker was drawn).</summary>
        static readonly string Reel46WindId = "reel46Wind";

        /// <summary>Saves dir/shot-N46-name.png; fills `now` (luma grid) and diff = (mean |now - baseline|).</summary>
        static IEnumerator Shot46(string dir, string name, float[] baseline, float[] now, double[] diff)
        {
            yield return new WaitForEndOfFrame();
            Texture2D t = null;
            diff[0] = -1;
            try
            {
                t = ScreenCapture.CaptureScreenshotAsTexture();
                if (t == null) yield break;
                Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir, "shot-N46-" + name + ".png"), t.EncodeToPNG());
                Luma43(t, now);
                double sum = 0;
                for (int i = 0; i < now.Length; i++) sum += Math.Abs(now[i] - baseline[i]);
                diff[0] = sum / now.Length;
            }
            finally { if (t != null) UnityEngine.Object.Destroy(t); }
            // NIGHT71 F3: with -skygrindSameRun63 <night.row> (e.g. n71.kicks71), the same instant held with the row off beside this shot (the
            // same-run SPOT pair, FeelReel.N63Same.cs); without the option nothing runs
            if (diff[0] >= 0) yield return SameRun63(dir, "shot-N46-" + name + ".png");
        }

        /// <summary>Mean |a - b| luma over the whole grid (0), the centre half each way (1) and the edge band outside Edge46From (2).</summary>
        static void Pair46(float[] a, float[] b, double[] o)
        {
            double all = 0, mid = 0, edge = 0;
            int nAll = 0, nMid = 0, nEdge = 0;
            for (int gy = 0; gy < Grid46H; gy++)
                for (int gx = 0; gx < Grid46W; gx++)
                {
                    int i = gy * Grid46W + gx;
                    double d = Math.Abs(a[i] - b[i]);
                    all += d;
                    nAll++;
                    float sx = Math.Abs((gx + 0.5f) / Grid46W * 2 - 1), sy = Math.Abs((gy + 0.5f) / Grid46H * 2 - 1);
                    if (sx < 0.5f && sy < 0.5f) { mid += d; nMid++; }
                    if (Math.Max(sx, sy) >= UnityTuning.Edge46From) { edge += d; nEdge++; }
                }
            o[0] = nAll > 0 ? all / nAll : 0;
            o[1] = nMid > 0 ? mid / nMid : 0;
            o[2] = nEdge > 0 ? edge / nEdge : 0;
        }

        /// <summary>The camera's pitch in degrees, + looking up (Unity's euler x is + down).</summary>
        static double Pitch46(SurvivalGame g)
        {
            float x = g.cam.transform.eulerAngles.x;
            return -(x > 180 ? x - 360 : x);
        }

        static IEnumerator Feel46Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)") + $" | map {g.MapId}";
            bool? forceWas = Feel46View.Force;
            offs.Add(() =>
            {
                Feel46View.Force = forceWas;
                g.forcedCombat = IdleCombat;
                g.husks.frozen = false;
            });
            g.forcedCombat = null;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            Pad27 pad = NewPad27(offs);
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = true;
            God(g);
            Start(g, "a546-feel46", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 300;

            int hits = 0, kills = 0, fires = 0;
            offs.Add(g.events.On<HitEvent>("hit", e => { if (e.owner == "player" && !e.killed) hits++; }));
            offs.Add(g.events.On<KillEvent>("kill", e => { if (e.owner == "player") kills++; }));
            offs.Add(g.events.On("fire", _ => fires++));

            var lines = new List<string>();
            var offGrids = new Dictionary<string, float[]>();
            var pairs = new Dictionary<string, double[]>();
            var onProbe = new List<string>();
            int framesSaved = 0, framesWanted = 0, moments = 0, momentsWanted = 0;
            var baseline = new float[Grid46W * Grid46H];
            var diff = new double[1];
            Husk bag = null;
            double placedAt = 0; // m: where the moment's husk stood (the first clear distance tried)
            Vec3 lookAt = default;
            bool lookAtSet = false;
            Action aim = () =>
            {
                God(g);
                if (bag != null && bag.alive && lookAtSet) Look(g, lookAt);
                else if (bag != null && bag.alive) Look(g, bag.AimPoint);
            };

            IEnumerator Frames46(string pass, string moment, double t0, double[] offsets, System.Text.StringBuilder sb)
            {
                for (int k = 0; k < offsets.Length; k++)
                {
                    framesWanted++;
                    while (FeelClock.RealNow - t0 < offsets[k] - 1e-6 && Time.realtimeSinceStartup < wallEnd) { aim(); yield return null; }
                    string tag = ((int)Math.Round(offsets[k] * 1000)).ToString("000", ci);
                    double pitch = Pitch46(g), fov = g.cam.fieldOfView;
                    var grid = new float[Grid46W * Grid46H];
                    yield return Shot46(dir, $"V46-{pass}-{moment}-{tag}", baseline, grid, diff);
                    if (diff[0] >= 0) framesSaved++;
                    string key = moment + "-" + tag;
                    sb.Append(' ').Append(tag).Append(" pitch ").Append(pitch.ToString("0.00", ci)).Append(" fov ").Append(fov.ToString("0.00", ci))
                        .Append(" change ").Append(diff[0].ToString("0.0000", ci));
                    if (pass == "off") offGrids[key] = grid;
                    else if (offGrids.TryGetValue(key, out float[] og))
                    {
                        var p = new double[3];
                        Pair46(grid, og, p);
                        pairs[key] = p;
                        sb.Append(" pair all ").Append(p[0].ToString("0.0000", ci)).Append(" mid ").Append(p[1].ToString("0.0000", ci)).Append(" edge ").Append(p[2].ToString("0.0000", ci));
                    }
                    sb.Append(';');
                }
            }

            IEnumerator Place(string gun, double d, double hp)
            {
                yield return Equip27(g, gun);
                g.forcedCombat = null;
                g.Respawn(spot, spotYaw * 180 / Math.PI);
                g.input.SetLook(spotYaw, 0); // the same view in both passes
                g.husks.KillAll();
                lookAtSet = false;
                bag = null;
                if (d > 0) foreach (double dd in new[] { d, d - 2, d - 3, 4.0, 3.0 }) { if (dd < 2.5) continue; bag = PlaceAt(g, dd, 0, out double _); if (bag != null) { placedAt = dd; break; } } // n46-r1: 8 m found no clear floor on n46-foundry's spawn
                if (bag != null) bag.hp = bag.maxHp = hp;
                yield return Wait27(g, 0.9, wallEnd, aim); // the KESTREL's tap interval (0.483 s) and the THUNDERCLAP's pump have passed
            }

            // a tap: pressed one frame, released the next (p39 / charge40: a tap fires on its release)
            IEnumerator Tap()
            {
                pad.Send(0f, 1f);
                yield return null;
                yield return null;
                pad.Send(0f, 0f);
            }

            foreach (bool on in new[] { false, true })
            {
                string pass = on ? "on" : "off";
                Feel46View.Force = on;
                yield return Frames(2);
                Feel46Probe.Clear();

                // ---- G1: a husk 3 m behind the right shoulder winds up (a 1.2 s huskMelee tell, raised by the reel on the frozen husk) ----
                momentsWanted++;
                yield return Place("revolver", 3, 1e9);
                if (bag == null) lines.Add($"V46-{pass}-behind: NO HUSK PLACED");
                else
                {
                    // face away from it, the husk ~35 deg right of straight behind (an off-screen wind-up, the commonest unseen blow: n46feel unseen)
                    Vec3 to = bag.position - g.Feet;
                    double yawTo = Math.Atan2(-to.x, -to.z);
                    double away = yawTo + Math.PI - 35 * Math.PI / 180;
                    g.input.SetLook(away, 0);
                    lookAt = g.Eye + new Vec3(-Math.Sin(away) * 10, 0, -Math.Cos(away) * 10);
                    lookAtSet = true;
                    yield return Wait27(g, 0.5, wallEnd, aim);
                    yield return Grab43(baseline);
                    Tell36 t = sim.tells36 != null && sim.tells36.enabled
                        ? sim.tells36.Raise(bag.id, Reel46WindId, Tell36Kind.Red, MovementMath.StepsFor(1.2, sim.step), bag.AimPoint) : null;
                    if (t == null) lines.Add($"V46-{pass}-behind: NO TELL (tells36 {(sim.tells36 != null && sim.tells36.enabled ? "refused" : "off")})");
                    else
                    {
                        moments++;
                        double t0 = FeelClock.RealNow;
                        var sb = new System.Text.StringBuilder($"V46-{pass}-behind (raised, 1.2 s):");
                        yield return Frames46(pass, "behind", t0, new[] { 0.05, 0.6 }, sb);
                        sb.Append(" markers ").Append(Feel46Probe.MarkersNow);
                        lines.Add(sb.ToString());
                        sim.tells36.End(bag.id);
                    }
                    lookAtSet = false;
                }
                yield return Wait27(g, 0.4, wallEnd, aim);

                // ---- G2: a KESTREL tap into a 1e9-hp husk at 8 m ----
                momentsWanted++;
                yield return Place("revolver", 8, 1e9);
                yield return Grab43(baseline);
                if (bag != null)
                {
                    int h0 = hits;
                    yield return Tap();
                    double w0 = Time.realtimeSinceStartup;
                    while (hits == h0 && Time.realtimeSinceStartup - w0 < 1.5) { aim(); yield return null; }
                    if (hits == h0) lines.Add($"V46-{pass}-hit: NO HIT in 1.5 s (weapon {g.loadout.Weapon})");
                    else
                    {
                        moments++;
                        var sb = new System.Text.StringBuilder($"V46-{pass}-hit ({g.loadout.Weapon}, husk at {placedAt} m):");
                        yield return Frames46(pass, "hit", FeelClock.RealNow, new[] { 0, 0.05 }, sb);
                        lines.Add(sb.ToString());
                    }
                }
                else lines.Add($"V46-{pass}-hit: NO HUSK PLACED");

                // ---- G3: a KESTREL tap killing a 1-hp husk at 8 m ----
                momentsWanted++;
                yield return Place("revolver", 8, 1);
                yield return Grab43(baseline);
                if (bag != null)
                {
                    int k0 = kills;
                    yield return Tap();
                    double w0 = Time.realtimeSinceStartup;
                    while (kills == k0 && Time.realtimeSinceStartup - w0 < 1.5) { aim(); yield return null; }
                    if (kills == k0) lines.Add($"V46-{pass}-kill: NO KILL in 1.5 s");
                    else
                    {
                        moments++;
                        var sb = new System.Text.StringBuilder($"V46-{pass}-kill (husk at {placedAt} m):");
                        yield return Frames46(pass, "kill", FeelClock.RealNow, new[] { 0, 0.05, 0.15 }, sb);
                        lines.Add(sb.ToString());
                    }
                }
                else lines.Add($"V46-{pass}-kill: NO HUSK PLACED");

                // ---- G4: a THUNDERCLAP tap at nothing, then one killing a 1-hp husk at 6 m ----
                momentsWanted++;
                yield return Place("shotgun", 0, 1);
                yield return Grab43(baseline);
                {
                    int f0 = fires;
                    yield return Tap();
                    double w0 = Time.realtimeSinceStartup;
                    while (fires == f0 && Time.realtimeSinceStartup - w0 < 1.5) yield return null;
                    if (fires == f0) lines.Add($"V46-{pass}-pump: NO SHOT in 1.5 s (weapon {g.loadout.Weapon})");
                    else
                    {
                        moments++;
                        var sb = new System.Text.StringBuilder($"V46-{pass}-pump ({g.loadout.Weapon}):");
                        yield return Frames46(pass, "pump", FeelClock.RealNow, new[] { 0.05 }, sb);
                        lines.Add(sb.ToString());
                    }
                }
                momentsWanted++;
                yield return Place("shotgun", 6, 1);
                yield return Grab43(baseline);
                if (bag != null)
                {
                    int k0 = kills;
                    yield return Tap();
                    double w0 = Time.realtimeSinceStartup;
                    while (kills == k0 && Time.realtimeSinceStartup - w0 < 1.5) { aim(); yield return null; }
                    if (kills == k0) lines.Add($"V46-{pass}-pumpkill: NO KILL in 1.5 s");
                    else
                    {
                        moments++;
                        var sb = new System.Text.StringBuilder($"V46-{pass}-pumpkill (husk at {placedAt} m):");
                        yield return Frames46(pass, "pumpkill", FeelClock.RealNow, new[] { 0.05 }, sb);
                        lines.Add(sb.ToString());
                    }
                }
                else lines.Add($"V46-{pass}-pumpkill: NO HUSK PLACED");

                // ---- G5: a synthetic parry (UiFeel's white flash) and a synthetic 30-hp hurt from ahead (juice38's static) ----
                momentsWanted++;
                yield return Place("revolver", 0, 1);
                yield return Wait27(g, 1.2, wallEnd, null); // the flash gate's second (FlashGate: three a second)
                yield return Grab43(baseline);
                {
                    g.events.Emit("parry", new HitEvent { owner = "player", weapon = "punch", kind = "drone", enemy = -1, perfect = false, position = g.Eye + new Vec3(-Math.Sin(spotYaw) * 3, 0, -Math.Cos(spotYaw) * 3) });
                    moments++;
                    var sb = new System.Text.StringBuilder($"V46-{pass}-parry (synthetic):");
                    yield return Frames46(pass, "parry", FeelClock.RealNow, new[] { 0.02 }, sb);
                    lines.Add(sb.ToString());
                }
                yield return Wait27(g, 1.5, wallEnd, null); // the parry's freeze and flash are over; juice38's hurt cooldown is per source
                momentsWanted++;
                yield return Grab43(baseline);
                {
                    Vec3 from = g.Feet + new Vec3(-Math.Sin(spotYaw) * 2, 0, -Math.Cos(spotYaw) * 2);
                    g.events.Emit("hurt", new HurtEvent { owner = "player", amount = 30, hp = g.health.hp, source = "husk", from = from, hasFrom = true });
                    moments++;
                    var sb = new System.Text.StringBuilder($"V46-{pass}-hurt (synthetic):");
                    yield return Frames46(pass, "hurt", FeelClock.RealNow, new[] { 0.1 }, sb);
                    sb.Append(" static ").Append(Feel46Probe.StaticNow.ToString("0.00", ci)).Append(" max ").Append(Feel46Probe.StaticMax.ToString("0.00", ci));
                    lines.Add(sb.ToString());
                }
                // ---- aim41 (DECIDE46: judged on a pad frame pair): the pad's ASSIST on a weak point. A synthetic weak point on a frozen 1e9-hp
                // husk at 8 m (0.45 m right of its centre, at knee height: THE LOOM's lit knee's place on a body), the look started 4 deg right
                // of the centre, then left alone for 0.8 s; pass off = AimAssist.weakPoint41 false (centre mass), on = true with design.n41.aim41's
                // reach / acquire (what N41AimStartRun sets with the row on). PlayerDevices.Last held on Pad (the assist's breakaway cap is the pad's).
                momentsWanted++;
                yield return Place("revolver", 8, 1e9);
                if (bag == null || sim.assist == null) lines.Add($"A41-{pass}-lock: {(bag == null ? "NO HUSK PLACED" : "NO ASSIST")}");
                else
                {
                    AimAssist asst = sim.assist;
                    bool weakWas = asst.weakPoint41;
                    double reachWas = asst.weakReach41, acqWas = asst.weakAcquire41;
                    InputDevice devWas = PlayerDevices.Last;
                    const string R41 = "design.n41.aim41.";
                    asst.weakPoint41 = on;
                    asst.weakReach41 = on && g.cfg.Has(R41 + "reachM") ? g.cfg.Num(R41 + "reachM") : 0;
                    asst.weakAcquire41 = on && g.cfg.Has(R41 + "acquireScale") ? g.cfg.Num(R41 + "acquireScale") : 1;
                    Husk body = bag;
                    double yaw0 = g.input.yaw;
                    Vec3 Knee() => body.position + new Vec3(Math.Cos(yaw0) * 0.45, 0.55, -Math.Sin(yaw0) * 0.45);
                    Action was = aim;
                    aim = () =>
                    {
                        God(g);
                        PlayerDevices.Last = InputDevice.Pad;
                        if (body.alive) body.weakPoint41.Set(Knee());
                    };
                    aim();
                    Look(g, body.AimPoint, 4);
                    yield return Frames(2);
                    yield return Grab43(baseline);
                    moments++;
                    var sb = new System.Text.StringBuilder($"A41-{pass}-lock (aim41 {(on ? "on" : "off")}, synthetic weak point):");
                    yield return Frames46(pass, "aim41", FeelClock.RealNow, new[] { 0, 0.8 }, sb);
                    var (eye, look) = sim.Aim();
                    double Deg(Vec3 p)
                    {
                        Vec3 d = p - eye;
                        double len = d.Length;
                        if (len < 1e-9) return 0;
                        double c = (d.x * look.x + d.y * look.y + d.z * look.z) / len;
                        return Math.Acos(Math.Max(-1, Math.Min(1, c))) * 180 / Math.PI;
                    }
                    sb.Append(" | after 0.8 s: off the weak point ").Append(Deg(Knee()).ToString("0.00", ci)).Append(" deg, off the centre ")
                        .Append(Deg(body.AimPoint).ToString("0.00", ci)).Append(" deg, locked ").Append(asst.Target != null ? "yes" : "no")
                        .Append(", device ").Append(PlayerDevices.Last);
                    lines.Add(sb.ToString());
                    aim = was;
                    body.weakPoint41.Clear();
                    asst.weakPoint41 = weakWas;
                    asst.weakReach41 = reachWas;
                    asst.weakAcquire41 = acqWas;
                    PlayerDevices.Last = devWas;
                }

                string probe = Feel46Probe.Line();
                lines.Add($"V46-{pass} probe: {probe}");
                if (on) onProbe.Add(probe);
                yield return Wait27(g, 1.2, wallEnd, null); // the static decays before the next pass
            }
            g.husks.KillAll();
            yield return Wait27(g, 0.3, wallEnd, null);
            // the "on" pass's probe must show each fix answered (the frames are the verdict; this says each fix ran); read before the select
            // round 4: the kicks (G2-G4) are required only when they run (design.n46.feel46.numbers.kicks, or ForceKicks); with them off the on pass
            // must show none (nothing of them leaks)
            bool kicks46 = Feel46View.KicksOn(g);
            bool kicksOk = kicks46
                ? Feel46Probe.HitKicks > 0 && Feel46Probe.KillKicks > 0 && Feel46Probe.FovPunches > 0 && Feel46Probe.HeavyShotsCut > 0
                : Feel46Probe.HitKicks == 0 && Feel46Probe.KillKicks == 0 && Feel46Probe.FovPunches == 0 && Feel46Probe.HeavyShotsCut == 0;
            bool ran = Feel46Probe.MarkersShown > 0 && Feel46Probe.StaticMax > 0 && kicksOk; // the parry flash is not required: hud33 hides its layer today (Feel46View.Parry)
            lines.Add($"feel46 kicks {(kicks46 ? "on" : "off")} (design.n46.feel46.numbers.kicks{(Feel46View.ForceKicks.HasValue ? ", forced" : "")}; design.n71.kicks71 {(Kicks71.On(g.cfg) ? "on" : "off: " + Kicks71.OffReason(g.cfg, Environment.GetCommandLineArgs()))}): the kicks' counts {(kicksOk ? "as expected" : "WRONG")}");
            yield return Stop();

            // ---- select41 (DECIDE46: ON only if the frame reads): the level select with design.n41.select41 off, then on. The row is read live
            // on each refresh (UiLevelSelect41.Select41On); the reel flips the run's config value (and its read cache) and puts it back after.
            momentsWanted += 2;
            {
                Dictionary<string, object> row = null;
                try { row = (Dictionary<string, object>)((Dictionary<string, object>)((Dictionary<string, object>)g.cfg.root["design"])["n41"])["select41"]; }
                catch (Exception) { row = null; }
                var cacheF = typeof(GameConfig).GetField("cache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (row == null || cacheF == null) lines.Add("S41: NO design.n41.select41 row (or no config cache) to flip");
                else
                {
                    object selWas = row.TryGetValue("enabled", out object sw) ? sw : false;
                    void SetSel(object v)
                    {
                        row["enabled"] = v;
                        (cacheF.GetValue(g.cfg) as Dictionary<string, object>)?.Remove("design.n41.select41.enabled");
                    }
                    offs.Add(() => SetSel(selWas));
                    foreach (bool sel in new[] { false, true })
                    {
                        SetSel(sel);
                        g.ToTitle();
                        yield return Frames(60);
                        g.setup.mode = RunSetup.Campaign;
                        bool pressed = g.UiView != null && g.UiView.ReelPressPlay26();
                        yield return Frames(Ms(900));
                        string pass = sel ? "on" : "off";
                        var sel46 = new float[Grid46W * Grid46H];
                        Array.Clear(baseline, 0, baseline.Length);
                        framesWanted++;
                        yield return Shot46(dir, $"S41-{pass}-select", baseline, sel46, diff);
                        if (diff[0] >= 0) framesSaved++;
                        if (pressed && Ui.LevelSel26Open) moments++;
                        string rows = string.Join(" ;; ", Ui.LevelSel26RowsNow.GetRange(0, Math.Min(6, Ui.LevelSel26RowsNow.Count)));
                        lines.Add($"S41-{pass}-select: select41 {(N41View.RowOn(g.cfg, "select41") ? "on" : "off")}, PLAY pressed {pressed}, open {Ui.LevelSel26Open}, rows [{rows}]");
                    }
                    SetSel(selWas);
                }
            }
            bool ok = moments == momentsWanted && framesSaved == framesWanted && ran;
            var pl = new List<string>();
            foreach (KeyValuePair<string, double[]> kv in pairs)
                pl.Add($"{kv.Key} {kv.Value[0].ToString("0.0000", ci)}/{kv.Value[1].ToString("0.0000", ci)}/{kv.Value[2].ToString("0.0000", ci)}");
            res[0] = $"{(ok ? "GREEN" : "RED")} a546 feel46 | moments {moments}/{momentsWanted}, frames {framesSaved}/{framesWanted}, every fix ran on the on pass {ran}"
                + $" | pairs all/mid/edge [{string.Join(", ", pl)}] ||{string.Join(" || ", lines)} | {pc}";
        }
    }
}
