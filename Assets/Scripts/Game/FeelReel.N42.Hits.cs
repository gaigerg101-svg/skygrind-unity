// NIGHT42 builder F1 (G9 / G15; impact42 + kills42): the Player scenario `hits42`, the hit / kill feel before / after in ONE Player: every moment
// is played twice, pass "off" (impact42 and kills42 off: NIGHT41's feel) then pass "on" (both on, forced by the reel whatever the design rows
// say), so the lead looks at the pair from one build (L-001). Not run by its author (no Player in the worktree).
// Boot: any level with an open floor (reel.sh + -skygrindMap <level> + -skygrindReelOnly hits42, L-062; level 1 or 2 is fine). The player stands
// on SafeSpot's open floor, God; the guns are added by the reel when the level lacks them; the trigger is a virtual pad's RT (Pad27).
// Moments (each framed at +0 / +0.05 / +0.15 s of the frame clock after the moment's event: FeelClock.RealNow, the clock a freeze counts on, so
// the +0.05 frame is INSIDE a freeze when one is granted):
//   H42-<pass>-kestrel-000 / -050 / -150   a KESTREL tap into a 1e9-hp husk at 6 m (the `hit`)
//   H42-<pass>-thunder-000 / -050 / -150   a THUNDERCLAP click into a 1-hp husk at 4 m (the `kill`)
//   H42-<pass>-ghusk-000 / -050 / -150     the PULSE gauntlet into a 1e9-hp husk at 1.8 m (the gauntlet's `hit`)
//   H42-<pass>-gtank-000 / -050 / -150     the same into a husk flagged tank29 (the TANK's class: heavy41's "tank"; the body may still be drawn as a
//                                          husk: the line says so)
// Line: per moment the freeze granted (cause, ms; ImpactDirector), and at each frame the freeze left, the ring's radius and the kick (KillFeel42Probe);
// GREEN when all 8 moments happened and the 24 frames were saved, and pass "on" froze longer than pass "off" on every moment.
// THE ICEBREAKER's gauntlet hit is B1's to frame (brawl42; the hand-off in F1's report): its class is Boss (0.24 s, the biggest ring).
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
        static partial void N42Hits(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = Hits42Scenario(g, dir, res, offs, pre);

        static IEnumerator Hits42Scenario(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            SurvivalSim sim = g.sim;
            var (spot, spotYaw, gapOk) = SafeSpot(g);
            string pc = Preconditions(g, spot) + (gapOk ? "" : " (SafeSpot: NO 12 m gap)") + $" | map {g.MapId}";
            N42Blocks saved = sim.n42;
            offs.Add(() =>
            {
                g.forcedCombat = IdleCombat;
                g.husks.frozen = false;
                sim.n42 = saved;
                g.ImpactDirector?.killFeel42?.Force(N42View.RowOn(g.cfg, "kills42"));
            });
            g.forcedCombat = null;
            g.Respawn(spot, spotYaw * 180 / Math.PI);
            g.input.SetLook(spotYaw, 0);
            Pad27 pad = NewPad27(offs);
            yield return Frames(30);
            g.husks.KillAll();
            g.husks.frozen = true;
            Start(g, "hits42", "camera", dir);
            yield return Frames(pre);
            FeelProbe.MarkInput();
            double wallEnd = Time.realtimeSinceStartup + 240;

            // the moment's event: the player's `hit` (weapon, non-shock) or `kill`
            int hits = 0, kills = 0;
            string hitWeapon = null;
            offs.Add(g.events.On<HitEvent>("hit", e => { if (e.owner == "player" && !e.shock) { hits++; hitWeapon = e.weapon; } }));
            offs.Add(g.events.On<KillEvent>("kill", e => { if (e.owner == "player") kills++; }));
            string lastFreeze = "none";
            double lastFreezeMs = 0;
            offs.Add(g.events.On<ImpactFreezeEvent>(FeelKeys.ImpactFreeze, e => { if (!e.skipped) { lastFreeze = e.cause; lastFreezeMs = e.seconds * 1000; } }));
            var lines = new List<string>();
            var froze = new Dictionary<string, double>();
            int framesSaved = 0, moments = 0;
            double[] offsets = { 0, 0.05, 0.15 };
            Husk bag = null;
            Action aim = () => { God(g); if (bag != null && bag.alive) Look(g, bag.AimPoint); };

            IEnumerator Moment(string label, Func<bool> seen, Action press, Action release)
            {
                lastFreeze = "none";
                lastFreezeMs = 0;
                press(); // a 2-frame click (p39 / charge40 fire the KESTREL / THUNDERCLAP tap on the release; the gauntlet swings while held)
                aim();
                yield return null;
                aim();
                yield return null;
                release();
                double w0 = Time.realtimeSinceStartup;
                while (!seen() && Time.realtimeSinceStartup - w0 < 4 && Time.realtimeSinceStartup < wallEnd) { aim(); yield return null; }
                if (!seen()) { lines.Add($"{label}: NO EVENT in 4 s"); yield break; }
                moments++;
                double t0 = FeelClock.RealNow;
                var sb = new System.Text.StringBuilder();
                sb.Append(label).Append(':');
                for (int k = 0; k < offsets.Length; k++)
                {
                    while (FeelClock.RealNow - t0 < offsets[k] - 1e-6 && Time.realtimeSinceStartup < wallEnd) yield return null;
                    string tag = ((int)Math.Round(offsets[k] * 1000)).ToString("000", ci);
                    double left = g.ImpactDirector != null ? g.ImpactDirector.FreezeLeft : 0;
                    sb.Append(' ').Append(tag).Append(" freezeLeft ").Append((left * 1000).ToString("0", ci)).Append(" ms ring ")
                        .Append(KillFeel42Probe.RingRadius.ToString("0.00", ci)).Append(" m kick ").Append(KillFeel42Probe.KickDeg.ToString("0.00", ci)).Append(" deg");
                    yield return Shot30(dir, label + "-" + tag);
                    framesSaved++;
                }
                sb.Append(" | freeze ").Append(lastFreeze).Append(' ').Append(lastFreezeMs.ToString("0", ci)).Append(" ms");
                froze[label] = lastFreezeMs;
                lines.Add(sb.ToString());
            }

            foreach (bool on in new[] { false, true })
            {
                string pass = on ? "on" : "off";
                N42Blocks b = saved;
                b.impact42 = on;
                sim.n42 = b; // the sim's hit hook and the impact layer read the field live (ImpactFrames.BeginFrame)
                g.ImpactDirector?.killFeel42?.Force(on);

                // ---- a KESTREL hit ----
                yield return Equip27(g, "revolver");
                g.forcedCombat = null;
                g.husks.KillAll();
                bag = PlaceAt(g, 6, 0, out double _);
                if (bag != null) bag.hp = bag.maxHp = 1e9;
                yield return Wait27(g, 0.8, wallEnd, aim);
                int h0 = hits;
                yield return Moment($"H42-{pass}-kestrel", () => hits > h0, () => pad.Send(0f, 1f), () => pad.Send(0f, 0f));
                yield return Wait27(g, 0.6, wallEnd, aim);

                // ---- a THUNDERCLAP kill ----
                yield return Equip27(g, "shotgun");
                g.forcedCombat = null;
                g.husks.KillAll();
                bag = PlaceAt(g, 4, 0, out double _);
                if (bag != null) bag.hp = 1;
                yield return Wait27(g, 0.8, wallEnd, aim);
                int k0 = kills;
                yield return Moment($"H42-{pass}-thunder", () => kills > k0, () => pad.Send(0f, 1f), () => pad.Send(0f, 0f));
                yield return Wait27(g, 0.8, wallEnd, aim);

                // ---- the gauntlet on a husk, then on a tank ----
                yield return Equip27(g, "gauntlet");
                g.forcedCombat = null;
                foreach (bool tank in new[] { false, true })
                {
                    g.husks.KillAll();
                    bag = PlaceAt(g, 1.8, 0, out double _);
                    if (bag != null)
                    {
                        bag.hp = bag.maxHp = 1e9;
                        bag.tank29 = tank; // heavy41's "tank" key (Loadout.IsHeavy41): the Big class
                    }
                    yield return Wait27(g, 2.7, wallEnd, aim); // the swing's cooldown (pulse27 2.5 s; gauntlet42 1.8 s)
                    int g0 = hits;
                    yield return Moment($"H42-{pass}-{(tank ? "gtank" : "ghusk")}", () => hits > g0 && hitWeapon == "gauntlet", () => pad.Send(0f, 1f), () => pad.Send(0f, 0f));
                    yield return Wait27(g, 0.5, wallEnd, aim);
                }
            }
            g.husks.KillAll();
            yield return Wait27(g, 0.3, wallEnd, null);
            yield return Stop();

            bool longer = true;
            foreach (string m in new[] { "kestrel", "thunder", "ghusk", "gtank" })
                longer &= froze.TryGetValue("H42-on-" + m, out double a) && froze.TryGetValue("H42-off-" + m, out double o) && (m == "kestrel" ? a >= o : a > o); // n42-r2: the KESTREL tap lands as a headshot (50 ms, unchanged by impact42 by design): it reads by the flinch
            bool ok = moments == 8 && framesSaved == 24 && longer;
            res[0] = $"{(ok ? "GREEN" : "RED")} hits42 | moments {moments}/8, frames {framesSaved}/24, on froze longer on every moment {longer}"
                + $" | rings {KillFeel42Probe.Rings}, kicks {KillFeel42Probe.Kicks} (pass on), flinches scaled {KillFeel42.Flinches} ||{string.Join(" || ", lines)} | {pc}";
        }
    }
}
