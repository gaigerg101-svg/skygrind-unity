// NIGHT42 (the shared commit, the lead owns this file): the nine sim-changing NIGHT42 design blocks in the sim (Core/N42Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n42Override), --off42 / -skygrindOff42 (and every older off switch) take
// every block off (N42Blocks.ForceOff), otherwise the design defaults. The sim hooks a builder may fill in its OWN partial file (a partial
// with no body costs nothing; the call sites are the lead's, in SurvivalSim.Run.cs):
// - M1 (wallfling42, boost42): N42MoveStartRun / N42MoveStep (hand the flags to the mover / loadout here).
// - W1 (gauntlet42, parry42, charge42): N42WeaponsStartRun / N42WeaponsStep.
// - F1 (impact42): N42ImpactStartRun / N42ImpactStep.
// - B1 (brawl42): N42BrawlStartRun / N42BrawlStep.
// - K1 (arena42): N42ArenaStartRun / N42ArenaStep.
// - L1 (foundry42): a level row, data only (no code hook).
// The step hooks run right after N41Step (SurvivalSim.Run.cs).
// The level row foundry42 swaps its `levelFrom` (n41-foundry) for its `levelTo` (n42-foundry) in the campaign order (Curriculum25.N42Levels,
// after N41Levels). An n42 level is its levelFrom to every older system: N41BaseId first asks N42BaseId, so n42-foundry -> n41-foundry ->
// n38-foundry while foundry41 and foundry42 are on this run.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        N42Blocks n42Design;

        /// <summary>A replay's or a harness's NIGHT42 blocks (a tape's bits); null = the design's, or off when forced off.</summary>
        public N42Blocks? n42Override;

        /// <summary>The NIGHT42 blocks the current run reads (set at StartRun).</summary>
        public N42Blocks n42;

        /// <summary>The level rows: bit index in N42Blocks, levelFrom, levelTo (read once at Init; null when the row has none).</summary>
        static readonly int[] N42LevelBits = { 2 }; // foundry42
        readonly string[] n42From = new string[1], n42To = new string[1];

        void N42Init()
        {
            n42Design = N42Blocks.From(cfg);
            for (int k = 0; k < N42LevelBits.Length; k++)
            {
                string row = "design.n42." + N42Blocks.Names[N42LevelBits[k]];
                n42From[k] = cfg.Has(row + ".levelFrom") ? cfg.Get(row + ".levelFrom") as string : null;
                n42To[k] = cfg.Has(row + ".levelTo") ? cfg.Get(row + ".levelTo") as string : null;
            }
        }

        void N42StartRun()
        {
            n42 = n42Override ?? (N42Blocks.ForceOff ? N42Blocks.Off : n42Design);
            N42MoveStartRun();
            N42WeaponsStartRun();
            N42ImpactStartRun();
            N42BrawlStartRun();
            N42ArenaStartRun();
        }

        partial void N42MoveStartRun(); // NIGHT42 M1 (wallfling42, boost42)
        partial void N42MoveStep(double dt); // NIGHT42 M1
        partial void N42WeaponsStartRun(); // NIGHT42 W1 (gauntlet42, parry42, charge42)
        partial void N42WeaponsStep(double dt); // NIGHT42 W1
        partial void N42ImpactStartRun(); // NIGHT42 F1 (impact42)
        partial void N42ImpactStep(double dt); // NIGHT42 F1
        partial void N42BrawlStartRun(); // NIGHT42 B1 (brawl42)
        partial void N42BrawlStep(double dt); // NIGHT42 B1
        partial void N42ArenaStartRun(); // NIGHT42 K1 (arena42)
        partial void N42ArenaStep(double dt); // NIGHT42 K1

        /// <summary>NIGHT42: every step, after N41Step (SurvivalSim.Run.cs, the lead's line).</summary>
        void N42Step(double dt)
        {
            N42MoveStep(dt);
            N42WeaponsStep(dt);
            N42ImpactStep(dt);
            N42BrawlStep(dt);
            N42ArenaStep(dt);
        }

        /// <summary>The id NIGHT41's systems know this level by: an n42 level row's levelTo (its row on this run) reads as its levelFrom
        /// (n42-foundry -> n41-foundry); any other id is itself. With every NIGHT42 block off no n42 id is ever loaded: the identity.</summary>
        public string N42BaseId(string levelId)
        {
            if (levelId == null) return null;
            levelId = N43BaseId(levelId); // NIGHT43 (the lead's hook): an n43 level is its levelFrom first (n43-foundry -> n42-foundry)
            for (int k = 0; k < N42LevelBits.Length; k++)
                if (n42To[k] == levelId && n42From[k] != null && n42.Flag(N42LevelBits[k])) return n42From[k];
            return levelId;
        }
    }
}
