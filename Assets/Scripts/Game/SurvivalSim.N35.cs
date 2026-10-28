// NIGHT35 (the shared commit, the lead owns this file): the four sim-changing NIGHT35 design blocks in the sim (Core/N35Blocks.cs).
// A run takes them at its start: a tape's replay hands its own (n35Override), --off35 / -skygrindOff35 (and every older off switch)
// take every block off (N35Blocks.ForceOff), otherwise the design defaults. The builders hook in through the declaration-only
// partial methods below (a partial method with no body costs nothing; the call sites in SurvivalSim.Run.cs are the lead's):
// - W (weapons) implements N35WeaponsStartRun / N35WeaponsStep (Game/SurvivalSim.Weapons35.cs): charge35.
// - M (movement) implements N35MoveStartRun / N35MoveStep (Game/SurvivalSim.Move35.cs): wallride35 (its Mover.cs branches read the
//   mover's own flags, set at the start of the run as flow34's are) and grapple35. M also sets `grapple35Here` in N35MoveStartRun:
//   false on a campaign level before design.n35.grapple35.fromLevel, where the auto-own (SurvivalSim.Systems30.cs N30SystemsStep)
//   then leaves the grapple unowned (and with it the bend, which fires on a latch).
// - T (teaching) implements N35TeachStartRun / N35TeachStep (Game/SurvivalSim.Teach35.cs): teach35.
// The three Step hooks run every step right after N34BendStep (the player's side), W then M then T.
using Skygrind.Core;

namespace Skygrind.Game
{
    public sealed partial class SurvivalSim
    {
        N35Blocks n35Design;

        /// <summary>A replay's or a harness's NIGHT35 blocks (a tape's bits); null = the design's, or off when forced off.</summary>
        public N35Blocks? n35Override;

        /// <summary>The NIGHT35 blocks the current run reads (set at StartRun).</summary>
        public N35Blocks n35;

        /// <summary>NIGHT35 M (grapple35): may the grapple be owned from the start of this campaign level? True at every run start; M's
        /// N35MoveStartRun clears it on a level before design.n35.grapple35.fromLevel. Read only while n35.grapple35 is on.</summary>
        public bool grapple35Here = true;

        void N35Init() => n35Design = N35Blocks.From(cfg);

        void N35StartRun()
        {
            n35 = n35Override ?? (N35Blocks.ForceOff ? N35Blocks.Off : n35Design);
            grapple35Here = true;
            N35WeaponsStartRun();
            N35MoveStartRun();
            N35TeachStartRun();
        }

        /// <summary>NIGHT35: the grapple's auto-own on a campaign level (N30SystemsStep) is allowed here: always with grapple35 off (NIGHT34's
        /// rule: a level with nodes), else only on a level at or after design.n35.grapple35.fromLevel (M's grapple35Here), with or without
        /// nodes (the grapple pulls to walls, so a level from 4 on never loses it).</summary>
        bool Grapple35AutoOwn => !n35.grapple35 || grapple35Here;

        partial void N35WeaponsStartRun();
        partial void N35WeaponsStep(double dt); // NIGHT35 W: every step, after N34BendStep
        partial void N35MoveStartRun();
        partial void N35MoveStep(double dt); // NIGHT35 M: every step, after W's
        partial void N35TeachStartRun();
        partial void N35TeachStep(double dt); // NIGHT35 T: every step, after M's
    }
}
