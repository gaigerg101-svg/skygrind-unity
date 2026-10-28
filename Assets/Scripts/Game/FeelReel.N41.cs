// NIGHT41 builder T2: the proof reels' dispatch and shared helpers (INVENTORY A1 / A2 / A9 / A14 / C1; NIGHT41.md card T2). The lead's hook in
// FeelReel.N27.cs: `|| Array.IndexOf(PhaseN41T, s) >= 0` in IsPhaseN37 and, in the body chain, `else if (Array.IndexOf(PhaseN41T, s) >= 0)
// N41ProofBody(s, g, dir, res, offs, pre, ref body);`. A name listed here with no body yet leaves res[0] as NOT WRITTEN (RED).
//   weapons41    FeelReel.N41.Weapons.cs    the KESTREL's taps / charge, the THUNDERCLAP's volley / pump and slug, framed at +0 / 0.15 / 0.30 / 0.45 s
//   entrance41   FeelReel.N41.Entrance.cs   the booted level's boss room entered for real, framed 0-1.6 s after bossRise (one Player per boss)
//   doors41      FeelReel.N41.Doors.cs      WITHOUT DOORFREE: a locked doorway while its room's wave lives, the open one, the boss door; far / near
//   pad41        FeelReel.N41.Pad.cs        with -skygrindReelPad: every room visited, each new {token} line framed on the pad; keyboard names flagged
//   boss61       FeelReel.N61G.cs           NIGHT61 G (S3 bossRule61): entrance41's walk-in, the boss's rule line framed 1.0 s after bossRise at
//                                           1920x1080 and measured at 1280x720 (one Player per boss; registered here: no new line in FeelReel.N27.cs)
//   boss63       FeelReel.N63V.cs           NIGHT63 V (B1 bossMotion63): boss61's walk-in, then 25 s of the fight firing at the weak point; the
//                                           [boss63] attack / flinch / stagger / rms lines (BossAttackClock63) and the B63-<boss>-* frames
//   near63       FeelReel.N63V.cs           NIGHT63 V (C2 nearFade63): a survival run's open spot, a frozen husk 3.0 / 1.5 / 1.0 / 0.7 / 0.5 / 0.3 m
//                                           from the eye, the N63-near-<m> frames and the [near63] dissolve lines
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN41T = { "weapons41", "entrance41", "doors41", "pad41", "boss61", "boss63", "near63" }; // NIGHT61 G: boss61 (FeelReel.N61G.cs); NIGHT63 V: boss63, near63 (FeelReel.N63V.cs)

        static void N41ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "weapons41") body = Weapons41Scenario(g, dir, res, offs, pre);
            else if (s == "entrance41") body = Entrance41Scenario(g, dir, res, offs, pre);
            else if (s == "doors41") body = Doors41Scenario(g, dir, res, offs, pre);
            else if (s == "pad41") body = Pad41Scenario(g, dir, res, offs, pre);
            else if (s == "boss61") body = Boss61Scenario(g, dir, res, offs, pre); // NIGHT61 G (S3 bossRule61; FeelReel.N61G.cs)
            else if (s == "boss63") body = Boss63Scenario(g, dir, res, offs, pre); // NIGHT63 V (B1 bossMotion63's proof, B2's cue leads; FeelReel.N63V.cs)
            else if (s == "near63") body = Near63Scenario(g, dir, res, offs, pre); // NIGHT63 V (C2 nearFade63's SPOT pose: a husk 3.0 .. 0.3 m from the eye; FeelReel.N63V.cs)
        }

        /// <summary>The campaign run of a T2 reel (as brawl40 / loom37 start theirs): Quiet, no records, Campaign24.Mode, StartRun, the run-start
        /// title run out. `doorFree`: DOORFREE on, then the banners its zones queued cleared (A9: Ui.ClearBanners41). ok[0]: a campaign run is live.</summary>
        static IEnumerator Campaign41(SurvivalGame g, List<Action> offs, bool doorFree, bool[] ok)
        {
            ok[0] = false;
            if (N24CampaignGate(g) != null) yield break;
            offs.Add(() => { Quiet(g); g.setup.mode = RunSetup.Survival; });
            Quiet(g);
            g.noRecords = g.noLab = true;
            g.setup.mode = Campaign24.Mode;
            g.StartRun(Seed);
            yield return Frames(240); // the run-start title has run out before any frame
            SurvivalSim sim = g.sim;
            for (int k = 0; k < 120 && sim.campaign == null; k++) yield return null;
            if (doorFree) DoorFree41(g);
            ok[0] = sim.campaignOn && sim.campaign != null;
        }

        /// <summary>A9: DOORFREE on for a reel, and the room banners it queued ("4.2 FIRST GAP OPEN" ...) silenced at once.</summary>
        static void DoorFree41(SurvivalGame g)
        {
            g.sim.campaign?.SetDoorFree29(true);
            g.UiView?.ClearBanners41();
        }

        /// <summary>Every public static field / property of the named probe classes that exist in this build (J2's Entrance41Probe, Juice38Probe,
        /// BossBar41Probe; a class not in this build reads "absent"), plus the old bars' flags. Read by reflection so the reel compiles alone.</summary>
        static string Probes41()
        {
            var sb = new StringBuilder();
            Assembly asm = typeof(FeelReel).Assembly;
            foreach (string n in new[] { "Entrance41Probe", "Juice38Probe", "BossBar41Probe" })
            {
                Type t = asm.GetType("Skygrind.Game." + n);
                sb.Append(n).Append('{');
                if (t == null) sb.Append("absent");
                else
                {
                    bool first = true;
                    foreach (FieldInfo f in t.GetFields(BindingFlags.Public | BindingFlags.Static))
                    {
                        if (f.IsLiteral) continue;
                        if (!first) sb.Append(", ");
                        first = false;
                        sb.Append(f.Name).Append('=').Append(Fmt41(f.GetValue(null)));
                    }
                    foreach (PropertyInfo p in t.GetProperties(BindingFlags.Public | BindingFlags.Static))
                    {
                        if (!p.CanRead) continue;
                        if (!first) sb.Append(", ");
                        first = false;
                        object v;
                        try { v = p.GetValue(null); } catch (Exception) { v = "?"; }
                        sb.Append(p.Name).Append('=').Append(Fmt41(v));
                    }
                }
                sb.Append("} ");
            }
            sb.Append("Hud33BossBarShown=").Append(Ui.Hud33BossBarShown).Append(" Boss37StaggerBar.Shown=").Append(Boss37StaggerBar.Shown)
                .Append(" BossBar41Live=").Append(N41View.BossBar41Live);
            return sb.ToString();
        }

        static string Fmt41(object v) => v switch
        {
            null => "null",
            double d => d.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
            float f => f.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
            string s => "'" + s + "'",
            _ => v.ToString(),
        };
    }
}
