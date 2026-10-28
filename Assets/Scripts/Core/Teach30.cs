// NIGHT30 (builder S; design.n30.teach, item 12 + P0 b): the boss lines' budget. A boss explains its rule in at most two short lines, each shown on
// the first occurrence of its tell and at most design.n30.teach.maxShows times per FIGHT (a boss room entered, or re-entered after a death,
// starts a new fight's count only when the boss is begun again: ResetFight). The sim owns one (SurvivalSim.Systems30.cs: BossLine30); a boss
// rule (builder B's Lantern30 / Glacier30 / Wyrm30 / Loom) asks Allow(key) before it shows a line. Pure: no Unity, no randomness.
using System.Collections.Generic;

namespace Skygrind.Core
{
    public sealed class Teach30
    {
        /// <summary>design.n30.teach.maxShows: how many times one line may show in one fight.</summary>
        public readonly int maxShows;
        readonly Dictionary<string, int> shows = new Dictionary<string, int>();

        /// <summary>Lines refused this fight (over the budget): the harness's count.</summary>
        public int refused;

        public Teach30(int maxShows) => this.maxShows = maxShows;

        public static Teach30 From(GameConfig cfg) => new Teach30((int)cfg.Num("design.n30.teach.maxShows"));

        /// <summary>A new fight: every line's count back to 0.</summary>
        public void ResetFight()
        {
            shows.Clear();
            refused = 0;
        }

        /// <summary>May the line `key` show now? Counts it when it may.</summary>
        public bool Allow(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            shows.TryGetValue(key, out int n);
            if (n >= maxShows)
            {
                refused++;
                return false;
            }
            shows[key] = n + 1;
            return true;
        }

        /// <summary>How many times `key` showed this fight.</summary>
        public int Shown(string key) => key != null && shows.TryGetValue(key, out int n) ? n : 0;
    }
}
