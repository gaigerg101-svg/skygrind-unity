// NIGHT48 (the shared commit; Tools/night/n48-shared.cjs): the builders' proof reels, dispatched from FeelReel.N27.cs (two lines, the
// lead's). Each builder fills its own partials in its own file: a148 (W: FeelReel.N48.Wick.cs), a248 / a348 (B: FeelReel.N48.Brawl.cs /
// FeelReel.N48.Wyrm.cs), a448 / a548 (L: FeelReel.N48.Loom.cs / FeelReel.N48.Crown.cs), a648 / a748 / a848 / a948 (F: FeelReel.N48.Hud.cs /
// FeelReel.N48.Fx.cs / FeelReel.N48.Look.cs / FeelReel.N48.Lineup.cs). a948 is the lineup reel (lineup48: no viewmodel, no HUD, each kind
// alone on a neutral plate; G12-G14); a848 stays F's look reel as a847 was. A partial with no body leaves body null (res[0] stays NOT
// WRITTEN, RED). A reel forces a sim row before the run starts with sim.rowsOverride = sim.rowsRegistry.Defaults().Set("wyrmTell48", true);
// view rows are forced by the build copy's config (node Tools/night/n46-build-rows.cjs <tag> on n48.<row>) or by a reel-only switch the
// builder reads.
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN48 = { "a148", "a248", "a348", "a448", "a548", "a648", "a748", "a848", "a948" };

        static void N48ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a148") N48Wick(g, dir, res, offs, pre, ref body);
            else if (s == "a248") N48Brawl(g, dir, res, offs, pre, ref body);
            else if (s == "a348") N48Wyrm(g, dir, res, offs, pre, ref body);
            else if (s == "a448") N48Loom(g, dir, res, offs, pre, ref body);
            else if (s == "a548") N48Crown(g, dir, res, offs, pre, ref body);
            else if (s == "a648") N48Hud(g, dir, res, offs, pre, ref body);
            else if (s == "a748") N48Fx(g, dir, res, offs, pre, ref body);
            else if (s == "a848") N48Look(g, dir, res, offs, pre, ref body);
            else if (s == "a948") N48Lineup(g, dir, res, offs, pre, ref body);
        }

        static partial void N48Wick(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // W
        static partial void N48Brawl(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // B
        static partial void N48Wyrm(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // B
        static partial void N48Loom(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L
        static partial void N48Crown(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L
        static partial void N48Hud(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // F
        static partial void N48Fx(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // F
        static partial void N48Look(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // F
        static partial void N48Lineup(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // F: the lineup reel (lineup48)
    }
}
