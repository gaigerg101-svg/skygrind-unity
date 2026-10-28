// NIGHT43 (the shared commit, the lead): the proof reels' dispatch for the NIGHT43 builders (NIGHT43.md hand-offs). The lead's hook in
// FeelReel.N27.cs: `|| Array.IndexOf(PhaseN43, s) >= 0` in IsPhaseN37 and, in the body chain, `else if (Array.IndexOf(PhaseN43, s) >= 0)
// N43ProofBody(...)`. Each builder fills its own partial in its own file FeelReel.N43.<Name>.cs (a partial with no body leaves body null:
// res[0] stays NOT WRITTEN, RED). Use NIGHT41's helpers (FeelReel.N41.cs: the campaign start, DoorFree41) for the run's start.
//   move43    M1  FeelReel.N43.Move.cs     a chimney climb between two facing walls (5+ jumps), a B press just after a ride, a jump + shot boost
//   foundry43 L1  FeelReel.N43.Foundry.cs  THE GANTRY from the lip and mid-chain, THE FLUE mid-climb, PIERCE's lesson order, the boss door
//   weapons43 W1  FeelReel.N43.Weapons.cs  a DRONE orb parried at range, a FUSION (dash into a husk + the gauntlet)
//   feel43    F1  FeelReel.N43.Feel.cs     a FUSION at +0 / +0.05 / +0.15 s, a THUNDERCLAP slug kill, the TORCH firing
//   ui43      U1  FeelReel.N43.Ui.cs       the health bar hit / heal, the counter in a locked room, a centre teach card, the shop, results
//   rooms43   E1  FeelReel.N43.Rooms.cs    a locked room cleared to its door opening (the counter to 0), a level 2 rehearsal
//   kiln43    K1  FeelReel.N43.Kiln.cs     level 3's first room (the boost steps), a burrower room, THE RING
//   burrow43  A1  FeelReel.N43.Burrow.cs   a BURROWER's tell, emergence and counter; the enemy line-up (models43)
//   wick43    B1  FeelReel.N43.Wick.cs     THE WICK's reveal, its bar in each state, a leg change
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN43 = { "move43", "foundry43", "weapons43", "feel43", "ui43", "rooms43", "kiln43", "burrow43", "wick43" };

        static void N43ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "move43") N43Move(g, dir, res, offs, pre, ref body);
            else if (s == "foundry43") N43Foundry(g, dir, res, offs, pre, ref body);
            else if (s == "weapons43") N43Weapons(g, dir, res, offs, pre, ref body);
            else if (s == "feel43") N43Feel(g, dir, res, offs, pre, ref body);
            else if (s == "ui43") N43Ui(g, dir, res, offs, pre, ref body);
            else if (s == "rooms43") N43Rooms(g, dir, res, offs, pre, ref body);
            else if (s == "kiln43") N43Kiln(g, dir, res, offs, pre, ref body);
            else if (s == "burrow43") N43Burrow(g, dir, res, offs, pre, ref body);
            else if (s == "wick43") N43Wick(g, dir, res, offs, pre, ref body);
        }

        static partial void N43Move(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // M1
        static partial void N43Foundry(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L1
        static partial void N43Weapons(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // W1
        static partial void N43Feel(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // F1
        static partial void N43Ui(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // U1
        static partial void N43Rooms(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // E1
        static partial void N43Kiln(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // K1
        static partial void N43Burrow(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // A1
        static partial void N43Wick(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // B1
    }
}
