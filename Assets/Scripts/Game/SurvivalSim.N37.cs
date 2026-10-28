// NIGHT37 (the shared commit, the lead owns this file): the five sim-changing NIGHT37 design blocks in the sim (Core/N37Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n37Override), --off37 / -skygrindOff37 (and every older off switch)
// take every block off (N37Blocks.ForceOff), otherwise the design defaults. Each boss builder hooks in through the declaration-only
// partial methods below (a partial method with no body costs nothing; the call sites are the lead's):
// - W (THE WICK, boss id "wick"), S (THE STAG, "stag37"), O (THE LOOM, "loom37"), C (THE CROWN, "crown37") each implement
//   N37<Name>StartRun / N37<Name>Step / N37<Name>Begin / N37<Name>System in their own Game/SurvivalSim.<Name>37.cs.
//   - <Name>Begin runs first in BossRoomBegin (SurvivalSim.Boss25.cs), W then S then O then C: handled = true owns the begin.
//   - <Name>System runs in BossRoomSystem (after NIGHT36's): set boss for your id.
//   - Set boss37WalkInHere in StartRun when your row is on and the run is on your level: Campaign24 then keeps the player where they
//     walked in (no Respawn into a far arena: ICampaignWorld.BossWalkIn, Boss36WalkIn below includes it).
// - K (the Boss37 brain, Core/Boss37.cs) is a library the four use; it needs no sim hook.
// The Step hooks run every step right after N36BossStep (the player's side), W, S, O, C in that order.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        N37Blocks n37Design;

        /// <summary>A replay's or a harness's NIGHT37 blocks (a tape's bits); null = the design's, or off when forced off.</summary>
        public N37Blocks? n37Override;

        /// <summary>The NIGHT37 blocks the current run reads (set at StartRun).</summary>
        public N37Blocks n37;

        /// <summary>NIGHT37 (W / S / O / C): does this level's boss fight where the player walked in? False at every run start; a boss
        /// builder's StartRun sets it on its own level with its row on.</summary>
        public bool boss37WalkInHere;

        void N37Init() => n37Design = N37Blocks.From(cfg);

        void N37StartRun()
        {
            n37 = n37Override ?? (N37Blocks.ForceOff ? N37Blocks.Off : n37Design);
            boss37WalkInHere = false;
            N37WickStartRun();
            N37StagStartRun();
            N37LoomStartRun();
            N37CrownStartRun();
        }

        partial void N37WickStartRun();
        partial void N37WickStep(double dt); // NIGHT37 W: every step, after N36BossStep
        partial void N37WickBegin(string id, ref bool handled, ref bool ok, ref Vec3 spawn, ref double yawDeg);
        partial void N37WickSystem(string id, ref IBossSystem boss);
        partial void N37StagStartRun();
        partial void N37StagStep(double dt); // NIGHT37 S: after W's
        partial void N37StagBegin(string id, ref bool handled, ref bool ok, ref Vec3 spawn, ref double yawDeg);
        partial void N37StagSystem(string id, ref IBossSystem boss);
        partial void N37LoomStartRun();
        partial void N37LoomStep(double dt); // NIGHT37 O: after S's
        partial void N37LoomBegin(string id, ref bool handled, ref bool ok, ref Vec3 spawn, ref double yawDeg);
        partial void N37LoomSystem(string id, ref IBossSystem boss);
        partial void N37CrownStartRun();
        partial void N37CrownStep(double dt); // NIGHT37 C: after O's
        partial void N37CrownBegin(string id, ref bool handled, ref bool ok, ref Vec3 spawn, ref double yawDeg);
        partial void N37CrownSystem(string id, ref IBossSystem boss);

        /// <summary>NIGHT37: the four bosses' begins, in order; the first that handles the id wins (BossRoomBegin, the lead's line).</summary>
        bool N37BossBegin(string id, ref Vec3 spawn, ref double yawDeg, out bool ok)
        {
            bool handled = false;
            ok = false;
            N37WickBegin(id, ref handled, ref ok, ref spawn, ref yawDeg);
            if (!handled) N37StagBegin(id, ref handled, ref ok, ref spawn, ref yawDeg);
            if (!handled) N37LoomBegin(id, ref handled, ref ok, ref spawn, ref yawDeg);
            if (!handled) N37CrownBegin(id, ref handled, ref ok, ref spawn, ref yawDeg);
            return handled;
        }

        /// <summary>NIGHT37: the four bosses' systems for an id (BossRoomSystem, the lead's line); null when none owns it.</summary>
        IBossSystem N37BossSystem(string id)
        {
            IBossSystem b = null;
            N37WickSystem(id, ref b);
            if (b == null) N37StagSystem(id, ref b);
            if (b == null) N37LoomSystem(id, ref b);
            if (b == null) N37CrownSystem(id, ref b);
            return b;
        }

        /// <summary>NIGHT37: every step, after N36BossStep (SurvivalSim.Run.cs, the lead's line).</summary>
        void N37Step(double dt)
        {
            N37WickStep(dt);
            N37StagStep(dt);
            N37LoomStep(dt);
            N37CrownStep(dt);
        }
    }
}
