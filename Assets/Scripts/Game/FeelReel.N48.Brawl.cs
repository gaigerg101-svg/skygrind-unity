// NIGHT48 builder B: the Player scenario a248 (level 2: THE ICEBREAKER's motion and hit tiers, G29 / G30), dispatched by FeelReel.N48.cs (the
// lead's routing). Not run by its author (no Player in a builder worktree). Boot: -skygrindMap n47-iceline -skygrindReelOnly a248 (reel.sh,
// L-062). Forced sim rows: charger47 + brawl47 (sim.rowsOverride before the run; put back after); the view row brawlLook47 is the build copy's
// (take the reel with it ON; once OFF for the before strip). The player is God; input by forced frames only (L-046; no button is pressed).
// Frames (dir/shot-N29-<name>.png), the camera 9 m from the body (pulled toward the arena's centre, never outside its wall: a247's r1e note):
//   G29  B48-brawl-<attack>-strip-00 .. -11   per attack (haymaker, slam, block, rush, sweep, leap: each forced once), 12 frames 10 sim steps
//        apart (1/12 s; 1.0 s in all) from 50 % of its wind-up: anticipation -> strike -> settle (the strip spans the tell's end and the strike)
//   G30  B48-brawl-<attack>-impact            the attack's strike 3 sim steps in (its own hit tier: light / heavy / critical, B-cards.md 3)
//        B48-brawl-flinch-00 .. -07           a gauntlet hit while it idles (a real hit through the fight's filter), 8 frames 2 steps apart
// Line: GREEN when every attack gave its 12 strip frames and its impact frame, and the flinch strip was taken. It prints each strip's first and
// last sim step, so the lead can read the frames' spacing (a frame's capture may cost a render frame; the sim steps are the clock).
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        static partial void N48Brawl(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body) =>
            body = Brawl48Reel(g, dir, res, offs, pre);

        /// <summary>God (hp held at 1e6) and never hurt: no "hurt" event, so none of the player's own hurt feedback fills the frame.</summary>
        static void Safe48(SurvivalGame g)
        {
            God(g);
            if (g.sim != null && g.sim.mover != null) g.sim.mover.invulnerable = true;
        }

        static IEnumerator Brawl48Reel(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre)
        {
            SurvivalSim sim = g.sim;
            NightRowState before = sim.rowsOverride;
            offs.Add(() => { g.forcedMove = null; g.forcedCombat = IdleCombat; g.sim.rowsOverride = before; });
            try { sim.rowsOverride = sim.rowsRegistry.Defaults().Set("charger47", true).Set("brawl47", true); }
            catch (Exception e) { res[0] = "RED a248: the build lacks the rows charger47 / brawl47 (" + e.Message + ")"; yield break; }
            var ok = new bool[1];
            yield return Campaign41(g, offs, true, ok);
            if (!ok[0]) { res[0] = "RED a248: no campaign run (" + (N24CampaignGate(g) ?? "the campaign did not start") + ")"; yield break; }
            RoomDef x9 = sim.campaignLevel?.rooms.Find(r => r.id == "x9");
            if (x9 == null || sim.campaignLevel.id != "n47-iceline") { res[0] = $"RED a248: the run is on {sim.campaignLevel?.id ?? "?"} (boot -skygrindMap n47-iceline through reel.sh, L-062)"; yield break; }
            g.forcedCombat = IdleCombat;
            Safe48(g);
            var st = new double[4];
            var shots = new List<string>();
            IEnumerator Shot(string name) { Wyrm46ClearCards(g); yield return null; Wyrm46ClearCards(g); yield return Shot29(dir, name, st); shots.Add(name); }
            Start(g, "a248", "camera", dir);
            yield return Frames(pre);
            g.Respawn(x9.spawn, 0);
            for (int f = 0; f < 600 && (sim.brawl40 == null || !sim.brawl40.Alive); f++)
            {
                Safe48(g);
                g.forcedMove = new InputFrame { yaw = 0, pitch = 0, moveZ = 1 };
                yield return null;
            }
            Brawl40 b = sim.brawl40;
            if (b == null || !b.Alive) { res[0] = "RED a248: THE ICEBREAKER's fight did not begin"; yield break; }
            void Face()
            {
                Vec3 d = b.Centre - (sim.mover.position + new Vec3(0, 1.6, 0));
                double hz = Math.Sqrt(d.x * d.x + d.z * d.z);
                Vec3 ac = sim.brawl40Centre ?? b.Centre;
                if ((hz < 7.5 || hz > 11) && hz > 1e-3)
                {
                    Vec3 at = new Vec3(b.Centre.x - d.x / hz * 9, 0, b.Centre.z - d.z / hz * 9);
                    double ox = at.x - ac.x, oz = at.z - ac.z, ol = Math.Sqrt(ox * ox + oz * oz);
                    if (ol > 14) at = new Vec3(ac.x + ox / ol * 14, 0, ac.z + oz / ol * 14);
                    g.Respawn(new Vec3(at.x, ac.y, at.z), 0);
                }
                d = b.Centre - (sim.mover.position + new Vec3(0, 1.6, 0));
                double y = Math.Atan2(-d.x, -d.z), p = Math.Atan2(d.y, Math.Sqrt(d.x * d.x + d.z * d.z));
                g.input.SetLook(y, p);
                g.forcedMove = new InputFrame { yaw = y, pitch = p };
                sim.mover.invulnerable = true; // after the respawn (Mover.Reset clears it)
            }
            // n48-r2 (the lead): the SLAM / BLOCK / RUSH impact frames were whole-screen washes: those three reach the camera 9 m away, and the
            // player's own hurt feedback (Juice38's static + flicker, Ui's hurt vignette border, PostFx's chroma kick) filled the view. The camera
            // is now never hurt (invulnerable, Safe48), and those three impacts are framed from the side of the strike, 10 m off its line, the
            // reticle on the strike area (the SLAM's ring at its feet, the BLOCK where it lands, the RUSH's body): -impact-local
            IEnumerator ImpactShot(string atk)
            {
                bool side = atk == Brawl40.Slam || atk == Brawl40.Block || atk == Brawl40.Rush;
                if (!side) { yield return Shot("B48-brawl-" + atk + "-impact"); yield break; }
                Vec3 P = sim.mover.position, C = b.body.position, ac = sim.brawl40Centre ?? C;
                double lx = P.x - C.x, lz = P.z - C.z, ll = Math.Sqrt(lx * lx + lz * lz);
                if (ll < 1e-3) { lx = 1; lz = 0; ll = 1; }
                Vec3 mid = new Vec3((C.x + P.x) * 0.5, ac.y, (C.z + P.z) * 0.5);
                Vec3 at = new Vec3(mid.x - lz / ll * 10, ac.y, mid.z + lx / ll * 10);
                double ox = at.x - ac.x, oz = at.z - ac.z, ol = Math.Sqrt(ox * ox + oz * oz);
                if (ol > 14) at = new Vec3(ac.x + ox / ol * 14, ac.y, ac.z + oz / ol * 14);
                Vec3 target = atk == Brawl40.Slam ? C + new Vec3(0, 0.3, 0) : atk == Brawl40.Block ? P + new Vec3(0, 1.0, 0) : b.Centre;
                g.Respawn(at, 0);
                sim.mover.invulnerable = true;
                Vec3 d = target - (sim.mover.position + new Vec3(0, 1.6, 0));
                double y = Math.Atan2(-d.x, -d.z), pch = Math.Atan2(d.y, Math.Sqrt(d.x * d.x + d.z * d.z));
                g.input.SetLook(y, pch);
                g.forcedMove = new InputFrame { yaw = y, pitch = pch };
                yield return null;
                yield return Shot("B48-brawl-" + atk + "-impact-local");
            }
            for (float t = 0; t < 2f; t += Time.unscaledDeltaTime) { Safe48(g); Face(); yield return null; }

            var lines = new List<string>();
            int strips = 0, impacts = 0, hurts0 = Juice38Probe.Hurts;
            var queue = new[] { Brawl40.Haymaker, Brawl40.Slam, Brawl40.Block, Brawl40.Rush, Brawl40.Sweep, Brawl40.Leap };
            foreach (string atk in queue)
            {
                if (!b.Alive) break;
                // wait for idle, force the attack, wait for its tell to reach 50 %
                for (float t = 0; t < 12f && b.Alive && !(b.state == Brawl40State.Tell && b.attack == atk && b.Progress >= 0.5); t += Time.unscaledDeltaTime)
                {
                    Safe48(g);
                    Face();
                    if (b.state == Brawl40State.Idle && !b.ForcePending) b.Force(atk);
                    yield return null;
                }
                if (!(b.state == Brawl40State.Tell && b.attack == atk)) { lines.Add(atk + " not told"); continue; }
                int s0 = b.stepNo, taken = 0, last = s0, tellEnd = -1, impactAt = -1;
                bool impact = false;
                // n48-r1: the impact frame waited for Active with stateSteps >= 3, but the SLAM and the BLOCK hold Active one step (Go(Active, 1)), so
                // it never came: the impact is now 3 sim steps after the tell ends, whatever the state (Active or Recover); its real offset is printed
                void Track() { if (tellEnd < 0 && b.state != Brawl40State.Tell) tellEnd = b.stepNo; }
                for (int k = 0; k < 12 && b.Alive; k++)
                {
                    for (int f = 0; f < 240 && b.stepNo < s0 + 10 * k; f++)
                    {
                        Safe48(g);
                        Face();
                        Track();
                        if (!impact && tellEnd >= 0 && b.stepNo >= tellEnd + 3) { impact = true; impactAt = b.stepNo - tellEnd; yield return ImpactShot(atk); impacts++; }
                        yield return null;
                    }
                    Track();
                    yield return Shot($"B48-brawl-{atk}-strip-{k:00}");
                    taken++;
                    last = b.stepNo;
                }
                for (float t = 0; t < 3f && b.Alive && !impact; t += Time.unscaledDeltaTime)
                {
                    Safe48(g);
                    Face();
                    Track();
                    if (tellEnd >= 0 && b.stepNo >= tellEnd + 3) { impact = true; impactAt = b.stepNo - tellEnd; yield return ImpactShot(atk); impacts++; }
                    yield return null;
                }
                if (taken == 12) strips++;
                lines.Add($"{atk} strip steps {s0}-{last}, tell end {tellEnd}, impact {impact} (+{impactAt} steps after the tell)");
            }
            // the flinch: a real gauntlet hit while it idles
            int flinch = 0;
            for (float t = 0; t < 10f && b.Alive && b.state != Brawl40State.Idle; t += Time.unscaledDeltaTime) { Safe48(g); Face(); yield return null; }
            if (b.Alive)
            {
                sim.loadout.Damage(b.body, b.body.maxHp * 0.01, false, "gauntlet", sim.mover.position + new Vec3(0, 1.6, 0));
                int f0 = b.stepNo;
                for (int k = 0; k < 8; k++)
                {
                    for (int f = 0; f < 60 && b.stepNo < f0 + 2 * k; f++) { Safe48(g); Face(); yield return null; }
                    yield return Shot($"B48-brawl-flinch-{k:00}");
                    flinch++;
                }
            }
            int hurts = Juice38Probe.Hurts - hurts0;
            bool green = strips == queue.Length && impacts == queue.Length && flinch == 8 && hurts == 0;
            res[0] = (green ? "GREEN" : "RED") + $" a248: n47-iceline x9; strips {strips}/{queue.Length}, impacts {impacts}/{queue.Length}, flinch frames {flinch}/8, the camera hurt {hurts} times (want 0: no whole-screen hurt wash); {string.Join("; ", lines)}; frames {shots.Count}";
        }
    }
}
