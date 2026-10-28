// NIGHT42 (the shared commit, the lead): the proof reels' dispatch for the NIGHT42 builders (NIGHT42.md hand-offs). The lead's hook in
// FeelReel.N27.cs: `|| Array.IndexOf(PhaseN42, s) >= 0` in IsPhaseN37 and, in the body chain, `else if (Array.IndexOf(PhaseN42, s) >= 0)
// N42ProofBody(...)`. Each builder fills its own partial in its own file FeelReel.N42.<Name>.cs (a partial with no body leaves body null:
// res[0] stays NOT WRITTEN, RED). Use NIGHT41's helpers (FeelReel.N41.cs: the campaign start, DoorFree41) for the run's start.
//   wall42     M1  FeelReel.N42.Wall.cs     a jump press beside a wall (walk-in) framed at +0 / +0.15 / +0.30 s; a boost from the kiln's first climb
//   weapons42  W1  FeelReel.N42.Weapons.cs  the held second charge, the reticle at +0 / +0.1 / +0.2 s, a DRONE parry at the orb's arrival
//   hits42     F1  FeelReel.N42.Hits.cs     a KESTREL hit, a THUNDERCLAP kill, a gauntlet hit on a husk / tank at +0 / +0.05 / +0.15 s
//   ui42       U1  FeelReel.N42.Ui.cs       a combo climbing two ranks, a hit taken, a heal, the meter through a charge
//   arena42    K1  FeelReel.N42.Arena.cs    each arena element in use
//   first42    L1  FeelReel.N42.First.cs    level 1's first 60 s (frames at 5 / 15 / 30 / 60 s); THE GANTRY from the lip and mid-chain
//   brawl42    B1  FeelReel.N42.Brawl.cs    each new ICEBREAKER attack's tell and its answer
using System;
using System.Collections;
using System.Collections.Generic;

namespace Skygrind.Game
{
    public static partial class FeelReel
    {
        public static readonly string[] PhaseN42 = { "wall42", "weapons42", "hits42", "ui42", "arena42", "first42", "brawl42" };

        static void N42ProofBody(string s, SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body)
        {
            if (s == "wall42") N42Wall(g, dir, res, offs, pre, ref body);
            else if (s == "weapons42") N42Weapons(g, dir, res, offs, pre, ref body);
            else if (s == "hits42") N42Hits(g, dir, res, offs, pre, ref body);
            else if (s == "ui42") N42Ui(g, dir, res, offs, pre, ref body);
            else if (s == "arena42") N42Arena(g, dir, res, offs, pre, ref body);
            else if (s == "first42") N42First(g, dir, res, offs, pre, ref body);
            else if (s == "brawl42") N42Brawl(g, dir, res, offs, pre, ref body);
        }

        static partial void N42Wall(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // M1
        static partial void N42Weapons(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // W1
        static partial void N42Hits(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // F1
        static partial void N42Ui(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // U1
        static partial void N42Arena(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // K1
        static partial void N42First(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // L1
        static partial void N42Brawl(SurvivalGame g, string dir, string[] res, List<Action> offs, int pre, ref IEnumerator body); // B1
    }
}
