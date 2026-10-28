// NIGHT53 builder U (the polish lane; NIGHT53.md section 4): the render, layout and reel numbers of design.n53.polish53 (Polish53.cs; off =
// NIGHT52's look exactly; -skygrindOff53 turns it off). Unity-only numbers: the sim never reads them, so golden vectors, replays and
// fingerprints are untouched. Colours are the one palette's (UnityTuning.Look49.cs / UiStyle33: bone, cyan, red-orange, ink).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---- item 1 (T-122): the first shot's path compiled at boot (FirstShot53, Polish53.cs) -------------------------------------------------------
        // The types whose methods the first KESTREL / THUNDERCLAP shot calls for the first time (the fire, the cone, the hit, the view's handlers),
        // in the order compiled (the budget stops the list, the big HUD class last). Full names; a name not found is counted and skipped.
        public static readonly string[] Polish53JitTypes =
        {
            "Skygrind.Core.Loadout", "Skygrind.Core.CombatMath", "Skygrind.Core.WeaponMath", "Skygrind.Core.ChargeShots27", "Skygrind.Core.Husks",
            "Skygrind.Core.ShotEvent", "Skygrind.Core.EnemyRegistry",
            "Skygrind.Game.WeaponFx", "Skygrind.Game.LightPools", "Skygrind.Game.Viewmodel", "Skygrind.Game.ImpactFx", "Skygrind.Game.Decals",
            "Skygrind.Game.FlashGate", "Skygrind.Game.GunFeel43", "Skygrind.Game.Fx43Pool", "Skygrind.Game.Fx43Shade", "Skygrind.Game.ImpactFrames",
            "Skygrind.Game.FxLines", "Skygrind.Game.FxCubes", "Skygrind.Game.CubeBatch", "Skygrind.Game.FxDraw", "Skygrind.Game.FxGates",
            "Skygrind.Game.WeaponLook", "Skygrind.Game.CrunchMath", "Skygrind.Game.HalfLifeKick", "Skygrind.Game.HandsRecoilClock",
            "Skygrind.Game.HandsFlashGate", "Skygrind.Game.HandsImpactHold", "Skygrind.Game.EnemyLook", "Skygrind.Game.HuskView",
            "Skygrind.Game.Particles", "Skygrind.Game.Chunks", "Skygrind.Game.ChunkRing",
            // n53-r1 (the THUNDERCLAP's first shot still hitched): the fx49 kit (the cone, the boost ring, the hit bursts) and fx47's weapon layers
            "Skygrind.Game.Fx49Kit", "Skygrind.Game.Fx49", "Skygrind.Game.FxSprites", "Skygrind.Game.Fx47View", "Skygrind.Game.Feel43",
            "Skygrind.Game.TorchFeel43", "Skygrind.Game.Fusion43View", "Skygrind.Game.KillConfirm", "Skygrind.Game.SpawnFlashes",
            "Skygrind.Game.Ui",
        };
        public const double Polish53JitBudgetMs = 900; // ms at boot (the title's load, before the first frame): the list stops when it is spent (a boot hitch is paid once, behind the load; a shot's is paid in the fight)

        // ---- item 4 (T-116): iceline's grade (IcelineGrade53, Polish53.cs) ----
        public static readonly string[] Polish53IcelineLevels = { "n47-iceline", "n52-iceline", "n67-iceline" }; // THE ICE LINE's map ids (lower case): look52's grade list takes them while icelineGrade is on (NIGHT67 A1: n67-iceline, row design.n67.iceline67, Tools/night/n67-a1.cjs)
        public const int Polish53IcelineSlot = 0; // iceline's V50 slot (UnityTuning.V50Themes order: iceline 0, span 1)

        // ---- item 5: the kill pop's shards (UiKillPop53.cs; px at 1080p: the canvas scales with the height) ----
        public const float Polish53PopShardLen = 18f; // px: a shard's length (on the axes between the kill marker's diagonal wedges, Hud18KillMarkerInner 45 / Outer 75)
        public const float Polish53PopShardW = 4f; // px: its width (the hit marker's bold arms' family)
        public const float Polish53PopShardFrom = 34f; // px from the reticle to a shard's inner end on the kill's frame (clear of the 18 px reticle box)
        public const float Polish53PopShardTo = 92f; // px at the end of its flight (past the wedges' outer end: the burst reads outside the marker)

        // the a153 reel's item 1: the first shot's frames against the run's median frame
        public const int Polish53ReelMedianFrames = 90; // frames of the still pose measured before the shot (their median is the run's frame)
        public const int Polish53ReelShotFrames = 5; // frames from the shot's frame measured after it (the CPU waits on the GPU up to 2 frames late)
        public const float Polish53ReelHitchK = 2.0f; // GREEN: every one of those frames under this x the median (a hitch is a frame twice the run's)
        public const float Polish53ReelHitchFloorMs = 8f; // ... or under the median + this many ms (a 60 Hz median of 16.7 ms lets a 24 ms frame through either way)
    }
}
