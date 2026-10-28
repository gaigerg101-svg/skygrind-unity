// NIGHT28 builder B2: THE GLACIER lap fight's look and HUD numbers (Game/GlacierView28.cs). Visual only: the sim never reads these (its numbers are design.n28.glacier). Dusk palette (rule 8): cyan is
// player-usable (the glaze plates, the marker ring, the PULSE-READY shell), red-orange is the attacks (danger only), pale violet-white the boundary tone; magenta never.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The pooled attack boxes the view can show at once (the sim has at most a trail of shards + a P3 object at a time).</summary>
        public const int Glacier28HazardMax = 16;
        /// <summary>The cyan marker on the boss, so it is never a speck across the island: a thin beam this tall (m) and wide (m), and a ring (a square frame of four bars) this wide (m) at this height (m); gains on the additive colour.</summary>
        public const float G28BeamHeight = 14f, G28BeamWidth = 0.3f, G28RingSize = 4.4f, G28RingBar = 0.14f, G28RingLift = 1.2f, G28BeamGain = 1.4f, G28RingGain = 2.0f;
        /// <summary>The glaze plates: a cyan shell this much bigger than the body while glazed (gain falls as the crack fills), and the PULSE-READY shell while a window is open (the NIGHT27 ready colour).</summary>
        public const float G28GlazeGrow = 1.12f, G28GlazeGain = 1.6f, G28ReadyGrow = 1.3f;
        /// <summary>The attack decal (the tell) and the standing object: gains on the red-orange danger colour (the decal is flat, this thin in m).</summary>
        public const float G28DecalThick = 0.04f, G28DecalGain = 1.2f, G28ObjectGain = 1.0f;
        /// <summary>The gap tape (TextMeshPro, top centre of the screen): font size in px at 1080p (UI text is at least 16 px; rule 8), distance from the top edge in px.</summary>
        public const float G28TapeFont = 30f, G28TapeTop = 150f;
        /// <summary>The boss's chasm leap: the body rises this high (m) over the chasm's middle.</summary>
        public const float G28LeapHeight = 2.6f;
    }
}
