// NIGHT40 builder B5 (crown40): THE CROWN's NIGHT40 look and words (Game/Crown37View.Crown40.cs). Unity-only (render, UI layout); every
// gameplay number is in design.n40.crown40 (the pass speed is design.n37.crown37.crown.passSpeed).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>The ring when it may be passed (no tell, no window): CYAN (player-usable, rule 8) at a low gain; LIT (his flat speed at the
        /// pass speed: the jump-dash) at a high gain. Red-orange stays only on its attacks' tells (D2: the idle ring read as fire, frames f0300-f1000).</summary>
        public const float Crown40IdleGain = 0.55f, Crown40LitGain = 1.9f, Crown40LitPulseHz = 2.5f;
        /// <summary>The gems when idle: pale bone (the thing to break, not danger), gain.</summary>
        public const uint Crown40GemHex = 0xf2ead8;
        public const float Crown40GemGain = 1.1f;
        /// <summary>v10's rehearsal ring: the same idle cyan (the fight's ring rehearsed).</summary>
        public const float Crown40HoopGain = 0.7f;
        /// <summary>The words (px at 1080p; at least 16, rule 8): the gem counter under the boss bar (its y from the top), the window's and the
        /// blocked hit's line under the crosshair (its y from the centre), BLOCKED held this long (s) after a blocked hit.</summary>
        public const float Crown40CounterPx = 24f, Crown40CounterY = -104f, Crown40CallPx = 30f, Crown40CallY = -86f, Crown40BlockedSeconds = 0.6f;
        /// <summary>The blade ring's decal through its tell: the red-orange band at its full reach, gain (rising 40 % to 100 % over the tell).</summary>
        public const float Crown40DecalGain = 0.6f;
        /// <summary>The words' canvas order (over the world, under the menus).</summary>
        public const int Crown40CanvasOrder = 20;
    }
}
