// NIGHT34 builder A (design.n34.calm34; NIGHT34.md A1; GG 2026-10-11: the game is "cluttered with detail for the sake of detail"; research/arenas.md
// rule 1: "cut every prop that is not a surface you use"). A runtime switch over NIGHT33's dressing: with the row on, Kit33Props draws a dress33 row
// (id "k33-...", `decor: true`, no collider) only when its kind is in design.n34.calm34.keepKinds (a purpose: a fixture that lights the room, a
// door's frame and its light, the room's one landmark, the wall wash). Every row with a collider (cover, crates, pillars, consoles) and every
// device or pickup is not a dress33 row and is never touched. The level files stay NIGHT33's: the row off draws NIGHT33's look byte for byte.
// Unity-free (the SimHarness's calm34 command counts with the same rule).
using System.Collections.Generic;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static class Calm34
    {
        /// <summary>dress33's own rows: their ids start with "k33-" (Tools/night/dress33.cjs `row`), and only they are decor.</summary>
        public static bool IsDressing(string id) => id != null && id.StartsWith("k33-", System.StringComparison.Ordinal);

        /// <summary>The kinds kept with the row on (design.n34.calm34.keepKinds); null when the config has none.</summary>
        public static HashSet<string> KeepKinds(GameConfig cfg)
        {
            if (cfg == null || !cfg.Has("design.n34.calm34.keepKinds")) return null;
            return new HashSet<string>(cfg.Strs("design.n34.calm34.keepKinds"));
        }

        /// <summary>False: a dressing row calm34 cuts (keep is the row's keep set; null keep = the row off: every row stays).</summary>
        public static bool Keep(HashSet<string> keep, string id, string kind) => keep == null || !IsDressing(id) || keep.Contains(kind ?? "");
    }
}
