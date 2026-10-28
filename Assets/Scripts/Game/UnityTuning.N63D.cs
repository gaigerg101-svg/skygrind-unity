// NIGHT63 builder D (slate D1, same-run SPOT pairs: SameRun63Math.N63D.cs, FeelReel.N63Same.cs, SameRun63View.N63D.cs): Unity-only numbers (the
// reel's planted cases and its planted scenario), each with its reason. No gameplay number here: the toggle changes no sim value.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>same63: the planted square's side (screen px). D1's doneWhen: a planted 10 x 10 px view change gives exactly 100 changed px
        /// between the two captures of one shot (n62-gun-yellow.py --pair counts a pixel whose channels differ by more than 24).</summary>
        public const int Same63PlantPx = 10;

        /// <summary>same63: the planted square's top-left corner (screen px from the top-left, both axes): inside the vignette's dark corner,
        /// clear of the HUD's 16 px edge margin, so every one of its pixels differs from what is under it by far more than 24.</summary>
        public const int Same63PlantAt = 40;

        /// <summary>same63: the planted square's colour (opaque magenta 255, 0, 255): no dusk pixel under it is within 24 of it on every channel.
        /// A reel-only test mark (never drawn without -skygrindSameRun63 n63.plant63), so CLAUDE.md rule 8's palette is not in play.</summary>
        public const float Same63PlantR = 1f, Same63PlantG = 0f, Same63PlantB = 1f;

        /// <summary>same63 (the planted scenario): the recorded frames (60 fps fixed clock: 4 s) and the frames between its shots; the first shot
        /// waits Same63FirstShot frames so the husks walk and the gun fires before it.</summary>
        public const int Same63Frames = 240, Same63ShotEvery = 36, Same63FirstShot = 24;

        /// <summary>same63: the husks' distances (m) ahead of the player, walking in (unfrozen) so the sim moves in every frame around the shots.</summary>
        public static readonly double[] Same63HuskAt = { 7, 11, 15 };

        /// <summary>colours63 (C1, Colours63.N63D.cs): the first scan of a map's colours, frames after the map is seen: after the build and decor54's
        /// first pass (Decor54SettleFrames 3) and Fixtures33's trims, so the decor already wears the level's accent when it is routed.</summary>
        public const int Colours63FirstScanFrames = 8;

        /// <summary>colours63: the second scan, s after the map is seen on decor54's own clock (FeelClock.UnscaledDelta summed; NIGHT63 review S2:
        /// not the wall clock): after decor54's second pass (Decor54SecondPassS 3 s), so a piece it
        /// moved late is routed too.</summary>
        public const float Colours63SecondScanS = 3.5f;

        /// <summary>colours63: the scene roots whose renderers and lights are THE WORLD (the map with its decor, string course and frieze, the light
        /// kits, props, roofs, lava): the only colours the world tier moves. Enemies ("Husks", "Swarmers", "Spitters"), bosses, the viewmodel, the HUD
        /// and the effects live under other roots and are never touched (each scan's line names the other roots that hold off-palette colours).</summary>
        public static readonly string[] Colours63WorldRoots = { "Survival map", "Light kit 29", "Props 29", "Roof 27", "Roof meshes", "Look 43", "Lava 30", "LavaView30", "Decor54", "Decor55", "Colour56", "Light pools" };

        /// <summary>same63: the KESTREL fires every this many frames at the nearest husk (the aim bot): muzzle, tracer, hits and chunks on screen,
        /// so the planted no-op checks the effects' clocks too.</summary>
        public const int Same63FireEvery = 9;
    }
}
