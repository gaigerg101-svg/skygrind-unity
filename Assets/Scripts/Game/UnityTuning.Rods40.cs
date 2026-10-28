// NIGHT40 builder S1 (rods40, a view row: design.n40.rods40; GG 2026-10-13: "Many arenas contain awkwardly placed white rods that you clip
// through"): the play volume a drawn piece with no collider may not enter (Rods40.cs). Unity-only numbers (CLAUDE.md rule 5): the web draws no
// decor, light fixtures or shafts, so it has no such rule. Pure numbers (no UnityEngine): the harness's `n40start rods` scan reads the same values.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The play volume's floor band: a point this far or more above the floor under it (m) is where the body moves (NIGHT40.md S1:
        /// "0.2-12 m above a floor"); lower is the floor's own trim.</summary>
        public const double Rods40FloorMin = 0.2;
        /// <summary>... and at most this far above it (m): level 3's climbs reach 12 m (the boost gates), so a truss at 8.7-11 m is in the flight path.</summary>
        public const double Rods40FloorMax = 12;
        /// <summary>A piece closer than this to a wall (m, horizontal) is flush with it: the capsule (radius 0.35) never fits between (NIGHT40.md S1: 0.3 m).</summary>
        public const double Rods40WallClear = 0.3;
        /// <summary>A ROD: a piece at most this thin across (m, its thinner horizontal side) ...</summary>
        public const double Rods40ThinMax = 0.6;
        /// <summary>... and at least this long (m, its longest side: a chain 1.95, a pole 2.6, a truss 17.8; a wall rib or pilaster too, kept by the share below).</summary>
        public const double Rods40LongMin = 1.0;
        /// <summary>A rod is skipped when at least this share of its samples is in the play volume: a wall rib 0.34 m deep has half its samples flush (kept);
        /// a truss across a room or a free pole has nearly all of them out in the room (skipped). Any other collider-less piece is skipped only when
        /// EVERY sample is in the play volume (a lamp hanging in the air); a light shaft (a beam) when more than half of it is.</summary>
        public const double Rods40FreeShare = 0.6;
        /// <summary>Kit kinds that wrap an opening (their box spans the door's free space, their parts sit on the wall face): never rods.</summary>
        public static readonly string[] Rods40KeepKinds = { "doorFrame", "kilnArch", "lintelLight", "recess" };
        /// <summary>The scan's sample spacing along a piece (m, horizontal / vertical): every piece is sampled at least at its centre.</summary>
        public const double Rods40StepXZ = 0.5, Rods40StepY = 0.5;
    }
}
