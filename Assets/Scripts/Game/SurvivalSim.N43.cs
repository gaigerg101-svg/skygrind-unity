// NIGHT43 (the shared commit, the lead owns this file): the twelve sim-changing NIGHT43 design blocks in the sim (Core/N43Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n43Override), --off43 / -skygrindOff43 (and every older off switch) take
// every block off (N43Blocks.ForceOff), otherwise the design defaults. The sim hooks a builder may fill in its OWN partial file (a partial
// with no body costs nothing; the call sites are the lead's, in SurvivalSim.Run.cs):
// - M1 (chimney43, ride43, boost43): N43MoveStartRun / N43MoveStep (hand the flags to the mover / loadout here).
// - W1 (gauntlet43, parry43, fusion43): N43WeaponsStartRun / N43WeaponsStep.
// - E1 (rooms43, iceline43): N43RoomsStartRun / N43RoomsStep; N43RoomLeftCount (the locked room's enemies left, for U1's counter).
// - A1 (burrow43): N43BurrowStartRun / N43BurrowStep.
// - B1 (wick43): N43WickStartRun / N43WickStep; N43BossStateFill (the boss bar's state, for U1's bossbar43).
// - K1 (kiln43): N43KilnStartRun / N43KilnStep (level-3-only rules, if any; the level itself is data).
// - L1 (foundry43): a level row, data only (L1 asks E1 for any room rule it needs, e.g. wave.afterLesson).
// The step hooks run right after N42Step (SurvivalSim.Run.cs).
// The level rows (foundry43, iceline43, kiln43) swap their `levelFrom` for their `levelTo` in the campaign order (Curriculum25.N43Levels,
// after N42Levels). An n43 level is its levelFrom to every older system: N42BaseId first asks N43BaseId, so n43-foundry -> n42-foundry ->
// n41-foundry -> n38-foundry while foundry41 / 42 / 43 are on this run.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        N43Blocks n43Design;

        /// <summary>A replay's or a harness's NIGHT43 blocks (a tape's bits); null = the design's, or off when forced off.</summary>
        public N43Blocks? n43Override;

        /// <summary>The NIGHT43 blocks the current run reads (set at StartRun).</summary>
        public N43Blocks n43;

        /// <summary>The level rows: bit index in N43Blocks (foundry43, iceline43, kiln43); levelFrom / levelTo read once at Init.</summary>
        static readonly int[] N43LevelBits = { 3, 8, 9 };
        readonly string[] n43From = new string[3], n43To = new string[3];

        void N43Init()
        {
            n43Design = N43Blocks.From(cfg);
            for (int k = 0; k < N43LevelBits.Length; k++)
            {
                string row = "design.n43." + N43Blocks.Names[N43LevelBits[k]];
                n43From[k] = cfg.Has(row + ".levelFrom") ? cfg.Get(row + ".levelFrom") as string : null;
                n43To[k] = cfg.Has(row + ".levelTo") ? cfg.Get(row + ".levelTo") as string : null;
            }
        }

        void N43StartRun()
        {
            n43 = n43Override ?? (N43Blocks.ForceOff ? N43Blocks.Off : n43Design);
            N43MoveStartRun();
            N43WeaponsStartRun();
            N43RoomsStartRun();
            N43BurrowStartRun();
            N43WickStartRun();
            N43KilnStartRun();
        }

        partial void N43MoveStartRun(); // NIGHT43 M1 (chimney43, ride43, boost43)
        partial void N43MoveStep(double dt); // NIGHT43 M1
        partial void N43WeaponsStartRun(); // NIGHT43 W1 (gauntlet43, parry43, fusion43)
        partial void N43WeaponsStep(double dt); // NIGHT43 W1
        partial void N43RoomsStartRun(); // NIGHT43 E1 (rooms43, iceline43)
        partial void N43RoomsStep(double dt); // NIGHT43 E1
        partial void N43BurrowStartRun(); // NIGHT43 A1 (burrow43)
        partial void N43BurrowStep(double dt); // NIGHT43 A1
        partial void N43WickStartRun(); // NIGHT43 B1 (wick43)
        partial void N43WickStep(double dt); // NIGHT43 B1
        partial void N43KilnStartRun(); // NIGHT43 K1 (kiln43)
        partial void N43KilnStep(double dt); // NIGHT43 K1

        /// <summary>NIGHT43 E1 fills it: the enemies left in the locked room the player is fighting (n stays -1 when E1 has no answer).</summary>
        partial void N43RoomLeftCount(ref int n);

        /// <summary>NIGHT43 B1 fills it (and any builder for its own boss): the boss bar's state (valid stays false when there is none).</summary>
        partial void N43BossStateFill(ref Boss43State s);

        /// <summary>NIGHT43 (U1's counter43 reads it): the enemies left in the locked room, -1 when there is no locked fight. E1's answer
        /// when it has one, else the campaign's whole-level count while a room is fighting (NIGHT42's door rule).</summary>
        public int N43RoomLeft
        {
            get
            {
                int n = -1;
                N43RoomLeftCount(ref n);
                return n;
            }
        }

        /// <summary>NIGHT43 (U1's bossbar43 reads it): the boss bar's state; valid false when no builder filled it.</summary>
        public Boss43State N43BossState
        {
            get
            {
                var s = new Boss43State { stagger01 = -1 };
                N43BossStateFill(ref s);
                return s;
            }
        }

        /// <summary>NIGHT43: every step, after N42Step (SurvivalSim.Run.cs, the lead's line).</summary>
        void N43Step(double dt)
        {
            N43MoveStep(dt);
            N43WeaponsStep(dt);
            N43RoomsStep(dt);
            N43BurrowStep(dt);
            N43WickStep(dt);
            N43KilnStep(dt);
        }

        /// <summary>The id NIGHT42's systems know this level by: an n43 level row's levelTo (its row on this run) reads as its levelFrom
        /// (n43-foundry -> n42-foundry); any other id is itself. With every NIGHT43 block off no n43 id is ever loaded: the identity.</summary>
        public string N43BaseId(string levelId)
        {
            if (levelId == null) return null;
            levelId = N45Levels.BaseId(cfg, levelId); // NIGHT45 (the lead's hook): an n45 level is its n43 base first (unconditional; Core/N45Levels.cs)
            for (int k = 0; k < N43LevelBits.Length; k++)
                if (n43To[k] == levelId && n43From[k] != null && n43.Flag(N43LevelBits[k])) return n43From[k];
            return levelId;
        }
    }
}
