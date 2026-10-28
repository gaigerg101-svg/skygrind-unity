// NIGHT41 (the shared commit, the lead owns this file): the eight sim-changing NIGHT41 design blocks in the sim (Core/N41Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n41Override), --off41 / -skygrindOff41 (and every older off switch) take
// every block off (N41Blocks.ForceOff), otherwise the design defaults. The sim hooks a builder may fill in its OWN partial file (a partial
// with no body costs nothing; the call sites are the lead's, in SurvivalSim.Run.cs):
// - B8 (sweep41; THE LOOM's and THE ICEBREAKER's WeakPoint41): N41LoomStartRun / N41LoomStep, N41BrawlStartRun / N41BrawlStep.
// - E6 (tokens41, heavy41, kiln41): N41FoesStartRun / N41FoesStep. heavy41's numbers go through Loadout.Heavy41 (Core/Heavy41.cs, E6's
//   new file: the partial's body); a hitstop it asks for reaches SurvivalSim.Hitstop through loadout.hitstop41 (set here).
// - P3 (aim41; THE CROWN's WeakPoint41): N41AimStartRun / N41CrownStep.
// - R5 (foundry41, spire41) and L6 (finale41): level rows, data only (no code hook).
// The step hooks run right after N40Step (SurvivalSim.Run.cs).
// The level rows (foundry41, kiln41, spire41) each swap their `levelFrom` for their `levelTo` in the campaign order (Curriculum25.N41Levels);
// finale41 (OFF tonight, DECIDE41 D-3) puts its `levelTo` back as the last level. An n41 level is its levelFrom to every older system that
// knows a level by its id (N41BaseId): NIGHT40's start sets / awards / the kiln's boost chain, the walk pilots, the verbs reel's start rules.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        N41Blocks n41Design;

        /// <summary>A replay's or a harness's NIGHT41 blocks (a tape's bits); null = the design's, or off when forced off.</summary>
        public N41Blocks? n41Override;

        /// <summary>The NIGHT41 blocks the current run reads (set at StartRun).</summary>
        public N41Blocks n41;

        /// <summary>The level rows: bit index in N41Blocks, levelFrom, levelTo (read once at Init; null when the row has none).</summary>
        static readonly int[] N41LevelBits = { 4, 5, 6, 7 }; // foundry41, kiln41, spire41, finale41
        readonly string[] n41From = new string[4], n41To = new string[4];

        void N41Init()
        {
            n41Design = N41Blocks.From(cfg);
            for (int k = 0; k < N41LevelBits.Length; k++)
            {
                string row = "design.n41." + N41Blocks.Names[N41LevelBits[k]];
                n41From[k] = cfg.Has(row + ".levelFrom") ? cfg.Get(row + ".levelFrom") as string : null;
                n41To[k] = cfg.Has(row + ".levelTo") ? cfg.Get(row + ".levelTo") as string : null;
            }
            loadout.hitstop41 = Hitstop; // NIGHT41 E6 heavy41: the gauntlet's harder answer asks the sim for its hitstop (nothing asks with heavy41 off)
        }

        void N41StartRun()
        {
            n41 = n41Override ?? (N41Blocks.ForceOff ? N41Blocks.Off : n41Design);
            loadout.heavy41 = n41.heavy41; // NIGHT41 E6 heavy41: the gauntlet's hook runs only while on (Core code takes the flag, Loadout.Heavy41)
            N41LoomStartRun();
            N41BrawlStartRun();
            N41FoesStartRun();
            N41AimStartRun();
        }

        partial void N41LoomStartRun(); // NIGHT41 B8 (sweep41, THE LOOM's weak point)
        partial void N41LoomStep(double dt); // NIGHT41 B8
        partial void N41BrawlStartRun(); // NIGHT41 B8 (THE ICEBREAKER's weak point)
        partial void N41BrawlStep(double dt); // NIGHT41 B8
        partial void N41FoesStartRun(); // NIGHT41 E6 (tokens41, heavy41, kiln41)
        partial void N41FoesStep(double dt); // NIGHT41 E6
        partial void N41AimStartRun(); // NIGHT41 P3 (aim41)
        partial void N41CrownStep(double dt); // NIGHT41 P3 (THE CROWN's weak point)

        /// <summary>NIGHT41: every step, after N40Step (SurvivalSim.Run.cs, the lead's line).</summary>
        void N41Step(double dt)
        {
            N41LoomStep(dt);
            N41BrawlStep(dt);
            N41FoesStep(dt);
            N41CrownStep(dt);
        }

        /// <summary>The id every older system knows this level by: an n41 level row's levelTo (its row on this run) reads as its levelFrom
        /// (n41-kiln -> n40-kiln, n41-spire -> n40-spire, n41-foundry -> n38-foundry); any other id is itself. With every NIGHT41 block off
        /// no n41 id is ever loaded, so this is the identity.</summary>
        public string N41BaseId(string levelId)
        {
            if (levelId == null) return null;
            levelId = N42BaseId(levelId); // NIGHT42 (the lead's hook): an n42 level is its levelFrom first (n42-foundry -> n41-foundry)
            for (int k = 0; k < N41LevelBits.Length; k++)
                if (n41To[k] == levelId && n41From[k] != null && n41.Flag(N41LevelBits[k])) return n41From[k];
            return levelId;
        }
    }
}
