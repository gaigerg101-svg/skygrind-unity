// NIGHT36 (the shared commit, the lead owns this file): the nine sim-changing NIGHT36 design blocks in the sim (Core/N36Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n36Override), --off36 / -skygrindOff36 (and every older off switch)
// take every block off (N36Blocks.ForceOff), otherwise the design defaults. The builders hook in through the declaration-only
// partial methods below (a partial method with no body costs nothing; the call sites in SurvivalSim.Run.cs are the lead's):
// - F (gun feel) implements N36GunsStartRun / N36GunsStep (Game/SurvivalSim.Guns36.cs): tap36, crunch36.
// - P (the core loop) implements N36CoreStartRun / N36CoreStep (Game/SurvivalSim.Core36.cs): tells36, parry36, launch36, heal36.
// - M (walls) implements N36MoveStartRun / N36MoveStep (Game/SurvivalSim.Move36.cs): wallgrip36 (its Mover.cs branch reads the
//   mover's own flags, set at the start of the run as flow34's are).
// - L (level 1) implements N36LevelStartRun / N36LevelStep (Game/SurvivalSim.Level36.cs): the arena flow on n36 levels only (a field
//   in the level file; older levels untouched), so it needs no bit.
// - B (bosses) implements N36BossStartRun / N36BossStep / N36BossBegin (Game/SurvivalSim.Boss36.cs): walkin36, boss36. N36BossBegin
//   runs first in BossRoomBegin (SurvivalSim.Boss25.cs): handled = true owns the begin. B sets boss36WalkInHere; with walkin36 on,
//   Campaign24 then keeps the player where they are (no Respawn into a far arena: ICampaignWorld.BossWalkIn).
// The Step hooks run every step right after N35TeachStep (the player's side), F, P, M, L, B in that order.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        N36Blocks n36Design;

        /// <summary>A replay's or a harness's NIGHT36 blocks (a tape's bits); null = the design's, or off when forced off.</summary>
        public N36Blocks? n36Override;

        /// <summary>The NIGHT36 blocks the current run reads (set at StartRun).</summary>
        public N36Blocks n36;

        /// <summary>NIGHT36 B (walkin36): does this level's boss fight in the level's own last room? False at every run start; B's
        /// N36BossStartRun sets it on a level built for it. Read only while n36.walkin36 is on.</summary>
        public bool boss36WalkInHere;

        /// <summary>NIGHT36: the boss room keeps the player where they stand (Campaign24's BossWalkIn: no Respawn into a far arena).</summary>
        public bool Boss36WalkIn => (n36.walkin36 && boss36WalkInHere) || boss37WalkInHere || walkIn46; // NIGHT46 B1 (walkin46, SurvivalSim.N46.cs; false = NIGHT45); NIGHT37: a NIGHT37 boss on its level fights where the player walked in too (SurvivalSim.N37.cs)

        void N36Init() => n36Design = N36Blocks.From(cfg);

        void N36StartRun()
        {
            n36 = n36Override ?? (N36Blocks.ForceOff ? N36Blocks.Off : n36Design);
            boss36WalkInHere = false;
            N36GunsStartRun();
            N36CoreStartRun();
            N36MoveStartRun();
            N36LevelStartRun();
            N36BossStartRun();
        }

        partial void N36GunsStartRun();
        partial void N36GunsStep(double dt); // NIGHT36 F: every step, after N35TeachStep
        partial void N36CoreStartRun();
        partial void N36CoreStep(double dt); // NIGHT36 P: after F's
        partial void N36MoveStartRun();
        partial void N36MoveStep(double dt); // NIGHT36 M: after P's
        partial void N36LevelStartRun();
        partial void N36LevelStep(double dt); // NIGHT36 L: after M's
        partial void N36BossStartRun();
        partial void N36BossStep(double dt); // NIGHT36 B: after L's
        partial void N36BossBegin(string id, ref bool handled, ref bool ok, ref Vec3 spawn, ref double yawDeg); // NIGHT36 B: first in BossRoomBegin
        partial void N36BossSystem(string id, ref IBossSystem boss); // NIGHT36 B: first in BossRoomSystem (set boss to own the id)
    }
}
