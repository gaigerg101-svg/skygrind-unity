// NIGHT26 (the lead, phase I): the reel scenarios for NIGHT26's rows, on the Player, proof-first (NIGHT26.md): a scenario is written and
// seen RED (the row's switch off or its feature absent) before the feature is built. Each scenario is in its owner's partial file
// (FeelReel.N26.<Topic>.cs) as a `static partial void N26<Topic>Body(...)` hook: the lead's switch below calls the hook, and a hook with
// no body answers "NOT WRITTEN (RED)". Every result line starts with the scenario's PRECONDITIONS (FeelReel.Preconditions / SafeSpot).
//   pulselt      W   the PULSE blast on the left trigger (charge by use, release to launch)
//   weapons      W   every weapon has an LT and a cost; the weapon-by-enemy table
//   ice          M   ice and boost strips: friction multiplier, speed, stopping distance
//   hulk         B   THE SLAG HULK, the easy level-1 boss
//   glacier      B   THE GLACIER, the ice chase boss (level 2)
//   wyrm26       B   the worm: proximity aggro, body x0.5 / head x2, the final attack (level 3)
//   level1       L   level 1: narrower, platforming run, wall-ride hall
//   level3       L   level 3: THE KILN rooms and the worm arena
//   levelselect  L   the level select screen
//   ranks        L   time / kills / style ranks, aggregate, P rank, best per level
//   wormcoil     V   the worm's glow: idle pulse, coil pulse rate, aura, release peak, the frame cost (FeelReel.N26.Worm.cs)
//   wormattack   V   the worm's glow: telegraph by kind, trail, impact, damage flash, HP dimming, death burn-out
using System;
using System.Collections;
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseI = { "pulselt", "weapons", "ice", "hulk", "glacier", "wyrm26", "level1", "level3", "levelselect", "ranks", "wormcoil", "wormattack" };

        static bool IsPhaseI(string s) => Array.IndexOf(PhaseI, s) >= 0;

        /// <summary>The owners' scenario bodies. A hook that no builder has written leaves `res[0]` as it is (NOT WRITTEN).</summary>
        static partial void N26PulseltBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N26WeaponsBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N26IceBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N26HulkBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N26GlacierBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N26Wyrm26Body(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N26Level1Body(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N26Level3Body(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N26LevelselectBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N26RanksBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N26WormcoilBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);
        static partial void N26WormattackBody(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body);

        static IEnumerator N26Scenario(SurvivalGame g, string s, string dir, string[] res, List<Action> offs, double yaw, int pre)
        {
            res[0] = "NOT WRITTEN (RED): no scenario body for '" + s + "'";
            IEnumerator body = null;
            switch (s)
            {
                case "pulselt": N26PulseltBody(g, dir, res, offs, pre, ref body); break;
                case "weapons": N26WeaponsBody(g, dir, res, offs, pre, ref body); break;
                case "ice": N26IceBody(g, dir, res, offs, pre, ref body); break;
                case "hulk": N26HulkBody(g, dir, res, offs, pre, ref body); break;
                case "glacier": N26GlacierBody(g, dir, res, offs, pre, ref body); break;
                case "wyrm26": N26Wyrm26Body(g, dir, res, offs, pre, ref body); break;
                case "level1": N26Level1Body(g, dir, res, offs, pre, ref body); break;
                case "level3": N26Level3Body(g, dir, res, offs, pre, ref body); break;
                case "levelselect": N26LevelselectBody(g, dir, res, offs, pre, ref body); break;
                case "ranks": N26RanksBody(g, dir, res, offs, pre, ref body); break;
                case "wormcoil": N26WormcoilBody(g, dir, res, offs, pre, ref body); break;
                case "wormattack": N26WormattackBody(g, dir, res, offs, pre, ref body); break;
            }
            if (body == null) yield break;
            yield return body;
        }
    }
}
