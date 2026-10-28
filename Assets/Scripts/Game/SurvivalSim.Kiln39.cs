// NIGHT39 builder K4 (NIGHT39.md card K4; GG 2026-10-13: level 3 needs "a new arena, and keep the worm AI the same"): THE KILN WYRM's arena chosen by
// the LEVEL. The rebuilt level 3 (Design/levels/n38-kiln.json, played when design.n38.kiln38 swaps n35-kiln for it) carries `wyrmArena` on its boss
// room row (boss "wyrm"): Design/maps/wyrm39-arena.json (Tools/night/gen-n39-wyrm-arena.cjs: wyrm26-arena.json's shell, half 30, height 30, so
// Wyrm.cs's arenaHalf clamp is the same; new perches, slabs and stacks inside, all clear of the worm's routes). The worm's paths, params and AI
// are untouched (design.n26.bosses.wyrm26.paths). A level without the field (n35-kiln, every other map) keeps design.n26.bosses.wyrm26.arena, so
// kiln38 off (or --off38) is NIGHT38's worm room bit for bit.
// The call sites are the lead's (SurvivalSim.Wyrm24.cs, Tools/night/n39k-wyrm-hook.cjs): Wyrm24Ensure takes Wyrm39Arena(the config's path) as the
// arena it loads, and Wyrm24BeginCore rebuilds a dead worm whose room was built from another file (Wyrm39ArenaStale). No UnityEngine: SimHarness
// compiles this file.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        /// <summary>The arena file (repo-relative) the worm's room was last built from; null before the first build.</summary>
        public string wyrm39ArenaBuilt;

        /// <summary>The worm's arena for this map: the campaign's wyrm boss room `wyrmArena` when the level carries one (n38-kiln), else `fallback` (the
        /// config's path). Records the choice in wyrm39ArenaBuilt.</summary>
        public string Wyrm39Arena(string fallback)
        {
            string a = Wyrm39LevelArena() ?? fallback;
            wyrm39ArenaBuilt = a;
            return a;
        }

        /// <summary>A built worm's room came from another file than this map wants now (a session that loaded another level): rebuild it.</summary>
        public bool Wyrm39ArenaStale(string fallback) => wyrm39ArenaBuilt != null && wyrm39ArenaBuilt != (Wyrm39LevelArena() ?? fallback);

        /// <summary>The map's own worm arena (the campaign's wyrm boss room row's `wyrmArena`), or null.</summary>
        public string Wyrm39LevelArena()
        {
            if (map?.json == null || !map.json.TryGetValue("campaign", out object co) || !(co is System.Collections.Generic.Dictionary<string, object> camp)) return null;
            if (!camp.TryGetValue("rooms", out object ro) || !(ro is System.Collections.Generic.List<object> rooms)) return null;
            foreach (object o in rooms)
                if (o is System.Collections.Generic.Dictionary<string, object> r && r.TryGetValue("boss", out object b) && b as string == "wyrm"
                    && r.TryGetValue("wyrmArena", out object wa) && wa is string s && s.Length > 0)
                    return s;
            return null;
        }
    }
}
