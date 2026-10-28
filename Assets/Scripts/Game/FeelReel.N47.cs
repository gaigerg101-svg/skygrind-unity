// NIGHT47 (the shared commit; Tools/night/n47-shared.cjs): the builders' proof reels, dispatched from FeelReel.N27.cs (two lines, the
// lead's). Each builder fills its own partials in its own file: a147 (W: FeelReel.N47.Wick.cs), a247 / a347 (B: FeelReel.N47.Brawl.cs /
// FeelReel.N47.Wyrm.cs), a447 / a547 (L: FeelReel.N47.Loom.cs / FeelReel.N47.Crown.cs), a647 / a747 / a847 (F: FeelReel.N47.Hud.cs /
// FeelReel.N47.Fx.cs / FeelReel.N47.Look.cs). A partial with no body leaves body null (res[0] stays NOT WRITTEN, RED). A reel forces a sim row
// before the run starts with sim.rowsOverride = sim.rowsRegistry.Defaults().Set("wick47", true); view rows are forced by the build copy's config
// (node Tools/night/n46-build-rows.cjs <tag> on n47.<row>) or by a reel-only switch the builder reads.
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN47 = { "a147", "a247", "a347", "a447", "a547", "a647", "a747", "a847" };

        static void N47ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a147") N47Wick(g, dir, res, offs, pre, ref body);
            else if (s == "a247") N47Brawl(g, dir, res, offs, pre, ref body);
            else if (s == "a347") N47Wyrm(g, dir, res, offs, pre, ref body);
            else if (s == "a447") N47Loom(g, dir, res, offs, pre, ref body);
            else if (s == "a547") N47Crown(g, dir, res, offs, pre, ref body);
            else if (s == "a647") N47Hud(g, dir, res, offs, pre, ref body);
            else if (s == "a747") N47Fx(g, dir, res, offs, pre, ref body);
            else if (s == "a847") N47Look(g, dir, res, offs, pre, ref body);
        }

        static partial void N47Wick(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // W
        static partial void N47Brawl(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // B
        static partial void N47Wyrm(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // B
        static partial void N47Loom(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L
        static partial void N47Crown(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L
        static partial void N47Hud(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // F
        static partial void N47Fx(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // F
        static partial void N47Look(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // F
    }
}
