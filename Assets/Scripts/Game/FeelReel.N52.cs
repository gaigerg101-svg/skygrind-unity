// NIGHT52 (the shared commit; Tools/night/n52-shared.cjs): the builders' proof reels, dispatched from FeelReel.N27.cs (the lead's line).
// a152 (U: FeelReel.N52.Hud.cs, hud52), a252 (U: FeelReel.N52.Skin.cs, skin52), a352 (U: FeelReel.N52.Guns.cs, guns52), a452 (K:
// FeelReel.N52.Rooms.cs, the room frames reel: on any level, each room row's frames52 spots {entry, mid, high} as [x, y, z, yawDeg, pitch],
// the mid frame with the room's fight live; F and the loop add frames52 to their rooms and use it). A partial with no body leaves body null
// (res[0] stays NOT WRITTEN, RED). View rows are forced by the build copy's config (node Tools/night/n46-build-rows.cjs <tag> on n52.<row>)
// or by a reel-only switch the builder reads; a campaign level boots only via reel.sh + -skygrindMap <id> -skygrindReelOnly <phase> (L-062).
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN52 = { "a152", "a252", "a352", "a452" };

        static void N52ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "a152") N52Hud(g, dir, res, offs, pre, ref body);
            else if (s == "a252") N52Skin(g, dir, res, offs, pre, ref body);
            else if (s == "a352") N52Guns(g, dir, res, offs, pre, ref body);
            else if (s == "a452") N52Rooms(g, dir, res, offs, pre, ref body);
        }

        static partial void N52Hud(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // U
        static partial void N52Skin(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // U
        static partial void N52Guns(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // U
        static partial void N52Rooms(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // K (F and the loop use it)
    }
}
